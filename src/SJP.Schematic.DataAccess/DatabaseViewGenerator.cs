using System;
using System.Collections.Generic;
using System.IO;
using SJP.Schematic.Core;

namespace SJP.Schematic.DataAccess
{
    public abstract class DatabaseViewGenerator : IDatabaseViewGenerator
    {
        protected DatabaseViewGenerator(INameProvider nameProvider)
        {
            NameProvider = nameProvider ?? throw new ArgumentNullException(nameof(nameProvider));
        }

        protected INameProvider NameProvider { get; }

        public abstract string Generate(IRelationalDatabaseView view);

        public virtual FileInfo GetFilePath(DirectoryInfo baseDirectory, Identifier objectName)
        {
            if (baseDirectory == null)
                throw new ArgumentNullException(nameof(baseDirectory));
            if (objectName == null || objectName.LocalName == null)
                throw new ArgumentNullException(nameof(objectName));

            var paths = new List<string> { baseDirectory.FullName, "Views" };
            if (objectName.Schema != null)
            {
                var schemaName = NameProvider.SchemaToNamespace(objectName);
                paths.Add(objectName.Schema);
            }

            var viewName = NameProvider.ViewToClassName(objectName);
            paths.Add(viewName + ".cs");

            var viewPath = Path.Combine(paths.ToArray());
            return new FileInfo(viewPath);
        }
    }
}
