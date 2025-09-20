using System.Linq;
using System.Threading.Tasks;
using LanguageExt;
using Moq;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Lint.Rules;

namespace SJP.Schematic.Lint.Tests.Rules;

[TestFixture]
internal static class CandidateKeyMissingRuleTests
{
    [Test]
    public static void Ctor_GivenInvalidLevel_ThrowsArgumentException()
    {
        const RuleLevel level = (RuleLevel)999;
        Assert.That(() => new CandidateKeyMissingRule(level), Throws.ArgumentException);
    }

    [Test]
    public static void AnalyseTables_GivenNullTables_ThrowsArgumentNullException()
    {
        var rule = new CandidateKeyMissingRule(RuleLevel.Error);
        Assert.That(() => rule.AnalyseTables(null), Throws.ArgumentNullException);
    }

    [Test]
    public static async Task AnalyseTables_GivenTableWithMissingPrimaryKeyAndNoUniqueKeys_ProducesMessages()
    {
        var rule = new CandidateKeyMissingRule(RuleLevel.Error);

        var table = new RelationalDatabaseTable(
            "test",
            [],
            null,
            [],
            [],
            [],
            [],
            [],
            []
        );
        var tables = new[] { table };

        var messages = await rule.AnalyseTables(tables).ConfigureAwait(false);

        Assert.That(messages, Is.Not.Empty);
    }

    [Test]
    public static async Task AnalyseTables_GivenTableWithPrimaryKey_ProducesNoMessages()
    {
        var rule = new CandidateKeyMissingRule(RuleLevel.Error);

        var testColumn = new DatabaseColumn(
            "test_column",
            Mock.Of<IDbType>(),
            false,
            null,
            null
        );
        var testPrimaryKey = new DatabaseKey(
            Option<Identifier>.Some("test_primary_key"),
            DatabaseKeyType.Primary,
            [testColumn],
            true
        );

        var table = new RelationalDatabaseTable(
            "test",
            [],
            testPrimaryKey,
            [],
            [],
            [],
            [],
            [],
            []
        );
        var tables = new[] { table };

        var messages = await rule.AnalyseTables(tables).ConfigureAwait(false);

        Assert.That(messages, Is.Empty);
    }

    [Test]
    public static async Task AnalyseTables_GivenTableWithUniqueKey_ProducesNoMessages()
    {
        var rule = new CandidateKeyMissingRule(RuleLevel.Error);

        var testColumn = new DatabaseColumn(
            "test_column",
            Mock.Of<IDbType>(),
            false,
            null,
            null
        );
        var testUniqueKey = new DatabaseKey(
            Option<Identifier>.Some("test_unique_key"),
            DatabaseKeyType.Unique,
            [testColumn],
            true
        );

        var table = new RelationalDatabaseTable(
            "test",
            [],
            null,
            [testUniqueKey],
            [],
            [],
            [],
            [],
            []
        );
        var tables = new[] { table };

        var messages = await rule.AnalyseTables(tables).ConfigureAwait(false);

        Assert.That(messages, Is.Empty);
    }
}