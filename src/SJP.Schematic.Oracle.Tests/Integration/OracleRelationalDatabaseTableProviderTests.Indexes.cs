using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;

namespace SJP.Schematic.Oracle.Tests.Integration;

internal sealed partial class OracleRelationalDatabaseTableProviderTests : OracleTest
{
    [Test]
    public async Task Indexes_WhenGivenTableWithNoIndexes_ReturnsEmptyCollection()
    {
        var table = await GetTableAsync("table_test_table_1").ConfigureAwait(false);

        Assert.That(table.Indexes, Is.Empty);
    }

    [Test]
    public async Task Indexes_WhenGivenTableWithSingleColumnIndex_ReturnsIndexWithColumnOnly()
    {
        var table = await GetTableAsync("table_test_table_8").ConfigureAwait(false);
        var index = table.Indexes.Single();
        var indexColumns = index.Columns
            .Select(c => c.DependentColumns.Single())
            .ToList();

        Assert.Multiple(() =>
        {
            Assert.That(indexColumns, Has.Exactly(1).Items);
            Assert.That(indexColumns.Single().Name.LocalName, Is.EqualTo("TEST_COLUMN"));
        });
    }

    [Test]
    public async Task Indexes_WhenGivenTableWithSingleColumnIndex_ReturnsIndexWithCorrectName()
    {
        var table = await GetTableAsync("table_test_table_8").ConfigureAwait(false);
        var index = table.Indexes.Single();

        Assert.That(index.Name.LocalName, Is.EqualTo("IX_TEST_TABLE_8"));
    }

    [Test]
    public async Task Indexes_WhenGivenTableWithMultiColumnIndex_ReturnsIndexWithColumnsInCorrectOrder()
    {
        var expectedColumnNames = new[] { "FIRST_NAME", "LAST_NAME", "MIDDLE_NAME" };

        var table = await GetTableAsync("table_test_table_9").ConfigureAwait(false);
        var index = table.Indexes.Single();
        var indexColumns = index.Columns
            .Select(c => c.DependentColumns.Single())
            .Select(c => c.Name.LocalName)
            .ToList();

        Assert.Multiple(() =>
        {
            Assert.That(indexColumns, Has.Exactly(3).Items);
            Assert.That(indexColumns, Is.EqualTo(expectedColumnNames));
        });
    }

    [Test]
    public async Task Indexes_WhenGivenTableWithMultiColumnIndex_ReturnsIndexWithCorrectName()
    {
        var table = await GetTableAsync("table_test_table_9").ConfigureAwait(false);
        var index = table.Indexes.Single();

        Assert.That(index.Name.LocalName, Is.EqualTo("IX_TEST_TABLE_9"));
    }

    [Test]
    public async Task Indexes_WhenGivenTableWithIndexContainingNoIncludedColumns_ReturnsIndexWithoutIncludedColumns()
    {
        var table = await GetTableAsync("table_test_table_9").ConfigureAwait(false);
        var index = table.Indexes.Single();

        Assert.That(index.IncludedColumns, Is.Empty);
    }

    [Test]
    public async Task Indexes_WhenGivenTableWithEnabledIndex_ReturnsIndexWithIsEnabledTrue()
    {
        var table = await GetTableAsync("table_test_table_9").ConfigureAwait(false);
        var index = table.Indexes.Single();

        Assert.That(index.IsEnabled, Is.True);
    }

    [Test]
    public async Task Indexes_WhenGivenTableWithNonUniqueIndex_ReturnsIndexWithIsUniqueFalse()
    {
        var table = await GetTableAsync("table_test_table_9").ConfigureAwait(false);
        var index = table.Indexes.Single();

        Assert.That(index.IsUnique, Is.False);
    }

    [Test]
    public async Task Indexes_WhenGivenTableWithUniqueIndex_ReturnsIndexWithIsUniqueTrue()
    {
        var table = await GetTableAsync("table_test_table_13").ConfigureAwait(false);
        var index = table.Indexes.Single();

        Assert.That(index.IsUnique, Is.True);
    }
}