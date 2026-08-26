using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LanguageExt;
using Moq;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Lint.Rules;

namespace SJP.Schematic.Lint.Tests.Rules;

internal sealed class RedundantIndexesRuleTests
{
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
        Assert.That(() => rule.AnalyseTables(null), Throws.ArgumentNullException);
    }

    [Test]
    public async Task AnalyseTables_GivenTablesWithOnlyTablesWithoutIndexes_ProducesNoMessages()
    {
        var rule = new RedundantIndexesRule(RuleLevel.Error);

        var table = CreateTable("table_a", []);

        var messages = await rule.AnalyseTables([table]);

        Assert.That(messages, Is.Empty);
    }

    [Test]
    public async Task AnalyseTables_GivenTablesWithOnlyTablesWithoutRedundantIndexes_ProducesNoMessages()
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

    [Test]
    public async Task AnalyseTables_GivenTablesWithOnlyTablesWithRedundantIndexes_ProducesMessages()
    {
        var rule = new RedundantIndexesRule(RuleLevel.Error);

        var indexes = new[]
        {
            CreateIndex("index_a", [new DatabaseIndexColumn("column_a", CreateColumn("column_a"), IndexColumnOrder.Ascending)], []),
            CreateIndex("index_a_dupe", [new DatabaseIndexColumn("column_a", CreateColumn("column_a"), IndexColumnOrder.Ascending)], []),
        };

        var table = CreateTable("table_a", indexes);

        var messages = await rule.AnalyseTables([table]);

        Assert.That(messages, Is.Not.Empty);
    }

    [Test]
    public async Task AnalyseTables_GivenTablesWithIndexesWithColumnSetAsPrefix_ProducesMessages()
    {
        var rule = new RedundantIndexesRule(RuleLevel.Error);

        var indexes = new[]
        {
            CreateIndex("index_a", [new DatabaseIndexColumn("column_a", CreateColumn("column_a"), IndexColumnOrder.Ascending)], []),
            CreateIndex(
                "index_a_dupe",
                [
                    new DatabaseIndexColumn("column_a", CreateColumn("column_a"), IndexColumnOrder.Ascending),
                    new DatabaseIndexColumn("column_b", CreateColumn("column_b"), IndexColumnOrder.Ascending),
                ],
                []),
        };

        var table = CreateTable("table_a", indexes);

        var messages = await rule.AnalyseTables([table]);

        Assert.That(messages, Is.Not.Empty);
    }

    [Test]
    public async Task AnalyseTables_GivenTablesWithIndexesWithDuplicateColumnSetsWithDifferentOrdering_ProducesNoMessages()
    {
        var rule = new RedundantIndexesRule(RuleLevel.Error);

        var indexes = new[]
        {
            CreateIndex(
                "index_a",
                [
                    new DatabaseIndexColumn("column_a", CreateColumn("column_a"), IndexColumnOrder.Ascending),
                    new DatabaseIndexColumn("column_b", CreateColumn("column_b"), IndexColumnOrder.Ascending),
                ],
                []),
            CreateIndex(
                "index_a_dupe",
                [
                    new DatabaseIndexColumn("column_a", CreateColumn("column_a"), IndexColumnOrder.Ascending),
                    new DatabaseIndexColumn("column_b", CreateColumn("column_b"), IndexColumnOrder.Descending), // key change, this ordering is important
                ],
                []),
        };

        var table = CreateTable("table_a", indexes);

        var messages = await rule.AnalyseTables([table]);

        Assert.That(messages, Is.Empty);
    }

    [Test]
    public async Task AnalyseTables_GivenTablesWithIndexesWithSameIncludedColumnSets_ProducesMessages()
    {
        var rule = new RedundantIndexesRule(RuleLevel.Error);

        var indexes = new[]
        {
            CreateIndex(
                "index_a",
                [new DatabaseIndexColumn("column_a", CreateColumn("column_a"), IndexColumnOrder.Ascending)],
                [CreateColumn("column_b")]),
            CreateIndex(
                "index_a_dupe",
                [new DatabaseIndexColumn("column_a", CreateColumn("column_a"), IndexColumnOrder.Ascending)],
                [CreateColumn("column_b")]),
        };

        var table = CreateTable("table_a", indexes);

        var messages = await rule.AnalyseTables([table]);

        Assert.That(messages, Is.Not.Empty);
    }

    [Test]
    public async Task AnalyseTables_GivenTablesWithIndexesWithSubsettedIncludedColumnSets_ProducesMessages()
    {
        var rule = new RedundantIndexesRule(RuleLevel.Error);

        var indexes = new[]
        {
            CreateIndex(
                "index_a",
                [new DatabaseIndexColumn("column_a", CreateColumn("column_a"), IndexColumnOrder.Ascending)],
                [CreateColumn("column_b"), CreateColumn("column_c")]),
            CreateIndex(
                "index_a_dupe",
                [new DatabaseIndexColumn("column_a", CreateColumn("column_a"), IndexColumnOrder.Ascending)],
                [CreateColumn("column_b"), CreateColumn("column_c"), CreateColumn("column_d")]), // has column_d which makes it a superset
        };

        var table = CreateTable("table_a", indexes);

        var messages = await rule.AnalyseTables([table]);

        Assert.That(messages, Is.Not.Empty);
    }

    [Test]
    public async Task AnalyseTables_GivenTablesWithIndexesWithDifferingIncludedColumnSets_ProducesNoMessages()
    {
        var rule = new RedundantIndexesRule(RuleLevel.Error);

        var indexes = new[]
        {
            CreateIndex(
                "index_a",
                [new DatabaseIndexColumn("column_a", CreateColumn("column_a"), IndexColumnOrder.Ascending)],
                [CreateColumn("column_b")]),
            CreateIndex(
                "index_a_dupe",
                [new DatabaseIndexColumn("column_a", CreateColumn("column_a"), IndexColumnOrder.Ascending)],
                [CreateColumn("column_c")]),
        };

        var table = CreateTable("table_a", indexes);

        var messages = await rule.AnalyseTables([table]);

        Assert.That(messages, Is.Empty);
    }

    [Test]
    public async Task AnalyseTables_GivenTablesWithUniqueIndexPrefixOfNonUniqueIndex_ProducesNoMessages()
    {
        var rule = new RedundantIndexesRule(RuleLevel.Error);

        var indexes = new[]
        {
            CreateIndex(
                "index_a",
                [new DatabaseIndexColumn("column_a", CreateColumn("column_a"), IndexColumnOrder.Ascending)],
                [],
                isUnique: true),
            CreateIndex(
                "index_a_dupe",
                [
                    new DatabaseIndexColumn("column_a", CreateColumn("column_a"), IndexColumnOrder.Ascending),
                    new DatabaseIndexColumn("column_b", CreateColumn("column_b"), IndexColumnOrder.Ascending),
                ],
                []),
        };

        var table = CreateTable("table_a", indexes);

        var messages = await rule.AnalyseTables([table]);

        Assert.That(messages, Is.Empty);
    }

    [Test]
    public async Task AnalyseTables_GivenTablesWithUniqueIndexDuplicatedByNonUniqueIndex_ProducesOnlyMessageForNonUniqueIndex()
    {
        var rule = new RedundantIndexesRule(RuleLevel.Error);

        var indexes = new[]
        {
            CreateIndex(
                "index_a",
                [new DatabaseIndexColumn("column_a", CreateColumn("column_a"), IndexColumnOrder.Ascending)],
                [],
                isUnique: true),
            CreateIndex("index_a_dupe", [new DatabaseIndexColumn("column_a", CreateColumn("column_a"), IndexColumnOrder.Ascending)], []),
        };

        var table = CreateTable("table_a", indexes);

        var messages = await rule.AnalyseTables([table]);

        Assert.That(messages, Has.Exactly(1).Items);
        Assert.That(messages.Single().Message, Does.Contain("index 'index_a_dupe'"));
    }

    [Test]
    public async Task AnalyseTables_GivenTablesWithUniqueIndexPrefixOfWiderUniqueIndex_ProducesNoMessages()
    {
        var rule = new RedundantIndexesRule(RuleLevel.Error);

        var indexes = new[]
        {
            CreateIndex(
                "index_a",
                [new DatabaseIndexColumn("column_a", CreateColumn("column_a"), IndexColumnOrder.Ascending)],
                [],
                isUnique: true),
            CreateIndex(
                "index_a_dupe",
                [
                    new DatabaseIndexColumn("column_a", CreateColumn("column_a"), IndexColumnOrder.Ascending),
                    new DatabaseIndexColumn("column_b", CreateColumn("column_b"), IndexColumnOrder.Ascending),
                ],
                [],
                isUnique: true),
        };

        var table = CreateTable("table_a", indexes);

        var messages = await rule.AnalyseTables([table]);

        Assert.That(messages, Is.Empty);
    }

    [Test]
    public async Task AnalyseTables_GivenTablesWithDuplicateUniqueIndexes_ProducesMessages()
    {
        var rule = new RedundantIndexesRule(RuleLevel.Error);

        var indexes = new[]
        {
            CreateIndex(
                "index_a",
                [new DatabaseIndexColumn("column_a", CreateColumn("column_a"), IndexColumnOrder.Ascending)],
                [],
                isUnique: true),
            CreateIndex(
                "index_a_dupe",
                [new DatabaseIndexColumn("column_a", CreateColumn("column_a"), IndexColumnOrder.Ascending)],
                [],
                isUnique: true),
        };

        var table = CreateTable("table_a", indexes);

        var messages = await rule.AnalyseTables([table]);

        Assert.That(messages, Is.Not.Empty);
    }

    [Test]
    public async Task AnalyseTables_GivenTablesWithFilteredAndUnfilteredIndexes_ProducesNoMessages()
    {
        var rule = new RedundantIndexesRule(RuleLevel.Error);

        var indexes = new[]
        {
            CreateIndex(
                "index_a",
                [new DatabaseIndexColumn("column_a", CreateColumn("column_a"), IndexColumnOrder.Ascending)],
                [],
                filterDefinition: "column_a IS NOT NULL"),
            CreateIndex("index_a_dupe", [new DatabaseIndexColumn("column_a", CreateColumn("column_a"), IndexColumnOrder.Ascending)], []),
        };

        var table = CreateTable("table_a", indexes);

        var messages = await rule.AnalyseTables([table]);

        Assert.That(messages, Is.Empty);
    }

    [Test]
    public async Task AnalyseTables_GivenTablesWithIndexesWithDifferentFilters_ProducesNoMessages()
    {
        var rule = new RedundantIndexesRule(RuleLevel.Error);

        var indexes = new[]
        {
            CreateIndex(
                "index_a",
                [new DatabaseIndexColumn("column_a", CreateColumn("column_a"), IndexColumnOrder.Ascending)],
                [],
                filterDefinition: "column_a IS NOT NULL"),
            CreateIndex(
                "index_a_dupe",
                [
                    new DatabaseIndexColumn("column_a", CreateColumn("column_a"), IndexColumnOrder.Ascending),
                    new DatabaseIndexColumn("column_b", CreateColumn("column_b"), IndexColumnOrder.Ascending),
                ],
                [],
                filterDefinition: "column_b IS NOT NULL"),
        };

        var table = CreateTable("table_a", indexes);

        var messages = await rule.AnalyseTables([table]);

        Assert.That(messages, Is.Empty);
    }

    [Test]
    public async Task AnalyseTables_GivenTablesWithIndexesWithMatchingFilters_ProducesMessages()
    {
        var rule = new RedundantIndexesRule(RuleLevel.Error);

        var indexes = new[]
        {
            CreateIndex(
                "index_a",
                [new DatabaseIndexColumn("column_a", CreateColumn("column_a"), IndexColumnOrder.Ascending)],
                [],
                filterDefinition: "column_a IS NOT NULL"),
            CreateIndex(
                "index_a_dupe",
                [
                    new DatabaseIndexColumn("column_a", CreateColumn("column_a"), IndexColumnOrder.Ascending),
                    new DatabaseIndexColumn("column_b", CreateColumn("column_b"), IndexColumnOrder.Ascending),
                ],
                [],
                filterDefinition: "column_a IS NOT NULL"),
        };

        var table = CreateTable("table_a", indexes);

        var messages = await rule.AnalyseTables([table]);

        Assert.That(messages, Is.Not.Empty);
    }

    [Test]
    public async Task AnalyseTables_GivenTablesWithEquivalentIndexes_ProducesOneMessageForThePair()
    {
        var rule = new RedundantIndexesRule(RuleLevel.Error);

        var indexes = new[]
        {
            CreateIndex("index_a", [new DatabaseIndexColumn("column_a", CreateColumn("column_a"), IndexColumnOrder.Ascending)], []),
            CreateIndex("index_a_dupe", [new DatabaseIndexColumn("column_a", CreateColumn("column_a"), IndexColumnOrder.Ascending)], []),
        };

        var table = CreateTable("table_a", indexes);

        var messages = await rule.AnalyseTables([table]);

        Assert.That(messages, Has.Exactly(1).Items);
    }

    private static IDatabaseColumn CreateColumn(string name)
    {
        var column = new Mock<IDatabaseColumn>(MockBehavior.Strict);
        column.Setup(c => c.Name).Returns(name);
        return column.Object;
    }

    private static IDatabaseIndex CreateIndex(string indexName, IReadOnlyCollection<IDatabaseIndexColumn> indexColumns, IReadOnlyCollection<IDatabaseColumn> includedColumns, bool isUnique = false, string filterDefinition = null)
    {
        var index = new Mock<IDatabaseIndex>(MockBehavior.Strict);
        index.Setup(c => c.Name).Returns(indexName);
        index.Setup(c => c.Columns).Returns(indexColumns);
        index.Setup(c => c.IncludedColumns).Returns(includedColumns);
        index.Setup(c => c.IsUnique).Returns(isUnique);
        index.Setup(c => c.FilterDefinition).Returns(filterDefinition == null ? Option<string>.None : Option<string>.Some(filterDefinition));

        return index.Object;
    }

    private static IRelationalDatabaseTable CreateTable(string tableName, IReadOnlyCollection<IDatabaseIndex> indexes)
    {
        var table = new Mock<IRelationalDatabaseTable>(MockBehavior.Strict);
        table.Setup(c => c.Name).Returns(tableName);
        table.Setup(c => c.Indexes).Returns(indexes);

        return table.Object;
    }
}