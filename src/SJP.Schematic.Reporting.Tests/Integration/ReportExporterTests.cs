using System.IO;
using System.Threading.Tasks;
using NUnit.Framework;
using SJP.Schematic.Tests.Utilities;

namespace SJP.Schematic.Reporting.Tests.Integration
{
    internal sealed class ReportExporterTests : SakilaTest
    {
        [Test]
        public async Task ExportAsync_GivenValidSqliteDatabase_GeneratesReportsWithoutError()
        {
            Assert.That(File.Exists(Config.SakilaDbPath), Is.True, "Expected to find a database at: " + Config.SakilaDbPath);

            var database = GetDatabase();

            using var tempDir = new TemporaryDirectory();
            var outDir = Path.Combine(tempDir.DirectoryPath, "SakilaExport");

            var exporter = new Html.ReportExporter(Connection, database, outDir);
            await exporter.ExportAsync().ConfigureAwait(false);
        }
    }
}
