using SJP.Schematic.Tool.Handlers;
using System.CommandLine;
using System.CommandLine.NamingConventionBinder;
using System.IO;
using System.Threading;

namespace SJP.Schematic.Tool.Commands;

internal sealed class LintCommand : Command
{
    public LintCommand()
        : base("lint", "Provides a lint report for potential schema issues.")
    {
        Handler = CommandHandler.Create<IConsole, FileInfo, CancellationToken>(static (console, config, cancellationToken) =>
        {
            var handler = new LintCommandHandler(console, config);
            return handler.HandleCommandAsync(cancellationToken);
        });
    }
}
