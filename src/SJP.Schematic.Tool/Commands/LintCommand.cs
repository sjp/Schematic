using System;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Tool
{
    [Command(Name = "lint", Description = "Provides a lint report for potential schema issues.")]
    internal sealed class LintCommand
    {
        private DatabaseCommand Parent { get; set; }

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
                await application.Out.WriteLineAsync("Successfully connected to the database.").ConfigureAwait(false);
                return 0;
            }

            application.ShowHelp();
            return 1;
        }
    }
}
