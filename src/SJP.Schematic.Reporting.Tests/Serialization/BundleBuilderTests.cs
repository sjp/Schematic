using System;
using System.IO;
using System.Linq;
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
        Assert.That(
            () => builder.AddSummary(null!, "{}"),
            Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("key"));
    }

    [Test]
    public static void AddSummary_GivenEmptyKey_ThrowsArgumentException()
    {
        var builder = new BundleBuilder();
        Assert.That(
            () => builder.AddSummary(string.Empty, "{}"),
            Throws.ArgumentException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("key"));
    }

    [Test]
    public static void AddSummary_GivenNullJson_ThrowsArgumentNullException()
    {
        var builder = new BundleBuilder();
        Assert.That(
            () => builder.AddSummary("tables", (string)null!),
            Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("json"));
    }

    [Test]
    public static void AddDetail_GivenNullTypeKey_ThrowsArgumentNullException()
    {
        var builder = new BundleBuilder();
        Assert.That(
            () => builder.AddDetail(null!, "safe-key", "{}"),
            Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("typeKey"));
    }

    [Test]
    public static void AddDetail_GivenNullSafeKey_ThrowsArgumentNullException()
    {
        var builder = new BundleBuilder();
        Assert.That(
            () => builder.AddDetail("table", null!, "{}"),
            Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("safeKey"));
    }

    [Test]
    public static void AddDetail_GivenNullJson_ThrowsArgumentNullException()
    {
        var builder = new BundleBuilder();
        Assert.That(
            () => builder.AddDetail("table", "safe-key", (string)null!),
            Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("json"));
    }

    [Test]
    public static void AddSummary_GivenNullKeyForFile_ThrowsArgumentNullException()
    {
        var builder = new BundleBuilder();
        Assert.That(
            () => builder.AddSummary(null!, new FileInfo("tables.json")),
            Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("key"));
    }

    [Test]
    public static void AddSummary_GivenEmptyKeyForFile_ThrowsArgumentException()
    {
        var builder = new BundleBuilder();
        Assert.That(
            () => builder.AddSummary(string.Empty, new FileInfo("tables.json")),
            Throws.ArgumentException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("key"));
    }

    [Test]
    public static void AddSummary_GivenNullJsonFile_ThrowsArgumentNullException()
    {
        var builder = new BundleBuilder();
        Assert.That(
            () => builder.AddSummary("tables", (FileInfo)null!),
            Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("jsonFile"));
    }

    [Test]
    public static void AddDetail_GivenNullTypeKeyForFile_ThrowsArgumentNullException()
    {
        var builder = new BundleBuilder();
        Assert.That(
            () => builder.AddDetail(null!, "safe-key", new FileInfo("table.json")),
            Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("typeKey"));
    }

    [Test]
    public static void AddDetail_GivenEmptySafeKeyForFile_ThrowsArgumentException()
    {
        var builder = new BundleBuilder();
        Assert.That(
            () => builder.AddDetail("table", string.Empty, new FileInfo("table.json")),
            Throws.ArgumentException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("safeKey"));
    }

    [Test]
    public static void AddDetail_GivenNullJsonFile_ThrowsArgumentNullException()
    {
        var builder = new BundleBuilder();
        Assert.That(
            () => builder.AddDetail("table", "safe-key", (FileInfo)null!),
            Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("jsonFile"));
    }

    [TestCase("..")]
    [TestCase(".")]
    [TestCase("nested/key")]
    [TestCase("nested\\key")]
    [TestCase("nul\0key")]
    public static void AddSummary_GivenKeyThatIsNotAFileName_ThrowsArgumentException(string key)
    {
        var builder = new BundleBuilder();
        Assert.That(
            () => builder.AddSummary(key, "{}"),
            Throws.ArgumentException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("key"));
    }

    [Test]
    public static void AddDetail_GivenTypeKeyThatIsNotAFileName_ThrowsArgumentException()
    {
        var builder = new BundleBuilder();
        Assert.That(
            () => builder.AddDetail("..", "safe-key", new FileInfo("table.json")),
            Throws.ArgumentException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("typeKey"));
    }

    [Test]
    public static void AddDetail_GivenSafeKeyThatIsNotAFileName_ThrowsArgumentException()
    {
        var builder = new BundleBuilder();
        Assert.That(
            () => builder.AddDetail("table", "../escape", "{}"),
            Throws.ArgumentException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("safeKey"));
    }

    [Test]
    public static void WriteBundleAsync_GivenNullDirectory_ThrowsArgumentNullException()
    {
        var builder = new BundleBuilder();
        Assert.That(
            () => builder.WriteBundleAsync(null!),
            Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("bundleDirectory"));
    }

    [Test]
    public static async Task WriteBundleAsync_GivenNoPayloads_CreatesEmptyDirectory()
    {
        using var tempDir = new TemporaryDirectory();
        var bundleDirectory = new DirectoryInfo(Path.Combine(tempDir.DirectoryPath, "bundle"));

        var builder = new BundleBuilder();
        await builder.WriteBundleAsync(bundleDirectory);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(Directory.Exists(bundleDirectory.FullName), Is.True);
            Assert.That(Directory.EnumerateFileSystemEntries(bundleDirectory.FullName), Is.Empty);
        }
    }

    [Test]
    public static async Task WriteBundleAsync_GivenSummaries_WritesScriptPerSummary()
    {
        using var tempDir = new TemporaryDirectory();
        var bundleDirectory = new DirectoryInfo(Path.Combine(tempDir.DirectoryPath, "bundle"));

        var builder = new BundleBuilder();
        builder.AddSummary("views", "[2]");
        builder.AddSummary("main", "[1]");

        await builder.WriteBundleAsync(bundleDirectory);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(ListFiles(bundleDirectory), Is.EquivalentTo(new[] { "main.js", "views.js" }));
            Assert.That(
                await File.ReadAllTextAsync(Path.Combine(bundleDirectory.FullName, "main.js")),
                Is.EqualTo("window.__schematic = window.__schematic || {};\nwindow.__schematic[\"main\"] = [1];\n"));
            Assert.That(
                await File.ReadAllTextAsync(Path.Combine(bundleDirectory.FullName, "views.js")),
                Is.EqualTo("window.__schematic = window.__schematic || {};\nwindow.__schematic[\"views\"] = [2];\n"));
        }
    }

    [Test]
    public static async Task WriteBundleAsync_GivenDetails_WritesScriptPerDetailInTypeDirectory()
    {
        using var tempDir = new TemporaryDirectory();
        var bundleDirectory = new DirectoryInfo(Path.Combine(tempDir.DirectoryPath, "bundle"));

        var builder = new BundleBuilder();
        builder.AddDetail("table", "zeta-key", "{\"name\":\"zeta\"}");
        builder.AddDetail("table", "alpha-key", "{\"name\":\"alpha\"}");
        builder.AddDetail("view", "alpha-key", "{\"name\":\"alpha view\"}");

        await builder.WriteBundleAsync(bundleDirectory);

        const string expectedAlpha =
            "window.__schematic = window.__schematic || {};\n" +
            "window.__schematic[\"table\"] = window.__schematic[\"table\"] || {};\n" +
            "window.__schematic[\"table\"][\"alpha-key\"] = {\"name\":\"alpha\"};\n";

        using (Assert.EnterMultipleScope())
        {
            Assert.That(
                ListFiles(bundleDirectory),
                Is.EquivalentTo(new[]
                {
                    Path.Combine("table", "alpha-key.js"),
                    Path.Combine("table", "zeta-key.js"),
                    Path.Combine("view", "alpha-key.js"),
                }));
            Assert.That(await File.ReadAllTextAsync(Path.Combine(bundleDirectory.FullName, "table", "alpha-key.js")), Is.EqualTo(expectedAlpha));
            Assert.That(
                await File.ReadAllTextAsync(Path.Combine(bundleDirectory.FullName, "view", "alpha-key.js")),
                Does.EndWith("window.__schematic[\"view\"][\"alpha-key\"] = {\"name\":\"alpha view\"};\n"));
        }
    }

    [Test]
    public static async Task WriteBundleAsync_GivenJsonFiles_CopiesFileBytesVerbatim()
    {
        using var tempDir = new TemporaryDirectory();
        var bundleDirectory = new DirectoryInfo(Path.Combine(tempDir.DirectoryPath, "bundle"));

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

        await builder.WriteBundleAsync(bundleDirectory);
        var summaryScript = await File.ReadAllBytesAsync(Path.Combine(bundleDirectory.FullName, "main.js"));
        var detailScript = await File.ReadAllBytesAsync(Path.Combine(bundleDirectory.FullName, "table", "actor-key.js"));

        byte[] expectedSummaryScript =
        [
            .. "window.__schematic = window.__schematic || {};\n"u8,
            .. "window.__schematic[\"main\"] = "u8, .. summaryBytes, .. ";\n"u8,
        ];
        byte[] expectedDetailScript =
        [
            .. "window.__schematic = window.__schematic || {};\n"u8,
            .. "window.__schematic[\"table\"] = window.__schematic[\"table\"] || {};\n"u8,
            .. "window.__schematic[\"table\"][\"actor-key\"] = "u8, .. detailBytes, .. ";\n"u8,
        ];

        using (Assert.EnterMultipleScope())
        {
            Assert.That(summaryScript, Is.EqualTo(expectedSummaryScript));
            Assert.That(detailScript, Is.EqualTo(expectedDetailScript));
        }
    }

    [Test]
    public static async Task WriteBundleAsync_GivenJsonFileRegisteredBeforeItIsWritten_UsesContentAtBundleTime()
    {
        using var tempDir = new TemporaryDirectory();
        var bundleDirectory = new DirectoryInfo(Path.Combine(tempDir.DirectoryPath, "bundle"));
        var jsonFile = new FileInfo(Path.Combine(tempDir.DirectoryPath, "main.json"));

        var builder = new BundleBuilder();
        builder.AddSummary("main", jsonFile);
        await File.WriteAllTextAsync(jsonFile.FullName, "[1]");

        await builder.WriteBundleAsync(bundleDirectory);
        var content = await File.ReadAllTextAsync(Path.Combine(bundleDirectory.FullName, "main.js"));

        Assert.That(content, Does.EndWith("window.__schematic[\"main\"] = [1];\n"));
    }

    [Test]
    public static async Task WriteBundleAsync_GivenSameKeyRegisteredTwice_WritesLatestPayload()
    {
        using var tempDir = new TemporaryDirectory();
        var bundleDirectory = new DirectoryInfo(Path.Combine(tempDir.DirectoryPath, "bundle"));
        var jsonFile = new FileInfo(Path.Combine(tempDir.DirectoryPath, "table.json"));
        await File.WriteAllTextAsync(jsonFile.FullName, "{\"from\":\"file\"}");

        var builder = new BundleBuilder();
        builder.AddDetail("table", "actor-key", "{\"from\":\"string\"}");
        builder.AddDetail("table", "actor-key", jsonFile);

        await builder.WriteBundleAsync(bundleDirectory);
        var content = await File.ReadAllTextAsync(Path.Combine(bundleDirectory.FullName, "table", "actor-key.js"));

        using (Assert.EnterMultipleScope())
        {
            Assert.That(content, Does.EndWith("window.__schematic[\"table\"][\"actor-key\"] = {\"from\":\"file\"};\n"));
            Assert.That(content, Does.Not.Contain("string"));
        }
    }

    [Test]
    public static void WriteBundleAsync_GivenMissingJsonFile_ThrowsFileNotFoundException()
    {
        using var tempDir = new TemporaryDirectory();
        var bundleDirectory = new DirectoryInfo(Path.Combine(tempDir.DirectoryPath, "bundle"));

        var builder = new BundleBuilder();
        builder.AddSummary("main", new FileInfo(Path.Combine(tempDir.DirectoryPath, "missing.json")));

        Assert.That(() => builder.WriteBundleAsync(bundleDirectory), Throws.InstanceOf<FileNotFoundException>());
    }

    [Test]
    public static async Task WriteBundleAsync_GivenExistingLongerScript_ReplacesContent()
    {
        using var tempDir = new TemporaryDirectory();
        var bundleDirectory = new DirectoryInfo(Path.Combine(tempDir.DirectoryPath, "bundle"));
        bundleDirectory.Create();
        var script = Path.Combine(bundleDirectory.FullName, "main.js");
        await File.WriteAllTextAsync(script, new string('x', 10_000));

        var builder = new BundleBuilder();
        builder.AddSummary("main", "[1]");
        await builder.WriteBundleAsync(bundleDirectory);

        var content = await File.ReadAllTextAsync(script);
        Assert.That(content, Is.EqualTo("window.__schematic = window.__schematic || {};\nwindow.__schematic[\"main\"] = [1];\n"));
    }

    [Test]
    public static async Task WriteBundleAsync_GivenKeyRequiringEscaping_EncodesKeyAsJsonString()
    {
        using var tempDir = new TemporaryDirectory();
        var bundleDirectory = new DirectoryInfo(Path.Combine(tempDir.DirectoryPath, "bundle"));

        var builder = new BundleBuilder();
        builder.AddSummary("o'brien", "{}");

        await builder.WriteBundleAsync(bundleDirectory);
        var content = await File.ReadAllTextAsync(Path.Combine(bundleDirectory.FullName, "o'brien.js"));

        // System.Text.Json's default (HTML-safe) encoder escapes '\'' as \u0027 -- still a valid,
        // unambiguous JS string literal.
        Assert.That(content, Does.Contain("window.__schematic[\"o\\u0027brien\"] = {};\n"));
    }

    [Test]
    public static async Task WriteBundleAsync_GivenMissingParentDirectory_CreatesDirectory()
    {
        using var tempDir = new TemporaryDirectory();
        var bundleDirectory = new DirectoryInfo(Path.Combine(tempDir.DirectoryPath, "nested", "data", "bundle"));

        var builder = new BundleBuilder();
        builder.AddDetail("table", "actor-key", "{}");
        await builder.WriteBundleAsync(bundleDirectory);

        Assert.That(File.Exists(Path.Combine(bundleDirectory.FullName, "table", "actor-key.js")), Is.True);
    }

    [Test]
    public static async Task WriteBundleAsync_WritesFilesAsUtf8WithoutBom()
    {
        using var tempDir = new TemporaryDirectory();
        var bundleDirectory = new DirectoryInfo(Path.Combine(tempDir.DirectoryPath, "bundle"));

        var builder = new BundleBuilder();
        builder.AddSummary("main", "{}");
        await builder.WriteBundleAsync(bundleDirectory);

        var bytes = await File.ReadAllBytesAsync(Path.Combine(bundleDirectory.FullName, "main.js"));

        Assert.That(bytes.AsSpan().StartsWith(Encoding.UTF8.GetPreamble()), Is.False);
    }

    private static string[] ListFiles(DirectoryInfo directory) => directory
        .EnumerateFiles("*", SearchOption.AllDirectories)
        .Select(f => Path.GetRelativePath(directory.FullName, f.FullName))
        .ToArray();
}
