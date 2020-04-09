using System;
using System.IO;
using System.Threading.Tasks;
using NUnit.Framework;
using SJP.Schematic.Tests.Utilities.Integration;

namespace SJP.Schematic.Reporting.Tests.Integration
{
    internal sealed class ReportExporterTests : SakilaTest
    {
        [Test]
        public async Task ExportAsync_GivenValidSqliteDatabase_GeneratesReportsWithoutError()
        {
            var database = GetDatabase();

            var outDir = string.Empty;
            try
            {
                outDir = Path.Combine(Environment.CurrentDirectory, "SakilaExport");
                if (Directory.Exists(outDir))
                    Directory.Delete(outDir, true);

                var exporter = new Html.ReportExporter(Connection, database, outDir);
                await exporter.ExportAsync().ConfigureAwait(false);
            }
            finally
            {
                if (Directory.Exists(outDir))
                    Directory.Delete(outDir, true);
            }
        }
    }
}
