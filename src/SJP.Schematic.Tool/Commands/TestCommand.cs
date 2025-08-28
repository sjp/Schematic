using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using SJP.Schematic.Tool.Handlers;
using Spectre.Console;
using Spectre.Console.Cli;

namespace SJP.Schematic.Tool.Commands;

[Description("Test a database connection to see whether it is available.")]
internal sealed class TestCommand : AsyncCommand<TestCommand.Settings>
{
    public sealed class Settings : CommonSettings
    {
        [CommandOption("-t|--timeout")]
        [Description("A timeout (in seconds) to wait for.")]
        [DefaultValue(10)]
        public int Timeout { get; init; }
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        var handler = new TestCommandHandler(AnsiConsole.Console, settings.ConfigFile!);
        return await handler.HandleCommandAsync(settings.Timeout, CancellationToken.None).ConfigureAwait(false);
    }
}