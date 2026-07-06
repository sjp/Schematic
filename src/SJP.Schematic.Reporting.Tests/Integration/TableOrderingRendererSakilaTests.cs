using System.IO;
using System.Threading.Tasks;
using NUnit.Framework;
using SJP.Schematic.Reporting.Html.Renderers;
using SJP.Schematic.Reporting.Serialization;
using SJP.Schematic.Reporting.Tests.Html.Renderers;
using SJP.Schematic.Tests.Utilities;
using SJP.Schematic.Tests.Utilities.Integration;

namespace SJP.Schematic.Reporting.Tests.Integration;

internal sealed class TableOrderingRendererSakilaTests : SakilaTest
{
    [Test]
    public void Ctor_GivenNullDialect_ThrowsArgumentNullException()
    {
        Assert.That(() => new TableOrderingRenderer(null!), Throws.ArgumentNullException);
    }

    [Test]
    public async Task RenderAsync_GivenSakilaTables_WritesInsertionAndDeletionOrderFiles()
    {
        using var tempDir = new TemporaryDirectory();
        var database = GetDatabase();
        var tables = await database.GetAllTables();

        var renderer = new TableOrderingRenderer(Connection.Dialect);
        var data = ReportDataFactory.Create(tables: tables);
        var context = new RenderContext(new JsonDataWriter(), new BundleBuilder(), new DirectoryInfo(tempDir.DirectoryPath));
        await renderer.RenderAsync(data, context);

        var exportsDirectory = Path.Combine(tempDir.DirectoryPath, "exports");
        using (Assert.EnterMultipleScope())
        {
            Assert.That(File.Exists(Path.Combine(exportsDirectory, "insertion-order.sql")), Is.True);
            Assert.That(File.Exists(Path.Combine(exportsDirectory, "deletion-order.sql")), Is.True);
        }
    }

    [Test]
    public async Task RenderAsync_GivenSakilaTables_InsertionOrderPlacesParentTableBeforeChild()
    {
        using var tempDir = new TemporaryDirectory();
        var database = GetDatabase();
        var tables = await database.GetAllTables();

        var renderer = new TableOrderingRenderer(Connection.Dialect);
        var data = ReportDataFactory.Create(tables: tables);
        var context = new RenderContext(new JsonDataWriter(), new BundleBuilder(), new DirectoryInfo(tempDir.DirectoryPath));
        await renderer.RenderAsync(data, context);

        var exportsDirectory = Path.Combine(tempDir.DirectoryPath, "exports");
        var content = await File.ReadAllTextAsync(Path.Combine(exportsDirectory, "insertion-order.sql"));
        var actorIndex = content.IndexOf("\"actor\"", System.StringComparison.Ordinal);
        var filmActorIndex = content.IndexOf("\"film_actor\"", System.StringComparison.Ordinal);

        // "film_actor" has a foreign key to "actor", so a correct insertion order must create
        // "actor" first.
        using (Assert.EnterMultipleScope())
        {
            Assert.That(actorIndex, Is.GreaterThanOrEqualTo(0));
            Assert.That(filmActorIndex, Is.GreaterThanOrEqualTo(0));
            Assert.That(actorIndex, Is.LessThan(filmActorIndex));
        }
    }
}
