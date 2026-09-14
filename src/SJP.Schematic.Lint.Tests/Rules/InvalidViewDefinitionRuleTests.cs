using System;
using System.Data.Common;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Moq;
using NUnit.Framework;
using Polly;
using SJP.Schematic.Core;
using SJP.Schematic.Lint.Rules;

namespace SJP.Schematic.Lint.Tests.Rules;

internal static class InvalidViewDefinitionRuleTests
{
    // Mirrors the rule's private batch size, so that view counts below can be expressed in probe queries.
    private const int ProbeBatchSize = 32;

    [Test]
    public static async Task AnalyseViews_GivenManyMoreBatchesThanProbePermits_NeverExceedsProbeConcurrencyLimit()
    {
        var queriesInFlight = 0;
        var peakQueriesInFlight = 0;
        var peakLock = new object();

        // each probe stays in flight long enough for every probe started alongside it to be observed
        async Task OnOpenAsync(CancellationToken cancellationToken)
        {
            var inFlight = Interlocked.Increment(ref queriesInFlight);
            lock (peakLock)
                peakQueriesInFlight = Math.Max(peakQueriesInFlight, inFlight);

            try
            {
                await Task.Delay(TimeSpan.FromMilliseconds(20), cancellationToken);
            }
            finally
            {
                Interlocked.Decrement(ref queriesInFlight);
            }
        }

        var rule = CreateRule(OnOpenAsync);
        var views = CreateViews(ProbeConcurrencyLimiter.MaxConcurrentQueries * ProbeBatchSize * 4);

        var messages = await rule.AnalyseViews(views).WaitAsync(TestTimeout);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(messages, Is.Empty);
            Assert.That(peakQueriesInFlight, Is.InRange(1, ProbeConcurrencyLimiter.MaxConcurrentQueries));
        }
    }

    [Test]
    public static async Task AnalyseViews_WhenDatabaseRejectsEveryProbe_ReportsEveryView()
    {
        var rule = CreateRule(static _ => throw new FakeDbException());
        var views = CreateViews(ProbeConcurrencyLimiter.MaxConcurrentQueries * ProbeBatchSize);

        // every batch is bisected down to single views while sharing the limited permits with its halves
        var messages = await rule.AnalyseViews(views).WaitAsync(TestTimeout);

        Assert.That(messages, Has.Count.EqualTo(views.Length));
    }

    [Test]
    public static void AnalyseViews_WhenConnectionPoolIsExhausted_ThrowsInsteadOfReportingViews()
    {
        var rule = CreateRule(static _ => throw new InvalidOperationException("Timeout expired. The timeout period elapsed prior to obtaining a connection from the pool."));
        var views = CreateViews(2);

        Assert.That(() => rule.AnalyseViews(views), Throws.InvalidOperationException);
    }

    [Test]
    public static void AnalyseViews_WhenProbeTimesOut_ThrowsInsteadOfReportingViews()
    {
        var rule = CreateRule(static _ => throw new TimeoutException());
        var views = CreateViews(2);

        Assert.That(() => rule.AnalyseViews(views), Throws.TypeOf<TimeoutException>());
    }

    [Test]
    public static void AnalyseViews_WhenProbeFailsWithTransientDatabaseError_ThrowsInsteadOfReportingViews()
    {
        var rule = CreateRule(static _ => throw new FakeDbException { Transient = true });
        var views = CreateViews(2);

        Assert.That(() => rule.AnalyseViews(views), Throws.TypeOf<FakeDbException>());
    }

    [Test]
    public static void AnalyseViews_WhenProbeFailsWithDatabaseErrorWrappingTimeout_ThrowsInsteadOfReportingViews()
    {
        var rule = CreateRule(static _ => throw new FakeDbException("The command timed out.", new TimeoutException()));
        var views = CreateViews(2);

        Assert.That(() => rule.AnalyseViews(views), Throws.TypeOf<FakeDbException>());
    }

    [Test]
    public static void AnalyseViews_WhenCancelledWhileProbeRaisesDatabaseError_ThrowsInsteadOfReportingViews()
    {
        using var cts = new CancellationTokenSource();

        // a driver may report a cancelled command as an ordinary database error
        var rule = CreateRule(_ =>
        {
            cts.Cancel();
            throw new FakeDbException();
        });
        var views = CreateViews(2);

        Assert.That(() => rule.AnalyseViews(views, cts.Token), Throws.TypeOf<FakeDbException>());
    }

    private static InvalidViewDefinitionRule CreateRule(Func<CancellationToken, Task> onOpenAsync)
    {
        // every view name is quoted as a derived table, so a probe that reaches the database always succeeds
        var dialect = Mock.Of<IDatabaseDialect>(d => d.QuoteName(It.IsAny<Identifier>()) == "(select 1)");
        var connection = new SchematicConnection(new ProbeConnectionFactory(onOpenAsync), dialect);

        return new InvalidViewDefinitionRule(connection, RuleLevel.Error);
    }

    private static IDatabaseView[] CreateViews(int count)
    {
        return Enumerable
            .Range(0, count)
            .Select(static i => Mock.Of<IDatabaseView>(v => v.Name == Identifier.CreateQualifiedIdentifier("view_" + i)))
            .ToArray();
    }

    private static readonly TimeSpan TestTimeout = TimeSpan.FromSeconds(30);

    /// <summary>
    /// Opens in-memory SQLite connections, first running a callback that can observe or fail the attempt.
    /// </summary>
    private sealed class ProbeConnectionFactory : IDbConnectionFactory
    {
        public ProbeConnectionFactory(Func<CancellationToken, Task> onOpenAsync)
        {
            _onOpenAsync = onOpenAsync;
        }

        public DbConnection CreateConnection() => new SqliteConnection("Data Source=:memory:");

        public DbConnection OpenConnection() => throw new NotSupportedException();

        public async Task<DbConnection> OpenConnectionAsync(CancellationToken cancellationToken = default)
        {
            await _onOpenAsync(cancellationToken);

            var connection = CreateConnection();
            await connection.OpenAsync(cancellationToken);
            return connection;
        }

        public bool DisposeConnection => true;

        // nothing is retried, so every failure reaches the rule exactly as the callback raised it
        public PolicyBuilder RetryPolicy { get; } = Policy.Handle<Exception>(static _ => false);

        // high enough that only the rule's own limit on probes can bound them
        public int MaxConcurrentQueries => 1024;

        private readonly Func<CancellationToken, Task> _onOpenAsync;
    }

    private sealed class FakeDbException : DbException
    {
        public FakeDbException()
        {
        }

        public FakeDbException(string message)
            : base(message)
        {
        }

        public FakeDbException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public FakeDbException(string message, int errorCode)
            : base(message, errorCode)
        {
        }

        public bool Transient { get; init; }

        public override bool IsTransient => Transient;
    }
}
