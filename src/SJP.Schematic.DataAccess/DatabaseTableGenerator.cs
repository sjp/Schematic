﻿using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using SJP.Schematic.Core;

namespace SJP.Schematic.DataAccess
{
    public abstract class DatabaseTableGenerator : IDatabaseTableGenerator
    {
        protected DatabaseTableGenerator(INameProvider nameProvider)
        {
            NameProvider = nameProvider ?? throw new ArgumentNullException(nameof(nameProvider));
        }

        protected INameProvider NameProvider { get; }

        public abstract string Generate(IRelationalDatabaseTable table);

        public virtual FileInfoBase GetFilePath(DirectoryInfoBase baseDirectory, Identifier objectName)
        {
            if (baseDirectory == null)
                throw new ArgumentNullException(nameof(baseDirectory));
            if (objectName == null || objectName.LocalName == null)
                throw new ArgumentNullException(nameof(objectName));

            var paths = new List<string> { baseDirectory.FullName, "Tables" };
            if (objectName.Schema != null)
            {
                var schemaName = NameProvider.SchemaToNamespace(objectName);
                paths.Add(schemaName);
            }

            var tableName = NameProvider.TableToClassName(objectName);
            paths.Add(tableName + ".cs");

            var tablePath = Path.Combine(paths.ToArray());
            return new FileInfo(tablePath);
        }
    }
}
