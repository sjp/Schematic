using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Lint;
using SJP.Schematic.Reporting.Html.Lint;
using SJP.Schematic.Tests.Utilities;
using SJP.Schematic.Tests.Utilities.Integration;

namespace SJP.Schematic.Reporting.Tests.Integration;

// A flagship end-to-end test: generates a full report against the real Sakila database and
// verifies the expected artefacts are written to disk, exercising every renderer, mapper, and
// the serialization/bundle pipeline together.
internal sealed class ReportGeneratorSakilaTests : SakilaTest
{
    [Test]
    public void Ctor_GivenNullConnection_ThrowsArgumentNullException()
    {
        using var tempDir = new TemporaryDirectory();
        Assert.That(
            () => new ReportGenerator(null!, DatabaseProvider, GetDatabase(), tempDir.DirectoryPath),
            Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("connection"));
    }

    [Test]
    public void Ctor_GivenNullDatabaseProvider_ThrowsArgumentNullException()
    {
        using var tempDir = new TemporaryDirectory();
        Assert.That(
            () => new ReportGenerator(Connection, null!, GetDatabase(), tempDir.DirectoryPath),
            Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("databaseProvider"));
    }

    [Test]
    public void Ctor_GivenNullDatabase_ThrowsArgumentNullException()
    {
        using var tempDir = new TemporaryDirectory();
        Assert.That(
            () => new ReportGenerator(Connection, DatabaseProvider, null!, tempDir.DirectoryPath),
            Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("database"));
    }

    [Test]
    public void Ctor_GivenNullDirectory_ThrowsArgumentNullException()
    {
        Assert.That(
            () => new ReportGenerator(Connection, DatabaseProvider, GetDatabase(), (string)null!),
            Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("directory"));
    }

    [Test]
    public async Task GenerateAsync_GivenSakilaDatabase_CompletesWithoutThrowing()
    {
        using var tempDir = new TemporaryDirectory();
        var generator = new ReportGenerator(Connection, DatabaseProvider, GetDatabase(), tempDir.DirectoryPath);

        Assert.That(() => generator.GenerateAsync(), Throws.Nothing);
    }

    [Test]
    public async Task GenerateAsync_GivenSakilaDatabase_WritesExpectedDataFiles()
    {
        using var tempDir = new TemporaryDirectory();
        var generator = new ReportGenerator(Connection, DatabaseProvider, GetDatabase(), tempDir.DirectoryPath);

        await generator.GenerateAsync();

        var dataDir = Path.Combine(tempDir.DirectoryPath, "data");

        using (Assert.EnterMultipleScope())
        {
            Assert.That(File.Exists(Path.Combine(dataDir, "bundle", "main.js")), Is.True);
            Assert.That(File.Exists(Path.Combine(dataDir, "main.json")), Is.True);
            Assert.That(File.Exists(Path.Combine(dataDir, "tables.json")), Is.True);
            Assert.That(File.Exists(Path.Combine(dataDir, "lint.json")), Is.True);
            Assert.That(File.Exists(Path.Combine(dataDir, "search.json")), Is.True);
            Assert.That(File.Exists(Path.Combine(dataDir, "schemas.json")), Is.True);
            Assert.That(File.Exists(Path.Combine(dataDir, "userDefinedTypes.json")), Is.True);
        }
    }

    [Test]
    public async Task GenerateAsync_GivenSakilaDatabase_ExtractsReportShell()
    {
        using var tempDir = new TemporaryDirectory();
        var generator = new ReportGenerator(Connection, DatabaseProvider, GetDatabase(), tempDir.DirectoryPath);

        await generator.GenerateAsync();

        Assert.That(File.Exists(Path.Combine(tempDir.DirectoryPath, "index.html")), Is.True);
    }

    [Test]
    public async Task GenerateAsync_GivenSakilaDatabase_WritesBundleScriptForEachPayload()
    {
        using var tempDir = new TemporaryDirectory();
        var generator = new ReportGenerator(Connection, DatabaseProvider, GetDatabase(), tempDir.DirectoryPath);

        await generator.GenerateAsync();

        var dataDir = Path.Combine(tempDir.DirectoryPath, "data");
        var tablesScript = await File.ReadAllTextAsync(Path.Combine(dataDir, "bundle", "tables.js"));
        var tableScripts = Directory.EnumerateFiles(Path.Combine(dataDir, "bundle", "table"), "*.js").Select(Path.GetFileNameWithoutExtension);
        var tableJsonFiles = Directory.EnumerateFiles(Path.Combine(dataDir, "tables"), "*.json").Select(Path.GetFileNameWithoutExtension);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(tablesScript, Does.StartWith("window.__schematic = window.__schematic || {};"));
            Assert.That(tablesScript, Does.Contain("window.__schematic[\"tables\"] = "));
            Assert.That(tableScripts, Is.EquivalentTo(tableJsonFiles));
            Assert.That(tableScripts, Is.Not.Empty);
        }
    }

    [Test]
    public async Task GenerateAsync_GivenSakilaDatabase_BundlePayloadsAreByteIdenticalToEveryJsonFile()
    {
        using var tempDir = new TemporaryDirectory();
        var generator = new ReportGenerator(Connection, DatabaseProvider, GetDatabase(), tempDir.DirectoryPath);

        await generator.GenerateAsync();

        var dataDir = Path.Combine(tempDir.DirectoryPath, "data");
        var bundleDir = Path.Combine(dataDir, "bundle");

        // A script's payload is assigned on its last line: serialized JSON escapes newlines in strings
        // and is written unindented. Summary payloads map to data/<key>.json and detail payloads to
        // data/<typeKey>s/<safeKey>.json.
        var payloadsByRelativePath = new Dictionary<string, byte[]>(StringComparer.Ordinal);
        var assignment = new Regex("""^window\.__schematic\[("(?:[^"\\]|\\.)*")\](?:\[("(?:[^"\\]|\\.)*")\])? = """, RegexOptions.CultureInvariant);
        var unexpectedLines = new List<string>();
        foreach (var script in Directory.EnumerateFiles(bundleDir, "*.js", SearchOption.AllDirectories))
        {
            var lines = SplitLines(await File.ReadAllBytesAsync(script));
            foreach (var line in lines[..^1])
            {
                var text = Encoding.UTF8.GetString(line);
                if (!text.EndsWith(" || {};", StringComparison.Ordinal))
                    unexpectedLines.Add(text);
            }

            var assignmentText = Encoding.UTF8.GetString(lines[^1]);
            var match = assignment.Match(assignmentText);
            Assert.That(match.Success, Is.True, $"Unexpected bundle line: {assignmentText[..Math.Min(assignmentText.Length, 80)]}");

            var firstKey = JsonSerializer.Deserialize<string>(match.Groups[1].Value)!;
            var relativePath = match.Groups[2].Success
                ? Path.Combine(firstKey + "s", JsonSerializer.Deserialize<string>(match.Groups[2].Value)! + ".json")
                : firstKey + ".json";
            var expectedScript = match.Groups[2].Success
                ? Path.Combine(firstKey, JsonSerializer.Deserialize<string>(match.Groups[2].Value)! + ".js")
                : firstKey + ".js";

            Assert.That(Path.GetRelativePath(bundleDir, script), Is.EqualTo(expectedScript));

            var prefixLength = Encoding.UTF8.GetByteCount(match.Value);
            payloadsByRelativePath[relativePath] = lines[^1][prefixLength..^1]; // drop the trailing ';'
        }

        var jsonFiles = Directory.EnumerateFiles(dataDir, "*.json", SearchOption.AllDirectories)
            .Select(f => Path.GetRelativePath(dataDir, f))
            .ToList();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(unexpectedLines, Is.Empty);
            Assert.That(payloadsByRelativePath.Keys, Is.EquivalentTo(jsonFiles));
            Assert.That(jsonFiles, Has.Some.StartsWith("tables" + Path.DirectorySeparatorChar));

            foreach (var jsonFile in jsonFiles)
            {
                var fileBytes = await File.ReadAllBytesAsync(Path.Combine(dataDir, jsonFile));
                if (payloadsByRelativePath.TryGetValue(jsonFile, out var payload))
                    Assert.That(payload, Is.EqualTo(fileBytes), jsonFile);
            }
        }
    }

    [Test]
    public async Task GenerateAsync_GivenRuleProvider_BuildsLintPageFromThatProvider()
    {
        using var tempDir = new TemporaryDirectory();
        var database = await GetSnapshotDatabaseAsync();
        var generator = new ReportGenerator(Connection, DatabaseProvider, database, tempDir.DirectoryPath, tableStatistics: null, new EmptyRuleProvider());

        await generator.GenerateAsync();

        using var lint = JsonDocument.Parse(await File.ReadAllBytesAsync(Path.Combine(tempDir.DirectoryPath, "data", "lint.json")));

        using (Assert.EnterMultipleScope())
        {
            Assert.That(lint.RootElement.GetProperty("messages").GetArrayLength(), Is.Zero);
            Assert.That(lint.RootElement.GetProperty("lintRules").GetArrayLength(), Is.Zero);
        }
    }

    [Test]
    public async Task GenerateAsync_GivenNoRuleProvider_LintQueriesTheDatabase()
    {
        using var tempDir = new TemporaryDirectory();
        var database = await GetSnapshotDatabaseAsync();
        var countingFactory = new CountingDbConnectionFactory(DbConnection);
        var connection = new SchematicConnection(countingFactory, Connection.Dialect);
        var generator = new ReportGenerator(connection, DatabaseProvider, database, tempDir.DirectoryPath, tableStatistics: null, ruleProvider: null);

        await generator.GenerateAsync();

        Assert.That(countingFactory.QueryCount, Is.Positive);
    }

    [Test]
    public async Task GenerateAsync_GivenRulesThatDoNotQueryTheDatabase_RunsNoQueriesThroughTheConnection()
    {
        using var tempDir = new TemporaryDirectory();
        // The snapshot already holds the schema, so any query through the connection would have to
        // come from a lint rule.
        var database = await GetSnapshotDatabaseAsync();
        var countingFactory = new CountingDbConnectionFactory(DbConnection);
        var connection = new SchematicConnection(countingFactory, Connection.Dialect);
        var ruleProvider = new DefaultHtmlRuleProvider(tableStatistics: null, queryDatabase: false);
        var generator = new ReportGenerator(connection, DatabaseProvider, database, tempDir.DirectoryPath, tableStatistics: null, ruleProvider);

        await generator.GenerateAsync();

        using var lint = JsonDocument.Parse(await File.ReadAllBytesAsync(Path.Combine(tempDir.DirectoryPath, "data", "lint.json")));

        using (Assert.EnterMultipleScope())
        {
            Assert.That(countingFactory.QueryCount, Is.Zero);
            Assert.That(lint.RootElement.GetProperty("messages").GetArrayLength(), Is.Positive);
        }
    }

    // Splits on '\n', dropping the empty remainder after the final newline.
    private static List<byte[]> SplitLines(byte[] content)
    {
        var lines = new List<byte[]>();
        var start = 0;
        for (var i = 0; i < content.Length; i++)
        {
            if (content[i] != (byte)'\n')
                continue;

            lines.Add(content[start..i]);
            start = i + 1;
        }

        if (start < content.Length)
            lines.Add(content[start..]);

        return lines;
    }
}
