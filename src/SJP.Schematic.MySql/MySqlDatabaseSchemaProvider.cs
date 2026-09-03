using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LanguageExt;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.MySql;

/// <summary>
/// A database schema provider for MySQL databases, backed by <c>information_schema.schemata</c>.
/// MySQL treats a schema and a database as the same thing, and records no owner for one.
/// </summary>
/// <seealso cref="IDatabaseSchemaProvider" />
public class MySqlDatabaseSchemaProvider : IDatabaseSchemaProvider
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MySqlDatabaseSchemaProvider"/> class.
    /// </summary>
    /// <param name="connection">A schematic connection.</param>
    /// <param name="identifierDefaults">Identifier defaults for the associated database.</param>
    /// <exception cref="ArgumentNullException"><paramref name="connection"/> or <paramref name="identifierDefaults"/> is <see langword="null" />.</exception>
    public MySqlDatabaseSchemaProvider(ISchematicConnection connection, IIdentifierDefaults identifierDefaults)
    {
        Connection = connection ?? throw new ArgumentNullException(nameof(connection));
        IdentifierDefaults = identifierDefaults ?? throw new ArgumentNullException(nameof(identifierDefaults));
    }

    /// <summary>
    /// A database connection that is specific to the given MySQL database.
    /// </summary>
    /// <value>A database connection.</value>
    protected ISchematicConnection Connection { get; }

    /// <summary>
    /// A database connection factory to query the database.
    /// </summary>
    /// <value>A connection factory.</value>
    protected IDbConnectionFactory DbConnection => Connection.ConnectionFactory;

    /// <summary>
    /// Identifier defaults for the associated database.
    /// </summary>
    /// <value>Identifier defaults.</value>
    protected IIdentifierDefaults IdentifierDefaults { get; }

    /// <summary>
    /// Enumerates all database schemas.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A collection of database schemas.</returns>
    public IAsyncEnumerable<IDatabaseSchema> EnumerateAllSchemas(CancellationToken cancellationToken = default)
    {
        return DbConnection.QueryEnumerableAsync<Queries.GetAllSchemas.Result>(Queries.GetAllSchemas.Sql, cancellationToken)
            .Select(MapSchema);
    }

    /// <summary>
    /// Gets all database schemas.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A collection of database schemas.</returns>
    public async Task<IReadOnlyCollection<IDatabaseSchema>> GetAllSchemas(CancellationToken cancellationToken = default)
    {
        var schemas = await DbConnection.QueryAsync<Queries.GetAllSchemas.Result>(Queries.GetAllSchemas.Sql, cancellationToken);
        return schemas.Select(MapSchema).ToList();
    }

    private IDatabaseSchema MapSchema(Queries.GetAllSchemas.Result row)
    {
        var isDefault = string.Equals(row.SchemaName, IdentifierDefaults.Schema, StringComparison.OrdinalIgnoreCase);

        return new DatabaseSchema(Identifier.CreateQualifiedIdentifier(row.SchemaName), Option<string>.None, isDefault, row.IsSystem);
    }
}
