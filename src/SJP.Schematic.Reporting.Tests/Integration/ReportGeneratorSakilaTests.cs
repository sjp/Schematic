using System.IO;
using System.Threading.Tasks;
using NUnit.Framework;
using SJP.Schematic.Tests.Utilities;
using SJP.Schematic.Tests.Utilities.Integration;

namespace SJP.Schematic.Reporting.Tests.Integration;

// A flagship end-to-end test: generates a full report against the real Sakila database and
// verifies the expected artefacts are written to disk, exercising every renderer, mapper, and
// the serialization/bundle pipeline together.
internal sealed class ReportGeneratorSakilaTests : SakilaTest
{
    [Test]
    public void Ctor_GivenNullConnection_ThrowsArgumentNullException()
    {
        using var tempDir = new TemporaryDirectory();
        Assert.That(() => new ReportGenerator(null!, DatabaseProvider, GetDatabase(), tempDir.DirectoryPath), Throws.ArgumentNullException);
    }

    [Test]
    public void Ctor_GivenNullDatabaseProvider_ThrowsArgumentNullException()
    {
        using var tempDir = new TemporaryDirectory();
        Assert.That(() => new ReportGenerator(Connection, null!, GetDatabase(), tempDir.DirectoryPath), Throws.ArgumentNullException);
    }

    [Test]
    public void Ctor_GivenNullDatabase_ThrowsArgumentNullException()
    {
        using var tempDir = new TemporaryDirectory();
        Assert.That(() => new ReportGenerator(Connection, DatabaseProvider, null!, tempDir.DirectoryPath), Throws.ArgumentNullException);
    }

    [Test]
    public void Ctor_GivenNullDirectory_ThrowsArgumentNullException()
    {
        Assert.That(() => new ReportGenerator(Connection, DatabaseProvider, GetDatabase(), (string)null!), Throws.ArgumentNullException);
    }

    [Test]
    public async Task GenerateAsync_GivenSakilaDatabase_CompletesWithoutThrowing()
    {
        using var tempDir = new TemporaryDirectory();
        var generator = new ReportGenerator(Connection, DatabaseProvider, GetDatabase(), tempDir.DirectoryPath);

        Assert.That(async () => await generator.GenerateAsync(), Throws.Nothing);
    }

    [Test]
    public async Task GenerateAsync_GivenSakilaDatabase_WritesExpectedDataFiles()
    {
        using var tempDir = new TemporaryDirectory();
        var generator = new ReportGenerator(Connection, DatabaseProvider, GetDatabase(), tempDir.DirectoryPath);

        await generator.GenerateAsync();

        var dataDir = Path.Combine(tempDir.DirectoryPath, "data");

        using (Assert.EnterMultipleScope())
        {
            Assert.That(File.Exists(Path.Combine(dataDir, "bundle.js")), Is.True);
            Assert.That(File.Exists(Path.Combine(dataDir, "main.json")), Is.True);
            Assert.That(File.Exists(Path.Combine(dataDir, "tables.json")), Is.True);
            Assert.That(File.Exists(Path.Combine(dataDir, "lint.json")), Is.True);
            Assert.That(File.Exists(Path.Combine(dataDir, "search.json")), Is.True);
        }
    }

    [Test]
    public async Task GenerateAsync_GivenSakilaDatabase_ExtractsReportShell()
    {
        using var tempDir = new TemporaryDirectory();
        var generator = new ReportGenerator(Connection, DatabaseProvider, GetDatabase(), tempDir.DirectoryPath);

        await generator.GenerateAsync();

        Assert.That(File.Exists(Path.Combine(tempDir.DirectoryPath, "index.html")), Is.True);
    }

    [Test]
    public async Task GenerateAsync_GivenSakilaDatabase_BundleContainsAccumulatedPayloads()
    {
        using var tempDir = new TemporaryDirectory();
        var generator = new ReportGenerator(Connection, DatabaseProvider, GetDatabase(), tempDir.DirectoryPath);

        await generator.GenerateAsync();

        var bundleContent = await File.ReadAllTextAsync(Path.Combine(tempDir.DirectoryPath, "data", "bundle.js"));

        using (Assert.EnterMultipleScope())
        {
            Assert.That(bundleContent, Does.StartWith("window.__schematic = window.__schematic || {};"));
            Assert.That(bundleContent, Does.Contain("window.__schematic[\"tables\"]"));
            Assert.That(bundleContent, Does.Contain("window.__schematic[\"table\"]"));
        }
    }
}
