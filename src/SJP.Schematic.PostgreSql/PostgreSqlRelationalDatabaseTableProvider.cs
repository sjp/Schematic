using SJP.Schematic.Core;

namespace SJP.Schematic.PostgreSql;

/// <summary>
/// A database table provider for PostgreSQL.
/// </summary>
/// <seealso cref="IRelationalDatabaseTableProvider" />
public class PostgreSqlRelationalDatabaseTableProvider : PostgreSqlRelationalDatabaseTableProviderBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PostgreSqlRelationalDatabaseTableProvider"/> class.
    /// </summary>
    /// <param name="connection">A schematic connection.</param>
    /// <param name="identifierDefaults">Database identifier defaults.</param>
    /// <param name="identifierResolver">A database identifier resolver.</param>
    /// <exception cref="System.ArgumentNullException"><paramref name="connection"/> or <paramref name="identifierDefaults"/> or <paramref name="identifierResolver"/> is <see langword="null" />.</exception>
    public PostgreSqlRelationalDatabaseTableProvider(ISchematicConnection connection, IIdentifierDefaults identifierDefaults, IIdentifierResolutionStrategy identifierResolver)
        : base(connection, identifierDefaults, identifierResolver)
    {
    }
}
