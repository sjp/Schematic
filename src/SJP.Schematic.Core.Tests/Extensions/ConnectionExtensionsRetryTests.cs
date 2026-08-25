using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Core.Tests.Fakes;
using SJP.Schematic.Sqlite;
using SJP.Schematic.Tests.Utilities;

namespace SJP.Schematic.Core.Tests.Extensions;

[TestFixture]
internal static class ConnectionExtensionsRetryTests
{
    private const string ThreeRowQuery = "select 'first' as dummy union all select 'second' as dummy union all select 'third' as dummy";

    [Test]
    public static async Task QueryEnumerableAsync_WhenFirstAttemptFailsBeforeAnyResults_RetriesAndReturnsAllResults()
    {
        var injector = new FaultInjector(rowsBeforeFailure: 0, failureCount: 1);
        var connectionFactory = CreateFaultInjectingConnectionFactory(injector);

        var results = await CollectAsync(connectionFactory.QueryEnumerableAsync<string>(ThreeRowQuery, CancellationToken.None));

        Assert.Multiple(() =>
        {
            Assert.That(results, Is.EqualTo(new[] { "first", "second", "third" }));
            Assert.That(injector.ExecutionCount, Is.EqualTo(2));
        });
    }

    [Test]
    public static void QueryEnumerableAsync_WhenAttemptFailsAfterFirstResult_PropagatesExceptionInsteadOfTruncating()
    {
        var injector = new FaultInjector(rowsBeforeFailure: 1, failureCount: 1);
        var connectionFactory = CreateFaultInjectingConnectionFactory(injector);
        var results = new List<string>();

        Assert.Multiple(() =>
        {
            Assert.That(async () => await CollectAsync(connectionFactory.QueryEnumerableAsync<string>(ThreeRowQuery, CancellationToken.None), results), Throws.InstanceOf<TimeoutException>());
            Assert.That(results, Is.EqualTo(new[] { "first" }));
            Assert.That(injector.ExecutionCount, Is.EqualTo(1));
        });
    }

    [Test]
    public static async Task QueryEnumerableAsync_WithParamsWhenFirstAttemptFailsBeforeAnyResults_RetriesAndReturnsAllResults()
    {
        var injector = new FaultInjector(rowsBeforeFailure: 0, failureCount: 1);
        var connectionFactory = CreateFaultInjectingConnectionFactory(injector);
        var param = new TestQuery { Test = "test" };

        var results = await CollectAsync(connectionFactory.QueryEnumerableAsync("select @Test as dummy", param, CancellationToken.None));

        Assert.Multiple(() =>
        {
            Assert.That(results, Is.EqualTo(new[] { "test" }));
            Assert.That(injector.ExecutionCount, Is.EqualTo(2));
        });
    }

    [Test]
    public static async Task QuerySingleOrNone_WhenFirstAttemptFailsBeforeAnyResults_RetriesAndReturnsResult()
    {
        var injector = new FaultInjector(rowsBeforeFailure: 0, failureCount: 1);
        var connectionFactory = CreateFaultInjectingConnectionFactory(injector);

        var result = await connectionFactory.QuerySingleOrNone<string>("select 'test' as dummy", CancellationToken.None).ToOption();

        Assert.Multiple(() =>
        {
            Assert.That(result.UnwrapSome(), Is.EqualTo("test"));
            Assert.That(injector.ExecutionCount, Is.EqualTo(2));
        });
    }

    [Test]
    public static void QuerySingleOrNone_WhenAttemptFailsAfterFirstResult_PropagatesExceptionInsteadOfReturningResult()
    {
        var injector = new FaultInjector(rowsBeforeFailure: 1, failureCount: 1);
        var connectionFactory = CreateFaultInjectingConnectionFactory(injector);

        Assert.Multiple(() =>
        {
            Assert.That(async () => await connectionFactory.QuerySingleOrNone<string>(ThreeRowQuery, CancellationToken.None).ToOption(), Throws.InstanceOf<TimeoutException>());
            Assert.That(injector.ExecutionCount, Is.EqualTo(1));
        });
    }

    [Test]
    public static void QueryEnumerableAsync_WhenEveryAttemptFails_PropagatesException()
    {
        var injector = new FaultInjector(rowsBeforeFailure: 0, failureCount: int.MaxValue);
        var connectionFactory = CreateFaultInjectingConnectionFactory(injector);

        Assert.That(async () => await CollectAsync(connectionFactory.QueryEnumerableAsync<string>(ThreeRowQuery, CancellationToken.None)), Throws.InstanceOf<TimeoutException>());
    }

    private static IDbConnectionFactory CreateFaultInjectingConnectionFactory(FaultInjector injector) =>
        new FaultInjectingConnectionFactory(new SqliteConnectionFactory("Data Source=:memory:"), injector);

    private static async Task<IEnumerable<string>> CollectAsync(IAsyncEnumerable<string> source, List<string> results = null)
    {
        results ??= [];

        await foreach (var item in source)
            results.Add(item);

        return results;
    }

    private sealed record TestQuery : ISqlQuery<string>
    {
        public required string Test { get; init; }
    }
}
