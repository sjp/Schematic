using System;
using System.IO;
using System.IO.Abstractions;
using McMaster.Extensions.CommandLineUtils;
using SJP.Schematic.Core.Caching;
using SJP.Schematic.DataAccess.OrmLite;

namespace SJP.Schematic.Tool
{
    [Command(Description = "Generate ORM classes for use with OrmLite.")]
    internal sealed class GenerateOrmLiteCommand
    {
        private DatabaseCommand DatabaseParent { get; set; }

        private GenerateCommand GenerateParent { get; set; }

        private int OnExecute(CommandLineApplication application)
        {
            if (application == null)
                throw new ArgumentNullException(nameof(application));

            var dialect = DatabaseParent.GetDatabaseDialect();
            var hasConnectionString = DatabaseParent.TryGetConnectionString(out var connectionString);
            if (!hasConnectionString)
            {
                application.Error.WriteLine();
                application.Error.WriteLine("Unable to continue without a connection string. Exiting.");
                return 1;
            }

            var status = DatabaseCommand.GetConnectionStatus(dialect, connectionString);
            if (!status.IsConnected)
            {
                application.Error.WriteLine("Could not connect to the database.");
                return 1;
            }

            var nameProvider = GenerateParent.GetNameProvider();
            if (nameProvider == null)
            {
                application.Error.WriteLine("Unknown or unsupported database name translator: " + GenerateParent.Translator);
                return 1;
            }

            var databaseFactory = DatabaseParent.GetRelationalDatabaseFactory();

            try
            {
                var cachedConnection = status.Connection.AsCachedConnection();
                var identifierDefaults = dialect.GetIdentifierDefaults(cachedConnection);
                var database = databaseFactory.Invoke(dialect, cachedConnection, identifierDefaults);

                var generator = new OrmLiteDataAccessGenerator(database, nameProvider);
                var fileSystem = new FileSystem();
                generator.Generate(fileSystem, GenerateParent.ProjectPath, GenerateParent.BaseNamespace);

                var dirName = Path.GetDirectoryName(GenerateParent.ProjectPath);
                application.Out.WriteLine("The OrmLite project has been exported to: " + dirName);
                return 0;
            }
            catch (Exception ex)
            {
                application.Error.WriteLine("An error occurred generating an OrmLite project.");
                application.Error.WriteLine();
                application.Error.WriteLine("Error message: " + ex.Message);
                application.Error.WriteLine("Stack trace: " + ex.StackTrace);

                return 1;
            }
            finally
            {
                status.Connection.Close();
            }
        }
    }
}
