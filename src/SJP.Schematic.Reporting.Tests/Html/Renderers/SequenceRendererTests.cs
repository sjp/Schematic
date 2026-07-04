using System.Collections.Generic;
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
    public static void Ctor_GivenNullSequences_ThrowsArgumentNullException()
    {
        using var tempDir = new TemporaryDirectory();
        Assert.That(
            () => new SequenceRenderer(null!, new JsonDataWriter(), new BundleBuilder(), new DirectoryInfo(tempDir.DirectoryPath)),
            Throws.ArgumentNullException);
    }

    [Test]
    public static void Ctor_GivenNullJsonWriter_ThrowsArgumentNullException()
    {
        using var tempDir = new TemporaryDirectory();
        Assert.That(
            () => new SequenceRenderer([], null!, new BundleBuilder(), new DirectoryInfo(tempDir.DirectoryPath)),
            Throws.ArgumentNullException);
    }

    [Test]
    public static void Ctor_GivenNullBundle_ThrowsArgumentNullException()
    {
        using var tempDir = new TemporaryDirectory();
        Assert.That(
            () => new SequenceRenderer([], new JsonDataWriter(), null!, new DirectoryInfo(tempDir.DirectoryPath)),
            Throws.ArgumentNullException);
    }

    [Test]
    public static void Ctor_GivenNullExportDirectory_ThrowsArgumentNullException()
    {
        Assert.That(
            () => new SequenceRenderer([], new JsonDataWriter(), new BundleBuilder(), null!),
            Throws.ArgumentNullException);
    }

    [Test]
    public static async Task RenderAsync_GivenNoSequences_CompletesWithoutThrowing()
    {
        using var tempDir = new TemporaryDirectory();
        var renderer = new SequenceRenderer([], new JsonDataWriter(), new BundleBuilder(), new DirectoryInfo(tempDir.DirectoryPath));

        Assert.That(async () => await renderer.RenderAsync(), Throws.Nothing);
    }

    [Test]
    public static async Task RenderAsync_GivenSequence_WritesDetailFileUnderSequencesSubdirectory()
    {
        using var tempDir = new TemporaryDirectory();
        var sequenceName = new Identifier("test_sequence");
        var sequence = new DatabaseSequence(sequenceName, 1M, 1M, Option<decimal>.None, Option<decimal>.None, false, 0);

        var renderer = new SequenceRenderer([sequence], new JsonDataWriter(), new BundleBuilder(), new DirectoryInfo(tempDir.DirectoryPath));
        await renderer.RenderAsync();

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

        var renderer = new SequenceRenderer([sequence], new JsonDataWriter(), bundle, new DirectoryInfo(tempDir.DirectoryPath));
        await renderer.RenderAsync();

        var bundleFile = new FileInfo(Path.Combine(tempDir.DirectoryPath, "bundle.js"));
        await bundle.WriteBundleAsync(bundleFile);
        var bundleContent = await File.ReadAllTextAsync(bundleFile.FullName);

        Assert.That(bundleContent, Does.Contain($"window.__schematic[\"sequence\"][\"{sequenceName.ToSafeKey()}\"]"));
    }
}
