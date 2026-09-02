using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Tests.Utilities;

namespace SJP.Schematic.MySql.Tests.Integration;

internal sealed partial class MySqlRelationalDatabaseTableProviderTests : MySqlTest
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
    public async Task Indexes_WhenGivenTableWithEnabledIndex_ReturnsIndexWithIsEnabledTrue()
    {
        var table = await GetTableAsync("table_test_table_11");
        var index = table.Indexes.Single();

        Assert.That(index.IsEnabled, Is.True);
    }

    [Test]
    public async Task Indexes_WhenGivenTableWithNonUniqueIndex_ReturnsIndexWithIsUniqueFalse()
    {
        var table = await GetTableAsync("table_test_table_9");
        var index = table.Indexes.Single();

        Assert.That(index.IsUnique, Is.False);
    }

    [Test]
    public async Task Indexes_WhenGivenTableWithUniqueIndex_ReturnsUniqueKeyWithBackingIndex()
    {
        // MySQL does not distinguish a unique index from a unique constraint: both appear in
        // information_schema.table_constraints, so the index is reported through the key it enforces
        var table = await GetTableAsync("table_test_table_13");
        var backingIndex = table.UniqueKeys.Single().BackingIndex.UnwrapSome();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(table.Indexes, Is.Empty);
            Assert.That(backingIndex.Name.LocalName, Is.EqualTo("ix_test_table_13"));
            Assert.That(backingIndex.IsUnique, Is.True);
        }
    }

    [Test]
    public async Task Indexes_WhenGivenTableWithSingleColumnIndex_ReturnsBTreeIndexType()
    {
        var table = await GetTableAsync("table_test_table_8");
        var index = table.Indexes.Single();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(index.IndexType, Is.EqualTo(IndexType.BTree));
            Assert.That(index.IsVisible, Is.True);
            Assert.That(index.Columns.Single().PrefixLength, OptionIs.None);
        }
    }

    [Test]
    public async Task Indexes_WhenGivenTableWithPrimaryKey_DoesNotIncludeItsBackingIndex()
    {
        var table = await GetTableAsync("table_test_table_3");

        Assert.That(table.Indexes, Is.Empty);
    }

    [Test]
    public async Task Indexes_WhenGivenTableWithDescendingIndexColumn_ReturnsDescendingOrder()
    {
        var table = await GetTableAsync("table_test_table_41");
        var index = table.Indexes.Single(i => i.Name.LocalName == "ix_test_table_41_1");

        Assert.That(index.Columns.Single().Order, Is.EqualTo(IndexColumnOrder.Descending));
    }

    [Test]
    public async Task Indexes_WhenGivenTableWithFunctionalIndex_ReturnsIndexWithExpressionColumn()
    {
        var table = await GetTableAsync("table_test_table_41");
        var index = table.Indexes.Single(i => i.Name.LocalName == "ix_test_table_41_2");
        var indexColumn = index.Columns.Single();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(indexColumn.Expression, Does.Contain("lower"));
            Assert.That(indexColumn.DependentColumns, Is.Empty);
        }
    }

    [Test]
    public async Task Indexes_WhenGivenTableWithPrefixIndex_ReturnsIndexColumnWithPrefixLength()
    {
        var table = await GetTableAsync("table_test_table_41");
        var index = table.Indexes.Single(i => i.Name.LocalName == "ix_test_table_41_3");

        Assert.That(index.Columns.Single().PrefixLength.UnwrapSome(), Is.EqualTo(10));
    }
}