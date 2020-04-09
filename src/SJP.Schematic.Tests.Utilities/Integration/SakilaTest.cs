using System.Data;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using SJP.Schematic.Core;
using SJP.Schematic.Sqlite;

namespace SJP.Schematic.Tests.Utilities.Integration
{
    internal static class Config
    {
        public static IDbConnectionFactory ConnectionFactory { get; } = new SqliteConnectionFactory();

        public static IDbConnection Connection
        {
            get
            {
                EnsureUnzipped();
                return ConnectionFactory.CreateConnection(ConnectionString);
            }
        }

        public static ISchematicConnection SchematicConnection { get; } = new SchematicConnection(Connection, new SqliteDialect());

        private static string ConnectionString => "Data Source=" + SakilaDbPath;

        private static void EnsureUnzipped()
        {
            if (File.Exists(SakilaDbPath))
                return;

            using var zipFile = File.OpenRead(SakilaZipPath);
            using var archive = new ZipArchive(zipFile);
            var dbEntry = archive.Entries.Single();
            dbEntry.ExtractToFile(SakilaDbPath);
        }

        public static string SakilaDbPath => Path.Combine(CurrentDirectory, "sakila.sqlite");

        public static string SakilaZipPath => Path.Combine(CurrentDirectory, "sakila.sqlite.zip");

        private static string CurrentDirectory => Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty;
    }

    [DatabaseTestFixture(typeof(Config), nameof(Config.Connection), "No Sakila DB available")]
    public abstract class SakilaTest
    {
        protected ISchematicConnection Connection { get; } = Config.SchematicConnection;

        protected IDbConnection DbConnection => Connection.DbConnection;

        protected IIdentifierDefaults IdentifierDefaults { get; } = Config.SchematicConnection.Dialect.GetIdentifierDefaultsAsync(Config.SchematicConnection).GetAwaiter().GetResult();

        protected ISqliteDatabase GetDatabase() => new SqliteRelationalDatabase(Config.SchematicConnection, IdentifierDefaults);
    }
}
