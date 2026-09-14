using System;
using System.Data.Common;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Polly;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Core.Tests.Fakes;
using SJP.Schematic.Sqlite;

namespace SJP.Schematic.Core.Tests.Extensions;

[CancelAfter(60 * 1000)]
internal static class ConnectionExtensionsConcurrencyTests
{
    private const string ThreeRowQuery = "select 'first' as dummy union all select 'second' as dummy union all select 'third' as dummy";

    // long enough to fail a test that is wrongly waiting, short enough not to hold up the run
    private static readonly TimeSpan SlotReleaseDeadline = TimeSpan.FromSeconds(10);

    [Test]
    public static void MaxConcurrentQueries_WhenNotImplemented_Returns16()
    {
        IDbConnectionFactory connectionFactory = new MinimalConnectionFactory();

        Assert.That(connectionFactory.MaxConcurrentQueries, Is.EqualTo(16));
    }

    [Test]
    public static async Task ExecuteScalarAsync_WhenMoreQueriesThanLimitRunAtOnce_OpensNoMoreConnectionsThanLimit()
    {
        const int maxConcurrentQueries = 3;
        var connectionFactory = new ConnectionTrackingConnectionFactory(CreateSqliteConnectionFactory(), maxConcurrentQueries, TimeSpan.FromMilliseconds(30));

        var results = await Task.WhenAll(Enumerable
            .Range(0, maxConcurrentQueries * 5)
            .Select(_ => connectionFactory.ExecuteScalarAsync<long>("select 1", TestContext.CurrentContext.CancellationToken)));

        using (Assert.EnterMultipleScope())
        {
            Assert.That(results, Is.All.EqualTo(1));
            Assert.That(connectionFactory.PeakOpenConnections, Is.EqualTo(maxConcurrentQueries));
        }
    }

    [Test]
    public static async Task QueryEnumerableAsync_WhileBeingEnumerated_HoldsQuerySlotUntilEnumerationCompletes()
    {
        var connectionFactory = new ConnectionTrackingConnectionFactory(CreateSqliteConnectionFactory(), 1);
        var cancellationToken = TestContext.CurrentContext.CancellationToken;

        var enumerator = connectionFactory.QueryEnumerableAsync<string>(ThreeRowQuery, cancellationToken).GetAsyncEnumerator(cancellationToken);
        Task<long> otherQuery;
        bool otherQueryCompletedWhileEnumerating;
        try
        {
            await enumerator.MoveNextAsync();

            otherQuery = connectionFactory.ExecuteScalarAsync<long>("select 1", cancellationToken);
            var completed = await Task.WhenAny(otherQuery, Task.Delay(TimeSpan.FromMilliseconds(250), cancellationToken));
            otherQueryCompletedWhileEnumerating = completed == otherQuery;
        }
        finally
        {
            await enumerator.DisposeAsync();
        }

        var otherResult = await otherQuery.WaitAsync(SlotReleaseDeadline, cancellationToken);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(otherQueryCompletedWhileEnumerating, Is.False);
            Assert.That(otherResult, Is.EqualTo(1));
        }
    }

    [Test]
    public static async Task QueryEnumerableAsync_WhenEnumerationStopsEarly_ReleasesQuerySlot()
    {
        var connectionFactory = new ConnectionTrackingConnectionFactory(CreateSqliteConnectionFactory(), 1);
        var cancellationToken = TestContext.CurrentContext.CancellationToken;

        await foreach (var _ in connectionFactory.QueryEnumerableAsync<string>(ThreeRowQuery, cancellationToken))
            break;

        var result = await connectionFactory.ExecuteScalarAsync<long>("select 1", cancellationToken).WaitAsync(SlotReleaseDeadline, cancellationToken);

        Assert.That(result, Is.EqualTo(1));
    }

    [Test]
    public static async Task QuerySingleOrNone_WhenCompleted_ReleasesQuerySlot()
    {
        var connectionFactory = new ConnectionTrackingConnectionFactory(CreateSqliteConnectionFactory(), 1);
        var cancellationToken = TestContext.CurrentContext.CancellationToken;

        _ = await connectionFactory.QuerySingleOrNone<string>(ThreeRowQuery, cancellationToken).ToOption();

        var result = await connectionFactory.ExecuteScalarAsync<long>("select 1", cancellationToken).WaitAsync(SlotReleaseDeadline, cancellationToken);

        Assert.That(result, Is.EqualTo(1));
    }

    [Test]
    public static async Task QueryAsync_WhenEveryConnectionOpenFails_ReleasesQuerySlot()
    {
        var injector = new FaultInjector(rowsBeforeFailure: 0, failureCount: 0, openFailureCount: AttemptsPerQuery);
        var connectionFactory = new ConnectionTrackingConnectionFactory(new FaultInjectingConnectionFactory(CreateSqliteConnectionFactory(), injector), 1);
        var cancellationToken = TestContext.CurrentContext.CancellationToken;

        Assert.That(async () => await connectionFactory.QueryAsync<string>(ThreeRowQuery, cancellationToken), Throws.InstanceOf<TimeoutException>());

        var results = await connectionFactory.QueryAsync<string>(ThreeRowQuery, cancellationToken).WaitAsync(SlotReleaseDeadline, cancellationToken);

        Assert.That(results, Is.EqualTo(new[] { "first", "second", "third" }));
    }

    [Test]
    public static async Task QueryEnumerableAsync_WhenEveryAttemptFails_ReleasesQuerySlot()
    {
        var injector = new FaultInjector(rowsBeforeFailure: 0, failureCount: AttemptsPerQuery);
        var connectionFactory = new ConnectionTrackingConnectionFactory(new FaultInjectingConnectionFactory(CreateSqliteConnectionFactory(), injector), 1);
        var cancellationToken = TestContext.CurrentContext.CancellationToken;

        Assert.That(async () => await connectionFactory.QueryEnumerableAsync<string>(ThreeRowQuery, cancellationToken).ToListAsync(cancellationToken), Throws.InstanceOf<TimeoutException>());

        var results = await connectionFactory.QueryAsync<string>(ThreeRowQuery, cancellationToken).WaitAsync(SlotReleaseDeadline, cancellationToken);

        Assert.That(results, Is.EqualTo(new[] { "first", "second", "third" }));
    }

    [Test]
    public static async Task QueryEnumerableAsync_WhenEveryConnectionOpenFails_ReleasesQuerySlot()
    {
        var injector = new FaultInjector(rowsBeforeFailure: 0, failureCount: 0, openFailureCount: AttemptsPerQuery);
        var connectionFactory = new ConnectionTrackingConnectionFactory(new FaultInjectingConnectionFactory(CreateSqliteConnectionFactory(), injector), 1);
        var cancellationToken = TestContext.CurrentContext.CancellationToken;

        Assert.That(async () => await connectionFactory.QueryEnumerableAsync<string>(ThreeRowQuery, cancellationToken).ToListAsync(cancellationToken), Throws.InstanceOf<TimeoutException>());

        var results = await connectionFactory.QueryAsync<string>(ThreeRowQuery, cancellationToken).WaitAsync(SlotReleaseDeadline, cancellationToken);

        Assert.That(results, Is.EqualTo(new[] { "first", "second", "third" }));
    }

    // the first attempt plus every retry
    private const int AttemptsPerQuery = 6;

    private static SqliteConnectionFactory CreateSqliteConnectionFactory() => new("Data Source=:memory:");

    private sealed class MinimalConnectionFactory : IDbConnectionFactory
    {
        public DbConnection CreateConnection() => throw new NotSupportedException();

        public DbConnection OpenConnection() => throw new NotSupportedException();

        public Task<DbConnection> OpenConnectionAsync(CancellationToken cancellationToken = default) => throw new NotSupportedException();

        public bool DisposeConnection => true;

        public PolicyBuilder RetryPolicy => Policy.Handle<TimeoutException>();
    }
}
