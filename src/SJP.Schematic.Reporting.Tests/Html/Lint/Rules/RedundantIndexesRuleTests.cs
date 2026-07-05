using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Lint;
using SJP.Schematic.Reporting.Html.Lint.Rules;

namespace SJP.Schematic.Reporting.Tests.Html.Lint.Rules;

[TestFixture]
internal static class RedundantIndexesRuleTests
{
    private static IDatabaseColumn CreateColumn(string name)
    {
        var column = new Mock<IDatabaseColumn>(MockBehavior.Strict);
        column.Setup(c => c.Name).Returns(name);
        return column.Object;
    }

    private static IDatabaseIndex CreateIndex(string indexName, IReadOnlyCollection<IDatabaseIndexColumn> indexColumns, IReadOnlyCollection<IDatabaseColumn> includedColumns)
    {
        var index = new Mock<IDatabaseIndex>(MockBehavior.Strict);
        index.Setup(c => c.Name).Returns(indexName);
        index.Setup(c => c.Columns).Returns(indexColumns);
        index.Setup(c => c.IncludedColumns).Returns(includedColumns);

        return index.Object;
    }

    private static IRelationalDatabaseTable CreateTable(Identifier tableName, IReadOnlyCollection<IDatabaseIndex> indexes)
    {
        var table = new Mock<IRelationalDatabaseTable>(MockBehavior.Strict);
        table.Setup(c => c.Name).Returns(tableName);
        table.Setup(c => c.Indexes).Returns(indexes);

        return table.Object;
    }

    [Test]
    public static void Ctor_GivenInvalidLevel_ThrowsArgumentException()
    {
        const RuleLevel level = (RuleLevel)999;
        Assert.That(() => new RedundantIndexesRule(level), Throws.ArgumentException);
    }

    [Test]
    public static void AnalyseTables_GivenNullTables_ThrowsArgumentNullException()
    {
        var rule = new RedundantIndexesRule(RuleLevel.Error);
        Assert.That(() => rule.AnalyseTables(null!), Throws.ArgumentNullException);
    }

    [Test]
    public static async Task AnalyseTables_GivenTableWithRedundantIndexes_ProducesMessageWithVisibleTableName()
    {
        var rule = new RedundantIndexesRule(RuleLevel.Error);
        var tableName = Identifier.CreateQualifiedIdentifier("test_schema", "test_table");

        var indexes = new[]
        {
            CreateIndex("index_a", [new DatabaseIndexColumn("column_a", CreateColumn("column_a"), IndexColumnOrder.Ascending)], []),
            CreateIndex("index_a_dupe", [new DatabaseIndexColumn("column_a", CreateColumn("column_a"), IndexColumnOrder.Ascending)], []),
        };

        var table = CreateTable(tableName, indexes);

        var messages = await rule.AnalyseTables([table]);

        Assert.That(messages, Is.Not.Empty);
        var message = messages.First();
        Assert.That(message.Message, Does.Contain("test_schema.test_table"));
        Assert.That(message.Message, Does.Not.Contain("LocalName ="));
    }

    [Test]
    public static async Task AnalyseTables_GivenTableWithoutRedundantIndexes_ProducesNoMessages()
    {
        var rule = new RedundantIndexesRule(RuleLevel.Error);

        var indexes = new[]
        {
            CreateIndex("index_a", [new DatabaseIndexColumn("column_a", CreateColumn("column_a"), IndexColumnOrder.Ascending)], []),
            CreateIndex("index_b", [new DatabaseIndexColumn("column_b", CreateColumn("column_b"), IndexColumnOrder.Ascending)], []),
        };

        var table = CreateTable("table_a", indexes);

        var messages = await rule.AnalyseTables([table]);

        Assert.That(messages, Is.Empty);
    }
}
