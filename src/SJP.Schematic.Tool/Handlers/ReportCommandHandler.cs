using System.IO;
using System.Threading;
using System.Threading.Tasks;
using SJP.Schematic.Reporting;

namespace SJP.Schematic.Tool.Handlers
{
    internal class ReportCommandHandler : DatabaseCommandHandler
    {
        public ReportCommandHandler(FileInfo filePath)
            : base(filePath)
        {
        }

        public async Task<int> HandleCommand(DirectoryInfo outputPath, CancellationToken cancellationToken)
        {
            var connection = await GetSchematicConnectionAsync(cancellationToken).ConfigureAwait(false);
            var database = await connection.Dialect.GetRelationalDatabaseAsync(connection, cancellationToken).ConfigureAwait(false);

            var reportGenerator = new ReportExporter(connection, database, outputPath);

            await reportGenerator.ExportAsync(cancellationToken).ConfigureAwait(false);
            return ErrorCode.Success;
        }
    }
}
