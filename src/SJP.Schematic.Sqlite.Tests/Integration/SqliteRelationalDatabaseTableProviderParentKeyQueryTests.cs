using System;
using System.Threading.Tasks;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Sqlite.Pragma;
using SJP.Schematic.Tests.Utilities;

namespace SJP.Schematic.Sqlite.Tests.Integration;

// Runs against a private in-memory database held by a single connection, so temp tables stay visible
// between queries and no other fixture's tables take part in the child key scan.
internal sealed class SqliteRelationalDatabaseTableProviderParentKeyQueryTests : SqliteTest
{
    private CachingConnectionFactory _connectionFactory;

    [OneTimeSetUp]
    public async Task Init()
    {
        var cancellationToken = TestContext.CurrentContext.CancellationToken;
        _connectionFactory = new CachingConnectionFactory(new SqliteConnectionFactory($"Data Source=ParentKeyQueries_{Guid.NewGuid():N};Mode=Memory;Cache=Shared"));

        await _connectionFactory.ExecuteAsync(@"
create table parent_table (
    id integer primary key,
    code_1 text not null unique,
    code_2 text not null unique,
    code_3 text not null unique,
    code_4 text not null unique
)", cancellationToken);
        await _connectionFactory.ExecuteAsync(@"
create table single_fk_child_table (
    code_1 text,
    constraint fk_single_fk_child_table_code_1 foreign key (code_1) references parent_table (code_1)
)", cancellationToken);
        await _connectionFactory.ExecuteAsync(@"
create table multiple_fk_child_table (
    code_1 text,
    code_2 text,
    code_3 text,
    code_4 text,
    constraint fk_multiple_fk_child_table_code_1 foreign key (code_1) references parent_table (code_1),
    constraint fk_multiple_fk_child_table_code_2 foreign key (code_2) references PARENT_TABLE (code_2),
    constraint fk_multiple_fk_child_table_code_3 foreign key (code_3) references Parent_Table (code_3),
    constraint fk_multiple_fk_child_table_code_4 foreign key (code_4) references parent_table (code_4)
)", cancellationToken);

        // the parent only exists in another schema, where SQLite never looks for a foreign key's parent
        await _connectionFactory.ExecuteAsync("create temp table temp_only_parent_table ( id integer primary key )", cancellationToken);
        await _connectionFactory.ExecuteAsync(@"
create table dangling_fk_child_table (
    parent_id integer,
    constraint fk_dangling_fk_child_table_parent_id foreign key (parent_id) references temp_only_parent_table (id)
)", cancellationToken);
        await _connectionFactory.ExecuteAsync("create table no_fk_child_table ( parent_id integer )", cancellationToken);
    }

    [OneTimeTearDown]
    public async Task CleanUp()
    {
        if (_connectionFactory != null)
            await _connectionFactory.DisposeAsync();
    }

    private (IRelationalDatabaseTableProvider TableProvider, CountingDbConnectionFactory ConnectionFactory) CreateCountingTableProvider()
    {
        var countingConnectionFactory = new CountingDbConnectionFactory(_connectionFactory);
        var countingConnection = new SchematicConnection(countingConnectionFactory, Dialect);
        var identifierDefaults = new IdentifierDefaults(null, "main", "main");
        var tableProvider = new SqliteRelationalDatabaseTableProvider(countingConnection, new ConnectionPragma(countingConnection), identifierDefaults);

        return (tableProvider, countingConnectionFactory);
    }

    private async Task<(IRelationalDatabaseTable Table, int QueryCount)> GetTableWithQueryCountAsync(string tableName)
    {
        var (tableProvider, countingConnectionFactory) = CreateCountingTableProvider();
        var table = await tableProvider.GetTable(tableName, TestContext.CurrentContext.CancellationToken).UnwrapSomeAsync();

        return (table, countingConnectionFactory.QueryCount);
    }

    [Test]
    public async Task GetTable_WhenAllForeignKeysReferenceTheSameParent_IssuesNoQueryPerForeignKey()
    {
        var (singleFkTable, singleFkQueryCount) = await GetTableWithQueryCountAsync("single_fk_child_table");
        var (multipleFkTable, multipleFkQueryCount) = await GetTableWithQueryCountAsync("multiple_fk_child_table");

        using (Assert.EnterMultipleScope())
        {
            Assert.That(singleFkTable.ParentKeys, Has.Count.EqualTo(1));
            Assert.That(multipleFkTable.ParentKeys, Has.Count.EqualTo(4));
            Assert.That(multipleFkTable.ParentKeys, Has.All.Property(nameof(IDatabaseRelationalKey.ParentTable)).EqualTo(Identifier.CreateQualifiedIdentifier("main", "parent_table")));
            Assert.That(multipleFkQueryCount, Is.EqualTo(singleFkQueryCount));
        }
    }

    [Test]
    public async Task GetTable_WhenForeignKeyParentOnlyExistsInAnotherSchema_ReturnsNoParentKeysAndSearchesNoOtherSchema()
    {
        var (noFkTable, noFkQueryCount) = await GetTableWithQueryCountAsync("no_fk_child_table");
        var (danglingFkTable, danglingFkQueryCount) = await GetTableWithQueryCountAsync("dangling_fk_child_table");

        using (Assert.EnterMultipleScope())
        {
            Assert.That(noFkTable.ParentKeys, Is.Empty);
            Assert.That(danglingFkTable.ParentKeys, Is.Empty);
            Assert.That(danglingFkQueryCount, Is.EqualTo(noFkQueryCount));
        }
    }
}
