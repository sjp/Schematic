using System;
using McMaster.Extensions.CommandLineUtils;
using SJP.Schematic.Core.Caching;
using SJP.Schematic.Reporting.Html;

namespace SJP.Schematic.Tool
{
    [Command(Name = "report", Description = "Creates a graphical report on database schema and relationships.")]
    internal sealed class ReportCommand
    {
        private DatabaseCommand Parent { get; set; }

        [Option(Description = "A directory where a database report will be exported", LongName = "report-dir", ShortName = "rd")]
        [LegalFilePath]
        public string ReportDirectory { get; set; }

        private int OnExecute(CommandLineApplication application)
        {
            if (application == null)
                throw new ArgumentNullException(nameof(application));

            var hasConnectionString = Parent.TryGetConnectionString(out var connectionString);
            if (!hasConnectionString)
            {
                application.Error.WriteLine();
                application.Error.WriteLine("Unable to continue without a connection string. Exiting.");
                return 1;
            }

            var status = Parent.GetConnectionStatus(connectionString);
            if (status.IsConnected)
            {
                try
                {
                    var cachedConnection = status.Connection.AsCachedConnection();
                    var dialect = Parent.GetDatabaseDialect(cachedConnection);
                    var database = dialect.GetRelationalDatabaseAsync().GetAwaiter().GetResult();

                    var reportExporter = new ReportExporter(cachedConnection, database, ReportDirectory);
                    reportExporter.ExportAsync().GetAwaiter().GetResult();

                    application.Out.WriteLine("The database report has been exported to: " + ReportDirectory);
                    return 0;
                }
                catch (Exception ex)
                {
                    application.Error.WriteLine("An error occurred generating a report.");
                    application.Error.WriteLine();
                    application.Error.WriteLine("Error message: " + ex.Message);
                    application.Error.WriteLine("Stack trace: " + ex.StackTrace);

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
