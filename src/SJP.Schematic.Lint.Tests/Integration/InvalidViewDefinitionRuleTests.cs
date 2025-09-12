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
    [OneTimeSetUp]
    public async Task Init()
    {
        await DbConnection.ExecuteAsync("create view valid_view_1 as select 1 as dummy", CancellationToken.None).ConfigureAwait(false);
        await DbConnection.ExecuteAsync("create view invalid_view_1 as select x from unknown_table", CancellationToken.None).ConfigureAwait(false);
    }

    [OneTimeTearDown]
    public async Task CleanUp()
    {
        await DbConnection.ExecuteAsync("drop view valid_view_1", CancellationToken.None).ConfigureAwait(false);
        await DbConnection.ExecuteAsync("drop view invalid_view_1", CancellationToken.None).ConfigureAwait(false);
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
            await database.GetView("valid_view_1").UnwrapSomeAsync().ConfigureAwait(false),
        };

        var hasMessages = await rule.AnalyseViews(views).AnyAsync().ConfigureAwait(false);

        Assert.That(hasMessages, Is.False);
    }

    [Test]
    public async Task AnalyseViews_GivenViewsWithOnlyInvalidViews_ProducesMessages()
    {
        var rule = new InvalidViewDefinitionRule(Connection, RuleLevel.Error);
        var database = GetSqliteDatabase();

        var views = new[]
        {
            await database.GetView("invalid_view_1").UnwrapSomeAsync().ConfigureAwait(false),
        };

        var hasMessages = await rule.AnalyseViews(views).AnyAsync().ConfigureAwait(false);

        Assert.That(hasMessages, Is.True);
    }

    [Test]
    public async Task AnalyseViews_GivenViewsWithValidAndInvalidViews_ProducesMessages()
    {
        var rule = new InvalidViewDefinitionRule(Connection, RuleLevel.Error);
        var database = GetSqliteDatabase();

        var views = new[]
        {
            await database.GetView("valid_view_1").UnwrapSomeAsync().ConfigureAwait(false),
            await database.GetView("invalid_view_1").UnwrapSomeAsync().ConfigureAwait(false),
        };

        var hasMessages = await rule.AnalyseViews(views).AnyAsync().ConfigureAwait(false);

        Assert.That(hasMessages, Is.True);
    }
}