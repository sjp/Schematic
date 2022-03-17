using System;
using System.CommandLine;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace SJP.Schematic.Tool.Handlers;

internal sealed class TestCommandHandler : DatabaseCommandHandler
{
    private readonly IConsole _console;

    public TestCommandHandler(IConsole console, FileInfo filePath)
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

            _console.SetTerminalForegroundGreen();
            _console.Out.Write("Successfully connected to the database.");
            _console.ResetTerminalForegroundColor();

            return ErrorCode.Success;
        }
        catch (OperationCanceledException)
        {
            // if this is a user-cancellation, throw, otherwise it must have timed-out
            cancellationToken.ThrowIfCancellationRequested();

            // only handling when timed out, not when user interrupts
            _console.SetTerminalForegroundRed();
            _console.Error.Write("Database connection request timed out.");
            _console.ResetTerminalForegroundColor();

            return ErrorCode.Error;
        }
        catch (Exception ex)
        {
            _console.SetTerminalForegroundRed();
            _console.Error.WriteLine("Failed to connect to the database.");
            _console.Error.Write("    " + ex.Message);
            _console.ResetTerminalForegroundColor();

            return ErrorCode.Error;
        }
    }
}
