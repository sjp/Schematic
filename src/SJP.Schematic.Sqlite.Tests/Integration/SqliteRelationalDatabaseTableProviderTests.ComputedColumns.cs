using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using SJP.Schematic.Core;

namespace SJP.Schematic.Sqlite.Tests.Integration;

internal sealed partial class SqliteRelationalDatabaseTableProviderTests : SqliteTest
{
    [Test]
    public async Task Columns_GivenTableWithComputedColumns_ReturnsColumnCollectionsWithExpectedCount()
    {
        var table = await GetTableAsync("table_test_table_37").ConfigureAwait(false);

        Assert.That(table.Columns, Has.Exactly(4).Items);
    }

    [Test]
    public async Task Columns_GivenTableWithComputedColumns_ReturnsExpectedNumberOfPhysicalColumns()
    {
        var table = await GetTableAsync("table_test_table_37").ConfigureAwait(false);

        var physicalColumns = table.Columns.Where(c => !c.IsComputed).ToList();

        Assert.That(physicalColumns, Has.Exactly(1).Items);
    }

    [Test]
    public async Task Columns_WhenGivenTableWithComputedColumns_ReturnsComputedColumnsWithCorrectNames()
    {
        var expectedColumnNames = new[] { "test_column_2", "test_column_3", "test_column_4" };
        var table = await GetTableAsync("table_test_table_37").ConfigureAwait(false);
        var columns = table.Columns;
        var columnNames = columns.Where(c => c.IsComputed).Select(c => c.Name.LocalName).ToList();

        var equalNames = columnNames.SequenceEqual(expectedColumnNames);
        Assert.That(equalNames, Is.True);
    }

    [Test]
    public async Task Columns_WhenGivenTableWithComputedColumns_ReturnsColumnsInCorrectOrder()
    {
        var expectedColumnNames = new[] { "test_column_1", "test_column_2", "test_column_3", "test_column_4" };
        var table = await GetTableAsync("table_test_table_37").ConfigureAwait(false);
        var columns = table.Columns;
        var columnNames = columns.Select(c => c.Name.LocalName).ToList();

        var equalNames = columnNames.SequenceEqual(expectedColumnNames);
        Assert.That(equalNames, Is.True);
    }

    [Test]
    public async Task Columns_WhenGivenTableWithComputedColumns_HasExpectedDefinitionsForComputedColumns()
    {
        var expectedDefinitions = new[]
        {
            "(test_column_1 * test_column_1)",
            "(test_column_1 * test_column_1 * test_column_1)",
            "(test_column_1 * test_column_1 * test_column_1 * test_column_1)"
        };
        var table = await GetTableAsync("table_test_table_37").ConfigureAwait(false);
        var columns = table.Columns;
        var definitions = columns.OfType<IDatabaseComputedColumn>()
            .Select(c => c.Definition)
            .SelectMany(x => x)
            .ToList();

        var equalDefinitions = definitions.SequenceEqual(expectedDefinitions);
        Assert.That(equalDefinitions, Is.True);
    }
}