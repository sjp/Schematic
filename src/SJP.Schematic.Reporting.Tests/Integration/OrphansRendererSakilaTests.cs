using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Reporting.Html.Renderers;
using SJP.Schematic.Reporting.Serialization;
using SJP.Schematic.Reporting.Tests.Html.Renderers;
using SJP.Schematic.Tests.Utilities;
using SJP.Schematic.Tests.Utilities.Integration;

namespace SJP.Schematic.Reporting.Tests.Integration;

internal sealed class OrphansRendererSakilaTests : SakilaTest
{
    [Test]
    public async Task RenderAsync_GivenSakilaTables_ExcludesTablesThatHaveRelationships()
    {
        using var tempDir = new TemporaryDirectory();
        var database = GetDatabase();
        var tables = await database.GetAllTables();
        var rowCounts = new Dictionary<Identifier, ulong>();

        var renderer = new OrphansRenderer();
        var data = ReportDataFactory.Create(tables: tables, rowCounts: rowCounts);
        var context = new RenderContext(new JsonDataWriter(), new BundleBuilder(), new DirectoryInfo(tempDir.DirectoryPath));
        await renderer.RenderAsync(data, context);

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

        var renderer = new OrphansRenderer();
        var data = ReportDataFactory.Create(tables: tables, rowCounts: rowCounts);
        var context = new RenderContext(new JsonDataWriter(), bundle, new DirectoryInfo(tempDir.DirectoryPath));
        await renderer.RenderAsync(data, context);

        var bundleFile = new FileInfo(Path.Combine(tempDir.DirectoryPath, "bundle.js"));
        await bundle.WriteBundleAsync(bundleFile);
        var bundleContent = await File.ReadAllTextAsync(bundleFile.FullName);

        Assert.That(bundleContent, Does.Contain("window.__schematic[\"orphans\"]"));
    }
}
