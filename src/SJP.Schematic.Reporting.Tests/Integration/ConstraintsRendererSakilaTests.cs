using System.IO;
using System.Threading.Tasks;
using NUnit.Framework;
using SJP.Schematic.Reporting.Html.Renderers;
using SJP.Schematic.Reporting.Serialization;
using SJP.Schematic.Tests.Utilities;
using SJP.Schematic.Tests.Utilities.Integration;

namespace SJP.Schematic.Reporting.Tests.Integration;

internal sealed class ConstraintsRendererSakilaTests : SakilaTest
{
    [Test]
    public void Ctor_GivenNullTables_ThrowsArgumentNullException()
    {
        using var tempDir = new TemporaryDirectory();
        Assert.That(
            () => new ConstraintsRenderer(null!, new JsonDataWriter(), new BundleBuilder(), new DirectoryInfo(tempDir.DirectoryPath)),
            Throws.ArgumentNullException);
    }

    [Test]
    public async Task RenderAsync_GivenSakilaTables_WritesSummaryFileWithNonZeroForeignKeys()
    {
        using var tempDir = new TemporaryDirectory();
        var database = GetDatabase();
        var tables = await database.GetAllTables();

        var renderer = new ConstraintsRenderer(tables, new JsonDataWriter(), new BundleBuilder(), new DirectoryInfo(tempDir.DirectoryPath));
        await renderer.RenderAsync();

        var outputFile = Path.Combine(tempDir.DirectoryPath, "data", "constraints.json");
        var content = await File.ReadAllTextAsync(outputFile);

        // "film_actor" has foreign keys to both "actor" and "film", so the schema-wide foreign
        // key count must be greater than zero.
        Assert.That(content, Does.Not.Contain("\"foreignKeysCount\":0"));
    }

    [Test]
    public async Task RenderAsync_GivenSakilaTables_RegistersSummaryPayloadUnderConstraintsBundleKey()
    {
        using var tempDir = new TemporaryDirectory();
        var database = GetDatabase();
        var tables = await database.GetAllTables();
        var bundle = new BundleBuilder();

        var renderer = new ConstraintsRenderer(tables, new JsonDataWriter(), bundle, new DirectoryInfo(tempDir.DirectoryPath));
        await renderer.RenderAsync();

        var bundleFile = new FileInfo(Path.Combine(tempDir.DirectoryPath, "bundle.js"));
        await bundle.WriteBundleAsync(bundleFile);
        var bundleContent = await File.ReadAllTextAsync(bundleFile.FullName);

        Assert.That(bundleContent, Does.Contain("window.__schematic[\"constraints\"]"));
    }
}
