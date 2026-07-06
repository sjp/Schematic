using System.IO;
using System.Threading.Tasks;
using LanguageExt;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Reporting.Html.Renderers;
using SJP.Schematic.Reporting.Serialization;
using SJP.Schematic.Tests.Utilities;

namespace SJP.Schematic.Reporting.Tests.Html.Renderers;

[TestFixture]
internal static class SequenceRendererTests
{
    [Test]
    public static async Task RenderAsync_GivenNoSequences_CompletesWithoutThrowing()
    {
        using var tempDir = new TemporaryDirectory();
        var renderer = new SequenceRenderer();
        var data = ReportDataFactory.Create();
        var context = new RenderContext(new JsonDataWriter(), new BundleBuilder(), new DirectoryInfo(tempDir.DirectoryPath));

        Assert.That(async () => await renderer.RenderAsync(data, context), Throws.Nothing);
    }

    [Test]
    public static async Task RenderAsync_GivenSequence_WritesDetailFileUnderSequencesSubdirectory()
    {
        using var tempDir = new TemporaryDirectory();
        var sequenceName = new Identifier("test_sequence");
        var sequence = new DatabaseSequence(sequenceName, 1M, 1M, Option<decimal>.None, Option<decimal>.None, false, 0);

        var renderer = new SequenceRenderer();
        var data = ReportDataFactory.Create(sequences: [sequence]);
        var context = new RenderContext(new JsonDataWriter(), new BundleBuilder(), new DirectoryInfo(tempDir.DirectoryPath));
        await renderer.RenderAsync(data, context);

        var expectedFile = Path.Combine(tempDir.DirectoryPath, "data", "sequences", sequenceName.ToSafeKey() + ".json");
        Assert.That(File.Exists(expectedFile), Is.True);
    }

    [Test]
    public static async Task RenderAsync_GivenSequence_RegistersDetailPayloadUnderSequenceBundleKey()
    {
        using var tempDir = new TemporaryDirectory();
        var sequenceName = new Identifier("test_sequence");
        var sequence = new DatabaseSequence(sequenceName, 1M, 1M, Option<decimal>.None, Option<decimal>.None, false, 0);
        var bundle = new BundleBuilder();

        var renderer = new SequenceRenderer();
        var data = ReportDataFactory.Create(sequences: [sequence]);
        var context = new RenderContext(new JsonDataWriter(), bundle, new DirectoryInfo(tempDir.DirectoryPath));
        await renderer.RenderAsync(data, context);

        var bundleFile = new FileInfo(Path.Combine(tempDir.DirectoryPath, "bundle.js"));
        await bundle.WriteBundleAsync(bundleFile);
        var bundleContent = await File.ReadAllTextAsync(bundleFile.FullName);

        Assert.That(bundleContent, Does.Contain($"window.__schematic[\"sequence\"][\"{sequenceName.ToSafeKey()}\"]"));
    }
}
