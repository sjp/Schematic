using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using MySqlConnector;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Tests.Utilities;

namespace SJP.Schematic.MySql.Tests.Integration;

/// <summary>
/// Loads child keys both as the test user and as a user without the <c>PROCESS</c> privilege. On MariaDB the
/// tables referencing a table are read from InnoDB's foreign key dictionary when the user can read it, and from
/// <c>information_schema.referential_constraints</c> otherwise. Both must report the same child keys.
/// </summary>
/// <remarks>
/// MariaDB hides a table's constraints from a user whose only privilege on it is <c>SELECT</c>, so the user is
/// granted <c>REFERENCES</c> as well.
/// </remarks>
[NonParallelizable]
internal sealed class MySqlChildKeyLookupTests : MySqlTest
{
    private const string UserName = "schematic_no_process";
    private const string Password = "Schematic_NoProcess1";
    private const string OtherDatabaseName = "schematic_child_key_lookup";

    private MySqlConnectionFactory _restrictedConnectionFactory = null!;

    private IRelationalDatabaseTableProvider TableProvider => new MySqlRelationalDatabaseTableProvider(Connection, IdentifierDefaults);

    private IRelationalDatabaseTableProvider RestrictedTableProvider => new MySqlRelationalDatabaseTableProvider(
        new SchematicConnection(_restrictedConnectionFactory, new MySqlDialect()),
        IdentifierDefaults);

    [OneTimeSetUp]
    public async Task Init()
    {
        await ExecuteBatchAsync(
            "create table child_key_lookup_parent (id int primary key, alt_id int not null, constraint uk_child_key_lookup_parent unique (alt_id))",
            "create table child_key_lookup_child_1 (id int primary key, parent_id int, constraint fk_child_key_lookup_child_1 foreign key (parent_id) references child_key_lookup_parent (id) on delete cascade)",
            "create table child_key_lookup_child_2 (id int primary key, parent_alt_id int, parent_id int, "
                + "constraint fk_child_key_lookup_child_2_alt foreign key (parent_alt_id) references child_key_lookup_parent (alt_id) on update set null, "
                + "constraint fk_child_key_lookup_child_2_id foreign key (parent_id) references child_key_lookup_parent (id))",
            "create table `child_key_lookup-escaped$parent` (id int primary key)",
            "create table `child_key_lookup-escaped$child` (id int primary key, parent_id int, constraint fk_child_key_lookup_escaped foreign key (parent_id) references `child_key_lookup-escaped$parent` (id))",
            "create table child_key_lookup_unescaped_parent (id int primary key)",
            "create table `child_key_lookup-escaped$child_2` (id int primary key, parent_id int, constraint fk_child_key_lookup_escaped_2 foreign key (parent_id) references child_key_lookup_unescaped_parent (id))",
            "create table child_key_lookup_keyless (id int)",
            $"create database if not exists {OtherDatabaseName}",
            $"create table {OtherDatabaseName}.child_key_lookup_other_child (id int primary key, parent_id int, constraint fk_child_key_lookup_other_child foreign key (parent_id) references {IdentifierDefaults.Schema}.child_key_lookup_parent (id) on delete set null)",
            $"create user '{UserName}'@'%' identified by '{Password}'",
            $"grant select, references on {IdentifierDefaults.Schema}.* to '{UserName}'@'%'",
            $"grant select, references on {OtherDatabaseName}.* to '{UserName}'@'%'"
        );

        var builder = new MySqlConnectionStringBuilder(Config.ConnectionString)
        {
            UserID = UserName,
            Password = Password,
        };
        _restrictedConnectionFactory = new MySqlConnectionFactory(builder.ConnectionString);
    }

    [OneTimeTearDown]
    public async Task CleanUp()
    {
        if (_restrictedConnectionFactory != null)
            await _restrictedConnectionFactory.DisposeAsync();

        await ExecuteBatchAsync(
            $"drop user if exists '{UserName}'@'%'",
            $"drop database if exists {OtherDatabaseName}",
            "drop table if exists `child_key_lookup-escaped$child_2`",
            "drop table if exists child_key_lookup_unescaped_parent",
            "drop table if exists `child_key_lookup-escaped$child`",
            "drop table if exists `child_key_lookup-escaped$parent`",
            "drop table if exists child_key_lookup_child_2",
            "drop table if exists child_key_lookup_child_1",
            "drop table if exists child_key_lookup_parent",
            "drop table if exists child_key_lookup_keyless"
        );
    }

    private static readonly string[] ExpectedParentChildKeys =
    [
        "child_key_lookup_child_1.fk_child_key_lookup_child_1(parent_id) -> PRIMARY(id) on delete Cascade on update NoAction|Restrict",
        "child_key_lookup_child_2.fk_child_key_lookup_child_2_alt(parent_alt_id) -> uk_child_key_lookup_parent(alt_id) on delete NoAction|Restrict on update SetNull",
        "child_key_lookup_child_2.fk_child_key_lookup_child_2_id(parent_id) -> PRIMARY(id) on delete NoAction|Restrict on update NoAction|Restrict",
        $"{OtherDatabaseName}.child_key_lookup_other_child.fk_child_key_lookup_other_child(parent_id) -> PRIMARY(id) on delete SetNull on update NoAction|Restrict",
    ];

    [Test]
    public async Task GetTable_WithProcessPrivilege_ReturnsChildKeysFromEveryDatabase()
    {
        var table = await TableProvider.GetTable("child_key_lookup_parent", TestContext.CurrentContext.CancellationToken).UnwrapSomeAsync();

        Assert.That(DescribeChildKeys(table), Is.EquivalentTo(ExpectedParentChildKeys));
    }

    [Test]
    public async Task GetTable_WithoutProcessPrivilege_ReturnsChildKeysFromEveryDatabase()
    {
        var table = await RestrictedTableProvider.GetTable("child_key_lookup_parent", TestContext.CurrentContext.CancellationToken).UnwrapSomeAsync();

        Assert.That(DescribeChildKeys(table), Is.EquivalentTo(ExpectedParentChildKeys));
    }

    [Test]
    public async Task GetAllTables_WithAndWithoutProcessPrivilege_ReturnSameChildKeys()
    {
        var tables = await TableProvider.GetAllTables(TestContext.CurrentContext.CancellationToken);
        var restrictedTables = await RestrictedTableProvider.GetAllTables(TestContext.CurrentContext.CancellationToken);

        var childKeys = DescribeLookupTableChildKeys(tables);
        var restrictedChildKeys = DescribeLookupTableChildKeys(restrictedTables);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(childKeys, Is.EqualTo(restrictedChildKeys));
            Assert.That(childKeys["child_key_lookup_parent"], Is.EquivalentTo(ExpectedParentChildKeys));
        }
    }

    [TestCase("child_key_lookup-escaped$parent", "child_key_lookup-escaped$child.fk_child_key_lookup_escaped(parent_id) -> PRIMARY(id) on delete NoAction|Restrict on update NoAction|Restrict")]
    [TestCase("child_key_lookup_unescaped_parent", "child_key_lookup-escaped$child_2.fk_child_key_lookup_escaped_2(parent_id) -> PRIMARY(id) on delete NoAction|Restrict on update NoAction|Restrict")]
    public async Task GetTable_WhenTableNamesNeedEscapingInInnoDb_ReturnsChildKeys(string tableName, string expectedChildKey)
    {
        var table = await TableProvider.GetTable(tableName, TestContext.CurrentContext.CancellationToken).UnwrapSomeAsync();
        var restrictedTable = await RestrictedTableProvider.GetTable(tableName, TestContext.CurrentContext.CancellationToken).UnwrapSomeAsync();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(DescribeChildKeys(table), Is.EqualTo(new[] { expectedChildKey }));
            Assert.That(DescribeChildKeys(restrictedTable), Is.EqualTo(new[] { expectedChildKey }));
        }
    }

    // Name resolution, columns, catalog features, indexes (from which the keys are built), parent keys, and
    // the table's options, checks and triggers. A table with no primary or unique key cannot be referenced,
    // so nothing is queried for its child keys.
    [Test]
    public async Task GetTable_ForTableWithNoPrimaryOrUniqueKey_DoesNotQueryForChildKeys()
    {
        var countingConnectionFactory = new CountingDbConnectionFactory(Config.ConnectionFactory);
        var tableProvider = new MySqlRelationalDatabaseTableProvider(new SchematicConnection(countingConnectionFactory, Dialect), IdentifierDefaults);

        var table = await tableProvider.GetTable("child_key_lookup_keyless", TestContext.CurrentContext.CancellationToken).UnwrapSomeAsync();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(table.ChildKeys, Is.Empty);
            Assert.That(countingConnectionFactory.QueryCount, Is.EqualTo(6));
        }
    }

    [Test]
    public async Task GetTable_OnMariaDbWithProcessPrivilege_ReadsChildTablesFromInnoDb()
    {
        Assume.That(IsMariaDb, Is.True, "InnoDB's foreign key dictionary is only read on MariaDB");
        Assume.That(await CanReadInnoDbForeignKeysAsync(), Is.True, "The test user lacks the PROCESS privilege");

        var statements = await RecordStatementsAsync(() => TableProvider.GetTable("child_key_lookup_parent", TestContext.CurrentContext.CancellationToken).UnwrapSomeAsync());

        using (Assert.EnterMultipleScope())
        {
            Assert.That(statements, Has.Some.Contains("innodb_sys_foreign"));
            Assert.That(statements, Has.None.Contains("rc.referenced_table_name = @"));
        }
    }

    [Test]
    public async Task GetTable_OnMariaDbWithoutProcessPrivilege_TriesInnoDbOnlyOnce()
    {
        Assume.That(IsMariaDb, Is.True, "InnoDB's foreign key dictionary is only read on MariaDB");

        var tableProvider = RestrictedTableProvider;
        var statements = await RecordStatementsAsync(async () =>
        {
            await tableProvider.GetTable("child_key_lookup_parent", TestContext.CurrentContext.CancellationToken).UnwrapSomeAsync();
            await tableProvider.GetTable("child_key_lookup_unescaped_parent", TestContext.CurrentContext.CancellationToken).UnwrapSomeAsync();
        });

        Assert.That(statements.Count(static s => s.Contains("innodb_sys_foreign", StringComparison.Ordinal)), Is.EqualTo(1));
    }

    private async Task<bool> CanReadInnoDbForeignKeysAsync()
    {
        try
        {
            await DbConnection.ExecuteAsync("select count(*) from information_schema.innodb_sys_foreign where ref_name = ''", TestContext.CurrentContext.CancellationToken);
            return true;
        }
        catch (MySqlException)
        {
            return false;
        }
    }

    // MySqlConnector reports each command it executes as an activity, tagged with the command text. The fixture
    // runs on its own, so every statement recorded comes from the given action.
    private static async Task<IReadOnlyList<string>> RecordStatementsAsync(Func<Task> action)
    {
        var statements = new ConcurrentQueue<string>();
        using var listener = new ActivityListener
        {
            ShouldListenTo = static source => source.Name == "MySqlConnector",
            Sample = static (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllData,
            ActivityStopped = activity =>
            {
                var statement = activity.GetTagItem("db.query.text") ?? activity.GetTagItem("db.statement");
                if (statement is string text)
                    statements.Enqueue(text);
            },
        };
        ActivitySource.AddActivityListener(listener);

        await action();

        return statements.ToList();
    }

    private Dictionary<string, IReadOnlyList<string>> DescribeLookupTableChildKeys(IEnumerable<IRelationalDatabaseTable> tables) => tables
        .Where(static t => t.Name.LocalName.StartsWith("child_key_lookup", StringComparison.Ordinal))
        .ToDictionary(static t => t.Name.LocalName, t => DescribeChildKeys(t), StringComparer.Ordinal);

    private IReadOnlyList<string> DescribeChildKeys(IRelationalDatabaseTable table) => table.ChildKeys
        .Select(k =>
        {
            var childTable = string.Equals(k.ChildTable.Schema, IdentifierDefaults.Schema, StringComparison.Ordinal)
                ? k.ChildTable.LocalName
                : k.ChildTable.Schema + "." + k.ChildTable.LocalName;
            var childColumns = string.Join(", ", k.ChildKey.Columns.Select(static c => c.Name.LocalName));
            var parentColumns = string.Join(", ", k.ParentKey.Columns.Select(static c => c.Name.LocalName));
            return $"{childTable}.{k.ChildKey.Name.UnwrapSome().LocalName}({childColumns}) -> {k.ParentKey.Name.UnwrapSome().LocalName}({parentColumns}) "
                + $"on delete {DescribeAction(k.DeleteAction)} on update {DescribeAction(k.UpdateAction)}";
        })
        .ToList();

    // MySQL treats RESTRICT and NO ACTION as the same thing, and servers differ in which one they report.
    private static string DescribeAction(ReferentialAction action) => action is ReferentialAction.NoAction or ReferentialAction.Restrict
        ? "NoAction|Restrict"
        : action.ToString();
}
