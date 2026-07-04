using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Reporting.Html.Renderers;
using SJP.Schematic.Reporting.Serialization;
using SJP.Schematic.Tests.Utilities;
using SJP.Schematic.Tests.Utilities.Integration;

namespace SJP.Schematic.Reporting.Tests.Integration;

internal sealed class TableRendererSakilaTests : SakilaTest
{
    [Test]
    public void Ctor_GivenNullTables_ThrowsArgumentNullException()
    {
        using var tempDir = new TemporaryDirectory();
        var rowCounts = new Dictionary<Identifier, ulong>();
        Assert.That(
            () => new TableRenderer(null!, rowCounts, new JsonDataWriter(), new BundleBuilder(), new DirectoryInfo(tempDir.DirectoryPath)),
            Throws.ArgumentNullException);
    }

    [Test]
    public void Ctor_GivenNullRowCounts_ThrowsArgumentNullException()
    {
        using var tempDir = new TemporaryDirectory();
        Assert.That(
            () => new TableRenderer([], null!, new JsonDataWriter(), new BundleBuilder(), new DirectoryInfo(tempDir.DirectoryPath)),
            Throws.ArgumentNullException);
    }

    [Test]
    public async Task RenderAsync_GivenSakilaTables_WritesDetailFilePerTableUnderTablesSubdirectory()
    {
        using var tempDir = new TemporaryDirectory();
        var database = GetDatabase();
        var tables = await database.GetAllTables();
        var rowCounts = new Dictionary<Identifier, ulong>();

        var renderer = new TableRenderer(tables, rowCounts, new JsonDataWriter(), new BundleBuilder(), new DirectoryInfo(tempDir.DirectoryPath));
        await renderer.RenderAsync();

        var actorTable = tables.Single(static t => t.Name.LocalName == "actor");
        var expectedFile = Path.Combine(tempDir.DirectoryPath, "data", "tables", actorTable.Name.ToSafeKey() + ".json");

        Assert.That(File.Exists(expectedFile), Is.True);
    }

    [Test]
    public async Task RenderAsync_GivenSakilaTables_RegistersDetailPayloadPerTableUnderTableBundleKey()
    {
        using var tempDir = new TemporaryDirectory();
        var database = GetDatabase();
        var tables = await database.GetAllTables();
        var rowCounts = new Dictionary<Identifier, ulong>();
        var bundle = new BundleBuilder();

        var renderer = new TableRenderer(tables, rowCounts, new JsonDataWriter(), bundle, new DirectoryInfo(tempDir.DirectoryPath));
        await renderer.RenderAsync();

        var actorTable = tables.Single(static t => t.Name.LocalName == "actor");
        var bundleFile = new FileInfo(Path.Combine(tempDir.DirectoryPath, "bundle.js"));
        await bundle.WriteBundleAsync(bundleFile);
        var bundleContent = await File.ReadAllTextAsync(bundleFile.FullName);

        Assert.That(bundleContent, Does.Contain($"window.__schematic[\"table\"][\"{actorTable.Name.ToSafeKey()}\"]"));
    }
}
