using System;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.FileProviders;

namespace SJP.Schematic.Reporting.Html;

internal sealed class AssetExporter
{
    public Task SaveAssetsAsync(string directory, bool overwrite = true, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(directory);

        var dirInfo = new DirectoryInfo(directory);
        return SaveAssetsAsync(dirInfo, overwrite, cancellationToken);
    }

    public Task SaveAssetsAsync(DirectoryInfo directory, bool overwrite = true, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(directory);

        return SaveAssetsAsyncCore(directory, overwrite, cancellationToken);
    }

    private static async Task SaveAssetsAsyncCore(DirectoryInfo directory, bool overwrite, CancellationToken cancellationToken)
    {
        if (!directory.Exists)
            directory.Create();

        await SaveDirectoryAsync("/", directory.FullName, overwrite, cancellationToken).ConfigureAwait(false);
    }

    // Recursively copies the embedded Vite build, preserving its directory structure. The manifest
    // file provider exposes the original relative paths (e.g. "index.html", "assets/app-<hash>.js"),
    // so the layout maps one-to-one onto the output directory with no path reconstruction.
    private static async Task SaveDirectoryAsync(string sourcePath, string targetDirectory, bool overwrite, CancellationToken cancellationToken)
    {
        foreach (var entry in _fileProvider.GetDirectoryContents(sourcePath))
        {
            cancellationToken.ThrowIfCancellationRequested();

            // The provider surfaces its own manifest at the root; it is an implementation detail
            // of the embedding, not part of the report, so don't copy it out.
            if (string.Equals(entry.Name, ManifestFileName, StringComparison.Ordinal))
                continue;

            // File provider paths are always '/'-separated, independent of the host OS.
            var childSourcePath = sourcePath == "/"
                ? "/" + entry.Name
                : sourcePath + "/" + entry.Name;

            if (entry.IsDirectory)
            {
                var childTargetDirectory = Path.Combine(targetDirectory, entry.Name);
                Directory.CreateDirectory(childTargetDirectory);
                await SaveDirectoryAsync(childSourcePath, childTargetDirectory, overwrite, cancellationToken).ConfigureAwait(false);
                continue;
            }

            var targetFile = new FileInfo(Path.Combine(targetDirectory, entry.Name));

            // overwrite == false means leave an existing file untouched.
            if (targetFile.Exists && !overwrite)
                continue;

            if (targetFile.Directory is { Exists: false } parent)
                parent.Create();

            // FileMode.Create truncates, so a shorter asset never leaves stale trailing bytes.
            await using var resourceStream = entry.CreateReadStream();
            await using var fileStream = new FileStream(targetFile.FullName, FileMode.Create, FileAccess.Write, FileShare.None);
            await resourceStream.CopyToAsync(fileStream, cancellationToken).ConfigureAwait(false);
        }
    }

    // Default name emitted by GenerateEmbeddedFilesManifest (see EmbeddedFilesManifestFileName).
    private const string ManifestFileName = "Microsoft.Extensions.FileProviders.Embedded.Manifest.xml";

    private static readonly IFileProvider _fileProvider = new ManifestEmbeddedFileProvider(Assembly.GetExecutingAssembly());
}
