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

internal sealed class RelationshipsRendererSakilaTests : SakilaTest
{
    [Test]
    public void Ctor_GivenNullTables_ThrowsArgumentNullException()
    {
        using var tempDir = new TemporaryDirectory();
        var rowCounts = new Dictionary<Identifier, ulong>();
        Assert.That(
            () => new RelationshipsRenderer(null!, rowCounts, new JsonDataWriter(), new BundleBuilder(), new DirectoryInfo(tempDir.DirectoryPath)),
            Throws.ArgumentNullException);
    }

    [Test]
    public void Ctor_GivenNullExportDirectory_ThrowsArgumentNullException()
    {
        var rowCounts = new Dictionary<Identifier, ulong>();
        Assert.That(
            () => new RelationshipsRenderer([], rowCounts, new JsonDataWriter(), new BundleBuilder(), null!),
            Throws.ArgumentNullException);
    }

    [Test]
    public async Task RenderAsync_GivenSakilaTables_WritesGraphWithMatchingNodeAndEdgeCounts()
    {
        using var tempDir = new TemporaryDirectory();
        var database = GetDatabase();
        var tables = await database.GetAllTables();
        var rowCounts = new Dictionary<Identifier, ulong>();

        var renderer = new RelationshipsRenderer(tables, rowCounts, new JsonDataWriter(), new BundleBuilder(), new DirectoryInfo(tempDir.DirectoryPath));
        await renderer.RenderAsync();

        var outputFile = Path.Combine(tempDir.DirectoryPath, "data", "relationships.json");
        var content = await File.ReadAllTextAsync(outputFile);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(content, Does.Contain($"\"nodesCount\":{tables.Count}"));
            // Sakila's foreign-key relationships (e.g. film_actor -> actor, film_actor -> film)
            // guarantee at least one edge in the graph.
            Assert.That(content, Does.Not.Contain("\"edgesCount\":0"));
        }
    }

    [Test]
    public async Task RenderAsync_GivenSakilaTables_RegistersSummaryPayloadUnderRelationshipsBundleKey()
    {
        using var tempDir = new TemporaryDirectory();
        var database = GetDatabase();
        var tables = await database.GetAllTables();
        var rowCounts = new Dictionary<Identifier, ulong>();
        var bundle = new BundleBuilder();

        var renderer = new RelationshipsRenderer(tables, rowCounts, new JsonDataWriter(), bundle, new DirectoryInfo(tempDir.DirectoryPath));
        await renderer.RenderAsync();

        var bundleFile = new FileInfo(Path.Combine(tempDir.DirectoryPath, "bundle.js"));
        await bundle.WriteBundleAsync(bundleFile);
        var bundleContent = await File.ReadAllTextAsync(bundleFile.FullName);

        Assert.That(bundleContent, Does.Contain("window.__schematic[\"relationships\"]"));
    }
}
