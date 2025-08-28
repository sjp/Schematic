using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Spectre.Console;

namespace SJP.Schematic.Tool.Handlers;

internal sealed class TestCommandHandler : DatabaseCommandHandler
{
    private readonly IAnsiConsole _console;

    public TestCommandHandler(IAnsiConsole console, FileInfo filePath)
        : base(filePath)
    {
        _console = console ?? throw new ArgumentNullException(nameof(console));
    }

    public async Task<int> HandleCommandAsync(int timeout, CancellationToken cancellationToken)
    {
        var factory = GetConnectionFactory();

        try
        {
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(TimeSpan.FromSeconds(timeout));

            _ = await factory.OpenConnectionAsync(cts.Token).ConfigureAwait(false);

            _console.MarkupLine("[green]Successfully connected to the database.[/green]");

            return ErrorCode.Success;
        }
        catch (OperationCanceledException)
        {
            // if this is a user-cancellation, throw, otherwise it must have timed-out
            cancellationToken.ThrowIfCancellationRequested();

            // only handling when timed out, not when user interrupts
            _console.MarkupLine("[red]Database connection request timed out.[/red]");

            return ErrorCode.Error;
        }
        catch (Exception ex)
        {
            _console.MarkupLine("[red]Failed to connect to the database.[/red]");
            _console.MarkupLine("    [red]" + ex.Message + "[/red]");

            return ErrorCode.Error;
        }
    }
}