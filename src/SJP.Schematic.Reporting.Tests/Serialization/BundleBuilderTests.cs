using System.IO;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using SJP.Schematic.Reporting.Serialization;
using SJP.Schematic.Tests.Utilities;

namespace SJP.Schematic.Reporting.Tests.Serialization;

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
        Assert.That(() => builder.AddSummary("tables", (string)null!), Throws.ArgumentNullException);
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
        Assert.That(() => builder.AddDetail("table", "safe-key", (string)null!), Throws.ArgumentNullException);
    }

    [Test]
    public static void AddSummary_GivenNullKeyForFile_ThrowsArgumentNullException()
    {
        var builder = new BundleBuilder();
        Assert.That(() => builder.AddSummary(null!, new FileInfo("tables.json")), Throws.ArgumentNullException);
    }

    [Test]
    public static void AddSummary_GivenEmptyKeyForFile_ThrowsArgumentException()
    {
        var builder = new BundleBuilder();
        Assert.That(() => builder.AddSummary(string.Empty, new FileInfo("tables.json")), Throws.ArgumentException);
    }

    [Test]
    public static void AddSummary_GivenNullJsonFile_ThrowsArgumentNullException()
    {
        var builder = new BundleBuilder();
        Assert.That(() => builder.AddSummary("tables", (FileInfo)null!), Throws.ArgumentNullException);
    }

    [Test]
    public static void AddDetail_GivenNullTypeKeyForFile_ThrowsArgumentNullException()
    {
        var builder = new BundleBuilder();
        Assert.That(() => builder.AddDetail(null!, "safe-key", new FileInfo("table.json")), Throws.ArgumentNullException);
    }

    [Test]
    public static void AddDetail_GivenEmptySafeKeyForFile_ThrowsArgumentException()
    {
        var builder = new BundleBuilder();
        Assert.That(() => builder.AddDetail("table", string.Empty, new FileInfo("table.json")), Throws.ArgumentException);
    }

    [Test]
    public static void AddDetail_GivenNullJsonFile_ThrowsArgumentNullException()
    {
        var builder = new BundleBuilder();
        Assert.That(() => builder.AddDetail("table", "safe-key", (FileInfo)null!), Throws.ArgumentNullException);
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
    public static async Task WriteBundleAsync_GivenJsonFiles_CopiesFileBytesVerbatim()
    {
        using var tempDir = new TemporaryDirectory();
        var bundleFile = new FileInfo(Path.Combine(tempDir.DirectoryPath, "bundle.js"));

        // Non-ASCII text checks the bytes are copied as-is rather than decoded and re-encoded.
        var summaryFile = new FileInfo(Path.Combine(tempDir.DirectoryPath, "main.json"));
        var summaryBytes = "{\"name\":\"café ☕\"}"u8.ToArray();
        await File.WriteAllBytesAsync(summaryFile.FullName, summaryBytes);

        var detailFile = new FileInfo(Path.Combine(tempDir.DirectoryPath, "tables", "actor.json"));
        detailFile.Directory!.Create();
        var detailBytes = "{\"name\":\"actor\"}"u8.ToArray();
        await File.WriteAllBytesAsync(detailFile.FullName, detailBytes);

        var builder = new BundleBuilder();
        builder.AddSummary("main", summaryFile);
        builder.AddDetail("table", "actor-key", detailFile);

        await builder.WriteBundleAsync(bundleFile);
        var bytes = await File.ReadAllBytesAsync(bundleFile.FullName);

        byte[] expected =
        [
            .. "window.__schematic = window.__schematic || {};\n"u8,
            .. "window.__schematic[\"main\"] = "u8, .. summaryBytes, .. ";\n"u8,
            .. "window.__schematic[\"table\"] = window.__schematic[\"table\"] || {};\n"u8,
            .. "window.__schematic[\"table\"][\"actor-key\"] = "u8, .. detailBytes, .. ";\n"u8,
        ];

        Assert.That(bytes, Is.EqualTo(expected));
    }

    [Test]
    public static async Task WriteBundleAsync_GivenJsonFileRegisteredBeforeItIsWritten_UsesContentAtBundleTime()
    {
        using var tempDir = new TemporaryDirectory();
        var bundleFile = new FileInfo(Path.Combine(tempDir.DirectoryPath, "bundle.js"));
        var jsonFile = new FileInfo(Path.Combine(tempDir.DirectoryPath, "main.json"));

        var builder = new BundleBuilder();
        builder.AddSummary("main", jsonFile);
        await File.WriteAllTextAsync(jsonFile.FullName, "[1]");

        await builder.WriteBundleAsync(bundleFile);
        var content = await File.ReadAllTextAsync(bundleFile.FullName);

        Assert.That(content, Does.EndWith("window.__schematic[\"main\"] = [1];\n"));
    }

    [Test]
    public static async Task WriteBundleAsync_GivenInlineAndFilePayloads_WritesEntriesOrderedByKey()
    {
        using var tempDir = new TemporaryDirectory();
        var bundleFile = new FileInfo(Path.Combine(tempDir.DirectoryPath, "bundle.js"));
        var viewsFile = new FileInfo(Path.Combine(tempDir.DirectoryPath, "views.json"));
        await File.WriteAllTextAsync(viewsFile.FullName, "[2]");

        var builder = new BundleBuilder();
        builder.AddSummary("views", viewsFile);
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
    public static async Task WriteBundleAsync_GivenSameKeyRegisteredTwice_WritesLatestPayload()
    {
        using var tempDir = new TemporaryDirectory();
        var bundleFile = new FileInfo(Path.Combine(tempDir.DirectoryPath, "bundle.js"));
        var jsonFile = new FileInfo(Path.Combine(tempDir.DirectoryPath, "table.json"));
        await File.WriteAllTextAsync(jsonFile.FullName, "{\"from\":\"file\"}");

        var builder = new BundleBuilder();
        builder.AddDetail("table", "actor-key", "{\"from\":\"string\"}");
        builder.AddDetail("table", "actor-key", jsonFile);

        await builder.WriteBundleAsync(bundleFile);
        var content = await File.ReadAllTextAsync(bundleFile.FullName);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(content, Does.Contain("window.__schematic[\"table\"][\"actor-key\"] = {\"from\":\"file\"};\n"));
            Assert.That(content, Does.Not.Contain("string"));
        }
    }

    [Test]
    public static void WriteBundleAsync_GivenMissingJsonFile_ThrowsFileNotFoundException()
    {
        using var tempDir = new TemporaryDirectory();
        var bundleFile = new FileInfo(Path.Combine(tempDir.DirectoryPath, "bundle.js"));

        var builder = new BundleBuilder();
        builder.AddSummary("main", new FileInfo(Path.Combine(tempDir.DirectoryPath, "missing.json")));

        Assert.That(() => builder.WriteBundleAsync(bundleFile), Throws.InstanceOf<FileNotFoundException>());
    }

    [Test]
    public static async Task WriteBundleAsync_GivenExistingLongerBundle_ReplacesContent()
    {
        using var tempDir = new TemporaryDirectory();
        var bundleFile = new FileInfo(Path.Combine(tempDir.DirectoryPath, "bundle.js"));
        await File.WriteAllTextAsync(bundleFile.FullName, new string('x', 10_000));

        var builder = new BundleBuilder();
        await builder.WriteBundleAsync(bundleFile);

        var content = await File.ReadAllTextAsync(bundleFile.FullName);
        Assert.That(content, Is.EqualTo("window.__schematic = window.__schematic || {};\n"));
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
