using System;
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

    private readonly IAnsiConsole _console;
    private readonly IDatabaseCommandDependencyProviderFactory _dependencyProviderFactory;

    public TestCommand(
        IAnsiConsole console,
        IDatabaseCommandDependencyProviderFactory dependencyProviderFactory)
    {
        ArgumentNullException.ThrowIfNull(console);
        ArgumentNullException.ThrowIfNull(dependencyProviderFactory);

        _console = console;
        _dependencyProviderFactory = dependencyProviderFactory;
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        var cancellationToken = CancellationToken.None;
        var dependencyProvider = _dependencyProviderFactory.GetDbDependencies(settings.ConfigFile!.FullName);
        var connectionFactory = dependencyProvider.GetConnectionFactory();

        try
        {
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(TimeSpan.FromSeconds(settings.Timeout));

            _ = await connectionFactory.OpenConnectionAsync(cts.Token).ConfigureAwait(false);

            _console.MarkupLine("[green]Successfully connected to the database.[/]");

            return ErrorCode.Success;
        }
        catch (OperationCanceledException)
        {
            // if this is a user-cancellation, throw, otherwise it must have timed-out
            cancellationToken.ThrowIfCancellationRequested();

            // only handling when timed out, not when user interrupts
            _console.MarkupLine("[red]Database connection request timed out.[/]");

            return ErrorCode.Error;
        }
        catch (Exception ex)
        {
            _console.MarkupLine("[red]Failed to connect to the database.[/red]");
            _console.MarkupLine("    [red]" + ex.Message + "[/]");

            return ErrorCode.Error;
        }
    }
}