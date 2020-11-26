using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.Threading;
using SJP.Schematic.Tool.Handlers;

namespace SJP.Schematic.Tool.Commands
{
    internal sealed class LintCommand : Command
    {
        public LintCommand()
            : base("lint", "Provides a lint report for potential schema issues.")
        {
            Handler = CommandHandler.Create<IConsole, FileInfo, CancellationToken>(static (console, config, cancellationToken) =>
            {
                var handler = new LintCommandHandler(config);
                return handler.HandleCommandAsync(console, cancellationToken);
            });
        }
    }
}
