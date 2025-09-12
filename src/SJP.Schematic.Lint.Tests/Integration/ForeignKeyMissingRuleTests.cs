using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Lint.Rules;
using SJP.Schematic.Tests.Utilities;

namespace SJP.Schematic.Lint.Tests.Integration;

internal sealed class ForeignKeyMissingRuleTests : SqliteTest
{
    [OneTimeSetUp]
    public async Task Init()
    {
        await DbConnection.ExecuteAsync("create table no_foreign_key_parent_1 ( column_1 integer not null primary key autoincrement )", CancellationToken.None).ConfigureAwait(false);
        await DbConnection.ExecuteAsync("create table NoForeignKeyParent1 ( Column1 integer not null primary key autoincrement )", CancellationToken.None).ConfigureAwait(false);
        await DbConnection.ExecuteAsync(@"
create table no_foreign_key_child_with_key (
    column_1 integer,
    no_foreign_key_parent_1_id integer,
    constraint no_foreign_key_child_with_key_fk1 foreign key (no_foreign_key_parent_1_id) references no_foreign_key_parent_1 (column_1)
)", CancellationToken.None).ConfigureAwait(false);
        await DbConnection.ExecuteAsync(@"
create table no_foreign_key_child_without_key (
    column_1 integer,
    no_foreign_key_parent_1_id integer
)", CancellationToken.None).ConfigureAwait(false);
        await DbConnection.ExecuteAsync(@"
create table NoForeignKeyChildWithKey (
    Column1 integer,
    NoForeignKeyParent1Id integer,
    constraint NoForeignKeyChildWithKeyFk1 foreign key (NoForeignKeyParent1Id) references NoForeignKeyParent1 (Column1)
)", CancellationToken.None).ConfigureAwait(false);
        await DbConnection.ExecuteAsync(@"
create table NoForeignKeyChildWithoutKey (
    Column1 integer,
    NoForeignKeyParent1Id integer
)", CancellationToken.None).ConfigureAwait(false);
    }

    [OneTimeTearDown]
    public async Task CleanUp()
    {
        await DbConnection.ExecuteAsync("drop table no_foreign_key_child_with_key", CancellationToken.None).ConfigureAwait(false);
        await DbConnection.ExecuteAsync("drop table no_foreign_key_child_without_key", CancellationToken.None).ConfigureAwait(false);
        await DbConnection.ExecuteAsync("drop table NoForeignKeyChildWithKey", CancellationToken.None).ConfigureAwait(false);
        await DbConnection.ExecuteAsync("drop table NoForeignKeyChildWithoutKey", CancellationToken.None).ConfigureAwait(false);
        await DbConnection.ExecuteAsync("drop table no_foreign_key_parent_1", CancellationToken.None).ConfigureAwait(false);
        await DbConnection.ExecuteAsync("drop table NoForeignKeyParent1", CancellationToken.None).ConfigureAwait(false);
    }

    [Test]
    public static void Ctor_GivenInvalidLevel_ThrowsArgumentException()
    {
        const RuleLevel level = (RuleLevel)999;
        Assert.That(() => new ForeignKeyMissingRule(level), Throws.ArgumentException);
    }

    [Test]
    public static void AnalyseTables_GivenNullTables_ThrowsArgumentNullException()
    {
        var rule = new ForeignKeyMissingRule(RuleLevel.Error);
        Assert.That(() => rule.AnalyseTables(null), Throws.ArgumentNullException);
    }

    [Test]
    public async Task AnalyseTables_GivenSnakeCaseTablesContainingTableWithValidForeignKey_ProducesNoMessages()
    {
        var rule = new ForeignKeyMissingRule(RuleLevel.Error);
        var database = GetSqliteDatabase();

        var tables = new[]
        {
            await database.GetTable("no_foreign_key_parent_1").UnwrapSomeAsync().ConfigureAwait(false),
            await database.GetTable("no_foreign_key_child_with_key").UnwrapSomeAsync().ConfigureAwait(false),
        };

        var hasMessages = await rule.AnalyseTables(tables).AnyAsync().ConfigureAwait(false);

        Assert.That(hasMessages, Is.False);
    }

    [Test]
    public async Task AnalyseTables_GivenCamelCaseTablesContainingTableWithValidForeignKey_ProducesNoMessages()
    {
        var rule = new ForeignKeyMissingRule(RuleLevel.Error);
        var database = GetSqliteDatabase();

        var tables = new[]
        {
            await database.GetTable("NoForeignKeyChildWithKey").UnwrapSomeAsync().ConfigureAwait(false),
            await database.GetTable("NoForeignKeyParent1").UnwrapSomeAsync().ConfigureAwait(false),
        };

        var hasMessages = await rule.AnalyseTables(tables).AnyAsync().ConfigureAwait(false);

        Assert.That(hasMessages, Is.False);
    }

    [Test]
    public async Task AnalyseTables_GivenSnakeCaseTablesContainingTableWithValidForeignKey_ProducesMessages()
    {
        var rule = new ForeignKeyMissingRule(RuleLevel.Error);
        var database = GetSqliteDatabase();

        var tables = new[]
        {
            await database.GetTable("no_foreign_key_parent_1").UnwrapSomeAsync().ConfigureAwait(false),
            await database.GetTable("no_foreign_key_child_without_key").UnwrapSomeAsync().ConfigureAwait(false),
        };

        var hasMessages = await rule.AnalyseTables(tables).AnyAsync().ConfigureAwait(false);

        Assert.That(hasMessages, Is.True);
    }

    [Test]
    public async Task AnalyseTables_GivenCamelCaseTablesContainingTableWithValidForeignKey_ProducesMessages()
    {
        var rule = new ForeignKeyMissingRule(RuleLevel.Error);
        var database = GetSqliteDatabase();

        var tables = new[]
        {
            await database.GetTable("NoForeignKeyChildWithoutKey").UnwrapSomeAsync().ConfigureAwait(false),
            await database.GetTable("NoForeignKeyParent1").UnwrapSomeAsync().ConfigureAwait(false),
        };

        var hasMessages = await rule.AnalyseTables(tables).AnyAsync().ConfigureAwait(false);

        Assert.That(hasMessages, Is.True);
    }
}