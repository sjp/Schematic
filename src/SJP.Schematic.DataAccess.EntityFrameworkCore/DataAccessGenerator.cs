using System;
using System.IO;
using System.IO.Abstractions;
using SJP.Schematic.Core;

namespace SJP.Schematic.DataAccess.EntityFrameworkCore
{
    public class DataAccessGenerator : IDataAccessGenerator
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
            if (projectPath.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(projectPath));
            if (baseNamespace.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(baseNamespace));

            var projectFileInfo = fileSystem.FileInfo.FromFileName(projectPath);
            if (!string.Equals(projectFileInfo.Extension, ".csproj", StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException("The given path to a project must be a csproj file.", nameof(projectPath));

            if (projectFileInfo.Exists)
                projectFileInfo.Delete();

            if (!projectFileInfo.Directory.Exists)
                projectFileInfo.Directory.Create();

            fileSystem.File.WriteAllText(projectPath, ProjectGenerator.ProjectDefinition);

            var dbContextGenerator = new DbContextBuilder(Database, NameProvider, baseNamespace);
            var tableGenerator = new TableGenerator(NameProvider, baseNamespace);

            foreach (var table in Database.Tables)
            {
                var tableClass = tableGenerator.Generate(table);
                var tablePath = tableGenerator.GetFilePath(projectFileInfo.Directory, table.Name);

                if (!tablePath.Directory.Exists)
                    tablePath.Directory.Create();

                if (tablePath.Exists)
                    tablePath.Delete();

                fileSystem.File.WriteAllText(tablePath.FullName, tableClass);
            }

            var dbContextText = dbContextGenerator.Generate();
            var dbContextPath = Path.Combine(projectPath, "AppContext.cs");
            File.WriteAllText(dbContextPath, dbContextText);
        }
    }
}
