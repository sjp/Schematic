using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.FileProviders;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Reporting.Html
{
    public class AssetExporter
    {
        public void SaveAssets(string directory, bool overwrite = true)
        {
            if (string.IsNullOrWhiteSpace(directory))
                throw new ArgumentNullException(nameof(directory));

            var dirInfo = new DirectoryInfo(directory);
            SaveAssets(dirInfo, overwrite);
        }

        public void SaveAssets(DirectoryInfo directory, bool overwrite = true)
        {
            if (directory == null)
                throw new ArgumentNullException(nameof(directory));

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

                if (!targetFile.Directory.Exists)
                    targetFile.Directory.Create();

                using (var stream = targetFile.OpenWrite())
                using (var resourceStream = resourceFile.CreateReadStream())
                    resourceStream.CopyTo(stream);
            }
        }

        public Task SaveAssetsAsync(string directory, bool overwrite = true)
        {
            if (string.IsNullOrWhiteSpace(directory))
                throw new ArgumentNullException(nameof(directory));

            var dirInfo = new DirectoryInfo(directory);
            return SaveAssetsAsync(dirInfo, overwrite);
        }

        public async Task SaveAssetsAsync(DirectoryInfo directory, bool overwrite = true)
        {
            if (directory == null)
                throw new ArgumentNullException(nameof(directory));

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

                if (!targetFile.Directory.Exists)
                    targetFile.Directory.Create();

                using (var stream = targetFile.OpenWrite())
                using (var resourceStream = resourceFile.CreateReadStream())
                    await resourceStream.CopyToAsync(stream).ConfigureAwait(false);
            }
        }

        private static string FileNameToRelativePath(string fileName)
        {
            if (fileName.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(fileName));

            var pieces = fileName.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries);

            // not a file path, only a file name
            if (pieces.Length < 3)
                return fileName;

            var dirNamePieces = pieces.Take(pieces.Length - 2).ToArray();
            var dirName = Path.Combine(dirNamePieces);
            var fileNameWithExtension = pieces[pieces.Length - 2] + "." + pieces[pieces.Length - 1];

            // assuming no more than 2 extensions -- refactor if more are needed
            if (!_nonStandardExtensions.Contains("." + fileNameWithExtension))
                return Path.Combine(dirName, fileNameWithExtension);

            dirNamePieces = pieces.Take(pieces.Length - 3).ToArray();
            dirName = Path.Combine(dirNamePieces);
            fileNameWithExtension = pieces[pieces.Length - 3] + "." + pieces[pieces.Length - 2] + "." + pieces[pieces.Length - 1];

            return Path.Combine(dirName, fileNameWithExtension);
        }

        private static readonly IEnumerable<string> _nonStandardExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            ".render.js", // for viz.js
            ".otf.woff",
            ".ttf.woff",
            ".otf.woff2",
            ".ttf.woff2"
        };

        private static readonly IFileProvider _fileProvider = new EmbeddedFileProvider(Assembly.GetExecutingAssembly(), Assembly.GetExecutingAssembly().GetName().Name + ".assets");
    }
}
