using SJP.Schematic.Tool.Handlers;
using System.CommandLine;
using System.CommandLine.NamingConventionBinder;
using System.IO;
using System.Threading;

namespace SJP.Schematic.Tool.Commands;

internal sealed class TestCommand : Command
{
    public TestCommand()
        : base("test", "Test a database connection to see whether it is available.")
    {
        var timeoutOption = new Option<int>(
            "--timeout",
            getDefaultValue: static () => 10,
            description: "A timeout (in seconds) to wait for."
        );

        AddOption(timeoutOption);

        Handler = CommandHandler.Create<FileInfo, IConsole, int, CancellationToken>(static (config, console, timeout, cancellationToken) =>
        {
            var handler = new TestCommandHandler(console, config);
            return handler.HandleCommandAsync(timeout, cancellationToken);
        });
    }
}
