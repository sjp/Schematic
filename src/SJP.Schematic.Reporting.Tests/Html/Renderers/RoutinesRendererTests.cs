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
    public static void Ctor_GivenNullRoutines_ThrowsArgumentNullException()
    {
        using var tempDir = new TemporaryDirectory();
        Assert.That(
            () => new RoutinesRenderer(null!, new JsonDataWriter(), new BundleBuilder(), new DirectoryInfo(tempDir.DirectoryPath)),
            Throws.ArgumentNullException);
    }

    [Test]
    public static void Ctor_GivenNullExportDirectory_ThrowsArgumentNullException()
    {
        Assert.That(
            () => new RoutinesRenderer([], new JsonDataWriter(), new BundleBuilder(), null!),
            Throws.ArgumentNullException);
    }

    [Test]
    public static async Task RenderAsync_GivenRoutines_WritesSummaryFileWithExpectedCount()
    {
        using var tempDir = new TemporaryDirectory();
        var first = new DatabaseRoutine(new Identifier("routine_one"), "select 1");
        var second = new DatabaseRoutine(new Identifier("routine_two"), "select 2");

        var renderer = new RoutinesRenderer([first, second], new JsonDataWriter(), new BundleBuilder(), new DirectoryInfo(tempDir.DirectoryPath));
        await renderer.RenderAsync();

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

        var renderer = new RoutinesRenderer([routine], new JsonDataWriter(), bundle, new DirectoryInfo(tempDir.DirectoryPath));
        await renderer.RenderAsync();

        var bundleFile = new FileInfo(Path.Combine(tempDir.DirectoryPath, "bundle.js"));
        await bundle.WriteBundleAsync(bundleFile);
        var bundleContent = await File.ReadAllTextAsync(bundleFile.FullName);

        Assert.That(bundleContent, Does.Contain("window.__schematic[\"routines\"]"));
    }
}
