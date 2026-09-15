using System.IO;
using System.Threading.Tasks;
using LanguageExt;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Reporting.Html.Renderers;
using SJP.Schematic.Reporting.Serialization;
using SJP.Schematic.Tests.Utilities;

namespace SJP.Schematic.Reporting.Tests.Html.Renderers;

internal static class UserDefinedTypesRendererTests
{
    [Test]
    public static async Task RenderAsync_GivenUserDefinedTypes_WritesSummaryFileWithExpectedCount()
    {
        using var tempDir = new TemporaryDirectory();
        var first = new DatabaseUserDefinedType(new Identifier("type_one"), UserDefinedTypeKind.Alias, Option<IDbType>.Some(TestDbTypes.BigInteger));
        var second = new DatabaseUserDefinedType(new Identifier("type_two"), UserDefinedTypeKind.Enum, Option<IDbType>.None);

        var renderer = new UserDefinedTypesRenderer();
        var data = ReportDataFactory.Create(userDefinedTypes: [first, second]);
        var context = new RenderContext(new JsonDataWriter(), new BundleBuilder(), new DirectoryInfo(tempDir.DirectoryPath));
        await renderer.RenderAsync(data, context);

        var outputFile = Path.Combine(tempDir.DirectoryPath, "data", "userDefinedTypes.json");
        var content = await File.ReadAllTextAsync(outputFile);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(content, Does.Contain("\"userDefinedTypesCount\":2"));
            Assert.That(content, Does.Contain("\"kind\":\"Alias\""));
            Assert.That(content, Does.Contain("\"baseType\":\"bigint\""));
        }
    }

    [Test]
    public static async Task RenderAsync_GivenUserDefinedTypes_RegistersSummaryPayloadUnderUserDefinedTypesBundleKey()
    {
        using var tempDir = new TemporaryDirectory();
        var userDefinedType = new DatabaseUserDefinedType(new Identifier("type_one"), UserDefinedTypeKind.Domain, Option<IDbType>.Some(TestDbTypes.BigInteger));
        var bundle = new BundleBuilder();

        var renderer = new UserDefinedTypesRenderer();
        var data = ReportDataFactory.Create(userDefinedTypes: [userDefinedType]);
        var context = new RenderContext(new JsonDataWriter(), bundle, new DirectoryInfo(tempDir.DirectoryPath));
        await renderer.RenderAsync(data, context);

        var bundleDirectory = new DirectoryInfo(Path.Combine(tempDir.DirectoryPath, "bundle"));
        await bundle.WriteBundleAsync(bundleDirectory);
        var bundleContent = await File.ReadAllTextAsync(Path.Combine(bundleDirectory.FullName, "userDefinedTypes.js"));

        Assert.That(bundleContent, Does.Contain("window.__schematic[\"userDefinedTypes\"] = "));
    }
}
