using System.IO;
using System.Threading.Tasks;
using NUnit.Framework;
using SJP.Schematic.Reporting.Html.Renderers;
using SJP.Schematic.Reporting.Serialization;
using SJP.Schematic.Reporting.Tests.Html.Renderers;
using SJP.Schematic.Tests.Utilities;
using SJP.Schematic.Tests.Utilities.Integration;

namespace SJP.Schematic.Reporting.Tests.Integration;

internal sealed class MainRendererSakilaTests : SakilaTest
{
    [Test]
    public async Task RenderAsync_GivenSakilaDatabase_WritesSummaryFileWithMatchingTableCount()
    {
        using var tempDir = new TemporaryDirectory();
        var database = GetDatabase();
        var tables = await database.GetAllTables();
        var views = await database.GetAllViews();

        var renderer = new MainRenderer();
        var data = ReportDataFactory.Create(database: database, tables: tables, views: views, databaseVersion: "1.0");
        var context = new RenderContext(new JsonDataWriter(), new BundleBuilder(), new DirectoryInfo(tempDir.DirectoryPath));
        await renderer.RenderAsync(data, context);

        var outputFile = Path.Combine(tempDir.DirectoryPath, "data", "main.json");
        var content = await File.ReadAllTextAsync(outputFile);

        Assert.That(content, Does.Contain($"\"tablesCount\":{tables.Count}"));
    }

    [Test]
    public async Task RenderAsync_GivenSakilaDatabase_RegistersSummaryPayloadUnderMainBundleKey()
    {
        using var tempDir = new TemporaryDirectory();
        var database = GetDatabase();
        var bundle = new BundleBuilder();

        var renderer = new MainRenderer();
        var data = ReportDataFactory.Create(database: database, databaseVersion: "1.0");
        var context = new RenderContext(new JsonDataWriter(), bundle, new DirectoryInfo(tempDir.DirectoryPath));
        await renderer.RenderAsync(data, context);

        var bundleFile = new FileInfo(Path.Combine(tempDir.DirectoryPath, "bundle.js"));
        await bundle.WriteBundleAsync(bundleFile);
        var bundleContent = await File.ReadAllTextAsync(bundleFile.FullName);

        Assert.That(bundleContent, Does.Contain("window.__schematic[\"main\"]"));
    }
}
