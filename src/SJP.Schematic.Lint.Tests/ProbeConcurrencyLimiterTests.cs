using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using SJP.Schematic.Core;

namespace SJP.Schematic.Lint.Tests;

[TestFixture]
internal static class ProbeConcurrencyLimiterTests
{
    [Test]
    public static void GetForConnection_GivenNullConnection_ThrowsArgumentNullException()
    {
        Assert.That(() => ProbeConcurrencyLimiter.GetForConnection(null), Throws.ArgumentNullException);
    }

    [Test]
    public static void GetForConnection_GivenSameConnection_ReturnsSameLimiter()
    {
        var connection = Mock.Of<ISchematicConnection>();

        var firstLimiter = ProbeConcurrencyLimiter.GetForConnection(connection);
        var secondLimiter = ProbeConcurrencyLimiter.GetForConnection(connection);

        Assert.That(firstLimiter, Is.SameAs(secondLimiter));
    }

    [Test]
    public static void GetForConnection_GivenDifferentConnections_ReturnsDifferentLimiters()
    {
        var firstLimiter = ProbeConcurrencyLimiter.GetForConnection(Mock.Of<ISchematicConnection>());
        var secondLimiter = ProbeConcurrencyLimiter.GetForConnection(Mock.Of<ISchematicConnection>());

        Assert.That(firstLimiter, Is.Not.SameAs(secondLimiter));
    }

    [Test]
    public static void RunAsync_GivenNullQuery_ThrowsArgumentNullException()
    {
        var limiter = ProbeConcurrencyLimiter.GetForConnection(Mock.Of<ISchematicConnection>());

        Assert.That(() => limiter.RunAsync<int>(null, CancellationToken.None), Throws.ArgumentNullException);
    }

    [Test]
    public static async Task RunAsync_GivenQuery_ReturnsQueryResult()
    {
        var limiter = ProbeConcurrencyLimiter.GetForConnection(Mock.Of<ISchematicConnection>());

        var result = await limiter.RunAsync(static _ => Task.FromResult(42), CancellationToken.None);

        Assert.That(result, Is.EqualTo(42));
    }

    [Test]
    public static async Task RunAsync_GivenMoreQueriesThanPermits_RunsNoMoreThanThePermittedNumberAtOnce()
    {
        var limiter = ProbeConcurrencyLimiter.GetForConnection(Mock.Of<ISchematicConnection>());

        // every query holds its permit until as many are held at once as the limiter permits, so a limiter
        // handing out too many permits is observed by the peak, and one handing out too few by the timeout
        var allPermitsHeld = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var queriesInFlight = 0;
        var peakQueriesInFlight = 0;
        var peakLock = new object();

        async Task<int> RunProbeAsync(CancellationToken cancellationToken)
        {
            var inFlight = Interlocked.Increment(ref queriesInFlight);
            lock (peakLock)
                peakQueriesInFlight = Math.Max(peakQueriesInFlight, inFlight);

            if (inFlight >= ProbeConcurrencyLimiter.MaxConcurrentQueries)
                allPermitsHeld.TrySetResult();

            await allPermitsHeld.Task.WaitAsync(PermitWaitTimeout, cancellationToken);

            Interlocked.Decrement(ref queriesInFlight);
            return inFlight;
        }

        var queryCount = ProbeConcurrencyLimiter.MaxConcurrentQueries * 4;
        await Task.WhenAll(Enumerable
            .Range(0, queryCount)
            .Select(_ => limiter.RunAsync(RunProbeAsync, CancellationToken.None)));

        Assert.That(peakQueriesInFlight, Is.EqualTo(ProbeConcurrencyLimiter.MaxConcurrentQueries));
    }

    [Test]
    public static async Task RunAsync_WhenQueryThrows_ReleasesItsPermit()
    {
        var limiter = ProbeConcurrencyLimiter.GetForConnection(Mock.Of<ISchematicConnection>());

        var failingQueryCount = ProbeConcurrencyLimiter.MaxConcurrentQueries * 2;
        for (var i = 0; i < failingQueryCount; i++)
        {
            try
            {
                await limiter.RunAsync<int>(static _ => throw new InvalidOperationException(), CancellationToken.None);
            }
            catch (InvalidOperationException)
            {
                // the failure itself is not what is under test, only that the permit did not leak with it
            }
        }

        var completed = await limiter
            .RunAsync(static _ => Task.FromResult(true), CancellationToken.None)
            .WaitAsync(PermitWaitTimeout);

        Assert.That(completed, Is.True);
    }

    [Test]
    public static void RunAsync_GivenCancelledToken_ThrowsOperationCancelledException()
    {
        var limiter = ProbeConcurrencyLimiter.GetForConnection(Mock.Of<ISchematicConnection>());
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        Assert.That(
            async () => await limiter.RunAsync(static _ => Task.FromResult(true), cts.Token),
            Throws.InstanceOf<OperationCanceledException>()
        );
    }

    private static readonly TimeSpan PermitWaitTimeout = TimeSpan.FromSeconds(30);
}
