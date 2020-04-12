using System;
using System.CommandLine;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace SJP.Schematic.Tool.Handlers
{
    internal class TestCommandHandler : DatabaseCommand
    {
        public TestCommandHandler(FileInfo filePath)
            : base(filePath)
        {
        }

        public async Task<int> HandleCommand(IConsole console, int timeout, CancellationToken cancellationToken)
        {
            var factory = GetConnectionFactory();
            var connectionString = GetConnectionString();

            try
            {
                using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                cts.CancelAfter(TimeSpan.FromSeconds(timeout));

                _ = await factory.CreateConnectionAsync(connectionString, cts.Token).ConfigureAwait(false);

                console.SetTerminalForegroundGreen();
                console.Out.Write("Successfully connected to the database.");
                console.ResetTerminalForegroundColor();

                return ErrorCode.Success;
            }
            catch (OperationCanceledException)
            {
                // if this is a user-cancellation, throw, otherwise it must have timed-out
                cancellationToken.ThrowIfCancellationRequested();

                // only handling when timed out, not when user interrupts
                console.SetTerminalForegroundRed();
                console.Error.Write("Database connection request timed out.");
                console.ResetTerminalForegroundColor();

                return ErrorCode.Error;
            }
            catch (Exception ex)
            {
                console.SetTerminalForegroundRed();
                console.Error.WriteLine("Failed to connect to the database.");
                console.Error.Write("    " + ex.Message);
                console.ResetTerminalForegroundColor();

                return ErrorCode.Error;
            }
        }
    }
}
