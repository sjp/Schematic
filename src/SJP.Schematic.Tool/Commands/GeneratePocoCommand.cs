using System.Threading;
using System.Threading.Tasks;
using SJP.Schematic.Tool.Handlers;
using Spectre.Console;
using Spectre.Console.Cli;

namespace SJP.Schematic.Tool.Commands;

internal sealed class GeneratePocoCommand : AsyncCommand<GeneratePocoCommand.Settings>
{
    public sealed class Settings : OrmCommand.Settings
    {
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        var handler = new GeneratePocoCommandHandler(AnsiConsole.Console, settings.ConfigFile!);
        return await handler.HandleCommandAsync(settings.ProjectPath!, settings.BaseNamespace!, settings.NamingConvention, CancellationToken.None).ConfigureAwait(false);
    }
}