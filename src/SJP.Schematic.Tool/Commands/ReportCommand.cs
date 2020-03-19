using System;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Reporting.Html;

namespace SJP.Schematic.Tool
{
    [Command(Name = "report", Description = "Creates a graphical report on database schema and relationships.")]
    internal sealed class ReportCommand
    {
        private DatabaseCommand Parent { get; set; }

        [Option(Description = "A directory where a database report will be exported", LongName = "report-export-dir", ShortName = "rd")]
        [LegalFilePath]
        public string ReportDirectory { get; set; }

        private Task<int> OnExecuteAsync(CommandLineApplication application)
        {
            if (application == null)
                throw new ArgumentNullException(nameof(application));

            return OnExecuteAsyncCore(application);
        }

        private async Task<int> OnExecuteAsyncCore(CommandLineApplication application)
        {
            var connectionString = await Parent.TryGetConnectionStringAsync().ConfigureAwait(false);
            if (connectionString.IsNullOrWhiteSpace())
            {
                await application.Error.WriteLineAsync().ConfigureAwait(false);
                await application.Error.WriteLineAsync("Unable to continue without a connection string. Exiting.").ConfigureAwait(false);
                return 1;
            }

            var status = await Parent.GetConnectionStatusAsync(connectionString).ConfigureAwait(false);
            if (status.IsConnected)
            {
                try
                {
                    var dialect = Parent.GetDatabaseDialect();
                    var database = await dialect.GetRelationalDatabaseAsync(status.Connection).ConfigureAwait(false);

                    var reportExporter = new ReportExporter(status.Connection, database, ReportDirectory);
                    await reportExporter.ExportAsync().ConfigureAwait(false);

                    await application.Out.WriteLineAsync("The database report has been exported to: " + ReportDirectory).ConfigureAwait(false);
                    return 0;
                }
                catch (Exception ex)
                {
                    await application.Error.WriteLineAsync("An error occurred generating a report.").ConfigureAwait(false);
                    await application.Error.WriteLineAsync().ConfigureAwait(false);
                    await application.Error.WriteLineAsync("Error message: " + ex.Message).ConfigureAwait(false);
                    await application.Error.WriteLineAsync("Stack trace: " + ex.StackTrace).ConfigureAwait(false);

                    return 1;
                }
                finally
                {
                    status.Connection.Close();
                }
            }

            application.ShowHelp();
            return 1;
        }
    }
}
