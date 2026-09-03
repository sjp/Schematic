using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LanguageExt;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.SqlServer.Queries;

namespace SJP.Schematic.SqlServer;

/// <summary>
/// A database schema provider for SQL Server databases, backed by <c>sys.schemas</c>.
/// </summary>
/// <seealso cref="IDatabaseSchemaProvider" />
public class SqlServerDatabaseSchemaProvider : IDatabaseSchemaProvider
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SqlServerDatabaseSchemaProvider"/> class.
    /// </summary>
    /// <param name="connection">A database connection factory.</param>
    /// <param name="identifierDefaults">Identifier defaults for the associated database.</param>
    /// <exception cref="ArgumentNullException"><paramref name="connection"/> or <paramref name="identifierDefaults"/> is <see langword="null" />.</exception>
    public SqlServerDatabaseSchemaProvider(IDbConnectionFactory connection, IIdentifierDefaults identifierDefaults)
    {
        Connection = connection ?? throw new ArgumentNullException(nameof(connection));
        IdentifierDefaults = identifierDefaults ?? throw new ArgumentNullException(nameof(identifierDefaults));
    }

    /// <summary>
    /// A database connection factory.
    /// </summary>
    /// <value>A database connection factory.</value>
    protected IDbConnectionFactory Connection { get; }

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
        return Connection.QueryEnumerableAsync<Queries.GetAllSchemas.Result>(Queries.GetAllSchemas.Sql, cancellationToken)
            .Select(MapSchema);
    }

    /// <summary>
    /// Gets all database schemas.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A collection of database schemas.</returns>
    public async Task<IReadOnlyCollection<IDatabaseSchema>> GetAllSchemas(CancellationToken cancellationToken = default)
    {
        var schemas = await Connection.QueryAsync<Queries.GetAllSchemas.Result>(Queries.GetAllSchemas.Sql, cancellationToken);
        return schemas.Select(MapSchema).ToList();
    }

    private IDatabaseSchema MapSchema(Queries.GetAllSchemas.Result row)
    {
        var owner = !row.OwnerName.IsNullOrWhiteSpace()
            ? Option<string>.Some(row.OwnerName)
            : Option<string>.None;
        var isDefault = string.Equals(row.SchemaName, IdentifierDefaults.Schema, StringComparison.OrdinalIgnoreCase);

        return new DatabaseSchema(Identifier.CreateQualifiedIdentifier(row.SchemaName), owner, isDefault, row.IsSystem);
    }
}
