using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using LanguageExt;
using SJP.Schematic.Core;
using SJP.Schematic.Sqlite.Pragma;
using SJP.Schematic.Sqlite.Pragma.Query;

namespace SJP.Schematic.Sqlite;

/// <summary>
/// A database schema provider for SQLite databases. SQLite has no schemas of its own, so each
/// database attached to the connection is reported as one schema. SQLite records no owner for a
/// database.
/// </summary>
/// <seealso cref="IDatabaseSchemaProvider" />
public class SqliteDatabaseSchemaProvider : IDatabaseSchemaProvider
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SqliteDatabaseSchemaProvider"/> class.
    /// </summary>
    /// <param name="connectionPragma">A connection pragma for the associated database.</param>
    /// <param name="identifierDefaults">Identifier defaults for the associated database.</param>
    /// <exception cref="ArgumentNullException"><paramref name="connectionPragma"/> or <paramref name="identifierDefaults"/> is <see langword="null" />.</exception>
    public SqliteDatabaseSchemaProvider(ISqliteConnectionPragma connectionPragma, IIdentifierDefaults identifierDefaults)
    {
        ConnectionPragma = connectionPragma ?? throw new ArgumentNullException(nameof(connectionPragma));
        IdentifierDefaults = identifierDefaults ?? throw new ArgumentNullException(nameof(identifierDefaults));
    }

    /// <summary>
    /// A connection pragma for the associated database.
    /// </summary>
    /// <value>A connection pragma.</value>
    protected ISqliteConnectionPragma ConnectionPragma { get; }

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
    public async IAsyncEnumerable<IDatabaseSchema> EnumerateAllSchemas([EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        // the attached database list is a single pragma result, so there is nothing to stream
        var schemas = await GetAllSchemas(cancellationToken);
        foreach (var schema in schemas)
            yield return schema;
    }

    /// <summary>
    /// Gets all database schemas.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A collection of database schemas.</returns>
    public async Task<IReadOnlyCollection<IDatabaseSchema>> GetAllSchemas(CancellationToken cancellationToken = default)
    {
        var databases = await ConnectionPragma.DatabaseListAsync(cancellationToken);
        return databases
            .OrderBy(static db => db.seq)
            .Select(MapSchema)
            .ToList();
    }

    private IDatabaseSchema MapSchema(pragma_database_list database)
    {
        var isDefault = string.Equals(database.name, IdentifierDefaults.Schema, StringComparison.OrdinalIgnoreCase);
        // 'temp' always exists and is created by SQLite rather than declared by a user
        var isSystem = string.Equals(database.name, TempSchemaName, StringComparison.OrdinalIgnoreCase);

        return new DatabaseSchema(Identifier.CreateQualifiedIdentifier(database.name), Option<string>.None, isDefault, isSystem);
    }

    private const string TempSchemaName = "temp";
}
