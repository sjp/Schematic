using System.IO;
using System.Text.Json;
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
        var database = await GetSnapshotDatabaseAsync();
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
    public async Task RenderAsync_GivenSakilaTables_WritesConstraintsCountMatchingConstraintsPage()
    {
        using var tempDir = new TemporaryDirectory();
        var database = await GetSnapshotDatabaseAsync();
        var tables = await database.GetAllTables();

        var data = ReportDataFactory.Create(database: database, tables: tables, databaseVersion: "1.0");
        var context = new RenderContext(new JsonDataWriter(), new BundleBuilder(), new DirectoryInfo(tempDir.DirectoryPath));
        await new MainRenderer().RenderAsync(data, context);
        await new ConstraintsRenderer().RenderAsync(data, context);

        var main = await ReadJsonAsync(tempDir, "main.json");
        var constraints = await ReadJsonAsync(tempDir, "constraints.json");
        var listedConstraints = constraints.GetProperty("primaryKeys").GetArrayLength()
            + constraints.GetProperty("uniqueKeys").GetArrayLength()
            + constraints.GetProperty("foreignKeys").GetArrayLength()
            + constraints.GetProperty("checkConstraints").GetArrayLength();

        Assert.That(main.GetProperty("constraintsCount").GetInt32(), Is.EqualTo(listedConstraints));
    }

    [Test]
    public async Task RenderAsync_GivenSakilaTables_WritesIndexesCountMatchingIndexesPage()
    {
        using var tempDir = new TemporaryDirectory();
        var database = await GetSnapshotDatabaseAsync();
        var tables = await database.GetAllTables();

        var data = ReportDataFactory.Create(database: database, tables: tables, databaseVersion: "1.0");
        var context = new RenderContext(new JsonDataWriter(), new BundleBuilder(), new DirectoryInfo(tempDir.DirectoryPath));
        await new MainRenderer().RenderAsync(data, context);
        await new IndexesRenderer().RenderAsync(data, context);

        var main = await ReadJsonAsync(tempDir, "main.json");
        var indexes = await ReadJsonAsync(tempDir, "indexes.json");

        Assert.That(main.GetProperty("indexesCount").GetInt32(), Is.EqualTo(indexes.GetProperty("tableIndexes").GetArrayLength()));
    }

    [Test]
    public async Task RenderAsync_GivenSakilaDatabase_RegistersSummaryPayloadUnderMainBundleKey()
    {
        using var tempDir = new TemporaryDirectory();
        var database = await GetSnapshotDatabaseAsync();
        var bundle = new BundleBuilder();

        var renderer = new MainRenderer();
        var data = ReportDataFactory.Create(database: database, databaseVersion: "1.0");
        var context = new RenderContext(new JsonDataWriter(), bundle, new DirectoryInfo(tempDir.DirectoryPath));
        await renderer.RenderAsync(data, context);

        var bundleDirectory = new DirectoryInfo(Path.Combine(tempDir.DirectoryPath, "bundle"));
        await bundle.WriteBundleAsync(bundleDirectory);
        var bundleContent = await File.ReadAllTextAsync(Path.Combine(bundleDirectory.FullName, "main.js"));

        Assert.That(bundleContent, Does.Contain("window.__schematic[\"main\"] = "));
    }

    private static async Task<JsonElement> ReadJsonAsync(TemporaryDirectory tempDir, string fileName)
    {
        var outputFile = Path.Combine(tempDir.DirectoryPath, "data", fileName);
        await using var stream = File.OpenRead(outputFile);
        using var document = await JsonDocument.ParseAsync(stream);
        return document.RootElement.Clone();
    }
}
