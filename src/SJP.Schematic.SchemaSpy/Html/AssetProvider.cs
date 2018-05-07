using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace SJP.Schematic.SchemaSpy.Html
{
    public class AssetProvider
    {
        public Stream GetResource(string resourceName)
        {
            var assetResourceName = AssetResourceNames.FirstOrDefault(name => resourceName == name);
            if (assetResourceName == null)
                return null;

            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName))
                return stream;
        }

        public IEnumerable<string> AssetResourceNames => _assetResourceNames;

        private static readonly IEnumerable<string> _assetResourceNames = Assembly.GetExecutingAssembly()
            .GetManifestResourceNames()
            .Where(name => name.Contains(".assets."))
            .ToList();
    }
}
