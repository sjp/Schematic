using System;
using System.IO;
using System.Linq;
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

        var resourceFiles = _fileProvider.GetDirectoryContents("/");
        foreach (var resourceFile in resourceFiles)
        {
            var relativePath = FileNameToRelativePath(resourceFile.Name);
            var qualifiedPath = Path.Combine(directory.FullName, relativePath);
            var targetFile = new FileInfo(qualifiedPath);

            if (targetFile.Exists && overwrite)
                targetFile.Delete();

            if (targetFile.Directory != null && !targetFile.Directory.Exists)
                targetFile.Directory!.Create();

            await using var stream = targetFile.OpenWrite();
            await using var resourceStream = resourceFile.CreateReadStream();
            await resourceStream.CopyToAsync(stream, cancellationToken);
            await stream.FlushAsync(cancellationToken);
        }
    }

    private static string FileNameToRelativePath(string fileName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fileName);

        // The embedded resource name (after the assembly's "...assets" base namespace) flattens
        // subdirectories into dots, e.g. "assets.app-<hash>.js". Vite emits content-hashed names
        // with a single dot before the extension, so the last two pieces are always the file name
        // and extension, and any leading pieces are directory segments.
        var pieces = fileName.Split('.', StringSplitOptions.RemoveEmptyEntries);

        // not a file path, only a file name (e.g. "index.html")
        if (pieces.Length < 3)
            return fileName;

        var dirNamePieces = pieces.Take(pieces.Length - 2).ToArray();
        var dirName = Path.Combine(dirNamePieces);
        var fileNameWithExtension = pieces[^2] + "." + pieces[^1];

        return Path.Combine(dirName, fileNameWithExtension);
    }

    private static readonly IFileProvider _fileProvider = new EmbeddedFileProvider(Assembly.GetExecutingAssembly(), Assembly.GetExecutingAssembly().GetName().Name + ".assets");
}