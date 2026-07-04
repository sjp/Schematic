using System.IO;
using System.Threading.Tasks;
using NUnit.Framework;
using SJP.Schematic.Reporting.Html.Renderers;
using SJP.Schematic.Tests.Utilities;
using SJP.Schematic.Tests.Utilities.Integration;

namespace SJP.Schematic.Reporting.Tests.Integration;

internal sealed class TableOrderingRendererSakilaTests : SakilaTest
{
    [Test]
    public void Ctor_GivenNullDialect_ThrowsArgumentNullException()
    {
        using var tempDir = new TemporaryDirectory();
        Assert.That(
            () => new TableOrderingRenderer(null!, [], new DirectoryInfo(tempDir.DirectoryPath)),
            Throws.ArgumentNullException);
    }

    [Test]
    public void Ctor_GivenNullTables_ThrowsArgumentNullException()
    {
        using var tempDir = new TemporaryDirectory();
        Assert.That(
            () => new TableOrderingRenderer(Connection.Dialect, null!, new DirectoryInfo(tempDir.DirectoryPath)),
            Throws.ArgumentNullException);
    }

    [Test]
    public void Ctor_GivenNullExportDirectory_ThrowsArgumentNullException()
    {
        Assert.That(() => new TableOrderingRenderer(Connection.Dialect, [], null!), Throws.ArgumentNullException);
    }

    [Test]
    public async Task RenderAsync_GivenSakilaTables_WritesInsertionAndDeletionOrderFiles()
    {
        using var tempDir = new TemporaryDirectory();
        var database = GetDatabase();
        var tables = await database.GetAllTables();

        var exportDirectory = new DirectoryInfo(Path.Combine(tempDir.DirectoryPath, "exports"));
        var renderer = new TableOrderingRenderer(Connection.Dialect, tables, exportDirectory);
        await renderer.RenderAsync();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(File.Exists(Path.Combine(exportDirectory.FullName, "insertion-order.sql")), Is.True);
            Assert.That(File.Exists(Path.Combine(exportDirectory.FullName, "deletion-order.sql")), Is.True);
        }
    }

    [Test]
    public async Task RenderAsync_GivenSakilaTables_InsertionOrderPlacesParentTableBeforeChild()
    {
        using var tempDir = new TemporaryDirectory();
        var database = GetDatabase();
        var tables = await database.GetAllTables();

        var exportDirectory = new DirectoryInfo(Path.Combine(tempDir.DirectoryPath, "exports"));
        var renderer = new TableOrderingRenderer(Connection.Dialect, tables, exportDirectory);
        await renderer.RenderAsync();

        var content = await File.ReadAllTextAsync(Path.Combine(exportDirectory.FullName, "insertion-order.sql"));
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
