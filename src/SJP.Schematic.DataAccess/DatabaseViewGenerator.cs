using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using SJP.Schematic.Core;

namespace SJP.Schematic.DataAccess
{
    public abstract class DatabaseViewGenerator : IDatabaseViewGenerator
    {
        protected DatabaseViewGenerator(INameProvider nameProvider, string indent = "    ")
        {
            NameProvider = nameProvider ?? throw new ArgumentNullException(nameof(nameProvider));
            Indent = indent ?? throw new ArgumentNullException(nameof(indent));
        }

        protected INameProvider NameProvider { get; }

        protected string Indent { get; }

        public abstract string Generate(IDatabaseView view);

        public virtual FileInfoBase GetFilePath(DirectoryInfoBase baseDirectory, Identifier objectName)
        {
            if (baseDirectory == null)
                throw new ArgumentNullException(nameof(baseDirectory));
            if (objectName == null)
                throw new ArgumentNullException(nameof(objectName));

            var paths = new List<string> { baseDirectory.FullName, "Views" };
            if (objectName.Schema != null)
            {
                var schemaName = NameProvider.SchemaToNamespace(objectName);
                paths.Add(schemaName);
            }

            var viewName = NameProvider.ViewToClassName(objectName);
            paths.Add(viewName + ".cs");

            var viewPath = Path.Combine(paths.ToArray());
            return new FileInfo(viewPath);
        }
    }
}
