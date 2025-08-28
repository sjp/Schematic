using System.Threading;
using System.Threading.Tasks;
using SJP.Schematic.Tool.Handlers;
using Spectre.Console;
using Spectre.Console.Cli;

namespace SJP.Schematic.Tool.Commands;

internal sealed class LintCommand : AsyncCommand<LintCommand.Settings>
{
    public sealed class Settings : CommonSettings
    {
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        var handler = new LintCommandHandler(AnsiConsole.Console, settings.ConfigFile!);
        return await handler.HandleCommandAsync(CancellationToken.None).ConfigureAwait(false);
    }
}