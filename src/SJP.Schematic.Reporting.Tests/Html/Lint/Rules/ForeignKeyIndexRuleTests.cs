using System.Linq;
using System.Threading.Tasks;
using LanguageExt;
using Moq;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Lint;
using SJP.Schematic.Reporting.Html.Lint.Rules;

namespace SJP.Schematic.Reporting.Tests.Html.Lint.Rules;

[TestFixture]
internal static class ForeignKeyIndexRuleTests
{
    private static IDatabaseColumn GetColumn(Identifier columnName)
    {
        var columnMock = new Mock<IDatabaseColumn>(MockBehavior.Strict);
        columnMock.Setup(c => c.Name).Returns(columnName);
        return columnMock.Object;
    }

    private static IDatabaseIndexColumn GetIndexColumn(Identifier columnName)
    {
        var indexColumnMock = new Mock<IDatabaseIndexColumn>(MockBehavior.Strict);
        indexColumnMock.Setup(c => c.DependentColumns).Returns([GetColumn(columnName)]);
        return indexColumnMock.Object;
    }

    [Test]
    public static void Ctor_GivenInvalidLevel_ThrowsArgumentException()
    {
        const RuleLevel level = (RuleLevel)999;
        Assert.That(() => new ForeignKeyIndexRule(level), Throws.ArgumentException);
    }

    [Test]
    public static void AnalyseTables_GivenNullTables_ThrowsArgumentNullException()
    {
        var rule = new ForeignKeyIndexRule(RuleLevel.Error);
        Assert.That(() => rule.AnalyseTables(null!), Throws.ArgumentNullException);
    }

    [Test]
    public static async Task AnalyseTables_GivenTableWithIndexWithMoreColumnsThanKeyInWrongOrder_ProducesMessageWithVisibleTableName()
    {
        var rule = new ForeignKeyIndexRule(RuleLevel.Error);
        var tableName = Identifier.CreateQualifiedIdentifier("test_schema", "test_table");

        var table = new RelationalDatabaseTable(
            tableName,
            [
                GetColumn("a"),
                GetColumn("b"),
                GetColumn("c"),
            ],
            null,
            [],
            [
                new DatabaseRelationalKey(
                    tableName,
                    new DatabaseKey(Option<Identifier>.Some("test_fk_1"), DatabaseKeyType.Foreign, [GetColumn("b")], true),
                    "test_parent",
                    new DatabaseKey(Option<Identifier>.Some("test_pk_1"), DatabaseKeyType.Primary, [GetColumn("b")], true),
                    ReferentialAction.Cascade,
                    ReferentialAction.Cascade
                ),
            ],
            [],
            [
                new DatabaseIndex(
                    "test_index_1",
                    false,
                    [GetIndexColumn("c"), GetIndexColumn("b")],
                    [],
                    true,
                    Option<string>.None
                ),
            ],
            [],
            []
        );
        var tables = new[] { table };

        var messages = await rule.AnalyseTables(tables);

        Assert.That(messages, Is.Not.Empty);
        var message = messages.Single();
        Assert.That(message.Message, Does.Contain("test_schema.test_table"));
        Assert.That(message.Message, Does.Not.Contain("LocalName ="));
    }

    [Test]
    public static async Task AnalyseTables_GivenTableWithIndexWithMoreColumnsThanKey_ProducesNoMessages()
    {
        var rule = new ForeignKeyIndexRule(RuleLevel.Error);

        var table = new RelationalDatabaseTable(
            "test",
            [
                GetColumn("a"),
                GetColumn("b"),
                GetColumn("c"),
            ],
            null,
            [],
            [
                new DatabaseRelationalKey(
                    "test",
                    new DatabaseKey(Option<Identifier>.Some("test_fk_1"), DatabaseKeyType.Foreign, [GetColumn("b")], true),
                    "test_parent",
                    new DatabaseKey(Option<Identifier>.Some("test_pk_1"), DatabaseKeyType.Primary, [GetColumn("b")], true),
                    ReferentialAction.Cascade,
                    ReferentialAction.Cascade
                ),
            ],
            [],
            [
                new DatabaseIndex(
                    "test_index_1",
                    false,
                    [GetIndexColumn("b"), GetIndexColumn("c")],
                    [],
                    true,
                    Option<string>.None
                ),
            ],
            [],
            []
        );
        var tables = new[] { table };

        var messages = await rule.AnalyseTables(tables);

        Assert.That(messages, Is.Empty);
    }
}
