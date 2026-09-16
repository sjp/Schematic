using System;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Sqlite.Pragma;
using SJP.Schematic.Tests.Utilities;

namespace SJP.Schematic.Sqlite.Tests.Integration;

// Covers the keys that SQLite accepts as a foreign key's parent but does not report as constraints.
// Runs against a private in-memory database held by a single connection, so these tables take no
// part in any other fixture's child key scan, and no other fixture's tables take part in theirs.
internal sealed class SqliteRelationalDatabaseTableProviderUniqueIndexKeyTests : SqliteTest
{
    private CachingConnectionFactory _connectionFactory;
    private IRelationalDatabaseTableProvider _tableProvider;

    [OneTimeSetUp]
    public async Task Init()
    {
        var cancellationToken = TestContext.CurrentContext.CancellationToken;
        _connectionFactory = new CachingConnectionFactory(new SqliteConnectionFactory($"Data Source=UniqueIndexKeys_{Guid.NewGuid():N};Mode=Memory;Cache=Shared"));

        var connection = new SchematicConnection(_connectionFactory, Dialect);
        _tableProvider = new SqliteRelationalDatabaseTableProvider(connection, new ConnectionPragma(connection), new IdentifierDefaults(null, "main", "main"));

        // a unique index declared outside of any constraint, which SQLite accepts as a parent key
        await _connectionFactory.ExecuteAsync("create table unique_index_parent ( id integer primary key, code integer not null )", cancellationToken);
        await _connectionFactory.ExecuteAsync("create unique index ux_unique_index_parent_code on unique_index_parent (code)", cancellationToken);
        await _connectionFactory.ExecuteAsync(@"
create table unique_index_child (
    code integer,
    constraint fk_unique_index_child_code foreign key (code) references unique_index_parent (code)
)", cancellationToken);

        await _connectionFactory.ExecuteAsync("create table composite_unique_index_parent ( first_name text, last_name text )", cancellationToken);
        await _connectionFactory.ExecuteAsync("create unique index ux_composite_unique_index_parent on composite_unique_index_parent (first_name, last_name)", cancellationToken);
        await _connectionFactory.ExecuteAsync(@"
create table composite_unique_index_child (
    first_name text,
    last_name text,
    constraint fk_composite_unique_index_child foreign key (last_name, first_name) references composite_unique_index_parent (last_name, first_name)
)", cancellationToken);

        // constraints whose columns the foreign key names in a different order, which SQLite also accepts
        await _connectionFactory.ExecuteAsync(@"
create table reordered_unique_constraint_parent (
    first_name text,
    last_name text,
    constraint uk_reordered_unique_constraint_parent unique (first_name, last_name)
)", cancellationToken);
        await _connectionFactory.ExecuteAsync(@"
create table reordered_unique_constraint_child (
    first_name text,
    last_name text,
    constraint fk_reordered_unique_constraint_child foreign key (last_name, first_name) references reordered_unique_constraint_parent (last_name, first_name)
)", cancellationToken);
        await _connectionFactory.ExecuteAsync(@"
create table reordered_primary_key_parent (
    first_name text,
    last_name text,
    constraint pk_reordered_primary_key_parent primary key (first_name, last_name)
)", cancellationToken);
        await _connectionFactory.ExecuteAsync(@"
create table reordered_primary_key_child (
    first_name text,
    last_name text,
    constraint fk_reordered_primary_key_child foreign key (last_name, first_name) references reordered_primary_key_parent (last_name, first_name)
)", cancellationToken);

        // indexes that SQLite refuses to satisfy a foreign key with, so no relationship exists to report
        await _connectionFactory.ExecuteAsync("create table partial_unique_index_parent ( code integer )", cancellationToken);
        await _connectionFactory.ExecuteAsync("create unique index ux_partial_unique_index_parent on partial_unique_index_parent (code) where code > 0", cancellationToken);
        await _connectionFactory.ExecuteAsync("create table partial_unique_index_child ( code integer references partial_unique_index_parent (code) )", cancellationToken);

        await _connectionFactory.ExecuteAsync("create table expression_unique_index_parent ( code integer )", cancellationToken);
        await _connectionFactory.ExecuteAsync("create unique index ux_expression_unique_index_parent on expression_unique_index_parent (abs(code))", cancellationToken);
        await _connectionFactory.ExecuteAsync("create table expression_unique_index_child ( code integer references expression_unique_index_parent (code) )", cancellationToken);

        await _connectionFactory.ExecuteAsync("create table non_unique_index_parent ( code integer )", cancellationToken);
        await _connectionFactory.ExecuteAsync("create index ix_non_unique_index_parent on non_unique_index_parent (code)", cancellationToken);
        await _connectionFactory.ExecuteAsync("create table non_unique_index_child ( code integer references non_unique_index_parent (code) )", cancellationToken);

        // only part of the unique index's column set, which is not unique on its own
        await _connectionFactory.ExecuteAsync("create table partial_column_set_child ( first_name text references composite_unique_index_parent (first_name) )", cancellationToken);
    }

    [OneTimeTearDown]
    public async Task CleanUp()
    {
        if (_connectionFactory != null)
            await _connectionFactory.DisposeAsync();
    }

    private Task<IRelationalDatabaseTable> GetTableAsync(string tableName) =>
        _tableProvider.GetTable(tableName, TestContext.CurrentContext.CancellationToken).UnwrapSomeAsync();

    [Test]
    public async Task ParentKeys_WhenForeignKeyReferencesUniqueIndex_ResolvesKeyNamedAfterTheIndex()
    {
        var table = await GetTableAsync("unique_index_child");
        var foreignKey = table.ParentKeys.Single();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(foreignKey.ChildKey.Name.UnwrapSome().LocalName, Is.EqualTo("fk_unique_index_child_code"));
            Assert.That(foreignKey.ParentTable.LocalName, Is.EqualTo("unique_index_parent"));
            Assert.That(foreignKey.ParentKey.Name.UnwrapSome().LocalName, Is.EqualTo("ux_unique_index_parent_code"));
            Assert.That(foreignKey.ParentKey.KeyType, Is.EqualTo(DatabaseKeyType.Unique));
            Assert.That(foreignKey.ParentKey.Columns.Select(c => c.Name.LocalName), Is.EqualTo(new[] { "code" }));
        }
    }

    [Test]
    public async Task ParentKeys_WhenForeignKeyReferencesUniqueIndex_ResolvesKeyBackedByThatIndex()
    {
        var table = await GetTableAsync("unique_index_child");
        var foreignKey = table.ParentKeys.Single();
        var backingIndex = foreignKey.ParentKey.BackingIndex.UnwrapSome();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(backingIndex.Name.LocalName, Is.EqualTo("ux_unique_index_parent_code"));
            Assert.That(backingIndex.IsUnique, Is.True);
        }
    }

    [Test]
    public async Task ChildKeys_WhenForeignKeyReferencesUniqueIndex_ReturnsRelationship()
    {
        var table = await GetTableAsync("unique_index_parent");
        var childKey = table.ChildKeys.Single();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(childKey.ChildTable.LocalName, Is.EqualTo("unique_index_child"));
            Assert.That(childKey.ChildKey.Name.UnwrapSome().LocalName, Is.EqualTo("fk_unique_index_child_code"));
            Assert.That(childKey.ParentKey.Name.UnwrapSome().LocalName, Is.EqualTo("ux_unique_index_parent_code"));
        }
    }

    [Test]
    public async Task UniqueKeys_WhenTableHasUniqueIndexWithNoConstraint_DoesNotReportTheIndexAsAKey()
    {
        var table = await GetTableAsync("unique_index_parent");

        using (Assert.EnterMultipleScope())
        {
            Assert.That(table.UniqueKeys, Is.Empty);
            Assert.That(table.Indexes.Select(i => i.Name.LocalName), Is.EqualTo(new[] { "ux_unique_index_parent_code" }));
        }
    }

    [Test]
    public async Task ParentKeys_WhenForeignKeyReferencesCompositeUniqueIndexInAnotherColumnOrder_ResolvesKey()
    {
        var table = await GetTableAsync("composite_unique_index_child");
        var foreignKey = table.ParentKeys.Single();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(foreignKey.ParentKey.Name.UnwrapSome().LocalName, Is.EqualTo("ux_composite_unique_index_parent"));
            Assert.That(foreignKey.ChildKey.Columns.Select(c => c.Name.LocalName), Is.EqualTo(new[] { "last_name", "first_name" }));
        }
    }

    [Test]
    public async Task ParentKeys_WhenForeignKeyReferencesUniqueConstraintInAnotherColumnOrder_ResolvesKey()
    {
        var table = await GetTableAsync("reordered_unique_constraint_child");
        var foreignKey = table.ParentKeys.Single();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(foreignKey.ParentKey.Name.UnwrapSome().LocalName, Is.EqualTo("uk_reordered_unique_constraint_parent"));
            Assert.That(foreignKey.ParentKey.KeyType, Is.EqualTo(DatabaseKeyType.Unique));
        }
    }

    [Test]
    public async Task ParentKeys_WhenForeignKeyReferencesPrimaryKeyInAnotherColumnOrder_ResolvesKey()
    {
        var table = await GetTableAsync("reordered_primary_key_child");
        var foreignKey = table.ParentKeys.Single();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(foreignKey.ParentKey.Name.UnwrapSome().LocalName, Is.EqualTo("pk_reordered_primary_key_parent"));
            Assert.That(foreignKey.ParentKey.KeyType, Is.EqualTo(DatabaseKeyType.Primary));
        }
    }

    [Test]
    public async Task ParentKeys_WhenForeignKeyReferencesPartialUniqueIndex_ReturnsNoParentKeys()
    {
        var table = await GetTableAsync("partial_unique_index_child");

        Assert.That(table.ParentKeys, Is.Empty);
    }

    [Test]
    public async Task ParentKeys_WhenForeignKeyReferencesUniqueIndexOverAnExpression_ReturnsNoParentKeys()
    {
        var table = await GetTableAsync("expression_unique_index_child");

        Assert.That(table.ParentKeys, Is.Empty);
    }

    [Test]
    public async Task ParentKeys_WhenForeignKeyReferencesNonUniqueIndex_ReturnsNoParentKeys()
    {
        var table = await GetTableAsync("non_unique_index_child");

        Assert.That(table.ParentKeys, Is.Empty);
    }

    [Test]
    public async Task ParentKeys_WhenForeignKeyReferencesOnlyPartOfAUniqueIndexColumnSet_ReturnsNoParentKeys()
    {
        var table = await GetTableAsync("partial_column_set_child");

        Assert.That(table.ParentKeys, Is.Empty);
    }
}
