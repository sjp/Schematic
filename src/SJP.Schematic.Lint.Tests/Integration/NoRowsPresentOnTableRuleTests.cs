using System;
using System.Data.Common;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Polly;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Lint.Rules;
using SJP.Schematic.Sqlite;
using SJP.Schematic.Tests.Utilities;

namespace SJP.Schematic.Lint.Tests.Integration;

internal sealed class NoRowsPresentOnTableRuleTests : SqliteTest
{
    [OneTimeSetUp]
    public async Task Init()
    {
        await DbConnection.ExecuteAsync("create table table_with_no_rows_1 ( column_1 integer not null )", CancellationToken.None);
        await DbConnection.ExecuteAsync("create table table_with_rows_1 ( column_1 integer not null )", CancellationToken.None);
        await DbConnection.ExecuteAsync("insert into table_with_rows_1 ( column_1 ) values (1)", CancellationToken.None);
    }

    [OneTimeTearDown]
    public async Task CleanUp()
    {
        await DbConnection.ExecuteAsync("drop table table_with_no_rows_1", CancellationToken.None);
        await DbConnection.ExecuteAsync("drop table table_with_rows_1", CancellationToken.None);
    }

    [Test]
    public static void Ctor_GivenNullConnection_ThrowsArgumentNullException()
    {
        ISchematicConnection connection = null;
        const RuleLevel level = RuleLevel.Error;
        Assert.That(() => new NoRowsPresentOnTableRule(connection, level), Throws.ArgumentNullException);
    }

    [Test]
    public static void Ctor_GivenInvalidLevel_ThrowsArgumentException()
    {
        var connection = Mock.Of<ISchematicConnection>();
        const RuleLevel level = (RuleLevel)999;
        Assert.That(() => new NoRowsPresentOnTableRule(connection, level), Throws.ArgumentException);
    }

    [Test]
    public static void AnalyseTables_GivenNullTables_ThrowsArgumentNullException()
    {
        var connection = Mock.Of<ISchematicConnection>();
        var rule = new NoRowsPresentOnTableRule(connection, RuleLevel.Error);
        Assert.That(() => rule.AnalyseTables(null), Throws.ArgumentNullException);
    }

    [Test]
    public async Task AnalyseTables_GivenTableWithRows_ProducesNoMessages()
    {
        var rule = new NoRowsPresentOnTableRule(Connection, RuleLevel.Error);
        var database = GetSqliteDatabase();

        var tables = new[]
        {
            await database.GetTable("table_with_rows_1").UnwrapSomeAsync(),
        };

        var messages = await rule.AnalyseTables(tables);

        Assert.That(messages, Is.Empty);
    }

    [Test]
    public async Task AnalyseTables_GivenTableWithNoRows_ProducesMessages()
    {
        var rule = new NoRowsPresentOnTableRule(Connection, RuleLevel.Error);
        var database = GetSqliteDatabase();

        var tables = new[]
        {
            await database.GetTable("table_with_no_rows_1").UnwrapSomeAsync(),
        };

        var messages = await rule.AnalyseTables(tables);

        Assert.That(messages, Is.Not.Empty);
    }

    [Test]
    public async Task AnalyseTables_GivenMoreTablesThanPermittedQueries_RunsNoMoreQueriesThanPermittedAtOnce()
    {
        var connectionFactory = new ConcurrencyTrackingDbConnectionFactory(DbConnection);
        var connection = new SchematicConnection(connectionFactory, new SqliteDialect());

        var database = GetSqliteDatabase();
        var table = await database.GetTable("table_with_rows_1").UnwrapSomeAsync();

        // the same table repeated, because what is under test is how many queries the rule has in
        // flight at once, not which tables they are directed at
        var tables = Enumerable
            .Repeat(table, ProbeConcurrencyLimiter.MaxConcurrentQueries * 3)
            .ToArray();

        var rule = new NoRowsPresentOnTableRule(connection, RuleLevel.Error);
        await rule.AnalyseTables(tables);

        Assert.That(connectionFactory.PeakConnectionsBeingOpened, Is.EqualTo(ProbeConcurrencyLimiter.MaxConcurrentQueries));
    }

    /// <summary>
    /// A connection factory decorator recording the largest number of connections being opened at once.
    /// Each query opens exactly one connection, and holds it for the duration of the query, so this
    /// measures how many queries a rule runs concurrently.
    /// </summary>
    private sealed class ConcurrencyTrackingDbConnectionFactory : IDbConnectionFactory
    {
        public ConcurrencyTrackingDbConnectionFactory(IDbConnectionFactory innerFactory)
        {
            InnerFactory = innerFactory ?? throw new ArgumentNullException(nameof(innerFactory));
        }

        private IDbConnectionFactory InnerFactory { get; }

        public int PeakConnectionsBeingOpened
        {
            get
            {
                lock (_peakLock)
                    return _peakConnectionsBeingOpened;
            }
        }

        public DbConnection CreateConnection() => InnerFactory.CreateConnection();

        public DbConnection OpenConnection() => InnerFactory.OpenConnection();

        public async Task<DbConnection> OpenConnectionAsync(CancellationToken cancellationToken = default)
        {
            var connectionsBeingOpened = Interlocked.Increment(ref _connectionsBeingOpened);
            lock (_peakLock)
                _peakConnectionsBeingOpened = Math.Max(_peakConnectionsBeingOpened, connectionsBeingOpened);

            try
            {
                // opening is deliberately slowed so that every query a rule is permitted to run at once is
                // still in flight when the last of them is counted
                await Task.Delay(OpenConnectionDelay, cancellationToken);
                return await InnerFactory.OpenConnectionAsync(cancellationToken);
            }
            finally
            {
                Interlocked.Decrement(ref _connectionsBeingOpened);
            }
        }

        public bool DisposeConnection => InnerFactory.DisposeConnection;

        public PolicyBuilder RetryPolicy => InnerFactory.RetryPolicy;

        private int _connectionsBeingOpened;

        private int _peakConnectionsBeingOpened;

        private readonly object _peakLock = new();

        private static readonly TimeSpan OpenConnectionDelay = TimeSpan.FromMilliseconds(100);
    }
}
