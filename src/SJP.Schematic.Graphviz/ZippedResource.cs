using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;

namespace SJP.Schematic.Graphviz
{
    internal sealed class ZippedResource
    {
        public ZippedResource(string resourceName)
        {
            if (string.IsNullOrWhiteSpace(resourceName))
                throw new ArgumentNullException(nameof(resourceName));

            var asm = Assembly.GetExecutingAssembly();
            var namePrefix = asm.GetName().Name + ".";
            var validNames = GetResourceNames();
            if (!validNames.Contains(namePrefix + resourceName))
                throw new FileNotFoundException("The given resource name '" + resourceName + "' does not exist within the assembly.");

            _resourceName = resourceName;
        }

        public void ExtractToDirectory(string directoryPath)
        {
            if (string.IsNullOrWhiteSpace(directoryPath))
                throw new ArgumentNullException(nameof(directoryPath));
            if (!Directory.Exists(directoryPath))
                throw new DirectoryNotFoundException(directoryPath);

            var bytes = GetResourceByName(_resourceName);
            var extractPath = Path.GetFullPath(directoryPath);

            using var sourceStream = new MemoryStream(bytes);
            using var archive = new ZipArchive(sourceStream);
            archive.ExtractToDirectory(extractPath);
        }

        private static IEnumerable<string> GetResourceNames()
        {
            var asm = Assembly.GetExecutingAssembly();
            return asm.GetManifestResourceNames();
        }

        private static byte[] GetResourceByName(string resourceName)
        {
            if (string.IsNullOrWhiteSpace(resourceName))
                throw new ArgumentNullException(nameof(resourceName));

            var asm = Assembly.GetExecutingAssembly();
            var namePrefix = asm.GetName().Name + ".";
            var qualifiedName = namePrefix + resourceName;

            using var stream = asm.GetManifestResourceStream(qualifiedName);
            using var memStream = new MemoryStream();
            stream.CopyTo(memStream);
            return memStream.ToArray();
        }

        private readonly string _resourceName;
    }
}
