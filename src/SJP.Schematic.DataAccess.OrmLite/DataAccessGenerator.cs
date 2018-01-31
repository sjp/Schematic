using System;
using System.IO;
using System.IO.Abstractions;
using SJP.Schematic.Core;

namespace SJP.Schematic.DataAccess.OrmLite
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
            if (projectPath.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(projectPath));
            if (baseNamespace.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(baseNamespace));

            var projectPathInfo = fileSystem.FileInfo.FromFileName(projectPath);
            if (!string.Equals(projectPathInfo.Extension, ".csproj"))
                throw new ArgumentException("The given path to a project must be a csproj file.", nameof(projectPath));

            if (projectPathInfo.Exists)
                projectPathInfo.Delete();
            File.WriteAllText(projectPath, ProjectGenerator.ProjectDefinition);

            var tableGenerator = new TableGenerator(NameProvider, baseNamespace);
            var viewGenerator = new ViewGenerator(NameProvider, baseNamespace);

            foreach (var table in Database.Tables)
            {
                var tableClass = tableGenerator.Generate(table);
                //var tablePath = tableGenerator.GetFilePath(projectPathInfo.Directory, table.Name);
                var tablePath = tableGenerator.GetFilePath(null, table.Name);

                if (!tablePath.Directory.Exists)
                    tablePath.Directory.Create();

                if (tablePath.Exists)
                    tablePath.Delete();

                File.WriteAllText(tablePath.FullName, tableClass);
            }

            foreach (var view in Database.Views)
            {
                var viewClass = viewGenerator.Generate(view);
                //var viewPath = viewGenerator.GetFilePath(projectPathInfo.Directory, view.Name);
                var viewPath = viewGenerator.GetFilePath(null, view.Name);

                if (!viewPath.Directory.Exists)
                    viewPath.Directory.Create();

                if (viewPath.Exists)
                    viewPath.Delete();

                File.WriteAllText(viewPath.FullName, viewClass);
            }
        }
    }
}
