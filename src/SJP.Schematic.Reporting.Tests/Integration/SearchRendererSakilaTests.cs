using System.IO;
using System.Threading.Tasks;
using NUnit.Framework;
using SJP.Schematic.Reporting.Html.Renderers;
using SJP.Schematic.Reporting.Serialization;
using SJP.Schematic.Tests.Utilities;
using SJP.Schematic.Tests.Utilities.Integration;

namespace SJP.Schematic.Reporting.Tests.Integration;

internal sealed class SearchRendererSakilaTests : SakilaTest
{
    [Test]
    public void Ctor_GivenNullTables_ThrowsArgumentNullException()
    {
        using var tempDir = new TemporaryDirectory();
        Assert.That(
            () => new SearchRenderer(null!, [], [], [], [], new JsonDataWriter(), new BundleBuilder(), new DirectoryInfo(tempDir.DirectoryPath)),
            Throws.ArgumentNullException);
    }

    [Test]
    public async Task RenderAsync_GivenSakilaTablesAndViews_IncludesTableAndColumnEntries()
    {
        using var tempDir = new TemporaryDirectory();
        var database = GetDatabase();
        var tables = await database.GetAllTables();
        var views = await database.GetAllViews();

        var renderer = new SearchRenderer(tables, views, [], [], [], new JsonDataWriter(), new BundleBuilder(), new DirectoryInfo(tempDir.DirectoryPath));
        await renderer.RenderAsync();

        var outputFile = Path.Combine(tempDir.DirectoryPath, "data", "search.json");
        var content = await File.ReadAllTextAsync(outputFile);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(content, Does.Contain("\"objectType\":\"Table\""));
            Assert.That(content, Does.Contain("\"objectType\":\"Column\""));
        }
    }

    [Test]
    public async Task RenderAsync_GivenSakilaTablesAndViews_RegistersSummaryPayloadUnderSearchBundleKey()
    {
        using var tempDir = new TemporaryDirectory();
        var database = GetDatabase();
        var tables = await database.GetAllTables();
        var bundle = new BundleBuilder();

        var renderer = new SearchRenderer(tables, [], [], [], [], new JsonDataWriter(), bundle, new DirectoryInfo(tempDir.DirectoryPath));
        await renderer.RenderAsync();

        var bundleFile = new FileInfo(Path.Combine(tempDir.DirectoryPath, "bundle.js"));
        await bundle.WriteBundleAsync(bundleFile);
        var bundleContent = await File.ReadAllTextAsync(bundleFile.FullName);

        Assert.That(bundleContent, Does.Contain("window.__schematic[\"search\"]"));
    }
}
