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
internal static class SynonymRendererTests
{
    [Test]
    public static void Ctor_GivenNullSynonyms_ThrowsArgumentNullException()
    {
        using var tempDir = new TemporaryDirectory();
        Assert.That(
            () => new SynonymRenderer(null!, EmptyTargets(), new JsonDataWriter(), new BundleBuilder(), new DirectoryInfo(tempDir.DirectoryPath)),
            Throws.ArgumentNullException);
    }

    [Test]
    public static void Ctor_GivenNullSynonymTargets_ThrowsArgumentNullException()
    {
        using var tempDir = new TemporaryDirectory();
        Assert.That(
            () => new SynonymRenderer([], null!, new JsonDataWriter(), new BundleBuilder(), new DirectoryInfo(tempDir.DirectoryPath)),
            Throws.ArgumentNullException);
    }

    [Test]
    public static void Ctor_GivenNullExportDirectory_ThrowsArgumentNullException()
    {
        Assert.That(
            () => new SynonymRenderer([], EmptyTargets(), new JsonDataWriter(), new BundleBuilder(), null!),
            Throws.ArgumentNullException);
    }

    [Test]
    public static async Task RenderAsync_GivenSynonym_WritesDetailFileUnderSynonymsSubdirectory()
    {
        using var tempDir = new TemporaryDirectory();
        var synonymName = new Identifier("test_synonym");
        var synonym = new DatabaseSynonym(synonymName, new Identifier("test_target"));

        var renderer = new SynonymRenderer([synonym], EmptyTargets(), new JsonDataWriter(), new BundleBuilder(), new DirectoryInfo(tempDir.DirectoryPath));
        await renderer.RenderAsync();

        var expectedFile = Path.Combine(tempDir.DirectoryPath, "data", "synonyms", synonymName.ToSafeKey() + ".json");
        Assert.That(File.Exists(expectedFile), Is.True);
    }

    [Test]
    public static async Task RenderAsync_GivenSynonymTargetingKnownTable_ResolvesTargetUrl()
    {
        using var tempDir = new TemporaryDirectory();
        var synonymName = new Identifier("test_synonym");
        Identifier targetTableName = "target_table";
        var synonym = new DatabaseSynonym(synonymName, targetTableName);
        var targets = new SynonymTargets([targetTableName], [], [], [], []);

        var renderer = new SynonymRenderer([synonym], targets, new JsonDataWriter(), new BundleBuilder(), new DirectoryInfo(tempDir.DirectoryPath));
        await renderer.RenderAsync();

        var outputFile = Path.Combine(tempDir.DirectoryPath, "data", "synonyms", synonymName.ToSafeKey() + ".json");
        var content = await File.ReadAllTextAsync(outputFile);

        Assert.That(content, Does.Contain("\"targetUrl\":\"" + UrlRouter.GetTableUrl(targetTableName) + "\""));
    }

    [Test]
    public static async Task RenderAsync_GivenSynonym_RegistersDetailPayloadUnderSynonymBundleKey()
    {
        using var tempDir = new TemporaryDirectory();
        var synonymName = new Identifier("test_synonym");
        var synonym = new DatabaseSynonym(synonymName, new Identifier("test_target"));
        var bundle = new BundleBuilder();

        var renderer = new SynonymRenderer([synonym], EmptyTargets(), new JsonDataWriter(), bundle, new DirectoryInfo(tempDir.DirectoryPath));
        await renderer.RenderAsync();

        var bundleFile = new FileInfo(Path.Combine(tempDir.DirectoryPath, "bundle.js"));
        await bundle.WriteBundleAsync(bundleFile);
        var bundleContent = await File.ReadAllTextAsync(bundleFile.FullName);

        Assert.That(bundleContent, Does.Contain($"window.__schematic[\"synonym\"][\"{synonymName.ToSafeKey()}\"]"));
    }

    private static SynonymTargets EmptyTargets() => new([], [], [], [], []);
}
