using System.Threading;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Lint.Rules;
using SJP.Schematic.Sqlite;
using SJP.Schematic.Tests.Utilities;

namespace SJP.Schematic.Lint.Tests.Integration;

internal sealed class ExistsQueryExecutorTests : SqliteTest
{
    [OneTimeSetUp]
    public async Task Init()
    {
        await DbConnection.ExecuteAsync(@"
create table exists_probe_table_1 (
    column_1 integer not null primary key autoincrement,
    column_2 integer,
    constraint exists_probe_fk_1 foreign key (column_2) references exists_probe_table_1 (column_1)
)", CancellationToken.None);
        await DbConnection.ExecuteAsync("insert into exists_probe_table_1 (column_1, column_2) values (1, 1)", CancellationToken.None);
    }

    [OneTimeTearDown]
    public async Task CleanUp()
    {
        await DbConnection.ExecuteAsync("drop table exists_probe_table_1", CancellationToken.None);
    }

    [Test]
    public static void GetForConnection_GivenNullConnection_ThrowsArgumentNullException()
    {
        Assert.That(() => ExistsQueryExecutor.GetForConnection(null), Throws.ArgumentNullException);
    }

    [Test]
    public static void GetForConnection_GivenSameConnection_ReturnsSameExecutor()
    {
        var connection = Mock.Of<ISchematicConnection>();

        var firstExecutor = ExistsQueryExecutor.GetForConnection(connection);
        var secondExecutor = ExistsQueryExecutor.GetForConnection(connection);

        Assert.That(firstExecutor, Is.SameAs(secondExecutor));
    }

    [Test]
    public static void GetForConnection_GivenDifferentConnections_ReturnsDifferentExecutors()
    {
        var firstExecutor = ExistsQueryExecutor.GetForConnection(Mock.Of<ISchematicConnection>());
        var secondExecutor = ExistsQueryExecutor.GetForConnection(Mock.Of<ISchematicConnection>());

        Assert.That(firstExecutor, Is.Not.SameAs(secondExecutor));
    }

    [Test]
    public async Task ExistsAsync_WhenRulesShareAConnection_ProbesFromQuerySuffixOnlyOnce()
    {
        var connectionFactory = new CountingDbConnectionFactory(DbConnection);
        var connection = new SchematicConnection(connectionFactory, new SqliteDialect());

        var database = GetSqliteDatabase();
        var tables = new[]
        {
            await database.GetTable("exists_probe_table_1").UnwrapSomeAsync(),
        };

        // the first rule to run pays for the suffix probe as well as its own query
        var selfReferenceRule = new ForeignKeySelfReferenceRule(connection, RuleLevel.Error);
        await selfReferenceRule.AnalyseTables(tables);
        var queryCountAfterProbingRule = connectionFactory.QueryCount;

        var noRowsRule = new NoRowsPresentOnTableRule(connection, RuleLevel.Error);
        await noRowsRule.AnalyseTables(tables);
        var queryCountAfterSharingRule = connectionFactory.QueryCount;

        Assert.Multiple(() =>
        {
            Assert.That(queryCountAfterProbingRule, Is.EqualTo(2));
            Assert.That(queryCountAfterSharingRule - queryCountAfterProbingRule, Is.EqualTo(1));
        });
    }
}
