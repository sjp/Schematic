using System.IO;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using SJP.Schematic.Reporting.Serialization;
using SJP.Schematic.Tests.Utilities;

namespace SJP.Schematic.Reporting.Tests.Serialization;

[TestFixture]
internal static class BundleBuilderTests
{
    [Test]
    public static void AddSummary_GivenNullKey_ThrowsArgumentNullException()
    {
        var builder = new BundleBuilder();
        Assert.That(() => builder.AddSummary(null!, "{}"), Throws.ArgumentNullException);
    }

    [Test]
    public static void AddSummary_GivenEmptyKey_ThrowsArgumentException()
    {
        var builder = new BundleBuilder();
        Assert.That(() => builder.AddSummary(string.Empty, "{}"), Throws.ArgumentException);
    }

    [Test]
    public static void AddSummary_GivenNullJson_ThrowsArgumentNullException()
    {
        var builder = new BundleBuilder();
        Assert.That(() => builder.AddSummary("tables", null!), Throws.ArgumentNullException);
    }

    [Test]
    public static void AddDetail_GivenNullTypeKey_ThrowsArgumentNullException()
    {
        var builder = new BundleBuilder();
        Assert.That(() => builder.AddDetail(null!, "safe-key", "{}"), Throws.ArgumentNullException);
    }

    [Test]
    public static void AddDetail_GivenNullSafeKey_ThrowsArgumentNullException()
    {
        var builder = new BundleBuilder();
        Assert.That(() => builder.AddDetail("table", null!, "{}"), Throws.ArgumentNullException);
    }

    [Test]
    public static void AddDetail_GivenNullJson_ThrowsArgumentNullException()
    {
        var builder = new BundleBuilder();
        Assert.That(() => builder.AddDetail("table", "safe-key", null!), Throws.ArgumentNullException);
    }

    [Test]
    public static void WriteBundleAsync_GivenNullFile_ThrowsArgumentNullException()
    {
        var builder = new BundleBuilder();
        Assert.That(() => builder.WriteBundleAsync(null!), Throws.ArgumentNullException);
    }

    [Test]
    public static async Task WriteBundleAsync_GivenNoPayloads_WritesOnlyHeader()
    {
        using var tempDir = new TemporaryDirectory();
        var bundleFile = new FileInfo(Path.Combine(tempDir.DirectoryPath, "bundle.js"));

        var builder = new BundleBuilder();
        await builder.WriteBundleAsync(bundleFile);

        var content = await File.ReadAllTextAsync(bundleFile.FullName);
        Assert.That(content, Is.EqualTo("window.__schematic = window.__schematic || {};\n"));
    }

    [Test]
    public static async Task WriteBundleAsync_GivenSummaries_WritesEntriesOrderedByKey()
    {
        using var tempDir = new TemporaryDirectory();
        var bundleFile = new FileInfo(Path.Combine(tempDir.DirectoryPath, "bundle.js"));

        var builder = new BundleBuilder();
        builder.AddSummary("views", "[2]");
        builder.AddSummary("main", "[1]");

        await builder.WriteBundleAsync(bundleFile);
        var content = await File.ReadAllTextAsync(bundleFile.FullName);

        const string expected =
            "window.__schematic = window.__schematic || {};\n" +
            "window.__schematic[\"main\"] = [1];\n" +
            "window.__schematic[\"views\"] = [2];\n";

        Assert.That(content, Is.EqualTo(expected));
    }

    [Test]
    public static async Task WriteBundleAsync_GivenDetails_WritesNestedSubMapOrderedByKey()
    {
        using var tempDir = new TemporaryDirectory();
        var bundleFile = new FileInfo(Path.Combine(tempDir.DirectoryPath, "bundle.js"));

        var builder = new BundleBuilder();
        builder.AddDetail("table", "zeta-key", "{\"name\":\"zeta\"}");
        builder.AddDetail("table", "alpha-key", "{\"name\":\"alpha\"}");

        await builder.WriteBundleAsync(bundleFile);
        var content = await File.ReadAllTextAsync(bundleFile.FullName);

        const string expected =
            "window.__schematic = window.__schematic || {};\n" +
            "window.__schematic[\"table\"] = window.__schematic[\"table\"] || {};\n" +
            "window.__schematic[\"table\"][\"alpha-key\"] = {\"name\":\"alpha\"};\n" +
            "window.__schematic[\"table\"][\"zeta-key\"] = {\"name\":\"zeta\"};\n";

        Assert.That(content, Is.EqualTo(expected));
    }

    [Test]
    public static async Task WriteBundleAsync_GivenKeyRequiringEscaping_EncodesKeyAsJsonString()
    {
        using var tempDir = new TemporaryDirectory();
        var bundleFile = new FileInfo(Path.Combine(tempDir.DirectoryPath, "bundle.js"));

        var builder = new BundleBuilder();
        builder.AddSummary("weird\"key", "{}");

        await builder.WriteBundleAsync(bundleFile);
        var content = await File.ReadAllTextAsync(bundleFile.FullName);

        // System.Text.Json's default (HTML-safe) encoder escapes '"' as " rather than \" --
        // still a valid, unambiguous JS string literal.
        Assert.That(content, Does.Contain("window.__schematic[\"weird\\u0022key\"] = {};\n"));
    }

    [Test]
    public static async Task WriteBundleAsync_GivenMissingParentDirectory_CreatesDirectory()
    {
        using var tempDir = new TemporaryDirectory();
        var bundleFile = new FileInfo(Path.Combine(tempDir.DirectoryPath, "nested", "data", "bundle.js"));

        var builder = new BundleBuilder();
        await builder.WriteBundleAsync(bundleFile);

        Assert.That(File.Exists(bundleFile.FullName), Is.True);
    }

    [Test]
    public static async Task WriteBundleAsync_WritesFileAsUtf8WithoutBom()
    {
        using var tempDir = new TemporaryDirectory();
        var bundleFile = new FileInfo(Path.Combine(tempDir.DirectoryPath, "bundle.js"));

        var builder = new BundleBuilder();
        await builder.WriteBundleAsync(bundleFile);

        var bytes = await File.ReadAllBytesAsync(bundleFile.FullName);
        var bom = Encoding.UTF8.GetPreamble();

        var startsWithBom = bytes.Length >= bom.Length;
        for (var i = 0; startsWithBom && i < bom.Length; i++)
        {
            if (bytes[i] != bom[i])
                startsWithBom = false;
        }

        Assert.That(startsWithBom, Is.False);
    }
}
