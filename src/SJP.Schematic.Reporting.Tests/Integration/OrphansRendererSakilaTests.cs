using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Reporting.Html.Renderers;
using SJP.Schematic.Reporting.Serialization;
using SJP.Schematic.Tests.Utilities;
using SJP.Schematic.Tests.Utilities.Integration;

namespace SJP.Schematic.Reporting.Tests.Integration;

internal sealed class OrphansRendererSakilaTests : SakilaTest
{
    [Test]
    public void Ctor_GivenNullTables_ThrowsArgumentNullException()
    {
        using var tempDir = new TemporaryDirectory();
        var rowCounts = new Dictionary<Identifier, ulong>();
        Assert.That(
            () => new OrphansRenderer(null!, rowCounts, new JsonDataWriter(), new BundleBuilder(), new DirectoryInfo(tempDir.DirectoryPath)),
            Throws.ArgumentNullException);
    }

    [Test]
    public void Ctor_GivenNullRowCounts_ThrowsArgumentNullException()
    {
        using var tempDir = new TemporaryDirectory();
        Assert.That(
            () => new OrphansRenderer([], null!, new JsonDataWriter(), new BundleBuilder(), new DirectoryInfo(tempDir.DirectoryPath)),
            Throws.ArgumentNullException);
    }

    [Test]
    public async Task RenderAsync_GivenSakilaTables_ExcludesTablesThatHaveRelationships()
    {
        using var tempDir = new TemporaryDirectory();
        var database = GetDatabase();
        var tables = await database.GetAllTables();
        var rowCounts = new Dictionary<Identifier, ulong>();

        var renderer = new OrphansRenderer(tables, rowCounts, new JsonDataWriter(), new BundleBuilder(), new DirectoryInfo(tempDir.DirectoryPath));
        await renderer.RenderAsync();

        var outputFile = Path.Combine(tempDir.DirectoryPath, "data", "orphans.json");
        var content = await File.ReadAllTextAsync(outputFile);

        // "actor" participates in a relationship (via film_actor), so it must never be reported
        // as an orphan.
        Assert.That(content, Does.Not.Contain("\"name\":\"actor\""));
    }

    [Test]
    public async Task RenderAsync_GivenSakilaTables_RegistersSummaryPayloadUnderOrphansBundleKey()
    {
        using var tempDir = new TemporaryDirectory();
        var database = GetDatabase();
        var tables = await database.GetAllTables();
        var rowCounts = new Dictionary<Identifier, ulong>();
        var bundle = new BundleBuilder();

        var renderer = new OrphansRenderer(tables, rowCounts, new JsonDataWriter(), bundle, new DirectoryInfo(tempDir.DirectoryPath));
        await renderer.RenderAsync();

        var bundleFile = new FileInfo(Path.Combine(tempDir.DirectoryPath, "bundle.js"));
        await bundle.WriteBundleAsync(bundleFile);
        var bundleContent = await File.ReadAllTextAsync(bundleFile.FullName);

        Assert.That(bundleContent, Does.Contain("window.__schematic[\"orphans\"]"));
    }
}
