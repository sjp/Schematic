using System;
using System.IO;
using SJP.Schematic.Core;

namespace SJP.Schematic.DataAccess.Poco
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

        public void Generate(FileInfo projectPath, string baseNamespace)
        {
            if (projectPath == null)
                throw new ArgumentNullException(nameof(projectPath));
            if (baseNamespace.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(baseNamespace));

            if (!string.Equals(projectPath.Extension, ".csproj"))
                throw new ArgumentException("The given path to a project must be a csproj file.", nameof(projectPath));

            if (projectPath.Exists)
                projectPath.Delete();
            File.WriteAllText(projectPath.FullName, ProjectGenerator.ProjectDefinition);

            var tableGenerator = new TableGenerator(NameProvider, baseNamespace);
            var viewGenerator = new ViewGenerator(NameProvider, baseNamespace);

            var projectDir = projectPath.Directory;
            var tablesDirectory = new DirectoryInfo(Path.Combine(projectDir.FullName, "Tables"));
            var viewsDirectory = new DirectoryInfo(Path.Combine(projectDir.FullName, "Views"));

            if (!tablesDirectory.Exists)
                tablesDirectory.Create();
            if (!viewsDirectory.Exists)
                viewsDirectory.Create();

            foreach (var table in Database.Tables)
            {
                var tableClass = tableGenerator.Generate(table);
                var tablePath = tableGenerator.GetFilePath(projectPath.Directory, table.Name);

                if (!tablePath.Directory.Exists)
                    tablePath.Directory.Create();

                if (tablePath.Exists)
                    tablePath.Delete();

                File.WriteAllText(tablePath.FullName, tableClass);
            }

            foreach (var view in Database.Views)
            {
                var viewClass = viewGenerator.Generate(view);
                var viewPath = viewGenerator.GetFilePath(projectPath.Directory, view.Name);

                if (!viewPath.Directory.Exists)
                    viewPath.Directory.Create();

                if (viewPath.Exists)
                    viewPath.Delete();

                File.WriteAllText(viewPath.FullName, viewClass);
            }
        }
    }
}
