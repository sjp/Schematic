using NUnit.Framework;
using SJP.Schematic.Tests.Utilities;
using SJP.Schematic.Tests.Utilities.Integration;

namespace SJP.Schematic.Reporting.Tests.Integration
{
    internal sealed class ReportGeneratorTests : SakilaTest
    {
        [Test, GraphvizAvailable]
        public void GenerateAsync_GivenValidSqliteDatabase_GeneratesReportsWithoutError()
        {
            var database = GetDatabase();

            using var tempDir = new TemporaryDirectory();
            var exporter = new ReportGenerator(Connection, database, tempDir.DirectoryPath);
            Assert.That(async () => await exporter.GenerateAsync().ConfigureAwait(false), Throws.Nothing);
        }
    }
}
