using System.IO;
using System.Threading.Tasks;
using LanguageExt;
using Moq;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Reporting.Html.Renderers;
using SJP.Schematic.Reporting.Serialization;
using SJP.Schematic.Tests.Utilities;

namespace SJP.Schematic.Reporting.Tests.Html.Renderers;

[TestFixture]
internal static class SchemasRendererTests
{
    [Test]
    public static async Task RenderAsync_GivenSchemas_WritesSummaryFileWithExpectedCount()
    {
        using var tempDir = new TemporaryDirectory();
        var first = new DatabaseSchema(new Identifier("first"), Option<string>.None, false, false);
        var second = new DatabaseSchema(new Identifier("second"), Option<string>.None, true, false);

        var renderer = new SchemasRenderer();
        var data = ReportDataFactory.Create(database: Database(), schemas: [first, second]);
        var context = new RenderContext(new JsonDataWriter(), new BundleBuilder(), new DirectoryInfo(tempDir.DirectoryPath));
        await renderer.RenderAsync(data, context);

        var outputFile = Path.Combine(tempDir.DirectoryPath, "data", "schemas.json");
        var content = await File.ReadAllTextAsync(outputFile);

        Assert.That(content, Does.Contain("\"schemasCount\":2"));
    }

    [Test]
    public static async Task RenderAsync_GivenSchemas_RegistersSummaryPayloadUnderSchemasBundleKey()
    {
        using var tempDir = new TemporaryDirectory();
        var schema = new DatabaseSchema(new Identifier("first"), Option<string>.None, false, false);
        var bundle = new BundleBuilder();

        var renderer = new SchemasRenderer();
        var data = ReportDataFactory.Create(database: Database(), schemas: [schema]);
        var context = new RenderContext(new JsonDataWriter(), bundle, new DirectoryInfo(tempDir.DirectoryPath));
        await renderer.RenderAsync(data, context);

        var bundleFile = new FileInfo(Path.Combine(tempDir.DirectoryPath, "bundle.js"));
        await bundle.WriteBundleAsync(bundleFile);
        var bundleContent = await File.ReadAllTextAsync(bundleFile.FullName);

        Assert.That(bundleContent, Does.Contain("window.__schematic[\"schemas\"]"));
    }

    [Test]
    public static async Task RenderAsync_GivenSystemSchemaWithNoObjects_OmitsIt()
    {
        using var tempDir = new TemporaryDirectory();
        var system = new DatabaseSchema(new Identifier("sys"), Option<string>.None, false, true);
        var user = new DatabaseSchema(new Identifier("app"), Option<string>.None, true, false);

        var renderer = new SchemasRenderer();
        var data = ReportDataFactory.Create(database: Database(), schemas: [system, user]);
        var context = new RenderContext(new JsonDataWriter(), new BundleBuilder(), new DirectoryInfo(tempDir.DirectoryPath));
        await renderer.RenderAsync(data, context);

        var outputFile = Path.Combine(tempDir.DirectoryPath, "data", "schemas.json");
        var content = await File.ReadAllTextAsync(outputFile);

        Assert.Multiple(() =>
        {
            // A schema a user declared is listed even when it holds nothing; a system schema
            // the report covers nothing in only adds noise.
            Assert.That(content, Does.Contain("\"schemasCount\":1"));
            Assert.That(content, Does.Contain("\"name\":\"app\""));
            Assert.That(content, Does.Not.Contain("\"name\":\"sys\""));
        });
    }

    [Test]
    public static async Task RenderAsync_GivenSchemaNamedOnlyByAnObject_ListsItWithThatObject()
    {
        using var tempDir = new TemporaryDirectory();
        var sequence = new DatabaseSequence(
            new Identifier("undeclared", "seq_one"), TestDbTypes.BigInteger, 1M, 1M,
            Option<decimal>.None, Option<decimal>.None, false, SequenceCacheMode.None, Option<int>.None, true);

        var renderer = new SchemasRenderer();
        var data = ReportDataFactory.Create(database: Database(), sequences: [sequence]);
        var context = new RenderContext(new JsonDataWriter(), new BundleBuilder(), new DirectoryInfo(tempDir.DirectoryPath));
        await renderer.RenderAsync(data, context);

        var outputFile = Path.Combine(tempDir.DirectoryPath, "data", "schemas.json");
        var content = await File.ReadAllTextAsync(outputFile);

        Assert.Multiple(() =>
        {
            // A dialect with no schema provider still gets a schema list, built from object names.
            Assert.That(content, Does.Contain("\"name\":\"undeclared\""));
            Assert.That(content, Does.Contain("\"sequencesCount\":1"));
            Assert.That(content, Does.Contain("\"objectCount\":1"));
        });
    }

    private static IRelationalDatabase Database()
    {
        var database = new Mock<IRelationalDatabase>();
        database.Setup(static db => db.IdentifierDefaults).Returns(new IdentifierDefaults(null, null, "app"));
        return database.Object;
    }
}
