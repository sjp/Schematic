using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using SJP.Schematic.Core;
using SJP.Schematic.Sqlite;
using SJP.Schematic.Sqlite.Pragma;

namespace SJP.Schematic.Tests.Utilities.Integration
{
    internal static class Config
    {
        public static IDbConnectionFactory ConnectionFactory
        {
            get
            {
                EnsureUnzipped();
                return new SqliteConnectionFactory(ConnectionString);
            }
        }

        public static ISchematicConnection SchematicConnection => new SchematicConnection(ConnectionFactory, new SqliteDialect());

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

    [DatabaseTestFixture(typeof(Config), nameof(Config.ConnectionFactory), "No Sakila DB available")]
    public abstract class SakilaTest
    {
        protected ISchematicConnection Connection { get; } = Config.SchematicConnection;

        protected IDbConnectionFactory DbConnection => Connection.DbConnection;

        protected IIdentifierDefaults IdentifierDefaults => Connection.Dialect.GetIdentifierDefaultsAsync(Config.SchematicConnection).GetAwaiter().GetResult();

        protected ISqliteConnectionPragma Pragma => new ConnectionPragma(Connection);

        protected ISqliteDatabase GetDatabase() => new SqliteRelationalDatabase(Connection, IdentifierDefaults, Pragma);
    }
}
