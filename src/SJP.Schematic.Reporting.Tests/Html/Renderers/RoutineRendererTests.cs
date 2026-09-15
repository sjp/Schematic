using System.IO;
using System.Threading.Tasks;
using LanguageExt;
using Moq;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Reporting.Html.Renderers;
using SJP.Schematic.Reporting.Html.ViewModels.Mappers;
using SJP.Schematic.Reporting.Serialization;
using SJP.Schematic.Tests.Utilities;

namespace SJP.Schematic.Reporting.Tests.Html.Renderers;

internal static class RoutineRendererTests
{
    [Test]
    public static async Task RenderAsync_GivenRoutine_WritesDetailFileUnderRoutinesSubdirectory()
    {
        using var tempDir = new TemporaryDirectory();
        var routineName = new Identifier("test_routine");
        var routine = new DatabaseRoutine(routineName, "select 1");

        var renderer = new RoutineRenderer();
        var data = ReportDataFactory.Create(routines: [routine]);
        var context = new RenderContext(new JsonDataWriter(), new BundleBuilder(), new DirectoryInfo(tempDir.DirectoryPath));
        await renderer.RenderAsync(data, context);

        var expectedFile = Path.Combine(tempDir.DirectoryPath, "data", "routines", routineName.ToSafeKey() + ".json");
        Assert.That(File.Exists(expectedFile), Is.True);
    }

    [Test]
    public static async Task RenderAsync_GivenRoutine_RegistersDetailPayloadUnderRoutineBundleKey()
    {
        using var tempDir = new TemporaryDirectory();
        var routineName = new Identifier("test_routine");
        var routine = new DatabaseRoutine(routineName, "select 1");
        var bundle = new BundleBuilder();

        var renderer = new RoutineRenderer();
        var data = ReportDataFactory.Create(routines: [routine]);
        var context = new RenderContext(new JsonDataWriter(), bundle, new DirectoryInfo(tempDir.DirectoryPath));
        await renderer.RenderAsync(data, context);

        var bundleDirectory = new DirectoryInfo(Path.Combine(tempDir.DirectoryPath, "bundle"));
        await bundle.WriteBundleAsync(bundleDirectory);
        var bundleContent = await File.ReadAllTextAsync(Path.Combine(bundleDirectory.FullName, "routine", routineName.ToSafeKey() + ".js"));

        Assert.That(bundleContent, Does.Contain($"window.__schematic[\"routine\"][\"{routineName.ToSafeKey()}\"] = "));
    }

    [Test]
    public static async Task RenderAsync_GivenRoutineWithSignature_WritesKindLanguageAndParameters()
    {
        using var tempDir = new TemporaryDirectory();
        var routineName = new Identifier("test_routine");
        var integerType = new ColumnDataType(
            "integer",
            DataType.Integer,
            "integer",
            typeof(int),
            false,
            0,
            Option<INumericPrecision>.None,
            Option<Identifier>.None
        );
        var parameter = new DatabaseRoutineParameter(
            Option<Identifier>.Some("test_parameter"),
            integerType,
            RoutineParameterDirection.InputOutput,
            Option<string>.Some("1"),
            1
        );
        var routine = new DatabaseRoutine(
            routineName,
            "select 1",
            RoutineType.Function,
            Option<string>.Some("plpgsql"),
            [parameter],
            Option<IDbType>.Some(integerType)
        );

        var renderer = new RoutineRenderer();
        var data = ReportDataFactory.Create(routines: [routine]);
        var context = new RenderContext(new JsonDataWriter(), new BundleBuilder(), new DirectoryInfo(tempDir.DirectoryPath));
        await renderer.RenderAsync(data, context);

        var detailFile = Path.Combine(tempDir.DirectoryPath, "data", "routines", routineName.ToSafeKey() + ".json");
        var detailJson = await File.ReadAllTextAsync(detailFile);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(detailJson, Does.Contain("\"routineType\":\"Function\""));
            Assert.That(detailJson, Does.Contain("\"language\":\"plpgsql\""));
            Assert.That(detailJson, Does.Contain("\"parameterName\":\"test_parameter\""));
            Assert.That(detailJson, Does.Contain("\"direction\":\"InputOutput\""));
            Assert.That(detailJson, Does.Contain("\"parametersCount\":1"));
        }
    }

    [Test]
    public static async Task RenderAsync_GivenRoutineReferencingATable_WritesLinkToTheTable()
    {
        using var tempDir = new TemporaryDirectory();
        var routineName = Identifier.CreateQualifiedIdentifier("app", "test_routine");
        var tableName = Identifier.CreateQualifiedIdentifier("app", "test_table");
        var routine = new DatabaseRoutine(routineName, "select * from test_table");

        var dependencyProvider = new Mock<IDependencyProvider>();
        dependencyProvider
            .Setup(static p => p.GetDependencies(It.IsAny<Identifier>(), It.IsAny<string>()))
            .Returns([new Identifier("test_table")]);

        var renderer = new RoutineRenderer();
        var data = ReportDataFactory.Create(
            routines: [routine],
            referencedObjectTargets: new ReferencedObjectTargets(dependencyProvider.Object, [tableName], [], [], [], [], []));
        var context = new RenderContext(new JsonDataWriter(), new BundleBuilder(), new DirectoryInfo(tempDir.DirectoryPath));
        await renderer.RenderAsync(data, context);

        var detailFile = Path.Combine(tempDir.DirectoryPath, "data", "routines", routineName.ToSafeKey() + ".json");
        var detailJson = await File.ReadAllTextAsync(detailFile);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(detailJson, Does.Contain("\"referencedObjectsCount\":1"));
            Assert.That(detailJson, Does.Contain("\"name\":\"app.test_table\""));
            Assert.That(detailJson, Does.Contain(UrlRouter.GetTableUrl(tableName)));
        }
    }
}
