using System.Data;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using SJP.Schematic.Core;
using SJP.Schematic.Sqlite;
using SJP.Schematic.Sqlite.Pragma;
using SJP.Schematic.Tests.Utilities;

namespace SJP.Schematic.Reporting.Tests.Integration
{
    internal static class Config
    {
        public static IDbConnection Connection
        {
            get
            {
                EnsureUnzipped();
                return SqliteDialect.CreateConnectionAsync(ConnectionString).GetAwaiter().GetResult();
            }
        }

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

        private static string CurrentDirectory => Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
    }

    [DatabaseTestFixture(typeof(Config), nameof(Config.Connection), "No Reporting DB available")]
    internal abstract class SakilaTest
    {
        protected IDbConnection Connection { get; } = Config.Connection;

        protected IDatabaseDialect Dialect { get; } = new SqliteDialect();

        protected ISqliteConnectionPragma Pragma { get; } = new ConnectionPragma(new SqliteDialect(), Config.Connection);

        protected IIdentifierDefaults IdentifierDefaults { get; } = new SqliteDialect().GetIdentifierDefaultsAsync(Config.Connection).GetAwaiter().GetResult();

        protected IRelationalDatabase GetDatabase() => new SqliteRelationalDatabase(Dialect, Connection, IdentifierDefaults);
    }
}
