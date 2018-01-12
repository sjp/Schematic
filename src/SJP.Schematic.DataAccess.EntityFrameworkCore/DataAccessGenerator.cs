using System;
using System.IO;
using SJP.Schematic.Core;

namespace SJP.Schematic.DataAccess.EntityFrameworkCore
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
            var dbContextGenerator = new DbContextBuilder(nameProvider, ns, Database);
            var tableGenerator = new TableGenerator(nameProvider, ns);

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

            var dbContextText = dbContextGenerator.Generate();
            var dbContextPath = Path.Combine(projectPath.Directory.FullName, "AppContext.cs");
            File.WriteAllText(dbContextPath, dbContextText);
        }
    }
}
