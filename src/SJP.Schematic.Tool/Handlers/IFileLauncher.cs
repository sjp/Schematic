namespace SJP.Schematic.Tool.Handlers;

// Seam over launching a file with its OS-default handler, so commands don't need to
// shell out directly and tests can substitute a fake instead of opening a real browser.
internal interface IFileLauncher
{
    bool TryOpen(string path);
}
