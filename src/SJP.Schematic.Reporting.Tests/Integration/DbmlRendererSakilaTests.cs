using System.IO;
using System.Threading.Tasks;
using NUnit.Framework;
using SJP.Schematic.Reporting.Html.Renderers;
using SJP.Schematic.Reporting.Serialization;
using SJP.Schematic.Reporting.Tests.Html.Renderers;
using SJP.Schematic.Tests.Utilities;
using SJP.Schematic.Tests.Utilities.Integration;

namespace SJP.Schematic.Reporting.Tests.Integration;

internal sealed class DbmlRendererSakilaTests : SakilaTest
{
    [Test]
    public async Task RenderAsync_GivenSakilaTables_WritesNonEmptyDbmlFile()
    {
        using var tempDir = new TemporaryDirectory();
        var database = GetDatabase();
        var tables = await database.GetAllTables();

        var renderer = new DbmlRenderer();
        var data = ReportDataFactory.Create(tables: tables);
        var context = new RenderContext(new JsonDataWriter(), new BundleBuilder(), new DirectoryInfo(tempDir.DirectoryPath));
        await renderer.RenderAsync(data, context);

        var outputFile = Path.Combine(tempDir.DirectoryPath, "exports", "relationships.dbml");
        var content = await File.ReadAllTextAsync(outputFile);

        Assert.That(content, Is.Not.Empty);
    }

    [Test]
    public async Task RenderAsync_GivenMissingExportDirectory_CreatesExportsDirectory()
    {
        using var tempDir = new TemporaryDirectory();

        var renderer = new DbmlRenderer();
        var data = ReportDataFactory.Create();
        var context = new RenderContext(new JsonDataWriter(), new BundleBuilder(), new DirectoryInfo(tempDir.DirectoryPath));
        await renderer.RenderAsync(data, context);

        var exportsDirectory = new DirectoryInfo(Path.Combine(tempDir.DirectoryPath, "exports"));
        Assert.That(exportsDirectory.Exists, Is.True);
    }
}
