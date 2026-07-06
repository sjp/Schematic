using System;
using System.Diagnostics;

namespace SJP.Schematic.Tool.Handlers;

internal sealed class FileLauncher : IFileLauncher
{
    public bool TryOpen(string path)
    {
        try
        {
            using var process = Process.Start(new ProcessStartInfo(path) { UseShellExecute = true });
            return process != null;
        }
        catch (Exception)
        {
            // Opening a file with the OS-default handler can fail for all sorts of
            // environmental reasons (headless CI, no configured handler, sandboxing).
            // The report itself was still generated successfully, so this is non-fatal.
            return false;
        }
    }
}
