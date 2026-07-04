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
internal static class SequencesRendererTests
{
    [Test]
    public static void Ctor_GivenNullSequences_ThrowsArgumentNullException()
    {
        using var tempDir = new TemporaryDirectory();
        Assert.That(
            () => new SequencesRenderer(null!, new JsonDataWriter(), new BundleBuilder(), new DirectoryInfo(tempDir.DirectoryPath)),
            Throws.ArgumentNullException);
    }

    [Test]
    public static void Ctor_GivenNullExportDirectory_ThrowsArgumentNullException()
    {
        Assert.That(
            () => new SequencesRenderer([], new JsonDataWriter(), new BundleBuilder(), null!),
            Throws.ArgumentNullException);
    }

    [Test]
    public static async Task RenderAsync_GivenSequences_WritesSummaryFileWithExpectedCount()
    {
        using var tempDir = new TemporaryDirectory();
        var first = new DatabaseSequence(new Identifier("seq_one"), 1M, 1M, Option<decimal>.None, Option<decimal>.None, false, 0);
        var second = new DatabaseSequence(new Identifier("seq_two"), 1M, 1M, Option<decimal>.None, Option<decimal>.None, false, 0);

        var renderer = new SequencesRenderer([first, second], new JsonDataWriter(), new BundleBuilder(), new DirectoryInfo(tempDir.DirectoryPath));
        await renderer.RenderAsync();

        var outputFile = Path.Combine(tempDir.DirectoryPath, "data", "sequences.json");
        var content = await File.ReadAllTextAsync(outputFile);

        Assert.That(content, Does.Contain("\"sequencesCount\":2"));
    }

    [Test]
    public static async Task RenderAsync_GivenSequences_RegistersSummaryPayloadUnderSequencesBundleKey()
    {
        using var tempDir = new TemporaryDirectory();
        var sequence = new DatabaseSequence(new Identifier("seq_one"), 1M, 1M, Option<decimal>.None, Option<decimal>.None, false, 0);
        var bundle = new BundleBuilder();

        var renderer = new SequencesRenderer([sequence], new JsonDataWriter(), bundle, new DirectoryInfo(tempDir.DirectoryPath));
        await renderer.RenderAsync();

        var bundleFile = new FileInfo(Path.Combine(tempDir.DirectoryPath, "bundle.js"));
        await bundle.WriteBundleAsync(bundleFile);
        var bundleContent = await File.ReadAllTextAsync(bundleFile.FullName);

        Assert.That(bundleContent, Does.Contain("window.__schematic[\"sequences\"]"));
    }
}
