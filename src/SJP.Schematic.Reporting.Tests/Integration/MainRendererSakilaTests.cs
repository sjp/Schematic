using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Reporting.Html.Renderers;
using SJP.Schematic.Reporting.Serialization;
using SJP.Schematic.Tests.Utilities;
using SJP.Schematic.Tests.Utilities.Integration;

namespace SJP.Schematic.Reporting.Tests.Integration;

internal sealed class MainRendererSakilaTests : SakilaTest
{
    [Test]
    public void Ctor_GivenNullDatabase_ThrowsArgumentNullException()
    {
        using var tempDir = new TemporaryDirectory();
        Assert.That(
            () => new MainRenderer(null!, [], [], [], [], [], "1.0", new JsonDataWriter(), new BundleBuilder(), new DirectoryInfo(tempDir.DirectoryPath)),
            Throws.ArgumentNullException);
    }

    [Test]
    public void Ctor_GivenNullTables_ThrowsArgumentNullException()
    {
        using var tempDir = new TemporaryDirectory();
        var database = GetDatabase();
        Assert.That(
            () => new MainRenderer(database, null!, [], [], [], [], "1.0", new JsonDataWriter(), new BundleBuilder(), new DirectoryInfo(tempDir.DirectoryPath)),
            Throws.ArgumentNullException);
    }

    [Test]
    public async Task RenderAsync_GivenSakilaDatabase_WritesSummaryFileWithMatchingTableCount()
    {
        using var tempDir = new TemporaryDirectory();
        var database = GetDatabase();
        var tables = await database.GetAllTables();
        var views = await database.GetAllViews();

        var renderer = new MainRenderer(database, tables, views, [], [], [], "1.0", new JsonDataWriter(), new BundleBuilder(), new DirectoryInfo(tempDir.DirectoryPath));
        await renderer.RenderAsync();

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

        var renderer = new MainRenderer(database, [], [], [], [], [], "1.0", new JsonDataWriter(), bundle, new DirectoryInfo(tempDir.DirectoryPath));
        await renderer.RenderAsync();

        var bundleFile = new FileInfo(Path.Combine(tempDir.DirectoryPath, "bundle.js"));
        await bundle.WriteBundleAsync(bundleFile);
        var bundleContent = await File.ReadAllTextAsync(bundleFile.FullName);

        Assert.That(bundleContent, Does.Contain("window.__schematic[\"main\"]"));
    }
}
