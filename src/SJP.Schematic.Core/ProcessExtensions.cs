using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace SJP.Schematic.Core.Extensions
{
    // largely taken from Microsoft.VisualStudio.Threading.AwaitExtensions
    public static class ProcessExtensions
    {
        /// <summary>
        /// Returns a task that completes when the process exits and provides the exit code of that process.
        /// </summary>
        /// <param name="process">The process to wait for exit.</param>
        /// <param name="cancellationToken">A token whose cancellation will cause the returned Task to complete before the process exits in a faulted state with an <see cref="OperationCanceledException"/>. This token has no effect on the <paramref name="process"/> itself.</param>
        /// <returns>A task whose result is the <see cref="Process.ExitCode"/> of the <paramref name="process"/>.</returns>
        public static Task<int> WaitForExitAsync(this Process process, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (process == null)
                throw new ArgumentNullException(nameof(process));

            return WaitForExitAsyncCore(process, cancellationToken);
        }

        private static async Task<int> WaitForExitAsyncCore(Process process, CancellationToken cancellationToken)
        {
            var tcs = new TaskCompletionSource<int>();

            void ExitHandler(object s, EventArgs e) => tcs.TrySetResult(process.ExitCode);

            try
            {
                process.EnableRaisingEvents = true;
                process.Exited += ExitHandler;
                if (process.HasExited)
                {
                    // Allow for the race condition that the process has already exited.
                    tcs.TrySetResult(process.ExitCode);
                }

                using (cancellationToken.Register(() => tcs.TrySetCanceled(cancellationToken)))
                {
                    return await tcs.Task.ConfigureAwait(false);
                }
            }
            finally
            {
                process.Exited -= ExitHandler;
            }
        }
    }
}
