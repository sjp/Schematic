using System;
using System.IO;
using SJP.Schematic.Core;

namespace SJP.Schematic.DataAccess.OrmLite
{
    public class DataAccessGenerator
    {
        public DataAccessGenerator(IRelationalDatabase database)
        {
            Database = database ?? throw new ArgumentNullException(nameof(database));
        }

        protected IRelationalDatabase Database { get; }

        public void Generate(FileInfo projectPath, string ns)
        {
            if (projectPath == null)
                throw new ArgumentNullException(nameof(projectPath));
            if (ns.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(ns));

            if (!string.Equals(projectPath.Extension, ".csproj"))
                throw new ArgumentException("The given path to a project must be a csproj file.", nameof(projectPath));

            if (projectPath.Exists)
                projectPath.Delete();
            File.WriteAllText(projectPath.FullName, ProjectGenerator.ProjectDefinition);

            var nameProvider = new VerbatimNameProvider();
            var tableGenerator = new TableGenerator(nameProvider, ns);
            var viewGenerator = new ViewGenerator(nameProvider, ns);

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
