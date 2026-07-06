using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Reporting.Html.Renderers;
using SJP.Schematic.Reporting.Serialization;
using SJP.Schematic.Reporting.Tests.Html.Renderers;
using SJP.Schematic.Tests.Utilities;
using SJP.Schematic.Tests.Utilities.Integration;

namespace SJP.Schematic.Reporting.Tests.Integration;

internal sealed class TableRendererSakilaTests : SakilaTest
{
    [Test]
    public async Task RenderAsync_GivenSakilaTables_WritesDetailFilePerTableUnderTablesSubdirectory()
    {
        using var tempDir = new TemporaryDirectory();
        var database = GetDatabase();
        var tables = await database.GetAllTables();
        var rowCounts = new Dictionary<Identifier, ulong>();

        var renderer = new TableRenderer();
        var data = ReportDataFactory.Create(tables: tables, rowCounts: rowCounts);
        var context = new RenderContext(new JsonDataWriter(), new BundleBuilder(), new DirectoryInfo(tempDir.DirectoryPath));
        await renderer.RenderAsync(data, context);

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

        var renderer = new TableRenderer();
        var data = ReportDataFactory.Create(tables: tables, rowCounts: rowCounts);
        var context = new RenderContext(new JsonDataWriter(), bundle, new DirectoryInfo(tempDir.DirectoryPath));
        await renderer.RenderAsync(data, context);

        var actorTable = tables.Single(static t => t.Name.LocalName == "actor");
        var bundleFile = new FileInfo(Path.Combine(tempDir.DirectoryPath, "bundle.js"));
        await bundle.WriteBundleAsync(bundleFile);
        var bundleContent = await File.ReadAllTextAsync(bundleFile.FullName);

        Assert.That(bundleContent, Does.Contain($"window.__schematic[\"table\"][\"{actorTable.Name.ToSafeKey()}\"]"));
    }
}
