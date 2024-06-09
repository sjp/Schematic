using System.Linq;
using System.Threading.Tasks;
using LanguageExt;
using Moq;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Lint.Rules;

namespace SJP.Schematic.Lint.Tests.Rules;

[TestFixture]
internal static class PrimaryKeyNotIntegerRuleTests
{
    [Test]
    public static void Ctor_GivenInvalidLevel_ThrowsArgumentException()
    {
        const RuleLevel level = (RuleLevel)999;
        Assert.That(() => new PrimaryKeyNotIntegerRule(level), Throws.ArgumentException);
    }

    [Test]
    public static void AnalyseTables_GivenNullTables_ThrowsArgumentNullException()
    {
        var rule = new PrimaryKeyNotIntegerRule(RuleLevel.Error);
        Assert.That(() => rule.AnalyseTables(null), Throws.ArgumentNullException);
    }

    [Test]
    public static async Task AnalyseTables_GivenTableWithMissingPrimaryKey_ProducesNoMessages()
    {
        var rule = new PrimaryKeyNotIntegerRule(RuleLevel.Error);

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

        var hasMessages = await rule.AnalyseTables(tables).AnyAsync().ConfigureAwait(false);

        Assert.That(hasMessages, Is.False);
    }

    [Test]
    public static async Task AnalyseTables_GivenTableWithPrimaryKeyWithSingleIntegerColumn_ProducesNoMessages()
    {
        var rule = new PrimaryKeyNotIntegerRule(RuleLevel.Error);

        var dataTypeMock = new Mock<IDbType>(MockBehavior.Strict);
        dataTypeMock.Setup(t => t.DataType).Returns(DataType.Integer);

        var testColumn = new DatabaseColumn(
            "test_column_1",
            dataTypeMock.Object,
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
            [testColumn],
            testPrimaryKey,
            [],
            [],
            [],
            [],
            [],
            []
        );
        var tables = new[] { table };

        var hasMessages = await rule.AnalyseTables(tables).AnyAsync().ConfigureAwait(false);

        Assert.That(hasMessages, Is.False);
    }

    [Test]
    public static async Task AnalyseTables_GivenTableWithPrimaryKeyWithSingleNonIntegerColumn_ProducesMessages()
    {
        var rule = new PrimaryKeyNotIntegerRule(RuleLevel.Error);

        var dataTypeMock = new Mock<IDbType>(MockBehavior.Strict);
        dataTypeMock.Setup(t => t.DataType).Returns(DataType.Binary);

        var testColumn = new DatabaseColumn(
            "test_column_1",
            dataTypeMock.Object,
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
            [testColumn],
            testPrimaryKey,
            [],
            [],
            [],
            [],
            [],
            []
        );
        var tables = new[] { table };

        var hasMessages = await rule.AnalyseTables(tables).AnyAsync().ConfigureAwait(false);

        Assert.That(hasMessages, Is.True);
    }

    [Test]
    public static async Task AnalyseTables_GivenTableWithPrimaryKeyWithMultipleColumns_ProducesMessages()
    {
        var rule = new PrimaryKeyNotIntegerRule(RuleLevel.Error);

        var testColumn1 = new DatabaseColumn(
            "test_column_1",
            Mock.Of<IDbType>(),
            false,
            null,
            null
        );
        var testColumn2 = new DatabaseColumn(
            "test_column_2",
            Mock.Of<IDbType>(),
            false,
            null,
            null
        );
        var testPrimaryKey = new DatabaseKey(
            Option<Identifier>.Some("test_primary_key"),
            DatabaseKeyType.Primary,
            [testColumn1, testColumn2],
            true
        );

        var table = new RelationalDatabaseTable(
            "test",
            [testColumn1, testColumn2],
            testPrimaryKey,
            [],
            [],
            [],
            [],
            [],
            []
        );
        var tables = new[] { table };

        var hasMessages = await rule.AnalyseTables(tables).AnyAsync().ConfigureAwait(false);

        Assert.That(hasMessages, Is.True);
    }
}