using System;
using System.IO;
using System.IO.Abstractions;
using SJP.Schematic.Core;

namespace SJP.Schematic.DataAccess.EntityFrameworkCore
{
    public class DataAccessGenerator
    {
        public DataAccessGenerator(IRelationalDatabase database, INameProvider nameProvider)
        {
            Database = database ?? throw new ArgumentNullException(nameof(database));
            NameProvider = nameProvider ?? throw new ArgumentNullException(nameof(nameProvider));
        }

        protected IRelationalDatabase Database { get; }

        protected INameProvider NameProvider { get; }

        public void Generate(IFileSystem fileSystem, string projectPath, string baseNamespace)
        {
            if (fileSystem == null)
                throw new ArgumentNullException(nameof(fileSystem));
            if (projectPath == null)
                throw new ArgumentNullException(nameof(projectPath));
            if (baseNamespace.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(baseNamespace));

            var projectFileInfo = fileSystem.FileInfo.FromFileName(projectPath);
            if (!string.Equals(projectFileInfo.Extension, ".csproj"))
                throw new ArgumentException("The given path to a project must be a csproj file.", nameof(projectPath));

            if (projectFileInfo.Exists)
                projectFileInfo.Delete();
            File.WriteAllText(projectPath, ProjectGenerator.ProjectDefinition);

            var dbContextGenerator = new DbContextBuilder(NameProvider, baseNamespace, Database);
            var tableGenerator = new TableGenerator(NameProvider, baseNamespace);

            foreach (var table in Database.Tables)
            {
                var tableClass = tableGenerator.Generate(table);
                //var tablePath = tableGenerator.GetFilePath(projectFileInfo.Directory, table.Name);
                var tablePath = tableGenerator.GetFilePath(null, table.Name);

                if (!tablePath.Directory.Exists)
                    tablePath.Directory.Create();

                if (tablePath.Exists)
                    tablePath.Delete();

                File.WriteAllText(tablePath.FullName, tableClass);
            }

            var dbContextText = dbContextGenerator.Generate();
            var dbContextPath = Path.Combine(projectFileInfo.Directory.FullName, "AppContext.cs");
            File.WriteAllText(dbContextPath, dbContextText);
        }
    }
}
