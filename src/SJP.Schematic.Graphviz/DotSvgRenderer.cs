using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace SJP.Schematic.Graphviz;

/// <summary>
/// A renderer that renders a DOT file into an SVG image.
/// </summary>
/// <seealso cref="IDotSvgRenderer" />
public class DotSvgRenderer : IDotSvgRenderer
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DotSvgRenderer"/> class.
    /// </summary>
    /// <param name="dotExecutablePath">The dot executable path.</param>
    /// <exception cref="ArgumentNullException"><paramref name="dotExecutablePath"/> is <c>null</c>, empty or whitespace.</exception>
    public DotSvgRenderer(string dotExecutablePath)
    {
        if (string.IsNullOrWhiteSpace(dotExecutablePath))
            throw new ArgumentNullException(nameof(dotExecutablePath));

        _dotPath = dotExecutablePath;
    }

    /// <summary>
    /// Renders a DOT file as an SVG image synchronously.
    /// </summary>
    /// <param name="dot">A dot graph in string form.</param>
    /// <returns>A rendered SVG image as a string.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="dot"/> is <c>null</c>, empty or whitespace.</exception>
    /// <exception cref="GraphvizException">Thrown when the Graphviz process exited unsuccessfully.</exception>
    public string RenderToSvg(string dot)
    {
        if (string.IsNullOrWhiteSpace(dot))
            throw new ArgumentNullException(nameof(dot));

        var tmpInputFilePath = Path.GetRandomFileName();
        var tmpOutputFilePath = Path.GetRandomFileName();

        File.Delete(tmpInputFilePath);
        File.Delete(tmpOutputFilePath);

        try
        {
            File.WriteAllText(tmpInputFilePath, dot);

            var startInfo = new ProcessStartInfo
            {
                FileName = _dotPath,
                Arguments = $"-Tsvg \"{tmpInputFilePath}\" -o \"{tmpOutputFilePath}\"",
                RedirectStandardError = true
            };
            using (var process = new Process { StartInfo = startInfo })
            {
                process.Start();
                process.WaitForExit();

                if (process.ExitCode != ExitSuccess)
                {
                    var stdErr = process.StandardError.ReadToEnd();
                    throw new GraphvizException(process.ExitCode, stdErr);
                }
            }

            return File.ReadAllText(tmpOutputFilePath);
        }
        finally
        {
            if (File.Exists(tmpInputFilePath))
                File.Delete(tmpInputFilePath);
            if (File.Exists(tmpOutputFilePath))
                File.Delete(tmpOutputFilePath);
        }
    }

    /// <summary>
    /// Renders a DOT file as an SVG image asynchronously.
    /// </summary>
    /// <param name="dot">A dot graph in string form.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A rendered SVG image as a string.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="dot"/> is <c>null</c>, empty or whitespace.</exception>
    /// <exception cref="GraphvizException">Thrown when the Graphviz process exited unsuccessfully.</exception>
    public Task<string> RenderToSvgAsync(string dot, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(dot))
            throw new ArgumentNullException(nameof(dot));

        return RenderToSvgAsyncCore(dot, cancellationToken);
    }

    private async Task<string> RenderToSvgAsyncCore(string dot, CancellationToken cancellationToken)
    {
        var tmpInputFilePath = Path.GetRandomFileName();
        var tmpOutputFilePath = Path.GetRandomFileName();

        File.Delete(tmpInputFilePath);
        File.Delete(tmpOutputFilePath);

        try
        {
            await File.WriteAllTextAsync(tmpInputFilePath, dot, cancellationToken).ConfigureAwait(false);

            var startInfo = new ProcessStartInfo
            {
                FileName = _dotPath,
                Arguments = $"-Tsvg \"{tmpInputFilePath}\" -o \"{tmpOutputFilePath}\"",
                RedirectStandardError = true
            };
            using (var process = new Process { StartInfo = startInfo })
            {
                process.Start();
                await process.WaitForExitAsync(cancellationToken).ConfigureAwait(false);

                if (process.ExitCode != ExitSuccess)
                {
                    var stdErr = await process.StandardError.ReadToEndAsync(cancellationToken).ConfigureAwait(false);
                    throw new GraphvizException(process.ExitCode, stdErr);
                }
            }

            return await File.ReadAllTextAsync(tmpOutputFilePath, cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            if (File.Exists(tmpInputFilePath))
                File.Delete(tmpInputFilePath);
            if (File.Exists(tmpOutputFilePath))
                File.Delete(tmpOutputFilePath);
        }
    }

    private readonly string _dotPath;

    private const int ExitSuccess = 0;
}