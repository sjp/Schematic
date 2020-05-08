using System;
using System.CommandLine;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Reporting;

namespace SJP.Schematic.Tool.Handlers
{
    internal class ReportCommandHandler : DatabaseCommandHandler
    {
        public ReportCommandHandler(FileInfo filePath)
            : base(filePath)
        {
            ConfigureDotPath();
        }

        private void ConfigureDotPath()
        {
            var dotPath = Configuration.GetValue<string>("Graphviz:Dot");
            if (!dotPath.IsNullOrWhiteSpace())
                Environment.SetEnvironmentVariable("SCHEMATIC_GRAPHVIZ_DOT", dotPath, EnvironmentVariableTarget.Process);
        }

        public async Task<int> HandleCommandAsync(IConsole console, DirectoryInfo outputPath, CancellationToken cancellationToken)
        {
            var connection = GetSchematicConnection();
            var database = await connection.Dialect.GetRelationalDatabaseAsync(connection, cancellationToken).ConfigureAwait(false);

            var reportGenerator = new ReportGenerator(connection, database, outputPath);

            await reportGenerator.GenerateAsync(cancellationToken).ConfigureAwait(false);

            console.Out.Write("Report generated to: " + outputPath.FullName);
            return ErrorCode.Success;
        }
    }
}
