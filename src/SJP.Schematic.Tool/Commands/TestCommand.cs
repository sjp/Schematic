using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.Threading;
using SJP.Schematic.Tool.Handlers;

namespace SJP.Schematic.Tool.Commands
{
    public class TestCommand : Command
    {
        public TestCommand()
            : base("test", "Test a database connection to see whether it is available.")
        {
            var timeoutOption = new Option<int>(
                "--timeout",
                getDefaultValue: () => 10,
                description: "A timeout (in seconds) to wait for."
            );

            AddOption(timeoutOption);

            Handler = CommandHandler.Create<FileInfo, IConsole, int, CancellationToken>((config, console, timeout, cancellationToken) =>
            {
                var handler = new TestCommandHandler(config);
                return handler.HandleCommandAsync(console, timeout, cancellationToken);
            });
        }
    }
}
