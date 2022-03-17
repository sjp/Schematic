using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using SJP.Schematic.Core;
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

        using var zipFile = File.OpenRead(SakilaZipPath);
        using var archive = new ZipArchive(zipFile);
        var dbEntry = archive.Entries.Single();
        dbEntry.ExtractToFile(SakilaDbPath);
    }

    public static string SakilaDbPath => Path.Combine(CurrentDirectory, "sakila.sqlite");

    public static string SakilaZipPath => Path.Combine(CurrentDirectory, "sakila.sqlite.zip");

    private static string CurrentDirectory => Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty;
}

/// <summary>
/// A test which uses the Sakila test database for assistance with testing against a conventional database.
/// </summary>
[DatabaseTestFixture(typeof(Config), nameof(Config.ConnectionFactory), "No Sakila DB available")]
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
    protected IDbConnectionFactory DbConnection => Connection.DbConnection;

    /// <summary>
    /// The identifier defaults for the Sakila database.
    /// </summary>
    /// <value>A set of identifier defaults.</value>
    protected IIdentifierDefaults IdentifierDefaults => Connection.Dialect.GetIdentifierDefaultsAsync(Config.SchematicConnection).GetAwaiter().GetResult();

    /// <summary>
    /// A pragma accessor for the database.
    /// </summary>
    /// <value>A connection pragma.</value>
    protected ISqliteConnectionPragma Pragma => new ConnectionPragma(Connection);

    /// <summary>
    /// Creates a new relational database that connects to the Sakila database.
    /// </summary>
    /// <returns>A relational database.</returns>
    protected ISqliteDatabase GetDatabase() => new SqliteRelationalDatabase(Connection, IdentifierDefaults, Pragma);
}
