using System.IO;
using System.Threading.Tasks;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Reporting.Html.Renderers;
using SJP.Schematic.Reporting.Serialization;
using SJP.Schematic.Tests.Utilities;

namespace SJP.Schematic.Reporting.Tests.Html.Renderers;

[TestFixture]
internal static class RoutinesRendererTests
{
    [Test]
    public static async Task RenderAsync_GivenRoutines_WritesSummaryFileWithExpectedCount()
    {
        using var tempDir = new TemporaryDirectory();
        var first = new DatabaseRoutine(new Identifier("routine_one"), "select 1");
        var second = new DatabaseRoutine(new Identifier("routine_two"), "select 2");

        var renderer = new RoutinesRenderer();
        var data = ReportDataFactory.Create(routines: [first, second]);
        var context = new RenderContext(new JsonDataWriter(), new BundleBuilder(), new DirectoryInfo(tempDir.DirectoryPath));
        await renderer.RenderAsync(data, context);

        var outputFile = Path.Combine(tempDir.DirectoryPath, "data", "routines.json");
        var content = await File.ReadAllTextAsync(outputFile);

        Assert.That(content, Does.Contain("\"routinesCount\":2"));
    }

    [Test]
    public static async Task RenderAsync_GivenRoutines_RegistersSummaryPayloadUnderRoutinesBundleKey()
    {
        using var tempDir = new TemporaryDirectory();
        var routine = new DatabaseRoutine(new Identifier("routine_one"), "select 1");
        var bundle = new BundleBuilder();

        var renderer = new RoutinesRenderer();
        var data = ReportDataFactory.Create(routines: [routine]);
        var context = new RenderContext(new JsonDataWriter(), bundle, new DirectoryInfo(tempDir.DirectoryPath));
        await renderer.RenderAsync(data, context);

        var bundleFile = new FileInfo(Path.Combine(tempDir.DirectoryPath, "bundle.js"));
        await bundle.WriteBundleAsync(bundleFile);
        var bundleContent = await File.ReadAllTextAsync(bundleFile.FullName);

        Assert.That(bundleContent, Does.Contain("window.__schematic[\"routines\"]"));
    }
}
