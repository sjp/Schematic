using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.PostgreSql.Comments;

namespace SJP.Schematic.PostgreSql.Tests.Integration;

// A caching connection factory permits a single query at a time, so any provider that loads objects while
// its own name query is still being streamed would wait forever for the query slot that stream holds.
// Each result is compared with the same call made through the pooled connection factory.
[CancelAfter(60 * 1000)]
internal sealed class PostgreSqlSingleConnectionEnumerationTests : PostgreSqlTest
{
    [OneTimeSetUp]
    public Task Init() => ExecuteBatchAsync(
        "create table single_conn_parent ( id int primary key, code text not null unique )",
        "create table single_conn_child ( id int primary key, parent_id int references single_conn_parent (id) )",
        "create view single_conn_view as select id, code from single_conn_parent",
        "create materialized view single_conn_matview as select id from single_conn_child",
        "create function single_conn_function(x int) returns int as 'select x' language sql",
        "comment on table single_conn_parent is 'parent comment'",
        "comment on view single_conn_view is 'view comment'",
        "comment on function single_conn_function(int) is 'function comment'"
    );

    [OneTimeTearDown]
    public Task CleanUp() => ExecuteBatchAsync(
        "drop function single_conn_function(int)",
        "drop materialized view single_conn_matview",
        "drop view single_conn_view",
        "drop table single_conn_child",
        "drop table single_conn_parent"
    );

    private static CancellationToken CancellationToken => TestContext.CurrentContext.CancellationToken;

    [Test]
    public Task EnumerateAllTables_WhenOnlyOneQueryMayRunAtOnce_ReturnsSameTablesAsPooledFactory() => AssertSameNamesAsync(
        static (connection, defaults, resolver) => new PostgreSqlRelationalDatabaseTableProvider(new SchematicConnection(connection, new PostgreSqlDialect()), defaults, resolver)
            .EnumerateAllTables(CancellationToken)
            .Select(static t => t.Name)
            .ToListAsync(CancellationToken)
            .AsTask());

    [Test]
    public Task GetAllTables_WhenOnlyOneQueryMayRunAtOnce_ReturnsSameTablesAsPooledFactory() => AssertSameNamesAsync(
        static async (connection, defaults, resolver) => (await new PostgreSqlRelationalDatabaseTableProvider(new SchematicConnection(connection, new PostgreSqlDialect()), defaults, resolver)
            .GetAllTables(CancellationToken))
            .Select(static t => t.Name)
            .ToList());

    [Test]
    public Task EnumerateAllViews_WhenOnlyOneQueryMayRunAtOnce_ReturnsSameViewsAsPooledFactory() => AssertSameNamesAsync(
        static (connection, defaults, resolver) => new PostgreSqlDatabaseViewProvider(new SchematicConnection(connection, new PostgreSqlDialect()), defaults, resolver)
            .EnumerateAllViews(CancellationToken)
            .Select(static v => v.Name)
            .ToListAsync(CancellationToken)
            .AsTask());

    [Test]
    public Task EnumerateAllRoutines_WhenOnlyOneQueryMayRunAtOnce_ReturnsSameRoutinesAsPooledFactory() => AssertSameNamesAsync(
        static (connection, defaults, resolver) => new PostgreSqlDatabaseRoutineProvider(connection, defaults, resolver)
            .EnumerateAllRoutines(CancellationToken)
            .Select(static r => r.Name)
            .ToListAsync(CancellationToken)
            .AsTask());

    [Test]
    public Task EnumerateAllTableComments_WhenOnlyOneQueryMayRunAtOnce_ReturnsSameTablesAsPooledFactory() => AssertSameNamesAsync(
        static (connection, defaults, resolver) => new PostgreSqlTableCommentProvider(connection, defaults, resolver)
            .EnumerateAllTableComments(CancellationToken)
            .Select(static c => c.TableName)
            .ToListAsync(CancellationToken)
            .AsTask());

    [Test]
    public Task EnumerateAllViewComments_WhenOnlyOneQueryMayRunAtOnce_ReturnsSameViewsAsPooledFactory() => AssertSameNamesAsync(
        static (connection, defaults, resolver) => new PostgreSqlViewCommentProvider(connection, defaults, resolver)
            .EnumerateAllViewComments(CancellationToken)
            .Select(static c => c.ViewName)
            .ToListAsync(CancellationToken)
            .AsTask());

    [Test]
    public Task EnumerateAllRoutineComments_WhenOnlyOneQueryMayRunAtOnce_ReturnsSameRoutinesAsPooledFactory() => AssertSameNamesAsync(
        static (connection, defaults, resolver) => new PostgreSqlRoutineCommentProvider(connection, defaults, resolver)
            .EnumerateAllRoutineComments(CancellationToken)
            .Select(static c => c.RoutineName)
            .ToListAsync(CancellationToken)
            .AsTask());

    private async Task AssertSameNamesAsync(Func<IDbConnectionFactory, IIdentifierDefaults, IIdentifierResolutionStrategy, Task<List<Identifier>>> getNames)
    {
        var expectedNames = await getNames(DbConnection, IdentifierDefaults, IdentifierResolver);

        await using var singleConnectionFactory = new CachingConnectionFactory(DbConnection);
        var names = await getNames(singleConnectionFactory, IdentifierDefaults, IdentifierResolver);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(names, Is.Not.Empty);
            Assert.That(names, Is.EqualTo(expectedNames));
        }
    }
}
