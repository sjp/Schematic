using System.IO;
using System.Threading.Tasks;
using NUnit.Framework;
using SJP.Schematic.Reporting.Html.Renderers;
using SJP.Schematic.Tests.Utilities;
using SJP.Schematic.Tests.Utilities.Integration;

namespace SJP.Schematic.Reporting.Tests.Integration;

internal sealed class DbmlRendererSakilaTests : SakilaTest
{
    [Test]
    public void Ctor_GivenNullTables_ThrowsArgumentNullException()
    {
        using var tempDir = new TemporaryDirectory();
        Assert.That(
            () => new DbmlRenderer(null!, new DirectoryInfo(tempDir.DirectoryPath)),
            Throws.ArgumentNullException);
    }

    [Test]
    public void Ctor_GivenNullExportDirectory_ThrowsArgumentNullException()
    {
        Assert.That(() => new DbmlRenderer([], null!), Throws.ArgumentNullException);
    }

    [Test]
    public async Task RenderAsync_GivenSakilaTables_WritesNonEmptyDbmlFile()
    {
        using var tempDir = new TemporaryDirectory();
        var database = GetDatabase();
        var tables = await database.GetAllTables();

        var exportDirectory = new DirectoryInfo(Path.Combine(tempDir.DirectoryPath, "exports"));
        var renderer = new DbmlRenderer(tables, exportDirectory);
        await renderer.RenderAsync();

        var outputFile = Path.Combine(exportDirectory.FullName, "relationships.dbml");
        var content = await File.ReadAllTextAsync(outputFile);

        Assert.That(content, Is.Not.Empty);
    }

    [Test]
    public async Task RenderAsync_GivenMissingExportDirectory_CreatesDirectory()
    {
        using var tempDir = new TemporaryDirectory();
        var exportDirectory = new DirectoryInfo(Path.Combine(tempDir.DirectoryPath, "does-not-exist-yet"));

        var renderer = new DbmlRenderer([], exportDirectory);
        await renderer.RenderAsync();

        Assert.That(exportDirectory.Exists, Is.True);
    }
}
