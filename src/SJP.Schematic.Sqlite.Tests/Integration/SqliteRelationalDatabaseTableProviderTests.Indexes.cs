using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Tests.Utilities;

namespace SJP.Schematic.Sqlite.Tests.Integration;

internal sealed partial class SqliteRelationalDatabaseTableProviderTests : SqliteTest
{
    [Test]
    public async Task Indexes_WhenGivenTableWithNoIndexes_ReturnsEmptyCollection()
    {
        var table = await GetTableAsync("table_test_table_1");

        Assert.That(table.Indexes, Is.Empty);
    }

    [Test]
    public async Task Indexes_WhenGivenTableWithSingleColumnIndex_ReturnsIndexWithColumnOnly()
    {
        var table = await GetTableAsync("table_test_table_8");
        var index = table.Indexes.Single();
        var indexColumns = index.Columns
            .Select(c => c.DependentColumns.Single())
            .ToList();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(indexColumns, Has.Exactly(1).Items);
            Assert.That(indexColumns.Single().Name.LocalName, Is.EqualTo("test_column"));
        }
    }

    [Test]
    public async Task Indexes_WhenGivenTableWithSingleColumnIndex_ReturnsIndexWithCorrectName()
    {
        var table = await GetTableAsync("table_test_table_8");
        var index = table.Indexes.Single();

        Assert.That(index.Name.LocalName, Is.EqualTo("ix_test_table_8"));
    }

    [Test]
    public async Task Indexes_WhenGivenTableWithMultiColumnIndex_ReturnsIndexWithColumnsInCorrectOrder()
    {
        var expectedColumnNames = new[] { "first_name", "last_name", "middle_name" };

        var table = await GetTableAsync("table_test_table_9");
        var index = table.Indexes.Single();
        var indexColumns = index.Columns
            .Select(c => c.DependentColumns.Single())
            .Select(c => c.Name.LocalName)
            .ToList();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(indexColumns, Has.Exactly(3).Items);
            Assert.That(indexColumns, Is.EqualTo(expectedColumnNames));
        }
    }

    [Test]
    public async Task Indexes_WhenGivenTableWithMultiColumnIndex_ReturnsIndexWithCorrectName()
    {
        var table = await GetTableAsync("table_test_table_9");
        var index = table.Indexes.Single();

        Assert.That(index.Name.LocalName, Is.EqualTo("ix_test_table_9"));
    }

    [Test]
    public async Task Indexes_WhenGivenTableWithIndexContainingNoIncludedColumns_ReturnsIndexWithoutIncludedColumns()
    {
        var table = await GetTableAsync("table_test_table_9");
        var index = table.Indexes.Single();
        var includedColumns = index.IncludedColumns
            .Select(c => c.Name.LocalName)
            .ToList();

        Assert.That(includedColumns, Is.Empty);
    }

    [Test]
    public async Task Indexes_WhenGivenTableWithNonUniqueIndex_ReturnsIndexWithIsUniqueFalse()
    {
        var table = await GetTableAsync("table_test_table_9");
        var index = table.Indexes.Single();

        Assert.That(index.IsUnique, Is.False);
    }

    [Test]
    public async Task Indexes_WhenGivenTableWithUniqueIndex_ReturnsIndexWithIsUniqueTrue()
    {
        var table = await GetTableAsync("table_test_table_13");
        var index = table.Indexes.Single();

        Assert.That(index.IsUnique, Is.True);
    }

    [Test]
    public async Task Indexes_WhenGivenTableWithExpressionIndex_ReturnsIndexWithExpressionColumn()
    {
        var table = await GetTableAsync("table_test_table_39");
        var index = table.Indexes.Single(i => i.Name.LocalName == "ix_test_table_39_1");
        var indexColumn = index.Columns.Single();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(indexColumn.Expression, Is.EqualTo("lower(test_column_1)"));
            Assert.That(indexColumn.DependentColumns.Select(c => c.Name.LocalName), Is.EqualTo(new[] { "test_column_1" }));
        }
    }

    [Test]
    public async Task Indexes_WhenGivenTableWithMixedColumnAndExpressionIndex_ReturnsColumnsInDefinedOrder()
    {
        var table = await GetTableAsync("table_test_table_39");
        var index = table.Indexes.Single(i => i.Name.LocalName == "ix_test_table_39_2");
        var indexColumns = index.Columns.ToList();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(indexColumns, Has.Exactly(2).Items);
            Assert.That(indexColumns[0].DependentColumns.Single().Name.LocalName, Is.EqualTo("test_column_1"));
            Assert.That(indexColumns[0].Collation.UnwrapSome().LocalName, Is.EqualTo("NOCASE").IgnoreCase);
            Assert.That(indexColumns[1].Expression, Is.EqualTo("lower(test_column_2)"));
            Assert.That(indexColumns[1].Order, Is.EqualTo(IndexColumnOrder.Descending));
        }
    }

    [Test]
    public async Task Indexes_WhenGivenTableWithUniqueExpressionIndex_ReturnsUniqueIndexWithExpressionColumn()
    {
        var table = await GetTableAsync("table_test_table_39");
        var index = table.Indexes.Single(i => i.Name.LocalName == "ix_test_table_39_3");

        using (Assert.EnterMultipleScope())
        {
            Assert.That(index.IsUnique, Is.True);
            Assert.That(index.Columns.Single().Expression, Is.EqualTo("upper(test_column_1)"));
        }
    }

    [Test]
    public async Task Indexes_WhenGivenTableWithConstraintBackedIndexes_DoesNotIncludeThemInIndexes()
    {
        var table = await GetTableAsync("table_test_table_39");
        var indexNames = table.Indexes.Select(i => i.Name.LocalName).ToList();

        Assert.That(indexNames, Is.EquivalentTo(new[] { "ix_test_table_39_1", "ix_test_table_39_2", "ix_test_table_39_3" }));
    }

    [Test]
    public async Task UniqueKeys_WhenGivenTableWithUniqueConstraint_ReturnsKeyWithBackingIndex()
    {
        var table = await GetTableAsync("table_test_table_39");
        var uniqueKey = table.UniqueKeys.Single();
        var backingIndex = uniqueKey.BackingIndex.UnwrapSome();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(backingIndex.IsUnique, Is.True);
            Assert.That(backingIndex.Columns.Single().DependentColumns.Single().Name.LocalName, Is.EqualTo("test_column_2"));
        }
    }

    [Test]
    public async Task PrimaryKey_WhenGivenTableWithCompositePrimaryKey_ReturnsKeyWithBackingIndex()
    {
        var table = await GetTableAsync("table_test_table_40");
        var primaryKey = table.PrimaryKey.UnwrapSome();
        var backingIndex = primaryKey.BackingIndex.UnwrapSome();
        var indexColumnNames = backingIndex.Columns
            .Select(c => c.DependentColumns.Single().Name.LocalName)
            .ToList();

        Assert.That(indexColumnNames, Is.EqualTo(new[] { "test_column_1", "test_column_2" }));
    }

    [Test]
    public async Task Indexes_WhenGivenTableWithFilteredIndexes_ReturnsIndexWithFilteredDefinition()
    {
        var table = await GetTableAsync("table_test_table_38");
        var index1 = table.Indexes.Single(i => i.Name.LocalName == "ix_test_table_38_1");
        var index2 = table.Indexes.Single(i => i.Name.LocalName == "ix_test_table_38_2");

        using (Assert.EnterMultipleScope())
        {
            Assert.That(index1.FilterDefinition, OptionIs.None);
            Assert.That(index2.FilterDefinition.UnwrapSome(), Is.EqualTo("test_column_2 < 100 and test_column_2 > 3"));
        }
    }
}