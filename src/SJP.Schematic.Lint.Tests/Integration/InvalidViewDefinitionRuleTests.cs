using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Lint.Rules;
using SJP.Schematic.Tests.Utilities;

namespace SJP.Schematic.Lint.Tests.Integration;

internal sealed class InvalidViewDefinitionRuleTests : SqliteTest
{
    // Mirrors the rule's private ProbeBatchSize. Kept in sync manually -- if the rule's batch size
    // changes, the query-count assertion below should be updated (and will fail loudly if it isn't).
    private const int ProbeBatchSize = 32;

    // Chosen so a 64-view batch spans exactly two 32-view probe batches (see ProbeBatchSize on the
    // rule), with invalid views planted at the first and last position of each -- the positions most
    // likely to reveal an off-by-one in the binary split.
    private const int BatchViewCount = 64;
    private static readonly IReadOnlyCollection<int> InvalidBatchViewIndexes = [0, 31, 32, 63];

    [OneTimeSetUp]
    public async Task Init()
    {
        await DbConnection.ExecuteAsync("create view valid_view_1 as select 1 as dummy", CancellationToken.None);
        await DbConnection.ExecuteAsync("create view invalid_view_1 as select x from unknown_table", CancellationToken.None);

        for (var i = 0; i < BatchViewCount; i++)
        {
            var sql = InvalidBatchViewIndexes.Contains(i)
                ? $"create view batch_view_{i} as select x from unknown_table_{i}"
                : $"create view batch_view_{i} as select {i} as dummy";
            await DbConnection.ExecuteAsync(sql, CancellationToken.None);
        }
    }

    [OneTimeTearDown]
    public async Task CleanUp()
    {
        await DbConnection.ExecuteAsync("drop view valid_view_1", CancellationToken.None);
        await DbConnection.ExecuteAsync("drop view invalid_view_1", CancellationToken.None);

        for (var i = 0; i < BatchViewCount; i++)
            await DbConnection.ExecuteAsync($"drop view batch_view_{i}", CancellationToken.None);
    }

    [Test]
    public static void Ctor_GivenNullConnection_ThrowsArgumentNullException()
    {
        ISchematicConnection connection = null;
        const RuleLevel level = RuleLevel.Error;
        Assert.That(() => new InvalidViewDefinitionRule(connection, level), Throws.ArgumentNullException);
    }

    [Test]
    public static void Ctor_GivenInvalidLevel_ThrowsArgumentException()
    {
        var connection = Mock.Of<ISchematicConnection>();
        const RuleLevel level = (RuleLevel)999;
        Assert.That(() => new InvalidViewDefinitionRule(connection, level), Throws.ArgumentException);
    }

    [Test]
    public void AnalyseViews_GivenNullViews_ThrowsArgumentNullException()
    {
        var rule = new InvalidViewDefinitionRule(Connection, RuleLevel.Error);
        Assert.That(() => rule.AnalyseViews(null), Throws.ArgumentNullException);
    }

    [Test]
    public async Task AnalyseViews_GivenDatabaseWithOnlyValidViews_ProducesNoMessages()
    {
        var rule = new InvalidViewDefinitionRule(Connection, RuleLevel.Error);
        var database = GetSqliteDatabase();

        var views = new[]
        {
            await database.GetView("valid_view_1").UnwrapSomeAsync(),
        };

        var messages = await rule.AnalyseViews(views);

        Assert.That(messages, Is.Empty);
    }

    [Test]
    public async Task AnalyseViews_GivenViewsWithOnlyInvalidViews_ProducesMessages()
    {
        var rule = new InvalidViewDefinitionRule(Connection, RuleLevel.Error);
        var database = GetSqliteDatabase();

        var views = new[]
        {
            await database.GetView("invalid_view_1").UnwrapSomeAsync(),
        };

        var messages = await rule.AnalyseViews(views);

        Assert.That(messages, Is.Not.Empty);
    }

    [Test]
    public async Task AnalyseViews_GivenViewsWithValidAndInvalidViews_ProducesMessages()
    {
        var rule = new InvalidViewDefinitionRule(Connection, RuleLevel.Error);
        var database = GetSqliteDatabase();

        var views = new[]
        {
            await database.GetView("valid_view_1").UnwrapSomeAsync(),
            await database.GetView("invalid_view_1").UnwrapSomeAsync(),
        };

        var messages = await rule.AnalyseViews(views);

        Assert.That(messages, Is.Not.Empty);
    }

    [Test]
    public async Task AnalyseViews_GivenManyValidViewsSpanningMultipleProbeBatches_ProducesNoMessages()
    {
        var rule = new InvalidViewDefinitionRule(Connection, RuleLevel.Error);
        var views = (await GetBatchViewsAsync())
            .Where(v => !InvalidBatchViewIndexes.Contains(IndexOfBatchView(v)))
            .ToArray();

        var messages = await rule.AnalyseViews(views);

        Assert.That(messages, Is.Empty);
    }

    [Test]
    public async Task AnalyseViews_GivenInvalidViewsAtProbeBatchBoundaries_ReportsExactlyThoseViews()
    {
        var rule = new InvalidViewDefinitionRule(Connection, RuleLevel.Error);
        var views = await GetBatchViewsAsync();

        var messages = await rule.AnalyseViews(views);

        Assert.That(messages, Has.Count.EqualTo(InvalidBatchViewIndexes.Count));
        foreach (var index in InvalidBatchViewIndexes)
        {
            var expectedViewName = $"batch_view_{index}";
            Assert.That(messages.Any(m => m.Message.Contains(expectedViewName)), Is.True, $"Expected a message naming '{expectedViewName}'.");
        }
    }

    [Test]
    public async Task AnalyseViews_GivenAllInvalidViews_ProducesMessageForEveryView()
    {
        var rule = new InvalidViewDefinitionRule(Connection, RuleLevel.Error);
        var views = (await GetBatchViewsAsync())
            .Where(v => InvalidBatchViewIndexes.Contains(IndexOfBatchView(v)))
            .ToArray();

        var messages = await rule.AnalyseViews(views);

        Assert.That(messages, Has.Count.EqualTo(views.Length));
    }

    [Test]
    public async Task AnalyseViews_GivenManyValidViews_ExecutesOneProbeQueryPerBatch()
    {
        var countingFactory = new CountingDbConnectionFactory(DbConnection);
        var countingConnection = new SchematicConnection(countingFactory, Connection.Dialect);
        var rule = new InvalidViewDefinitionRule(countingConnection, RuleLevel.Error);

        var views = (await GetBatchViewsAsync())
            .Where(v => !InvalidBatchViewIndexes.Contains(IndexOfBatchView(v)))
            .ToArray();

        await rule.AnalyseViews(views);

        var expectedQueryCount = (views.Length + ProbeBatchSize - 1) / ProbeBatchSize;
        Assert.That(countingFactory.QueryCount, Is.EqualTo(expectedQueryCount));
    }

    private async Task<IReadOnlyList<IDatabaseView>> GetBatchViewsAsync()
    {
        var database = GetSqliteDatabase();
        var views = new List<IDatabaseView>();
        for (var i = 0; i < BatchViewCount; i++)
            views.Add(await database.GetView($"batch_view_{i}").UnwrapSomeAsync());

        return views;
    }

    private static int IndexOfBatchView(IDatabaseView view)
    {
        var name = view.Name.LocalName;
        var separatorIndex = name.LastIndexOf('_');
        return int.Parse(name[(separatorIndex + 1)..]);
    }
}
