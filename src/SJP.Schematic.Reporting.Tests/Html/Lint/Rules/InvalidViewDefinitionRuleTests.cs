using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Lint;
using SJP.Schematic.Reporting.Html.Lint.Rules;
using SJP.Schematic.Tests.Utilities;

namespace SJP.Schematic.Reporting.Tests.Html.Lint.Rules;

internal sealed class InvalidViewDefinitionRuleTests : SqliteRuleTestBase
{
    [OneTimeSetUp]
    public Task Init() => DbConnection.ExecuteAsync("create view reporting_invalid_view_1 as select x from unknown_table", CancellationToken.None);

    [OneTimeTearDown]
    public Task CleanUp() => DbConnection.ExecuteAsync("drop view reporting_invalid_view_1", CancellationToken.None);

    [Test]
    public static void Ctor_GivenNullConnection_ThrowsArgumentNullException()
    {
        Assert.That(
            () => new InvalidViewDefinitionRule(null!, RuleLevel.Error),
            Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("connection")
        );
    }

    [Test]
    public static void Ctor_GivenInvalidLevel_ThrowsArgumentException()
    {
        var connection = Mock.Of<ISchematicConnection>();
        const RuleLevel level = (RuleLevel)999;
        Assert.That(
            () => new InvalidViewDefinitionRule(connection, level),
            Throws.ArgumentException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("level")
        );
    }

    [Test]
    public void AnalyseViews_GivenNullViews_ThrowsArgumentNullException()
    {
        var rule = new InvalidViewDefinitionRule(Connection, RuleLevel.Error);
        Assert.That(
            () => rule.AnalyseViews(null!),
            Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("views")
        );
    }

    [Test]
    public async Task AnalyseViews_GivenInvalidView_ProducesMessageWithVisibleViewName()
    {
        var rule = new InvalidViewDefinitionRule(Connection, RuleLevel.Error);
        var database = GetSqliteDatabase();

        var views = new[]
        {
            await database.GetView("reporting_invalid_view_1").UnwrapSomeAsync(),
        };

        var messages = await rule.AnalyseViews(views);

        Assert.That(messages, Is.Not.Empty);
        var message = messages.Single();
        Assert.That(message.Message, Does.Contain("reporting_invalid_view_1"));
        Assert.That(message.Message, Does.Not.Contain("LocalName ="));
    }
}
