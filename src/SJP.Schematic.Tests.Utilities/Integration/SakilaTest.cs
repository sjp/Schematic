using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Nito.AsyncEx;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Sqlite;
using SJP.Schematic.Sqlite.Pragma;

namespace SJP.Schematic.Tests.Utilities.Integration;

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

        // Extract to a uniquely-named temp file first and move it into place atomically, so that
        // concurrent fixtures racing on EnsureUnzipped() never observe (or write) a partial file.
        var tempPath = SakilaDbPath + "." + Guid.NewGuid().ToString("N") + ".tmp";
        try
        {
            using (var zipFile = File.OpenRead(SakilaZipPath))
            using (var archive = new ZipArchive(zipFile))
            {
                var dbEntry = archive.Entries.Single();
                dbEntry.ExtractToFile(tempPath, overwrite: true);
            }

            File.Move(tempPath, SakilaDbPath, overwrite: true);
        }
        finally
        {
            if (File.Exists(tempPath))
                File.Delete(tempPath);
        }
    }

    public static string SakilaDbPath => Path.Combine(CurrentDirectory, "sakila.sqlite");

    public static string SakilaZipPath => Path.Combine(CurrentDirectory, "sakila.sqlite.zip");

    private static string CurrentDirectory => Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty;
}

/// <summary>
/// A test which uses the Sakila test database for assistance with testing against a conventional database.
/// </summary>
[DatabaseTestFixture(typeof(Config), nameof(Config.ConnectionFactory), "No Sakila DB available")]
[Parallelizable(ParallelScope.Children)]
public abstract class SakilaTest
{
    /// <summary>
    /// A schematic connection for accessing the Sakila database.
    /// </summary>
    /// <value>A schematic connection.</value>
    protected ISchematicConnection Connection { get; } = Config.SchematicConnection;

    /// <summary>
    /// A connection factory for accessing the Sakila database.
    /// </summary>
    /// <value>A database connection factory.</value>
    protected IDbConnectionFactory DbConnection => Connection.ConnectionFactory;

    /// <summary>
    /// A database provider for accessing the Sakila database and its metadata.
    /// </summary>
    /// <value>A relational database provider.</value>
    protected SqliteDatabaseProvider DatabaseProvider { get; } = new(Config.SchematicConnection);

    /// <summary>
    /// The identifier defaults for the Sakila database. Resolved once per process: every fixture
    /// shares the same cached value rather than issuing its own blocking round-trip.
    /// </summary>
    /// <value>A set of identifier defaults.</value>
    protected IIdentifierDefaults IdentifierDefaults { get; } = IdentifierDefaultsLazy.Value;

    /// <summary>
    /// A pragma accessor for the database.
    /// </summary>
    /// <value>A connection pragma.</value>
    protected ISqliteConnectionPragma Pragma { get; } = new ConnectionPragma(Config.SchematicConnection);

    /// <summary>
    /// Creates a new relational database that connects to the Sakila database.
    /// </summary>
    /// <returns>A relational database.</returns>
    protected ISqliteDatabase GetDatabase() => new SqliteRelationalDatabase(Connection, IdentifierDefaults, Pragma);

    /// <summary>
    /// Returns an in-memory snapshot of the Sakila database, shared and cached across every
    /// Sakila-backed fixture in the process. Prefer this over <see cref="GetDatabase"/> for
    /// read-only assertions, so the schema is reflected once rather than once per fixture.
    /// </summary>
    /// <returns>A task containing a snapshot of the Sakila database.</returns>
    protected static Task<IRelationalDatabase> GetSnapshotDatabaseAsync() => SnapshotLazy.Task;

    private static readonly Lazy<IIdentifierDefaults> IdentifierDefaultsLazy = new(() =>
        new SqliteDatabaseProvider(Config.SchematicConnection).GetIdentifierDefaultsAsync().GetAwaiter().GetResult());

    private static readonly AsyncLazy<IRelationalDatabase> SnapshotLazy = new(async () =>
    {
        ISqliteDatabase database = new SqliteRelationalDatabase(
            Config.SchematicConnection,
            IdentifierDefaultsLazy.Value,
            new ConnectionPragma(Config.SchematicConnection));

        return await database.SnapshotAsync();
    });
}
