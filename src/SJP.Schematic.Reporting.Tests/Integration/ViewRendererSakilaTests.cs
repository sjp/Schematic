using System.IO;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using SJP.Schematic.Reporting.Html.Renderers;
using SJP.Schematic.Reporting.Html.ViewModels.Mappers;
using SJP.Schematic.Reporting.Serialization;
using SJP.Schematic.Reporting.Tests.Html.Renderers;
using SJP.Schematic.Tests.Utilities;
using SJP.Schematic.Tests.Utilities.Integration;

namespace SJP.Schematic.Reporting.Tests.Integration;

internal sealed class ViewRendererSakilaTests : SakilaTest
{
    [Test]
    public async Task RenderAsync_GivenSakilaViews_WritesDetailFilePerViewUnderViewsSubdirectory()
    {
        using var tempDir = new TemporaryDirectory();
        var database = await GetSnapshotDatabaseAsync();
        var views = await database.GetAllViews();

        var renderer = new ViewRenderer();
        var data = ReportDataFactory.Create(views: views, referencedObjectTargets: EmptyTargets());
        var context = new RenderContext(new JsonDataWriter(), new BundleBuilder(), new DirectoryInfo(tempDir.DirectoryPath));
        await renderer.RenderAsync(data, context);

        var firstView = views.First();
        var expectedFile = Path.Combine(tempDir.DirectoryPath, "data", "views", firstView.Name.ToSafeKey() + ".json");

        Assert.That(File.Exists(expectedFile), Is.True);
    }

    [Test]
    public async Task RenderAsync_GivenSakilaViews_RegistersDetailPayloadPerViewUnderViewBundleKey()
    {
        using var tempDir = new TemporaryDirectory();
        var database = await GetSnapshotDatabaseAsync();
        var views = await database.GetAllViews();
        var bundle = new BundleBuilder();

        var renderer = new ViewRenderer();
        var data = ReportDataFactory.Create(views: views, referencedObjectTargets: EmptyTargets());
        var context = new RenderContext(new JsonDataWriter(), bundle, new DirectoryInfo(tempDir.DirectoryPath));
        await renderer.RenderAsync(data, context);

        var firstView = views.First();
        var bundleDirectory = new DirectoryInfo(Path.Combine(tempDir.DirectoryPath, "bundle"));
        await bundle.WriteBundleAsync(bundleDirectory);
        var bundleContent = await File.ReadAllTextAsync(Path.Combine(bundleDirectory.FullName, "view", firstView.Name.ToSafeKey() + ".js"));

        Assert.That(bundleContent, Does.Contain($"window.__schematic[\"view\"][\"{firstView.Name.ToSafeKey()}\"] = "));
    }

    private ReferencedObjectTargets EmptyTargets() => new(Connection.Dialect.GetDependencyProvider(), [], [], [], [], [], []);
}
