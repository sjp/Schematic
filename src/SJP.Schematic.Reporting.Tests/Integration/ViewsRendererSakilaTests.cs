using System.IO;
using System.Threading.Tasks;
using NUnit.Framework;
using SJP.Schematic.Reporting.Html.Renderers;
using SJP.Schematic.Reporting.Serialization;
using SJP.Schematic.Tests.Utilities;
using SJP.Schematic.Tests.Utilities.Integration;

namespace SJP.Schematic.Reporting.Tests.Integration;

internal sealed class ViewsRendererSakilaTests : SakilaTest
{
    [Test]
    public void Ctor_GivenNullViews_ThrowsArgumentNullException()
    {
        using var tempDir = new TemporaryDirectory();
        Assert.That(
            () => new ViewsRenderer(null!, new JsonDataWriter(), new BundleBuilder(), new DirectoryInfo(tempDir.DirectoryPath)),
            Throws.ArgumentNullException);
    }

    [Test]
    public async Task RenderAsync_GivenSakilaViews_WritesSummaryFileWithMatchingViewCount()
    {
        using var tempDir = new TemporaryDirectory();
        var database = GetDatabase();
        var views = await database.GetAllViews();

        var renderer = new ViewsRenderer(views, new JsonDataWriter(), new BundleBuilder(), new DirectoryInfo(tempDir.DirectoryPath));
        await renderer.RenderAsync();

        var outputFile = Path.Combine(tempDir.DirectoryPath, "data", "views.json");
        var content = await File.ReadAllTextAsync(outputFile);

        Assert.That(content, Does.Contain($"\"viewsCount\":{views.Count}"));
    }

    [Test]
    public async Task RenderAsync_GivenSakilaViews_RegistersSummaryPayloadUnderViewsBundleKey()
    {
        using var tempDir = new TemporaryDirectory();
        var database = GetDatabase();
        var views = await database.GetAllViews();
        var bundle = new BundleBuilder();

        var renderer = new ViewsRenderer(views, new JsonDataWriter(), bundle, new DirectoryInfo(tempDir.DirectoryPath));
        await renderer.RenderAsync();

        var bundleFile = new FileInfo(Path.Combine(tempDir.DirectoryPath, "bundle.js"));
        await bundle.WriteBundleAsync(bundleFile);
        var bundleContent = await File.ReadAllTextAsync(bundleFile.FullName);

        Assert.That(bundleContent, Does.Contain("window.__schematic[\"views\"]"));
    }
}
