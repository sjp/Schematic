using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Lint;
using SJP.Schematic.Reporting.Html.Renderers;
using SJP.Schematic.Reporting.Serialization;
using SJP.Schematic.Tests.Utilities;

namespace SJP.Schematic.Reporting.Tests.Html.Renderers;

[TestFixture]
internal static class LintRendererTests
{
    [Test]
    public static void Ctor_GivenNullLinter_ThrowsArgumentNullException()
    {
        Assert.That(() => new LintRenderer(null!), Throws.ArgumentNullException);
    }

    [Test]
    public static async Task RenderAsync_GivenNoLintMessages_WritesEmptySummaryFile()
    {
        using var tempDir = new TemporaryDirectory();
        var mockLinter = CreateMockLinter([]);

        var renderer = new LintRenderer(mockLinter.Object);
        var data = ReportDataFactory.Create();
        var context = new RenderContext(new JsonDataWriter(), new BundleBuilder(), new DirectoryInfo(tempDir.DirectoryPath));
        await renderer.RenderAsync(data, context);

        var outputFile = Path.Combine(tempDir.DirectoryPath, "data", "lint.json");
        var content = await File.ReadAllTextAsync(outputFile);

        Assert.That(content, Does.Contain("\"lintRulesCount\":0"));
    }

    [Test]
    public static async Task RenderAsync_GivenTableLintMessage_GroupsMessagesByRuleId()
    {
        using var tempDir = new TemporaryDirectory();
        var tableMessages = new IRuleMessage[]
        {
            new RuleMessage("SCHEMATIC0001", "Example rule title", RuleLevel.Warning, "first message"),
            new RuleMessage("SCHEMATIC0001", "Example rule title", RuleLevel.Warning, "second message"),
        };
        var mockLinter = CreateMockLinter(tableMessages);

        var renderer = new LintRenderer(mockLinter.Object);
        var data = ReportDataFactory.Create();
        var context = new RenderContext(new JsonDataWriter(), new BundleBuilder(), new DirectoryInfo(tempDir.DirectoryPath));
        await renderer.RenderAsync(data, context);

        var outputFile = Path.Combine(tempDir.DirectoryPath, "data", "lint.json");
        var content = await File.ReadAllTextAsync(outputFile);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(content, Does.Contain("\"lintRulesCount\":1"));
            Assert.That(content, Does.Contain("\"messageCount\":2"));
            Assert.That(content, Does.Contain("Example rule title"));
        }
    }

    [Test]
    public static async Task RenderAsync_GivenLintMessages_RegistersSummaryPayloadUnderLintBundleKey()
    {
        using var tempDir = new TemporaryDirectory();
        var mockLinter = CreateMockLinter([]);
        var bundle = new BundleBuilder();

        var renderer = new LintRenderer(mockLinter.Object);
        var data = ReportDataFactory.Create();
        var context = new RenderContext(new JsonDataWriter(), bundle, new DirectoryInfo(tempDir.DirectoryPath));
        await renderer.RenderAsync(data, context);

        var bundleFile = new FileInfo(Path.Combine(tempDir.DirectoryPath, "bundle.js"));
        await bundle.WriteBundleAsync(bundleFile);
        var bundleContent = await File.ReadAllTextAsync(bundleFile.FullName);

        Assert.That(bundleContent, Does.Contain("window.__schematic[\"lint\"]"));
    }

    [Test]
    public static async Task RenderAsync_GivenLintMessages_WritesRuleIdAndLevelForEachRule()
    {
        using var tempDir = new TemporaryDirectory();
        var tableMessages = new IRuleMessage[]
        {
            new RuleMessage("SCHEMATIC0001", "Example rule title", RuleLevel.Error, "first message"),
        };
        var mockLinter = CreateMockLinter(tableMessages);

        var renderer = new LintRenderer(mockLinter.Object);
        var data = ReportDataFactory.Create();
        var context = new RenderContext(new JsonDataWriter(), new BundleBuilder(), new DirectoryInfo(tempDir.DirectoryPath));
        await renderer.RenderAsync(data, context);

        var content = await File.ReadAllTextAsync(Path.Combine(tempDir.DirectoryPath, "data", "lint.json"));

        using (Assert.EnterMultipleScope())
        {
            Assert.That(content, Does.Contain("\"ruleId\":\"SCHEMATIC0001\""));
            Assert.That(content, Does.Contain("\"level\":\"Error\""));
            Assert.That(content, Does.Contain("\"errorCount\":1"));
            Assert.That(content, Does.Contain("\"warningCount\":0"));
        }
    }

    [Test]
    public static async Task RenderAsync_GivenMessageWithObjectName_LinksMessageToThatObject()
    {
        using var tempDir = new TemporaryDirectory();
        var tableName = Identifier.CreateQualifiedIdentifier("main", "test_table");
        var tableMessages = new IRuleMessage[]
        {
            new RuleMessage("SCHEMATIC0001", "Example rule title", RuleLevel.Warning, "a message", tableName),
        };
        var mockLinter = CreateMockLinter(tableMessages);

        var renderer = new LintRenderer(mockLinter.Object);
        var data = ReportDataFactory.Create();
        var context = new RenderContext(new JsonDataWriter(), new BundleBuilder(), new DirectoryInfo(tempDir.DirectoryPath));
        await renderer.RenderAsync(data, context);

        var content = await File.ReadAllTextAsync(Path.Combine(tempDir.DirectoryPath, "data", "lint.json"));

        using (Assert.EnterMultipleScope())
        {
            Assert.That(content, Does.Contain("\"objectName\":\"main.test_table\""));
            Assert.That(content, Does.Contain("\"objectType\":\"Table\""));
            Assert.That(content, Does.Contain("\"objectUrl\":\"#/tables/test-table-"));
            Assert.That(content, Does.Contain("\"objectsAffectedCount\":1"));
        }
    }

    [Test]
    public static async Task RenderAsync_GivenMessageWithoutObjectName_OmitsObjectFields()
    {
        using var tempDir = new TemporaryDirectory();
        var tableMessages = new IRuleMessage[]
        {
            new RuleMessage("SCHEMATIC0001", "Example rule title", RuleLevel.Warning, "a schema-wide message"),
        };
        var mockLinter = CreateMockLinter(tableMessages);

        var renderer = new LintRenderer(mockLinter.Object);
        var data = ReportDataFactory.Create();
        var context = new RenderContext(new JsonDataWriter(), new BundleBuilder(), new DirectoryInfo(tempDir.DirectoryPath));
        await renderer.RenderAsync(data, context);

        var content = await File.ReadAllTextAsync(Path.Combine(tempDir.DirectoryPath, "data", "lint.json"));

        using (Assert.EnterMultipleScope())
        {
            Assert.That(content, Does.Not.Contain("\"objectUrl\""));
            Assert.That(content, Does.Contain("\"objectsAffectedCount\":0"));
        }
    }

    [Test]
    public static async Task RenderAsync_GivenLintMessages_OrdersMessagesDeterministically()
    {
        using var tempDir = new TemporaryDirectory();
        var tableMessages = new IRuleMessage[]
        {
            new RuleMessage("SCHEMATIC0002", "Second rule", RuleLevel.Warning, "zebra"),
            new RuleMessage("SCHEMATIC0001", "First rule", RuleLevel.Warning, "beta"),
            new RuleMessage("SCHEMATIC0001", "First rule", RuleLevel.Warning, "alpha"),
        };
        var mockLinter = CreateMockLinter(tableMessages);

        var renderer = new LintRenderer(mockLinter.Object);
        var data = ReportDataFactory.Create();
        var context = new RenderContext(new JsonDataWriter(), new BundleBuilder(), new DirectoryInfo(tempDir.DirectoryPath));
        await renderer.RenderAsync(data, context);

        var content = await File.ReadAllTextAsync(Path.Combine(tempDir.DirectoryPath, "data", "lint.json"));

        using (Assert.EnterMultipleScope())
        {
            Assert.That(content.IndexOf("alpha", StringComparison.Ordinal), Is.LessThan(content.IndexOf("beta", StringComparison.Ordinal)));
            Assert.That(content.IndexOf("beta", StringComparison.Ordinal), Is.LessThan(content.IndexOf("zebra", StringComparison.Ordinal)));
        }
    }

    [Test]
    public static async Task RenderAsync_GivenLintMessages_AlsoWritesSarifLog()
    {
        using var tempDir = new TemporaryDirectory();
        var tableName = Identifier.CreateQualifiedIdentifier("main", "test_table");
        var tableMessages = new IRuleMessage[]
        {
            new RuleMessage("SCHEMATIC0001", "Example rule title", RuleLevel.Error, "a message", tableName),
        };
        var mockLinter = CreateMockLinter(tableMessages);

        var renderer = new LintRenderer(mockLinter.Object);
        var data = ReportDataFactory.Create();
        var context = new RenderContext(new JsonDataWriter(), new BundleBuilder(), new DirectoryInfo(tempDir.DirectoryPath));
        await renderer.RenderAsync(data, context);

        var content = await File.ReadAllTextAsync(Path.Combine(tempDir.DirectoryPath, "data", "lint.sarif"));

        using (Assert.EnterMultipleScope())
        {
            Assert.That(content, Does.Contain("sarif-schema-2.1.0.json"));
            Assert.That(content, Does.Contain("\"level\":\"error\""));
            Assert.That(content, Does.Contain("\"fullyQualifiedName\":\"main.test_table\""));
        }
    }

    [Test]
    public static async Task RenderAsync_GivenLintMessages_DoesNotRegisterSarifInBundle()
    {
        using var tempDir = new TemporaryDirectory();
        var mockLinter = CreateMockLinter([]);
        var bundle = new BundleBuilder();

        var renderer = new LintRenderer(mockLinter.Object);
        var data = ReportDataFactory.Create();
        var context = new RenderContext(new JsonDataWriter(), bundle, new DirectoryInfo(tempDir.DirectoryPath));
        await renderer.RenderAsync(data, context);

        var bundleFile = new FileInfo(Path.Combine(tempDir.DirectoryPath, "bundle.js"));
        await bundle.WriteBundleAsync(bundleFile);
        var bundleContent = await File.ReadAllTextAsync(bundleFile.FullName);

        // The SARIF log is a sidecar for external tooling; nothing in the SPA reads it.
        Assert.That(bundleContent, Does.Not.Contain("sarif"));
    }

    private static Mock<IRelationalDatabaseLinter> CreateMockLinter(IReadOnlyCollection<IRuleMessage> tableMessages)
    {
        var mockLinter = new Mock<IRelationalDatabaseLinter>();
        mockLinter
            .Setup(static l => l.AnalyseTables(It.IsAny<IReadOnlyCollection<IRelationalDatabaseTable>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(tableMessages);
        mockLinter
            .Setup(static l => l.AnalyseViews(It.IsAny<IReadOnlyCollection<IDatabaseView>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<IRuleMessage>());
        mockLinter
            .Setup(static l => l.AnalyseSequences(It.IsAny<IReadOnlyCollection<IDatabaseSequence>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<IRuleMessage>());
        mockLinter
            .Setup(static l => l.AnalyseSynonyms(It.IsAny<IReadOnlyCollection<IDatabaseSynonym>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<IRuleMessage>());
        mockLinter
            .Setup(static l => l.AnalyseRoutines(It.IsAny<IReadOnlyCollection<IDatabaseRoutine>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<IRuleMessage>());

        return mockLinter;
    }
}
