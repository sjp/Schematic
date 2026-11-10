using System.IO;
using System.Threading.Tasks;
using LanguageExt;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Reporting.Html.Renderers;
using SJP.Schematic.Reporting.Serialization;
using SJP.Schematic.Tests.Utilities;

namespace SJP.Schematic.Reporting.Tests.Html.Renderers;

internal static class UserDefinedTypeRendererTests
{
    [Test]
    public static void RenderAsync_GivenNoUserDefinedTypes_CompletesWithoutThrowing()
    {
        using var tempDir = new TemporaryDirectory();
        var renderer = new UserDefinedTypeRenderer();
        var data = ReportDataFactory.Create();
        var context = new RenderContext(new JsonDataWriter(), new BundleBuilder(), new DirectoryInfo(tempDir.DirectoryPath));

        Assert.That(() => renderer.RenderAsync(data, context), Throws.Nothing);
    }

    [Test]
    public static async Task RenderAsync_GivenUserDefinedType_WritesDetailFileUnderUserDefinedTypesSubdirectory()
    {
        using var tempDir = new TemporaryDirectory();
        var typeName = new Identifier("test_type");
        var userDefinedType = new DatabaseUserDefinedType.Alias(typeName, TestDbTypes.BigInteger);

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
        var userDefinedType = new DatabaseUserDefinedType.Alias(typeName, TestDbTypes.BigInteger);
        var bundle = new BundleBuilder();

        var renderer = new UserDefinedTypeRenderer();
        var data = ReportDataFactory.Create(userDefinedTypes: [userDefinedType]);
        var context = new RenderContext(new JsonDataWriter(), bundle, new DirectoryInfo(tempDir.DirectoryPath));
        await renderer.RenderAsync(data, context);

        var bundleDirectory = new DirectoryInfo(Path.Combine(tempDir.DirectoryPath, "bundle"));
        await bundle.WriteBundleAsync(bundleDirectory);
        var bundleContent = await File.ReadAllTextAsync(Path.Combine(bundleDirectory.FullName, "userDefinedType", typeName.ToSafeKey() + ".js"));

        Assert.That(bundleContent, Does.Contain($"window.__schematic[\"userDefinedType\"][\"{typeName.ToSafeKey()}\"] = "));
    }

    [Test]
    public static async Task RenderAsync_GivenCompositeType_WritesAttributesToTheDetailFile()
    {
        var attribute = new DatabaseColumn(new Identifier("attr_one"), TestDbTypes.BigInteger, true, Option<DatabaseDefaultValue>.None, Option<IAutoIncrement>.None);
        var userDefinedType = new DatabaseUserDefinedType.Composite(
            new Identifier("test_type"),
            [attribute],
            "create type test_type as (attr_one bigint)"
        );

        var content = await RenderDetailAsync(userDefinedType);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(content, Does.Contain("\"kind\":\"Composite\""));
            Assert.That(content, Does.Contain("\"attributeName\":\"attr_one\""));
            Assert.That(content, Does.Contain("\"attributesCount\":1"));
            Assert.That(content, Does.Contain("\"definition\":\"create type test_type as (attr_one bigint)\""));
        }
    }

    [Test]
    public static async Task RenderAsync_GivenEnumType_WritesValuesToTheDetailFile()
    {
        var userDefinedType = new DatabaseUserDefinedType.Enum(new Identifier("test_type"), ["alpha", "beta"]);

        var content = await RenderDetailAsync(userDefinedType);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(content, Does.Contain("\"kind\":\"Enum\""));
            Assert.That(content, Does.Contain("\"enumValues\":[\"alpha\",\"beta\"]"));
            Assert.That(content, Does.Contain("\"enumValuesCount\":2"));
        }
    }

    [Test]
    public static async Task RenderAsync_GivenDomainType_WritesBaseTypeNullabilityDefaultAndChecksToTheDetailFile()
    {
        var check = new DatabaseCheckConstraint(new Identifier("ck_positive"), "value > 0", true);
        var userDefinedType = new DatabaseUserDefinedType.Domain(new Identifier("test_type"), TestDbTypes.BigInteger, false, "0", [check]);

        var content = await RenderDetailAsync(userDefinedType);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(content, Does.Contain("\"kind\":\"Domain\""));
            Assert.That(content, Does.Contain("\"baseType\":\"bigint\""));
            Assert.That(content, Does.Contain("\"isNullable\":false"));
            Assert.That(content, Does.Contain("\"defaultValue\":\"0\""));
            Assert.That(content, Does.Contain("\"constraintName\":\"ck_positive\""));
            Assert.That(content, Does.Contain("\"checksCount\":1"));
        }
    }

    private static async Task<string> RenderDetailAsync(DatabaseUserDefinedType userDefinedType)
    {
        using var tempDir = new TemporaryDirectory();

        var renderer = new UserDefinedTypeRenderer();
        var data = ReportDataFactory.Create(userDefinedTypes: [userDefinedType]);
        var context = new RenderContext(new JsonDataWriter(), new BundleBuilder(), new DirectoryInfo(tempDir.DirectoryPath));
        await renderer.RenderAsync(data, context);

        var outputFile = Path.Combine(tempDir.DirectoryPath, "data", "userDefinedTypes", userDefinedType.Name.ToSafeKey() + ".json");
        return await File.ReadAllTextAsync(outputFile);
    }
}
