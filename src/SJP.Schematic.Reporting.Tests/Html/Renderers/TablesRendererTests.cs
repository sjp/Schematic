using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using LanguageExt;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Reporting.Html.Renderers;
using SJP.Schematic.Reporting.Serialization;
using SJP.Schematic.Tests.Utilities;

namespace SJP.Schematic.Reporting.Tests.Html.Renderers;

[TestFixture]
internal static class TablesRendererTests
{
    [Test]
    public static async Task RenderAsync_GivenTableStatistics_WritesRowCountForTheTable()
    {
        using var tempDir = new TemporaryDirectory();
        var table = CreateTable("test_table");
        var statistics = new Dictionary<Identifier, ITableStatistics>
        {
            [table.Name] = new TableStatistics(table.Name, Option<long>.Some(1234), false, Option<long>.None, Option<long>.None),
        };

        var content = await RenderTablesAsync(tempDir, [table], statistics);

        Assert.That(content, Does.Contain("\"rowCount\":1234"));
    }

    [Test]
    public static async Task RenderAsync_GivenNoTableStatistics_WritesNoRowCount()
    {
        using var tempDir = new TemporaryDirectory();
        var table = CreateTable("test_table");

        var content = await RenderTablesAsync(tempDir, [table], new Dictionary<Identifier, ITableStatistics>());

        Assert.That(content, Does.Not.Contain("rowCount"));
    }

    private static async Task<string> RenderTablesAsync(
        TemporaryDirectory tempDir,
        IReadOnlyCollection<IRelationalDatabaseTable> tables,
        IReadOnlyDictionary<Identifier, ITableStatistics> statistics
    )
    {
        var renderer = new TablesRenderer();
        var data = ReportDataFactory.Create(tables: tables, tableStatistics: statistics);
        var context = new RenderContext(new JsonDataWriter(), new BundleBuilder(), new DirectoryInfo(tempDir.DirectoryPath));
        await renderer.RenderAsync(data, context);

        var outputFile = Path.Combine(tempDir.DirectoryPath, "data", "tables.json");
        return await File.ReadAllTextAsync(outputFile);
    }

    private static IRelationalDatabaseTable CreateTable(Identifier tableName)
    {
        return new RelationalDatabaseTable(
            tableName,
            [],
            Option<IDatabaseKey>.None,
            [],
            [],
            [],
            [],
            [],
            [],
            TableKind.Regular,
            Option<ITablePartitioning>.None,
            Option<ITableSystemVersioning>.None,
            true,
            Option<Identifier>.None
        );
    }
}
