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
internal static class UserDefinedTypeRendererTests
{
    [Test]
    public static void RenderAsync_GivenNoUserDefinedTypes_CompletesWithoutThrowing()
    {
        using var tempDir = new TemporaryDirectory();
        var renderer = new UserDefinedTypeRenderer();
        var data = ReportDataFactory.Create();
        var context = new RenderContext(new JsonDataWriter(), new BundleBuilder(), new DirectoryInfo(tempDir.DirectoryPath));

        Assert.That(async () => await renderer.RenderAsync(data, context), Throws.Nothing);
    }

    [Test]
    public static async Task RenderAsync_GivenUserDefinedType_WritesDetailFileUnderUserDefinedTypesSubdirectory()
    {
        using var tempDir = new TemporaryDirectory();
        var typeName = new Identifier("test_type");
        var userDefinedType = new DatabaseUserDefinedType(typeName, UserDefinedTypeKind.Alias, Option<IDbType>.Some(TestDbTypes.BigInteger));

        var renderer = new UserDefinedTypeRenderer();
        var data = ReportDataFactory.Create(userDefinedTypes: [userDefinedType]);
        var context = new RenderContext(new JsonDataWriter(), new BundleBuilder(), new DirectoryInfo(tempDir.DirectoryPath));
        await renderer.RenderAsync(data, context);

        var expectedFile = Path.Combine(tempDir.DirectoryPath, "data", "userDefinedTypes", typeName.ToSafeKey() + ".json");
        Assert.That(File.Exists(expectedFile), Is.True);
    }

    [Test]
    public static async Task RenderAsync_GivenUserDefinedType_RegistersDetailPayloadUnderUserDefinedTypeBundleKey()
    {
        using var tempDir = new TemporaryDirectory();
        var typeName = new Identifier("test_type");
        var userDefinedType = new DatabaseUserDefinedType(typeName, UserDefinedTypeKind.Alias, Option<IDbType>.Some(TestDbTypes.BigInteger));
        var bundle = new BundleBuilder();

        var renderer = new UserDefinedTypeRenderer();
        var data = ReportDataFactory.Create(userDefinedTypes: [userDefinedType]);
        var context = new RenderContext(new JsonDataWriter(), bundle, new DirectoryInfo(tempDir.DirectoryPath));
        await renderer.RenderAsync(data, context);

        var bundleFile = new FileInfo(Path.Combine(tempDir.DirectoryPath, "bundle.js"));
        await bundle.WriteBundleAsync(bundleFile);
        var bundleContent = await File.ReadAllTextAsync(bundleFile.FullName);

        Assert.That(bundleContent, Does.Contain($"window.__schematic[\"userDefinedType\"][\"{typeName.ToSafeKey()}\"]"));
    }

    [Test]
    public static async Task RenderAsync_GivenTypeWithAttributesValuesAndChecks_WritesThemToTheDetailFile()
    {
        using var tempDir = new TemporaryDirectory();
        var typeName = new Identifier("test_type");
        var attribute = new DatabaseColumn(new Identifier("attr_one"), TestDbTypes.BigInteger, true, Option<IDatabaseDefaultValue>.None, Option<IAutoIncrement>.None);
        var check = new DatabaseCheckConstraint(new Identifier("ck_positive"), "value > 0", true);
        var userDefinedType = new DatabaseUserDefinedType(
            typeName,
            UserDefinedTypeKind.Composite,
            Option<IDbType>.Some(TestDbTypes.BigInteger),
            ["alpha", "beta"],
            [attribute],
            [check],
            false,
            "0",
            "create type test_type as (attr_one bigint)"
        );

        var renderer = new UserDefinedTypeRenderer();
        var data = ReportDataFactory.Create(userDefinedTypes: [userDefinedType]);
        var context = new RenderContext(new JsonDataWriter(), new BundleBuilder(), new DirectoryInfo(tempDir.DirectoryPath));
        await renderer.RenderAsync(data, context);

        var outputFile = Path.Combine(tempDir.DirectoryPath, "data", "userDefinedTypes", typeName.ToSafeKey() + ".json");
        var content = await File.ReadAllTextAsync(outputFile);

        Assert.Multiple(() =>
        {
            Assert.That(content, Does.Contain("\"kind\":\"Composite\""));
            Assert.That(content, Does.Contain("\"attributeName\":\"attr_one\""));
            Assert.That(content, Does.Contain("\"attributesCount\":1"));
            Assert.That(content, Does.Contain("\"enumValues\":[\"alpha\",\"beta\"]"));
            Assert.That(content, Does.Contain("\"constraintName\":\"ck_positive\""));
            Assert.That(content, Does.Contain("\"checksCount\":1"));
        });
    }
}
