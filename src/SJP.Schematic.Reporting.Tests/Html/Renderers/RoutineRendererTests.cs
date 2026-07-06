using System.IO;
using System.Threading.Tasks;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Reporting.Html.Renderers;
using SJP.Schematic.Reporting.Serialization;
using SJP.Schematic.Tests.Utilities;

namespace SJP.Schematic.Reporting.Tests.Html.Renderers;

[TestFixture]
internal static class RoutineRendererTests
{
    [Test]
    public static async Task RenderAsync_GivenRoutine_WritesDetailFileUnderRoutinesSubdirectory()
    {
        using var tempDir = new TemporaryDirectory();
        var routineName = new Identifier("test_routine");
        var routine = new DatabaseRoutine(routineName, "select 1");

        var renderer = new RoutineRenderer();
        var data = ReportDataFactory.Create(routines: [routine]);
        var context = new RenderContext(new JsonDataWriter(), new BundleBuilder(), new DirectoryInfo(tempDir.DirectoryPath));
        await renderer.RenderAsync(data, context);

        var expectedFile = Path.Combine(tempDir.DirectoryPath, "data", "routines", routineName.ToSafeKey() + ".json");
        Assert.That(File.Exists(expectedFile), Is.True);
    }

    [Test]
    public static async Task RenderAsync_GivenRoutine_RegistersDetailPayloadUnderRoutineBundleKey()
    {
        using var tempDir = new TemporaryDirectory();
        var routineName = new Identifier("test_routine");
        var routine = new DatabaseRoutine(routineName, "select 1");
        var bundle = new BundleBuilder();

        var renderer = new RoutineRenderer();
        var data = ReportDataFactory.Create(routines: [routine]);
        var context = new RenderContext(new JsonDataWriter(), bundle, new DirectoryInfo(tempDir.DirectoryPath));
        await renderer.RenderAsync(data, context);

        var bundleFile = new FileInfo(Path.Combine(tempDir.DirectoryPath, "bundle.js"));
        await bundle.WriteBundleAsync(bundleFile);
        var bundleContent = await File.ReadAllTextAsync(bundleFile.FullName);

        Assert.That(bundleContent, Does.Contain($"window.__schematic[\"routine\"][\"{routineName.ToSafeKey()}\"]"));
    }
}
