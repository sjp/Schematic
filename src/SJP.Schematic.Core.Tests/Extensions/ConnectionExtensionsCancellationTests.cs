using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Core.Tests.Fakes;
using SJP.Schematic.Sqlite;
using SJP.Schematic.Tests.Utilities;

namespace SJP.Schematic.Core.Tests.Extensions;

internal static class ConnectionExtensionsCancellationTests
{
    private const string ThreeRowQuery = "select 'first' as dummy union all select 'second' as dummy union all select 'third' as dummy";

    [Test]
    public static async Task QueryEnumerableAsync_WhenAllResultsRead_DoesNotCancelCommand()
    {
        var injector = new FaultInjector(rowsBeforeFailure: 0, failureCount: 0);
        var connectionFactory = CreateFaultInjectingConnectionFactory(injector);

        var results = await CollectAsync(connectionFactory.QueryEnumerableAsync<string>(ThreeRowQuery, CancellationToken.None));

        using (Assert.EnterMultipleScope())
        {
            Assert.That(results, Is.EqualTo(new[] { "first", "second", "third" }));
            Assert.That(injector.CancelCount, Is.Zero);
        }
    }

    [Test]
    public static async Task QueryEnumerableAsync_WithParamsWhenAllResultsRead_DoesNotCancelCommand()
    {
        var injector = new FaultInjector(rowsBeforeFailure: 0, failureCount: 0);
        var connectionFactory = CreateFaultInjectingConnectionFactory(injector);
        var param = new TestQuery { Test = "test" };

        var results = await CollectAsync(connectionFactory.QueryEnumerableAsync("select @Test as dummy", param, CancellationToken.None));

        using (Assert.EnterMultipleScope())
        {
            Assert.That(results, Is.EqualTo(new[] { "test" }));
            Assert.That(injector.CancelCount, Is.Zero);
        }
    }

    [Test]
    public static async Task QueryEnumerableAsync_WhenEnumerationStopsEarly_CancelsCommand()
    {
        var injector = new FaultInjector(rowsBeforeFailure: 0, failureCount: 0);
        var connectionFactory = CreateFaultInjectingConnectionFactory(injector);

        string first = null;
        await foreach (var result in connectionFactory.QueryEnumerableAsync<string>(ThreeRowQuery, CancellationToken.None))
        {
            first = result;
            break;
        }

        using (Assert.EnterMultipleScope())
        {
            Assert.That(first, Is.EqualTo("first"));
            Assert.That(injector.CancelCount, Is.EqualTo(1));
        }
    }

    [Test]
    public static async Task QuerySingleOrNone_WhenSingleResult_DoesNotCancelCommand()
    {
        var injector = new FaultInjector(rowsBeforeFailure: 0, failureCount: 0);
        var connectionFactory = CreateFaultInjectingConnectionFactory(injector);

        var result = await connectionFactory.QuerySingleOrNone<string>("select 'test' as dummy", CancellationToken.None).ToOption();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.UnwrapSome(), Is.EqualTo("test"));
            Assert.That(injector.CancelCount, Is.Zero);
        }
    }

    [Test]
    public static async Task QuerySingleOrNone_WhenMoreThanOneResult_CancelsCommand()
    {
        var injector = new FaultInjector(rowsBeforeFailure: 0, failureCount: 0);
        var connectionFactory = CreateFaultInjectingConnectionFactory(injector);

        var result = await connectionFactory.QuerySingleOrNone<string>(ThreeRowQuery, CancellationToken.None).ToOption();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result, OptionIs.None);
            Assert.That(injector.CancelCount, Is.EqualTo(1));
        }
    }

    [Test]
    public static async Task QueryEnumerableAsync_ForSimpleValueType_ConvertsProviderValue()
    {
        var connectionFactory = new SqliteConnectionFactory("Data Source=:memory:");

        var results = new List<int>();
        await foreach (var result in connectionFactory.QueryEnumerableAsync<int>("select 1 union all select 2", CancellationToken.None))
            results.Add(result);

        Assert.That(results, Is.EqualTo(new[] { 1, 2 }));
    }

    private static IDbConnectionFactory CreateFaultInjectingConnectionFactory(FaultInjector injector) =>
        new FaultInjectingConnectionFactory(new SqliteConnectionFactory("Data Source=:memory:"), injector);

    private static async Task<IEnumerable<string>> CollectAsync(IAsyncEnumerable<string> source)
    {
        var results = new List<string>();

        await foreach (var item in source)
            results.Add(item);

        return results;
    }

    private sealed record TestQuery : ISqlQuery<string>
    {
        public required string Test { get; init; }
    }
}
