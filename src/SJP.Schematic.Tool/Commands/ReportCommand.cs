using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.Threading;
using SJP.Schematic.Tool.Handlers;

namespace SJP.Schematic.Tool.Commands
{
    internal class ReportCommand : Command
    {
        public ReportCommand()
            : base("report", "Creates a detailed graphical report on database schema and relationships.")
        {
            var outputOption = new Option<DirectoryInfo>(
                "--output",
                description: "The directory to save the generated report."
            )
            {
                IsRequired = true
            };
            AddOption(outputOption);

            Handler = CommandHandler.Create<FileInfo, IConsole, DirectoryInfo, CancellationToken>((config, console, output, cancellationToken) =>
            {
                var handler = new ReportCommandHandler(config);
                return handler.HandleCommandAsync(console, output, cancellationToken);
            });
        }
    }
}
