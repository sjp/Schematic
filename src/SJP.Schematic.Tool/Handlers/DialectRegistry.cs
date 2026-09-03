using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Tool.Handlers;

/// <summary>
/// Everything the tool needs to know about one database dialect: how to construct it, how to
/// connect to it, and how the guided prompts should describe it.
/// </summary>
public sealed record DialectDescriptor
{
    /// <summary>
    /// The name used to select this dialect in a configuration file.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Creates the dialect.
    /// </summary>
    public required Func<IDatabaseDialect> CreateDialect { get; init; }

    /// <summary>
    /// Creates a connection factory from a connection string.
    /// </summary>
    public required Func<string, IDbConnectionFactory> CreateConnectionFactory { get; init; }

    /// <summary>
    /// Creates a relational database provider from a connection.
    /// </summary>
    public required Func<ISchematicConnection, IRelationalDatabaseProvider> CreateDatabaseProvider { get; init; }

    /// <summary>
    /// Whether a connection addresses a file rather than a server, in which case there is no host,
    /// port or credentials to ask for.
    /// </summary>
    public required bool IsFileBased { get; init; }

    /// <summary>
    /// What the engine calls the database being connected to, e.g. a service name for Oracle.
    /// </summary>
    public required string DatabaseLabel { get; init; }

    /// <summary>
    /// Builds a connection string from a set of common connection details.
    /// </summary>
    public required Func<ConnectionStringFactory.ConnectionDetails, string> BuildConnectionString { get; init; }
}

/// <summary>
/// The dialects the tool supports, keyed by the name used as the <c>Dialect</c> value in a
/// configuration file. Adding a dialect to the tool is a single entry here.
/// </summary>
public static class DialectRegistry
{
    /// <summary>
    /// The supported dialect names, in the order they are offered to a user.
    /// </summary>
    public static IReadOnlyList<string> DialectNames { get; } =
    [
        "sqlserver",
        "postgresql",
        "mysql",
        "oracle",
        "sqlite",
    ];

    /// <summary>
    /// Retrieves the descriptor for a dialect name, ignoring case.
    /// </summary>
    /// <param name="name">A dialect name.</param>
    /// <returns>The descriptor describing <paramref name="name"/>.</returns>
    /// <exception cref="ArgumentException"><paramref name="name"/> is <see langword="null" />, empty or whitespace.</exception>
    /// <exception cref="NotSupportedException"><paramref name="name"/> is not a supported dialect.</exception>
    public static DialectDescriptor Get(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        if (!TryGet(name, out var descriptor))
            throw new NotSupportedException($"The given dialect is not supported: {name}. Expected one of: {DialectNames.Join(", ")}.");

        return descriptor;
    }

    /// <summary>
    /// Attempts to retrieve the descriptor for a dialect name, ignoring case.
    /// </summary>
    /// <param name="name">A dialect name.</param>
    /// <param name="descriptor">The descriptor describing <paramref name="name"/>, when one exists.</param>
    /// <returns><see langword="true" /> if <paramref name="name"/> is a supported dialect; otherwise, <see langword="false" />.</returns>
    public static bool TryGet(string? name, [NotNullWhen(true)] out DialectDescriptor? descriptor)
    {
        if (name.IsNullOrWhiteSpace())
        {
            descriptor = null;
            return false;
        }

        return Descriptors.TryGetValue(name, out descriptor);
    }

    private static readonly FrozenDictionary<string, DialectDescriptor> Descriptors = new[]
    {
        new DialectDescriptor
        {
            Name = "sqlserver",
            CreateDialect = static () => new SqlServer.SqlServerDialect(),
            CreateConnectionFactory = static connectionString => new SqlServer.SqlServerConnectionFactory(connectionString),
            CreateDatabaseProvider = static connection => new SqlServer.SqlServerDatabaseProvider(connection),
            IsFileBased = false,
            DatabaseLabel = "Database",
            BuildConnectionString = ConnectionStringFactory.BuildSqlServer,
        },
        new DialectDescriptor
        {
            Name = "postgresql",
            CreateDialect = static () => new PostgreSql.PostgreSqlDialect(),
            CreateConnectionFactory = static connectionString => new PostgreSql.PostgreSqlConnectionFactory(connectionString),
            CreateDatabaseProvider = static connection => new PostgreSql.PostgreSqlDatabaseProvider(connection),
            IsFileBased = false,
            DatabaseLabel = "Database",
            BuildConnectionString = ConnectionStringFactory.BuildPostgreSql,
        },
        new DialectDescriptor
        {
            Name = "mysql",
            CreateDialect = static () => new MySql.MySqlDialect(),
            CreateConnectionFactory = static connectionString => new MySql.MySqlConnectionFactory(connectionString),
            CreateDatabaseProvider = static connection => new MySql.MySqlDatabaseProvider(connection),
            IsFileBased = false,
            DatabaseLabel = "Database",
            BuildConnectionString = ConnectionStringFactory.BuildMySql,
        },
        new DialectDescriptor
        {
            Name = "oracle",
            CreateDialect = static () => new Oracle.OracleDialect(),
            CreateConnectionFactory = static connectionString => new Oracle.OracleConnectionFactory(connectionString),
            CreateDatabaseProvider = static connection => new Oracle.OracleDatabaseProvider(connection),
            IsFileBased = false,
            DatabaseLabel = "Service name",
            BuildConnectionString = ConnectionStringFactory.BuildOracle,
        },
        new DialectDescriptor
        {
            Name = "sqlite",
            CreateDialect = static () => new Sqlite.SqliteDialect(),
            CreateConnectionFactory = static connectionString => new Sqlite.SqliteConnectionFactory(connectionString),
            CreateDatabaseProvider = static connection => new Sqlite.SqliteDatabaseProvider(connection),
            IsFileBased = true,
            DatabaseLabel = "Database",
            // a SQLite connection is addressed by a file path, which the guided flow collects as the host
            BuildConnectionString = static details => ConnectionStringFactory.ForSqlite(details.Host),
        },
    }.ToFrozenDictionary(static d => d.Name, StringComparer.OrdinalIgnoreCase);
}
