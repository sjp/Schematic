using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using SJP.Schematic.Tool.Handlers;
using Spectre.Console;
using Spectre.Console.Cli;

namespace SJP.Schematic.Tool.Commands;

internal sealed class ReportCommand : AsyncCommand<ReportCommand.Settings>
{
    public sealed class Settings : CommonSettings
    {
        [CommandOption("--output <DIRECTORY>")]
        [Description("The directory to save the generated report.")]
        public DirectoryInfo? OutputDirectory { get; init; }
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        var handler = new ReportCommandHandler(AnsiConsole.Console, settings.ConfigFile!);
        return await handler.HandleCommandAsync(settings.OutputDirectory!, CancellationToken.None).ConfigureAwait(false);
    }
}