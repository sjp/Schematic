using System.Threading.Tasks;
using NUnit.Framework;
using SJP.Schematic.Tests.Utilities;
using SJP.Schematic.Tests.Utilities.Integration;

namespace SJP.Schematic.Reporting.Tests.Integration
{
    internal sealed class ReportGeneratorTests : SakilaTest
    {
        [Test]
        public async Task ExportAsync_GivenValidSqliteDatabase_GeneratesReportsWithoutError()
        {
            var database = GetDatabase();

            using var tempDir = new TemporaryDirectory();
            var exporter = new ReportGenerator(Connection, database, tempDir.DirectoryPath);
            await exporter.ExportAsync().ConfigureAwait(false);

            Assert.Pass();
        }
    }
}
