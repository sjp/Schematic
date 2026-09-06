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
internal static class SchemaRendererTests
{
    [Test]
    public static void RenderAsync_GivenNoSchemas_CompletesWithoutThrowing()
    {
        using var tempDir = new TemporaryDirectory();
        var renderer = new SchemaRenderer();
        var data = ReportDataFactory.Create(database: Database());
        var context = new RenderContext(new JsonDataWriter(), new BundleBuilder(), new DirectoryInfo(tempDir.DirectoryPath));

        Assert.That(async () => await renderer.RenderAsync(data, context), Throws.Nothing);
    }

    [Test]
    public static async Task RenderAsync_GivenSchema_WritesDetailFileUnderSchemasSubdirectory()
    {
        using var tempDir = new TemporaryDirectory();
        var schemaName = new Identifier("app");
        var schema = new DatabaseSchema(schemaName, Option<string>.None, true, false);

        var renderer = new SchemaRenderer();
        var data = ReportDataFactory.Create(database: Database(), schemas: [schema]);
        var context = new RenderContext(new JsonDataWriter(), new BundleBuilder(), new DirectoryInfo(tempDir.DirectoryPath));
        await renderer.RenderAsync(data, context);

        var expectedFile = Path.Combine(tempDir.DirectoryPath, "data", "schemas", schemaName.ToSafeKey() + ".json");
        Assert.That(File.Exists(expectedFile), Is.True);
    }

    [Test]
    public static async Task RenderAsync_GivenSchema_RegistersDetailPayloadUnderSchemaBundleKey()
    {
        using var tempDir = new TemporaryDirectory();
        var schemaName = new Identifier("app");
        var schema = new DatabaseSchema(schemaName, Option<string>.None, true, false);
        var bundle = new BundleBuilder();

        var renderer = new SchemaRenderer();
        var data = ReportDataFactory.Create(database: Database(), schemas: [schema]);
        var context = new RenderContext(new JsonDataWriter(), bundle, new DirectoryInfo(tempDir.DirectoryPath));
        await renderer.RenderAsync(data, context);

        var bundleFile = new FileInfo(Path.Combine(tempDir.DirectoryPath, "bundle.js"));
        await bundle.WriteBundleAsync(bundleFile);
        var bundleContent = await File.ReadAllTextAsync(bundleFile.FullName);

        Assert.That(bundleContent, Does.Contain($"window.__schematic[\"schema\"][\"{schemaName.ToSafeKey()}\"]"));
    }

    [Test]
    public static async Task RenderAsync_GivenSchemaHoldingObjects_LinksEachObjectToItsOwnPage()
    {
        using var tempDir = new TemporaryDirectory();
        var schemaName = new Identifier("app");
        var schema = new DatabaseSchema(schemaName, "dbo", true, false);
        var sequenceName = new Identifier("app", "seq_one");
        var sequence = new DatabaseSequence(
            sequenceName, TestDbTypes.BigInteger, 1M, 1M,
            Option<decimal>.None, Option<decimal>.None, false, SequenceCacheMode.None, Option<int>.None, true);

        var renderer = new SchemaRenderer();
        var data = ReportDataFactory.Create(database: Database(), schemas: [schema], sequences: [sequence]);
        var context = new RenderContext(new JsonDataWriter(), new BundleBuilder(), new DirectoryInfo(tempDir.DirectoryPath));
        await renderer.RenderAsync(data, context);

        var outputFile = Path.Combine(tempDir.DirectoryPath, "data", "schemas", schemaName.ToSafeKey() + ".json");
        var content = await File.ReadAllTextAsync(outputFile);

        Assert.Multiple(() =>
        {
            Assert.That(content, Does.Contain("\"owner\":\"dbo\""));
            Assert.That(content, Does.Contain("\"name\":\"seq_one\""));
            Assert.That(content, Does.Contain(UrlRouter.GetSequenceUrl(sequenceName)));
        });
    }

    private static IRelationalDatabase Database()
    {
        var database = new Mock<IRelationalDatabase>();
        database.Setup(static db => db.IdentifierDefaults).Returns(new IdentifierDefaults(null, null, "app"));
        return database.Object;
    }
}
