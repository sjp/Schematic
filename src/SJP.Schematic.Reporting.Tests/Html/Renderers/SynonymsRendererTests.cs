using System.IO;
using System.Threading.Tasks;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Reporting.Html.Renderers;
using SJP.Schematic.Reporting.Html.ViewModels.Mappers;
using SJP.Schematic.Reporting.Serialization;
using SJP.Schematic.Tests.Utilities;

namespace SJP.Schematic.Reporting.Tests.Html.Renderers;

[TestFixture]
internal static class SynonymsRendererTests
{
    [Test]
    public static void Ctor_GivenNullSynonyms_ThrowsArgumentNullException()
    {
        using var tempDir = new TemporaryDirectory();
        Assert.That(
            () => new SynonymsRenderer(null!, EmptyTargets(), new JsonDataWriter(), new BundleBuilder(), new DirectoryInfo(tempDir.DirectoryPath)),
            Throws.ArgumentNullException);
    }

    [Test]
    public static void Ctor_GivenNullSynonymTargets_ThrowsArgumentNullException()
    {
        using var tempDir = new TemporaryDirectory();
        Assert.That(
            () => new SynonymsRenderer([], null!, new JsonDataWriter(), new BundleBuilder(), new DirectoryInfo(tempDir.DirectoryPath)),
            Throws.ArgumentNullException);
    }

    [Test]
    public static void Ctor_GivenNullExportDirectory_ThrowsArgumentNullException()
    {
        Assert.That(
            () => new SynonymsRenderer([], EmptyTargets(), new JsonDataWriter(), new BundleBuilder(), null!),
            Throws.ArgumentNullException);
    }

    [Test]
    public static async Task RenderAsync_GivenSynonyms_WritesSummaryFileWithExpectedCount()
    {
        using var tempDir = new TemporaryDirectory();
        var first = new DatabaseSynonym(new Identifier("syn_one"), new Identifier("target_one"));
        var second = new DatabaseSynonym(new Identifier("syn_two"), new Identifier("target_two"));

        var renderer = new SynonymsRenderer([first, second], EmptyTargets(), new JsonDataWriter(), new BundleBuilder(), new DirectoryInfo(tempDir.DirectoryPath));
        await renderer.RenderAsync();

        var outputFile = Path.Combine(tempDir.DirectoryPath, "data", "synonyms.json");
        var content = await File.ReadAllTextAsync(outputFile);

        Assert.That(content, Does.Contain("\"synonymsCount\":2"));
    }

    [Test]
    public static async Task RenderAsync_GivenSynonyms_RegistersSummaryPayloadUnderSynonymsBundleKey()
    {
        using var tempDir = new TemporaryDirectory();
        var synonym = new DatabaseSynonym(new Identifier("syn_one"), new Identifier("target_one"));
        var bundle = new BundleBuilder();

        var renderer = new SynonymsRenderer([synonym], EmptyTargets(), new JsonDataWriter(), bundle, new DirectoryInfo(tempDir.DirectoryPath));
        await renderer.RenderAsync();

        var bundleFile = new FileInfo(Path.Combine(tempDir.DirectoryPath, "bundle.js"));
        await bundle.WriteBundleAsync(bundleFile);
        var bundleContent = await File.ReadAllTextAsync(bundleFile.FullName);

        Assert.That(bundleContent, Does.Contain("window.__schematic[\"synonyms\"]"));
    }

    private static SynonymTargets EmptyTargets() => new([], [], [], [], []);
}
