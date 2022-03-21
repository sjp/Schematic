using Moq;
using NUnit.Framework;
using SJP.Schematic.Core;

namespace SJP.Schematic.Reporting.Tests;

[TestFixture]
internal static class ReportingRuleProviderTests
{
    [Test]
    public static void Test()
    {
        var ruleProvider = new Html.Lint.ReportingRuleProvider();
        var dbConnection = Mock.Of<IDbConnectionFactory>();
        var dialect = new Sqlite.SqliteDialect();
        var connection = new SchematicConnection(dbConnection, dialect);

        var rules = ruleProvider.GetRules(connection, Lint.RuleLevel.Error);

        Assert.That(rules, Is.Not.Empty);
    }
}