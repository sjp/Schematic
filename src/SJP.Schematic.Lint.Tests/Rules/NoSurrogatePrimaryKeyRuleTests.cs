using System.Threading.Tasks;
using LanguageExt;
using Moq;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Lint.Rules;

namespace SJP.Schematic.Lint.Tests.Rules;

[TestFixture]
internal static class NoSurrogatePrimaryKeyRuleTests
{
    [Test]
    public static void Ctor_GivenInvalidLevel_ThrowsArgumentException()
    {
        const RuleLevel level = (RuleLevel)999;
        Assert.That(() => new NoSurrogatePrimaryKeyRule(level), Throws.ArgumentException);
    }

    [Test]
    public static void AnalyseTables_GivenNullTables_ThrowsArgumentNullException()
    {
        var rule = new NoSurrogatePrimaryKeyRule(RuleLevel.Error);
        Assert.That(() => rule.AnalyseTables(null), Throws.ArgumentNullException);
    }

    [Test]
    public static async Task AnalyseTables_GivenTableWithMissingPrimaryKey_ProducesNoMessages()
    {
        var rule = new NoSurrogatePrimaryKeyRule(RuleLevel.Error);

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

        var messages = await rule.AnalyseTables(tables);

        Assert.That(messages, Is.Empty);
    }

    [Test]
    public static async Task AnalyseTables_GivenTableWithSingleColumnPrimaryKey_ProducesNoMessages()
    {
        var rule = new NoSurrogatePrimaryKeyRule(RuleLevel.Error);

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

        var messages = await rule.AnalyseTables(tables);

        Assert.That(messages, Is.Empty);
    }

    [Test]
    public static async Task AnalyseTables_GivenTableWithMultiColumnPrimaryKey_ProducesMessages()
    {
        var rule = new NoSurrogatePrimaryKeyRule(RuleLevel.Error);

        var testColumnA = new DatabaseColumn(
            "test_column_a",
            Mock.Of<IDbType>(),
            false,
            null,
            null
        );
        var testColumnB = new DatabaseColumn(
            "test_column_b",
            Mock.Of<IDbType>(),
            false,
            null,
            null
        );
        var testPrimaryKey = new DatabaseKey(
            Option<Identifier>.Some("test_primary_key"),
            DatabaseKeyType.Primary,
            [testColumnA, testColumnB],
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

        var messages = await rule.AnalyseTables(tables);

        Assert.That(messages, Is.Not.Empty);
    }

    [Test]
    public static async Task AnalyseTables_GivenTableWithMultiColumnPrimaryKeyContainingAllForeignKeyColumns_ProducesNoMessages()
    {
        var rule = new NoSurrogatePrimaryKeyRule(RuleLevel.Error);

        var testColumnA = new DatabaseColumn(
            "test_column_a",
            Mock.Of<IDbType>(),
            false,
            null,
            null
        );
        var testColumnB = new DatabaseColumn(
            "test_column_b",
            Mock.Of<IDbType>(),
            false,
            null,
            null
        );
        var testPrimaryKey = new DatabaseKey(
            Option<Identifier>.Some("test_primary_key"),
            DatabaseKeyType.Primary,
            [testColumnA, testColumnB],
            true
        );

        var testForeignKey1 = new DatabaseKey(
            Option<Identifier>.Some("test_fk1"),
            DatabaseKeyType.Foreign,
            [testColumnA],
            true
        );
        var testForeignKey2 = new DatabaseKey(
            Option<Identifier>.Some("test_fk2"),
            DatabaseKeyType.Foreign,
            [testColumnB],
            true
        );

        var relationalKey1 = new DatabaseRelationalKey(
            "test",
            testForeignKey1,
            "test",
            testPrimaryKey,
            ReferentialAction.Cascade,
            ReferentialAction.Cascade
        );
        var relationalKey2 = new DatabaseRelationalKey(
            "test",
            testForeignKey2,
            "test",
            testPrimaryKey,
            ReferentialAction.Cascade,
            ReferentialAction.Cascade
        );

        var table = new RelationalDatabaseTable(
            "test",
            [],
            testPrimaryKey,
            [],
            [relationalKey1, relationalKey2],
            [],
            [],
            [],
            []
        );
        var tables = new[] { table };

        var messages = await rule.AnalyseTables(tables);

        Assert.That(messages, Is.Empty);
    }
}