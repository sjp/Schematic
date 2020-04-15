using System.CommandLine;
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

        public async Task<int> HandleCommand(IConsole console, DirectoryInfo outputPath, CancellationToken cancellationToken)
        {
            var connection = await GetSchematicConnectionAsync(cancellationToken).ConfigureAwait(false);
            var database = await connection.Dialect.GetRelationalDatabaseAsync(connection, cancellationToken).ConfigureAwait(false);

            var reportGenerator = new ReportGenerator(connection, database, outputPath);

            await reportGenerator.ExportAsync(cancellationToken).ConfigureAwait(false);

            console.Out.Write("Report generated to: " + outputPath.FullName);
            return ErrorCode.Success;
        }
    }
}
