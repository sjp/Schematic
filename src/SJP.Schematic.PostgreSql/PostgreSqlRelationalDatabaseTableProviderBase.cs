using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LanguageExt;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Core.Utilities;
using SJP.Schematic.PostgreSql.Queries;

namespace SJP.Schematic.PostgreSql;

/// <summary>
/// A database table provider for PostgreSQL.
/// </summary>
/// <seealso cref="IRelationalDatabaseTableProvider" />
public class PostgreSqlRelationalDatabaseTableProviderBase : IRelationalDatabaseTableProvider
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PostgreSqlRelationalDatabaseTableProviderBase"/> class.
    /// </summary>
    /// <param name="connection">A schematic connection.</param>
    /// <param name="identifierDefaults">Database identifier defaults.</param>
    /// <param name="identifierResolver">An identifier resolver.</param>
    /// <exception cref="ArgumentNullException"><paramref name="connection"/> or <paramref name="identifierDefaults"/> or <paramref name="identifierResolver"/> are <see langword="null" />.</exception>
    public PostgreSqlRelationalDatabaseTableProviderBase(ISchematicConnection connection, IIdentifierDefaults identifierDefaults, IIdentifierResolutionStrategy identifierResolver)
    {
        Connection = connection ?? throw new ArgumentNullException(nameof(connection));
        IdentifierDefaults = identifierDefaults ?? throw new ArgumentNullException(nameof(identifierDefaults));
        IdentifierResolver = identifierResolver ?? throw new ArgumentNullException(nameof(identifierResolver));
    }

    /// <summary>
    /// A database connection that is specific to a given PostgreSQL database.
    /// </summary>
    /// <value>A database connection.</value>
    protected ISchematicConnection Connection { get; }

    /// <summary>
    /// Identifier defaults for the associated database.
    /// </summary>
    /// <value>Identifier defaults.</value>
    protected IIdentifierDefaults IdentifierDefaults { get; }

    /// <summary>
    /// Gets an identifier resolver that enables more relaxed matching against database object names.
    /// </summary>
    /// <value>An identifier resolver.</value>
    protected IIdentifierResolutionStrategy IdentifierResolver { get; }

    /// <summary>
    /// A database connection factory used to query the database.
    /// </summary>
    /// <value>A database connection factory.</value>
    protected IDbConnectionFactory DbConnection => Connection.ConnectionFactory;

    /// <summary>
    /// The dialect for the associated database.
    /// </summary>
    /// <value>A database dialect.</value>
    protected IDatabaseDialect Dialect => Connection.Dialect;

    /// <summary>
    /// Gets a database column type provider.
    /// </summary>
    /// <value>A type provider.</value>
    protected IDbTypeProvider TypeProvider => Dialect.TypeProvider;

    /// <summary>
    /// Creates a query cache for a given query context
    /// </summary>
    /// <returns>A query cache.</returns>
    protected PostgreSqlTableQueryCache CreateQueryCache() => new(
        new AsyncCache<Identifier, Option<Identifier>, PostgreSqlTableQueryCache>((tableName, _, token) => GetResolvedTableName(tableName, token)),
        new AsyncCache<Identifier, IReadOnlyList<IDatabaseColumn>, PostgreSqlTableQueryCache>((tableName, _, token) => LoadColumnsAsync(tableName, token)),
        new AsyncCache<Identifier, IReadOnlyDictionary<Identifier, IDatabaseColumn>, PostgreSqlTableQueryCache>(async (tableName, cache, token) => GetColumnLookup(await cache.GetColumnsAsync(tableName, token))),
        new AsyncCache<Identifier, Option<IDatabaseKey>, PostgreSqlTableQueryCache>(LoadPrimaryKeyAsync),
        new AsyncCache<Identifier, IReadOnlyCollection<IDatabaseKey>, PostgreSqlTableQueryCache>(LoadUniqueKeysAsync),
        new AsyncCache<Identifier, IReadOnlyCollection<IDatabaseIndex>, PostgreSqlTableQueryCache>(LoadIndexesAsync),
        new AsyncCache<Identifier, IReadOnlyCollection<IDatabaseRelationalKey>, PostgreSqlTableQueryCache>(LoadParentKeysAsync)
    );

    /// <summary>
    /// Enumerates all database tables.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A collection of database tables.</returns>
    public IAsyncEnumerable<IRelationalDatabaseTable> EnumerateAllTables(CancellationToken cancellationToken = default)
    {
        var queryCache = CreateQueryCache();

        return DbConnection.QueryEnumerableAsync<GetAllTableNames.Result>(GetAllTableNames.Sql, cancellationToken)
            .Select(static dto => Identifier.CreateQualifiedIdentifier(dto.SchemaName, dto.TableName))
            .Select(QualifyTableName)
            .SelectAwait((tableName, ct) => LoadTableAsyncCore(tableName, queryCache, ct));
    }

    /// <summary>
    /// Gets all database tables.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A collection of database tables.</returns>
    public async Task<IReadOnlyCollection<IRelationalDatabaseTable>> GetAllTables(CancellationToken cancellationToken = default)
    {
        var queryCache = CreateQueryCache();

        var tableNames = await DbConnection.QueryEnumerableAsync<GetAllTableNames.Result>(GetAllTableNames.Sql, cancellationToken)
            .Select(static dto => Identifier.CreateQualifiedIdentifier(dto.SchemaName, dto.TableName))
            .Select(QualifyTableName)
            .ToListAsync(cancellationToken);

        return await tableNames
            .Select(tableName => LoadTableAsyncCore(tableName, queryCache, cancellationToken))
            .ToArray()
            .WhenAll();
    }

    /// <summary>
    /// Gets a database table.
    /// </summary>
    /// <param name="tableName">A database table name.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A database table in the 'some' state if found; otherwise 'none'.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="tableName"/> is <see langword="null" />.</exception>
    public OptionAsync<IRelationalDatabaseTable> GetTable(Identifier tableName, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(tableName);

        var queryCache = CreateQueryCache();
        var candidateTableName = QualifyTableName(tableName);
        return LoadTable(candidateTableName, queryCache, cancellationToken);
    }

    /// <summary>
    /// Gets the resolved name of the table. This enables non-strict name matching to be applied.
    /// </summary>
    /// <param name="tableName">A table name that will be resolved.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A table name that, if available, can be assumed to exist and applied strictly.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="tableName"/> is <see langword="null" />.</exception>
    protected Task<Option<Identifier>> GetResolvedTableName(Identifier tableName, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(tableName);

        var resolvedNames = IdentifierResolver
            .GetResolutionOrder(tableName)
            .Select(QualifyTableName);

        return resolvedNames
            .Select(name => GetResolvedTableNameStrict(name, cancellationToken))
            .FirstSome(cancellationToken)
            .ToOption();
    }

    /// <summary>
    /// Gets the resolved name of the table without name resolution. i.e. the name must match strictly to return a result.
    /// </summary>
    /// <param name="tableName">A table name that will be resolved.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A table name that, if available, can be assumed to exist and applied strictly.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="tableName"/> is <see langword="null" />.</exception>
    protected OptionAsync<Identifier> GetResolvedTableNameStrict(Identifier tableName, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(tableName);

        var candidateTableName = QualifyTableName(tableName);
        var qualifiedTableName = DbConnection.QueryFirstOrNone(
            GetTableName.Sql,
            new GetTableName.Query { SchemaName = candidateTableName.Schema!, TableName = candidateTableName.LocalName },
            cancellationToken
        );

        return qualifiedTableName.Map(name => Identifier.CreateQualifiedIdentifier(candidateTableName.Server, candidateTableName.Database, name.SchemaName, name.TableName));
    }

    /// <summary>
    /// Retrieves a table from the database, if available.
    /// </summary>
    /// <param name="tableName">A table name.</param>
    /// <param name="queryCache">The query cache.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A table, if available.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="tableName"/> or <paramref name="queryCache"/> is <see langword="null" />.</exception>
    protected OptionAsync<IRelationalDatabaseTable> LoadTable(Identifier tableName, PostgreSqlTableQueryCache queryCache, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(tableName);
        ArgumentNullException.ThrowIfNull(queryCache);

        var candidateTableName = QualifyTableName(tableName);
        return GetResolvedTableName(candidateTableName, cancellationToken)
            .MapAsync(name => LoadTableAsyncCore(name, queryCache, cancellationToken));
    }

    private async Task<IRelationalDatabaseTable> LoadTableAsyncCore(Identifier tableName, PostgreSqlTableQueryCache queryCache, CancellationToken cancellationToken)
    {
        var (
            columns,
            checks,
            triggers,
            indexes,
            primaryKey,
            uniqueKeys,
            parentKeys,
            childKeys
        ) = await (
            queryCache.GetColumnsAsync(tableName, cancellationToken),
            LoadChecksAsync(tableName, cancellationToken),
            LoadTriggersAsync(tableName, cancellationToken),
            queryCache.GetIndexesAsync(tableName, cancellationToken),
            queryCache.GetPrimaryKeyAsync(tableName, cancellationToken),
            queryCache.GetUniqueKeysAsync(tableName, cancellationToken),
            queryCache.GetForeignKeysAsync(tableName, cancellationToken),
            LoadChildKeysAsync(tableName, queryCache, cancellationToken)
        ).WhenAll();

        return new RelationalDatabaseTable(
            tableName,
            columns,
            primaryKey,
            uniqueKeys,
            parentKeys,
            childKeys,
            FilterConstraintIndexes(indexes, primaryKey, uniqueKeys),
            checks,
            triggers
        );
    }

    // An index that exists only to enforce a primary or unique key constraint is reported by that
    // constraint's IDatabaseKey.BackingIndex, so it is not repeated in the table's indexes.
    private static IReadOnlyCollection<IDatabaseIndex> FilterConstraintIndexes(
        IReadOnlyCollection<IDatabaseIndex> indexes,
        Option<IDatabaseKey> primaryKey,
        IReadOnlyCollection<IDatabaseKey> uniqueKeys
    )
    {
        var backingIndexNames = uniqueKeys
            .Concat(primaryKey.ToList())
            .SelectMany(static key => key.BackingIndex.ToList())
            .Select(static index => index.Name)
            .ToHashSet();
        if (backingIndexNames.Count == 0)
            return indexes;

        return indexes.Where(index => !backingIndexNames.Contains(index.Name)).ToList();
    }

    // PostgreSQL names the index enforcing a primary or unique key constraint after the constraint
    // itself, so the two are matched by name.
    private static Option<IDatabaseIndex> GetBackingIndex(IReadOnlyCollection<IDatabaseIndex> indexes, Identifier constraintName)
    {
        var backingIndex = indexes.FirstOrDefault(index => index.Name == constraintName);
        return backingIndex != null
            ? Option<IDatabaseIndex>.Some(backingIndex)
            : Option<IDatabaseIndex>.None;
    }

    /// <summary>
    /// Retrieves the primary key for the given table, if available.
    /// </summary>
    /// <param name="tableName">A table name.</param>
    /// <param name="queryCache">A query cache for the given context.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A primary key, if available.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="tableName"/> or <paramref name="queryCache"/> are <see langword="null" />.</exception>
    protected Task<Option<IDatabaseKey>> LoadPrimaryKeyAsync(Identifier tableName, PostgreSqlTableQueryCache queryCache, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(tableName);
        ArgumentNullException.ThrowIfNull(queryCache);

        return LoadPrimaryKeyAsyncCore(tableName, queryCache, cancellationToken);
    }

    private async Task<Option<IDatabaseKey>> LoadPrimaryKeyAsyncCore(Identifier tableName, PostgreSqlTableQueryCache queryCache, CancellationToken cancellationToken)
    {
        var primaryKeyColumns = await DbConnection.QueryAsync(
            GetTablePrimaryKey.Sql,
            new GetTablePrimaryKey.Query { SchemaName = tableName.Schema!, TableName = tableName.LocalName },
            cancellationToken
        );

        if (primaryKeyColumns.Empty())
            return Option<IDatabaseKey>.None;

        var columnLookup = await queryCache.GetColumnLookupAsync(tableName, cancellationToken);

        var groupedByName = primaryKeyColumns.GroupAsDictionary(static row => new { row.ConstraintName, row.IsDeferrable, row.IsInitiallyDeferred });
        var firstRow = groupedByName.First();
        var constraintName = firstRow.Key.ConstraintName;
        if (constraintName == null)
            return Option<IDatabaseKey>.None;

        var keyColumns = ResolveColumns(
            firstRow.Value
                .Where(static row => row.ColumnName != null)
                .OrderBy(static row => row.OrdinalPosition)
                .Select(static row => (Identifier)row.ColumnName!),
            columnLookup
        ).ToList();

        var indexes = await queryCache.GetIndexesAsync(tableName, cancellationToken);
        var backingIndex = GetBackingIndex(indexes, constraintName);

        var deferrability = GetDeferrability(firstRow.Key.IsDeferrable, firstRow.Key.IsInitiallyDeferred);

        var primaryKey = new PostgreSqlDatabaseKey(constraintName, DatabaseKeyType.Primary, keyColumns, backingIndex, true, deferrability);
        return Option<IDatabaseKey>.Some(primaryKey);
    }

    /// <summary>
    /// Retrieves indexes that relate to the given table.
    /// </summary>
    /// <param name="tableName">A table name.</param>
    /// <param name="queryCache">A query cache for the given context.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of indexes.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="tableName"/> or <paramref name="queryCache"/> are <see langword="null" />.</exception>
    protected virtual Task<IReadOnlyCollection<IDatabaseIndex>> LoadIndexesAsync(Identifier tableName, PostgreSqlTableQueryCache queryCache, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(tableName);
        ArgumentNullException.ThrowIfNull(queryCache);

        return LoadIndexesAsyncCore(tableName, queryCache, cancellationToken);
    }

    private async Task<IReadOnlyCollection<IDatabaseIndex>> LoadIndexesAsyncCore(Identifier tableName, PostgreSqlTableQueryCache queryCache, CancellationToken cancellationToken)
    {
        var queryResult = await DbConnection.QueryAsync(
            GetTableIndexes.Sql,
            new GetTableIndexes.Query { SchemaName = tableName.Schema!, TableName = tableName.LocalName },
            cancellationToken
        );

        if (queryResult.Empty())
            return [];

        var indexColumns = queryResult
            .GroupAsDictionary(static row => new
            {
                row.IndexName,
                row.IsUnique,
                row.IsPrimary,
                row.FilterDefinition,
                row.KeyColumnCount,
                row.IndexMethod,
                row.IsValid,
            })
            .ToList();
        if (indexColumns.Empty())
            return [];

        var columnLookup = await queryCache.GetColumnLookupAsync(tableName, cancellationToken);

        var result = new List<IDatabaseIndex>(indexColumns.Count);
        foreach (var indexInfo in indexColumns)
        {
            var isUnique = indexInfo.Key.IsUnique;
            var indexName = Identifier.CreateQualifiedIdentifier(indexInfo.Key.IndexName);

            var filterDefinition = !indexInfo.Key.FilterDefinition.IsNullOrWhiteSpace()
                ? Option<string>.Some(indexInfo.Key.FilterDefinition)
                : Option<string>.None;

            // sorted once and reused for both the key and included columns below, instead of sorting
            // the same rows twice. NOTE: the two branches deliberately keep their existing, slightly
            // different filter ordering relative to Take/Skip (key columns filter nulls before Take,
            // included columns filter nulls after Skip) -- preserved as-is, not a perf-motivated change.
            var sortedRows = indexInfo.Value.OrderBy(static row => row.IndexColumnId).ToList();

            var indexCols = sortedRows
                .Where(static row => row.IndexColumnExpression != null)
                .Select(row => new
                {
                    row.IsDescending,
                    row.IsNullsFirst,
                    row.IndexColumnCollation,
                    Expression = row.IndexColumnExpression,
                    Column = row.IndexColumnExpression != null && columnLookup.TryGetValue(row.IndexColumnExpression, out var indexColumn)
                        ? indexColumn
                        : null,
                })
                .Select(row =>
                {
                    var order = row.IsDescending ? IndexColumnOrder.Descending : IndexColumnOrder.Ascending;
                    var nullOrder = row.IsNullsFirst ? IndexColumnNullOrder.NullsFirst : IndexColumnNullOrder.NullsLast;
                    var collation = !row.IndexColumnCollation.IsNullOrWhiteSpace()
                        ? Option<Identifier>.Some(Identifier.CreateQualifiedIdentifier(row.IndexColumnCollation))
                        : Option<Identifier>.None;
                    var expression = row.Column != null
                        ? Dialect.QuoteName(row.Column.Name)
                        : row.Expression!;
                    return row.Column != null
                        ? new PostgreSqlDatabaseIndexColumn(expression, row.Column, order, nullOrder, collation)
                        : new PostgreSqlDatabaseIndexColumn(expression, order, nullOrder, collation);
                })
                .Take(indexInfo.Key.KeyColumnCount)
                .ToList();
            var includedCols = ResolveColumns(
                sortedRows
                    .Skip(indexInfo.Key.KeyColumnCount)
                    .Where(static row => row.IndexColumnExpression != null)
                    .Select(static row => (Identifier)row.IndexColumnExpression!),
                columnLookup
            ).ToList();

            var indexType = IndexTypeMapping.TryGetValue(indexInfo.Key.IndexMethod, out var mappedIndexType)
                ? mappedIndexType
                : IndexType.Other;

            var index = new PostgreSqlDatabaseIndex(indexName, isUnique, indexCols, includedCols, filterDefinition, indexType, indexInfo.Key.IsValid);
            result.Add(index);
        }

        return result;
    }

    /// <summary>
    /// Retrieves unique keys that relate to the given table.
    /// </summary>
    /// <param name="tableName">A table name.</param>
    /// <param name="queryCache">A query cache for the given context.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of unique keys.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="tableName"/> or <paramref name="queryCache"/> are <see langword="null" />.</exception>
    protected Task<IReadOnlyCollection<IDatabaseKey>> LoadUniqueKeysAsync(Identifier tableName, PostgreSqlTableQueryCache queryCache, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(tableName);
        ArgumentNullException.ThrowIfNull(queryCache);

        return LoadUniqueKeysAsyncCore(tableName, queryCache, cancellationToken);
    }

    private async Task<IReadOnlyCollection<IDatabaseKey>> LoadUniqueKeysAsyncCore(Identifier tableName, PostgreSqlTableQueryCache queryCache, CancellationToken cancellationToken)
    {
        var uniqueKeyColumns = await DbConnection.QueryAsync(
            GetTableUniqueKeys.Sql,
            new GetTableUniqueKeys.Query { SchemaName = tableName.Schema!, TableName = tableName.LocalName },
            cancellationToken
        );

        if (uniqueKeyColumns.Empty())
            return [];

        var columnLookup = await queryCache.GetColumnLookupAsync(tableName, cancellationToken);

        var groupedByName = uniqueKeyColumns.GroupAsDictionary(static row => new { row.ConstraintName, row.IsDeferrable, row.IsInitiallyDeferred });
        var constraintColumns = groupedByName
            .Select(g => new
            {
                g.Key.ConstraintName,
                Deferrability = GetDeferrability(g.Key.IsDeferrable, g.Key.IsInitiallyDeferred),
                Columns = ResolveColumns(
                    g.Value
                        .Where(static row => row.ColumnName != null)
                        .OrderBy(static row => row.OrdinalPosition)
                        .Select(static row => (Identifier)row.ColumnName!),
                    columnLookup
                ).ToList(),
            })
            .ToList();
        if (constraintColumns.Empty())
            return [];

        var indexes = await queryCache.GetIndexesAsync(tableName, cancellationToken);

        var result = new List<IDatabaseKey>(constraintColumns.Count);
        foreach (var uk in constraintColumns)
        {
            var backingIndex = GetBackingIndex(indexes, uk.ConstraintName);
            var uniqueKey = new PostgreSqlDatabaseKey(uk.ConstraintName, DatabaseKeyType.Unique, uk.Columns, backingIndex, true, uk.Deferrability);
            result.Add(uniqueKey);
        }
        return result;
    }

    /// <summary>
    /// Retrieves child keys that relate to the given table.
    /// </summary>
    /// <param name="tableName">A table name.</param>
    /// <param name="queryCache">A query cache for the given context.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of child keys.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="tableName"/> or <paramref name="queryCache"/> are <see langword="null" />.</exception>
    protected Task<IReadOnlyCollection<IDatabaseRelationalKey>> LoadChildKeysAsync(Identifier tableName, PostgreSqlTableQueryCache queryCache, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(tableName);
        ArgumentNullException.ThrowIfNull(queryCache);

        return LoadChildKeysAsyncCore(tableName, queryCache, cancellationToken);
    }

    private async Task<IReadOnlyCollection<IDatabaseRelationalKey>> LoadChildKeysAsyncCore(Identifier tableName, PostgreSqlTableQueryCache queryCache, CancellationToken cancellationToken)
    {
        var queryResult = await DbConnection.QueryAsync(
            GetTableChildKeys.Sql,
            new GetTableChildKeys.Query { SchemaName = tableName.Schema!, TableName = tableName.LocalName },
            cancellationToken
        );

        if (queryResult.Empty())
            return [];

        var groupedChildKeys = queryResult.GroupAsDictionary(static row =>
        new
        {
            row.ChildTableSchema,
            row.ChildTableName,
            row.ChildKeyName,
            row.ParentKeyName,
            row.ParentKeyType,
            row.DeleteAction,
            row.UpdateAction,
        }).ToList();
        if (groupedChildKeys.Empty())
            return [];

        var (primaryKey, uniqueKeys, indexes) = await (
            queryCache.GetPrimaryKeyAsync(tableName, cancellationToken),
            queryCache.GetUniqueKeysAsync(tableName, cancellationToken),
            queryCache.GetIndexesAsync(tableName, cancellationToken)
        ).WhenAll();
        var uniqueKeyLookup = GetDatabaseKeyLookup(uniqueKeys);
        var uniqueIndexLookup = GetUniqueIndexLookup(indexes);

        // memoises the child table's foreign-key lookup across grouped child-key rows that share the
        // same child table, instead of rebuilding it (and re-querying the cache) once per row.
        var childParentKeyLookups = new Dictionary<Identifier, IReadOnlyDictionary<Identifier, IDatabaseRelationalKey>>(IdentifierComparer.Ordinal);

        var result = new List<IDatabaseRelationalKey>(groupedChildKeys.Count);

        foreach (var groupedChildKey in groupedChildKeys)
        {
            // ensure we have a key to begin with
            IDatabaseKey? parentKey = null;
            if (string.Equals(groupedChildKey.Key.ParentKeyType, Constants.PrimaryKeyType, StringComparison.Ordinal))
                await primaryKey.IfSomeAsync(k => parentKey = k);
            else if (uniqueKeyLookup.TryGetValue(groupedChildKey.Key.ParentKeyName, out var uniqueKey))
                parentKey = uniqueKey;
            else if (uniqueIndexLookup.TryGetValue(groupedChildKey.Key.ParentKeyName, out var uniqueIndex))
                // the foreign key references a unique index with no backing UNIQUE constraint
                parentKey = CreateKeyFromUniqueIndex(uniqueIndex);

            if (parentKey == null)
                continue;

            var candidateChildTableName = Identifier.CreateQualifiedIdentifier(groupedChildKey.Key.ChildTableSchema, groupedChildKey.Key.ChildTableName);
            var childTableNameOption = queryCache.GetTableNameAsync(candidateChildTableName, cancellationToken);

            await childTableNameOption
                .BindAsync(async childTableName =>
                {
                    if (!childParentKeyLookups.TryGetValue(childTableName, out var parentKeyLookup))
                    {
                        var childParentKeys = await queryCache.GetForeignKeysAsync(childTableName, cancellationToken);
                        parentKeyLookup = GetRelationalKeyLookup(childParentKeys);
                        childParentKeyLookups[childTableName] = parentKeyLookup;
                    }

                    var childKeyName = Identifier.CreateQualifiedIdentifier(groupedChildKey.Key.ChildKeyName);
                    if (!parentKeyLookup.TryGetValue(childKeyName, out var childRelationalKey))
                        return OptionAsync<IDatabaseRelationalKey>.None;

                    var deleteAction = ReferentialActionMapping[groupedChildKey.Key.DeleteAction];
                    var updateAction = ReferentialActionMapping[groupedChildKey.Key.UpdateAction];
                    // the match type and ON DELETE SET NULL column subset describe the same constraint
                    // as seen from the child table, so they are taken from the child's own relational key
                    var relationalKey = new DatabaseRelationalKey(childTableName, childRelationalKey.ChildKey, tableName, parentKey, deleteAction, updateAction, childRelationalKey.MatchType, childRelationalKey.SetNullColumns);
                    return OptionAsync<IDatabaseRelationalKey>.Some(relationalKey);
                })
                .IfSome(result.Add);
        }

        return result;
    }

    /// <summary>
    /// Retrieves check constraints defined on a given table.
    /// </summary>
    /// <param name="tableName">A table name.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of check constraints.</returns>
    protected virtual async Task<IReadOnlyCollection<IDatabaseCheckConstraint>> LoadChecksAsync(Identifier tableName, CancellationToken cancellationToken)
    {
        const string checkPrefix = "CHECK (";
        const string checkSuffix = ")";

        return await DbConnection.QueryEnumerableAsync(
                GetTableChecks.Sql,
                new GetTableChecks.Query { SchemaName = tableName.Schema!, TableName = tableName.LocalName },
                cancellationToken
            )
            .Where(checkRow => !checkRow.Definition.IsNullOrWhiteSpace())
            .Select(checkRow =>
            {
                var definition = checkRow.Definition!;
                if (definition.StartsWith(checkPrefix, StringComparison.OrdinalIgnoreCase))
                    definition = definition[checkPrefix.Length..];
                if (definition.EndsWith(')') && definition.Length > 0) // check suffix
                    definition = definition[..^checkSuffix.Length];

                var constraintName = Identifier.CreateQualifiedIdentifier(checkRow.ConstraintName);

                return new PostgreSqlCheckConstraint(constraintName, definition, checkRow.IsValidated);
            })
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Retrieves foreign keys that relate to the given table.
    /// </summary>
    /// <param name="tableName">A table name.</param>
    /// <param name="queryCache">A query cache for the given context.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of foreign keys.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="tableName"/> or <paramref name="queryCache"/> are <see langword="null" />.</exception>
    protected Task<IReadOnlyCollection<IDatabaseRelationalKey>> LoadParentKeysAsync(Identifier tableName, PostgreSqlTableQueryCache queryCache, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(tableName);
        ArgumentNullException.ThrowIfNull(queryCache);

        return LoadParentKeysAsyncCore(tableName, queryCache, cancellationToken);
    }

    private async Task<IReadOnlyCollection<IDatabaseRelationalKey>> LoadParentKeysAsyncCore(Identifier tableName, PostgreSqlTableQueryCache queryCache, CancellationToken cancellationToken)
    {
        var queryResult = await DbConnection.QueryAsync(
            GetTableParentKeys.Sql,
            new GetTableParentKeys.Query { SchemaName = tableName.Schema!, TableName = tableName.LocalName },
            cancellationToken
        );

        if (queryResult.Empty())
            return [];

        var foreignKeys = queryResult.GroupAsDictionary(static row => new
        {
            row.ChildKeyName,
            row.ParentSchemaName,
            row.ParentTableName,
            row.ParentKeyName,
            KeyType = row.ParentKeyType,
            row.DeleteAction,
            row.UpdateAction,
            row.IsValidated,
            row.IsDeferrable,
            row.IsInitiallyDeferred,
            row.MatchType,
        }).ToList();
        if (foreignKeys.Empty())
            return [];

        var columnLookup = await queryCache.GetColumnLookupAsync(tableName, cancellationToken);

        // memoises the parent table's unique-key/unique-index lookups across foreign keys that share
        // the same parent table, instead of rebuilding them (and re-querying the cache) once per key.
        var parentUniqueKeyLookups = new Dictionary<Identifier, IReadOnlyDictionary<Identifier, IDatabaseKey>>(IdentifierComparer.Ordinal);
        var parentUniqueIndexLookups = new Dictionary<Identifier, IReadOnlyDictionary<Identifier, IDatabaseIndex>>(IdentifierComparer.Ordinal);

        var result = new List<IDatabaseRelationalKey>(foreignKeys.Count);
        foreach (var fkey in foreignKeys)
        {
            var candidateParentTableName = Identifier.CreateQualifiedIdentifier(fkey.Key.ParentSchemaName, fkey.Key.ParentTableName);
            var parentTableNameOption = await queryCache.GetTableNameAsync(candidateParentTableName, cancellationToken);
            Identifier? resolvedParentTableName = null;

            await parentTableNameOption
                .BindAsync(async parentTableName =>
                {
                    resolvedParentTableName = parentTableName;
                    if (string.Equals(fkey.Key.KeyType, Constants.PrimaryKeyType, StringComparison.Ordinal))
                    {
                        var pk = await queryCache.GetPrimaryKeyAsync(parentTableName, cancellationToken);
                        return pk.ToAsync();
                    }

                    var parentKeyName = Identifier.CreateQualifiedIdentifier(fkey.Key.ParentKeyName);

                    if (!parentUniqueKeyLookups.TryGetValue(parentTableName, out var uniqueKeyLookup))
                    {
                        var uniqueKeys = await queryCache.GetUniqueKeysAsync(parentTableName, cancellationToken);
                        uniqueKeyLookup = GetDatabaseKeyLookup(uniqueKeys);
                        parentUniqueKeyLookups[parentTableName] = uniqueKeyLookup;
                    }

                    if (uniqueKeyLookup.TryGetValue(parentKeyName.LocalName, out var uniqueKey))
                        return OptionAsync<IDatabaseKey>.Some(uniqueKey);

                    // the foreign key references a unique index with no backing UNIQUE constraint
                    if (!parentUniqueIndexLookups.TryGetValue(parentTableName, out var parentUniqueIndexLookup))
                    {
                        var parentIndexes = await queryCache.GetIndexesAsync(parentTableName, cancellationToken);
                        parentUniqueIndexLookup = GetUniqueIndexLookup(parentIndexes);
                        parentUniqueIndexLookups[parentTableName] = parentUniqueIndexLookup;
                    }

                    return parentUniqueIndexLookup.TryGetValue(parentKeyName.LocalName, out var uniqueIndex)
                        ? OptionAsync<IDatabaseKey>.Some(CreateKeyFromUniqueIndex(uniqueIndex))
                        : OptionAsync<IDatabaseKey>.None;
                })
                .Map(parentKey =>
                {
                    var parentTableName = resolvedParentTableName!;

                    var childKeyName = Identifier.CreateQualifiedIdentifier(fkey.Key.ChildKeyName);
                    var childKeyColumns = ResolveColumns(
                        fkey.Value
                            .Where(static row => row.ColumnName != null)
                            .OrderBy(static row => row.ConstraintColumnId)
                            .Select(static row => (Identifier)row.ColumnName!),
                        columnLookup
                    ).ToList();

                    var deferrability = GetDeferrability(fkey.Key.IsDeferrable, fkey.Key.IsInitiallyDeferred);
                    var childKey = new PostgreSqlDatabaseKey(childKeyName, DatabaseKeyType.Foreign, childKeyColumns, Option<IDatabaseIndex>.None, fkey.Key.IsValidated, deferrability);

                    var deleteAction = ReferentialActionMapping[fkey.Key.DeleteAction];
                    var updateAction = ReferentialActionMapping[fkey.Key.UpdateAction];
                    var matchType = MatchTypeMapping.TryGetValue(fkey.Key.MatchType, out var mappedMatchType)
                        ? mappedMatchType
                        : ForeignKeyMatchType.Simple;
                    var setNullColumns = ResolveColumns(
                        fkey.Value
                            .Where(static row => row.ColumnName != null && row.IsSetNullColumn)
                            .OrderBy(static row => row.ConstraintColumnId)
                            .Select(static row => (Identifier)row.ColumnName!),
                        columnLookup
                    ).ToList();

                    return new DatabaseRelationalKey(tableName, childKey, parentTableName, parentKey, deleteAction, updateAction, matchType, setNullColumns);
                })
                .IfSome(result.Add);
        }

        return result;
    }

    /// <summary>
    /// Retrieves the columns for a given table.
    /// </summary>
    /// <param name="tableName">A table name.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>An ordered collection of columns.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="tableName"/> is <see langword="null" />.</exception>
    protected virtual Task<IReadOnlyList<IDatabaseColumn>> LoadColumnsAsync(Identifier tableName, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(tableName);

        return LoadColumnsAsyncCore(tableName, cancellationToken);
    }

    private async Task<IReadOnlyList<IDatabaseColumn>> LoadColumnsAsyncCore(Identifier tableName, CancellationToken cancellationToken)
    {
        return await DbConnection.QueryEnumerableAsync(
                GetTableColumns.Sql,
                new GetTableColumns.Query { SchemaName = tableName.Schema!, TableName = tableName.LocalName },
                cancellationToken
            )
            .Select(row =>
            {
                var typeMetadata = PostgreSqlColumnTypeMetadata.Create(
                    TypeProvider,
                    new PostgreSqlColumnTypeMetadata.CatalogTypeInfo(
                        row.DataType,
                        row.UdtSchema,
                        row.UdtName,
                        row.DomainSchema,
                        row.DomainName,
                        row.TypeKind,
                        row.ElementTypeSchema,
                        row.ElementTypeName,
                        row.ElementTypeKind,
                        row.EnumLabels),
                    !row.CollationName.IsNullOrWhiteSpace()
                        ? Option<Identifier>.Some(Identifier.CreateQualifiedIdentifier(row.CollationCatalog, row.CollationSchema, row.CollationName))
                        : Option<Identifier>.None,
                    row.CharacterMaximumLength > 0
                        ? row.CharacterMaximumLength
                        : CreatePrecisionFromBase(row.NumericPrecision, row.NumericPrecisionRadix),
                    row.NumericPrecisionRadix > 0
                        ? Option<INumericPrecision>.Some(CreatePrecisionWithScaleFromBase(row.NumericPrecision, row.NumericScale, row.NumericPrecisionRadix))
                        : Option<INumericPrecision>.None);

                var columnType = TypeProvider.CreateColumnType(typeMetadata);
                var columnName = Identifier.CreateQualifiedIdentifier(row.ColumnName);

                var sequenceName = !row.SequenceSchemaName.IsNullOrWhiteSpace() && !row.SequenceLocalName.IsNullOrWhiteSpace()
                    ? Option<Identifier>.Some(Identifier.CreateQualifiedIdentifier(row.SequenceSchemaName, row.SequenceLocalName))
                    : Option<Identifier>.None;

                var isAutoIncrement = string.Equals(row.IsIdentity, Constants.Yes, StringComparison.Ordinal);
                var autoIncrement = isAutoIncrement
                    && decimal.TryParse(row.IdentityStart, NumberStyles.Float, CultureInfo.InvariantCulture, out var seqStart)
                    && decimal.TryParse(row.IdentityIncrement, NumberStyles.Float, CultureInfo.InvariantCulture, out var seqIncr)
                    ? Option<IAutoIncrement>.Some(new AutoIncrement(
                        seqStart,
                        seqIncr,
                        string.Equals(row.IdentityGeneration, Constants.Always, StringComparison.Ordinal)
                            ? IdentityGeneration.Always
                            : IdentityGeneration.ByDefault,
                        ParseNumericBound(row.IdentityMinimum),
                        ParseNumericBound(row.IdentityMaximum),
                        string.Equals(row.IdentityCycle, Constants.Yes, StringComparison.Ordinal),
                        sequenceName))
                    : Option<IAutoIncrement>.None;

                // A serial column is an ordinary column defaulting to nextval() over an owned
                // sequence, so its parameters live on the sequence rather than on the column, and an
                // explicitly supplied value is always accepted.
                var isSerialAutoIncrement = !isAutoIncrement && sequenceName.IsSome;
                if (isSerialAutoIncrement)
                {
                    autoIncrement = Option<IAutoIncrement>.Some(new AutoIncrement(
                        row.SequenceStart ?? 1,
                        row.SequenceIncrement is long increment && increment != 0 ? increment : 1,
                        IdentityGeneration.ByDefault,
                        ToNumericBound(row.SequenceMinValue),
                        ToNumericBound(row.SequenceMaxValue),
                        row.SequenceCycle == true,
                        sequenceName));
                }

                var defaultValue = !row.ColumnDefault.IsNullOrWhiteSpace()
                    ? Option<string>.Some(row.ColumnDefault)
                    : Option<string>.None;
                var isNullable = string.Equals(row.IsNullable, Constants.Yes, StringComparison.Ordinal);

                var isComputed = string.Equals(row.IsGenerated, Constants.Always, StringComparison.Ordinal);
                var computedDefinition = isComputed
                    ? Option<string>.Some(row.GenerationExpression ?? string.Empty)
                    : Option<string>.None;

                // Generated columns were stored-only until virtual generated columns arrived, so a
                // server that does not report the kind can only have stored ones.
                var computedStorage = string.Equals(row.GenerationKind, Constants.VirtualGenerated, StringComparison.Ordinal)
                    ? ComputedColumnStorage.Virtual
                    : ComputedColumnStorage.Stored;

                return new DatabaseColumn(
                    columnName,
                    columnType,
                    isNullable,
                    defaultValue,
                    autoIncrement,
                    isComputed,
                    computedDefinition,
                    computedStorage);
            })
            .ToListAsync(cancellationToken);
    }

    // information_schema reports sequence bounds as text, because they may exceed the range of any
    // one SQL numeric type.
    private static Option<decimal> ParseNumericBound(string? value)
    {
        return decimal.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var result)
            ? Option<decimal>.Some(result)
            : Option<decimal>.None;
    }

    private static Option<decimal> ToNumericBound(long? value)
    {
        return value.HasValue
            ? Option<decimal>.Some(value.Value)
            : Option<decimal>.None;
    }

    /// <summary>
    /// Retrieves all triggers defined on a table.
    /// </summary>
    /// <param name="tableName">A table name.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of triggers.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="tableName"/> is <see langword="null" />.</exception>
    protected Task<IReadOnlyCollection<IDatabaseTrigger>> LoadTriggersAsync(Identifier tableName, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(tableName);

        return LoadTriggersAsyncCore(tableName, cancellationToken);
    }

    private async Task<IReadOnlyCollection<IDatabaseTrigger>> LoadTriggersAsyncCore(Identifier tableName, CancellationToken cancellationToken)
    {
        var queryResult = await DbConnection.QueryAsync(
            GetTableTriggers.Sql,
            new GetTableTriggers.Query { SchemaName = tableName.Schema!, TableName = tableName.LocalName },
            cancellationToken
        );

        if (queryResult.Empty())
            return [];

        var triggers = queryResult.GroupAsDictionary(static row => new
        {
            row.TriggerName,
            row.Definition,
            row.Timing,
            row.Granularity,
            row.Condition,
            row.EnabledFlag,
        }).ToList();
        if (triggers.Empty())
            return [];

        var result = new List<IDatabaseTrigger>(triggers.Count);
        foreach (var trig in triggers)
        {
            var triggerName = Identifier.CreateQualifiedIdentifier(trig.Key.TriggerName);
            var queryTiming = Enum.TryParse(trig.Key.Timing, true, out TriggerQueryTiming timing) ? timing : TriggerQueryTiming.Before;
            var definition = trig.Key.Definition;

            var events = TriggerEvent.None;
            foreach (var triggerEvent in trig.Value.Select(t => t.TriggerEvent))
            {
                if (string.Equals(triggerEvent, Constants.Insert, StringComparison.Ordinal))
                    events |= TriggerEvent.Insert;
                else if (string.Equals(triggerEvent, Constants.Update, StringComparison.Ordinal))
                    events |= TriggerEvent.Update;
                else if (string.Equals(triggerEvent, Constants.Delete, StringComparison.Ordinal))
                    events |= TriggerEvent.Delete;
                else if (string.Equals(triggerEvent, Constants.Truncate, StringComparison.Ordinal))
                    events |= TriggerEvent.Truncate;
                else
                    events |= TriggerEvent.Other;
            }

            var granularity = string.Equals(trig.Key.Granularity, Constants.Row, StringComparison.Ordinal)
                ? TriggerGranularity.Row
                : TriggerGranularity.Statement;
            var condition = !trig.Key.Condition.IsNullOrWhiteSpace()
                ? Option<string>.Some(trig.Key.Condition)
                : Option<string>.None;
            // tgattr is per-trigger, so any row of the group carries the same UPDATE OF column list.
            var updateColumns = trig.Value[0].UpdateColumns?
                .Select(static c => Identifier.CreateQualifiedIdentifier(c))
                .ToList() ?? [];

            var isEnabled = !string.Equals(trig.Key.EnabledFlag, Constants.DisabledFlag, StringComparison.Ordinal);
            var trigger = new PostgreSqlDatabaseTrigger(
                triggerName,
                definition,
                queryTiming,
                events,
                isEnabled,
                granularity,
                condition,
                updateColumns
            );
            result.Add(trigger);
        }

        return result;
    }

    /// <summary>
    /// Creates a column lookup, keyed by the column's name.
    /// </summary>
    /// <param name="columns">Columns to create a lookup from.</param>
    /// <returns>A dictionary whose keys are column names, and the values are the columns associated with those names.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="columns"/> is <see langword="null" />.</exception>
    protected static IReadOnlyDictionary<Identifier, IDatabaseColumn> GetColumnLookup(IReadOnlyCollection<IDatabaseColumn> columns)
    {
        ArgumentNullException.ThrowIfNull(columns);

        var result = new Dictionary<Identifier, IDatabaseColumn>(columns.Count);

        foreach (var column in columns)
        {
            if (column.Name != null)
                result[column.Name.LocalName] = column;
        }

        return result;
    }

    // Resolves a sequence of column names against a lookup, preserving order and silently skipping any
    // name that has no corresponding column.
    private static IEnumerable<IDatabaseColumn> ResolveColumns(IEnumerable<Identifier> columnNames, IReadOnlyDictionary<Identifier, IDatabaseColumn> columnLookup)
    {
        foreach (var name in columnNames)
        {
            if (columnLookup.TryGetValue(name, out var column))
                yield return column;
        }
    }

    private static ConstraintDeferrability GetDeferrability(bool isDeferrable, bool isInitiallyDeferred)
    {
        if (!isDeferrable)
            return ConstraintDeferrability.NotDeferrable;

        return isInitiallyDeferred
            ? ConstraintDeferrability.DeferrableInitiallyDeferred
            : ConstraintDeferrability.DeferrableInitiallyImmediate;
    }

    private static IReadOnlyDictionary<Identifier, IDatabaseRelationalKey> GetRelationalKeyLookup(IReadOnlyCollection<IDatabaseRelationalKey> relationalKeys)
    {
        ArgumentNullException.ThrowIfNull(relationalKeys);

        var result = new Dictionary<Identifier, IDatabaseRelationalKey>(relationalKeys.Count);

        foreach (var relationalKey in relationalKeys)
        {
            relationalKey.ChildKey.Name.IfSome(name => result[name.LocalName] = relationalKey);
        }

        return result;
    }

    private static IReadOnlyDictionary<Identifier, IDatabaseKey> GetDatabaseKeyLookup(IReadOnlyCollection<IDatabaseKey> keys)
    {
        ArgumentNullException.ThrowIfNull(keys);

        var result = new Dictionary<Identifier, IDatabaseKey>(keys.Count);

        foreach (var key in keys)
        {
            key.Name.IfSome(name => result[name.LocalName] = key);
        }

        return result;
    }

    // Builds a lookup of unique indexes by name. Used to resolve foreign keys that reference a unique index which
    // may not appear in a table's unique keys at all. This looks up such indexes by name so they can still be
    // resolved to a key.
    private static IReadOnlyDictionary<Identifier, IDatabaseIndex> GetUniqueIndexLookup(IReadOnlyCollection<IDatabaseIndex> indexes)
    {
        ArgumentNullException.ThrowIfNull(indexes);

        var result = new Dictionary<Identifier, IDatabaseIndex>(indexes.Count);

        foreach (var index in indexes)
        {
            if (index.IsUnique)
                result[index.Name.LocalName] = index;
        }

        return result;
    }

    // Synthesizes a key from a unique index so that a foreign key referencing a bare unique index (i.e. one with
    // no backing UNIQUE constraint) can still be represented as an IDatabaseKey.
    private static IDatabaseKey CreateKeyFromUniqueIndex(IDatabaseIndex uniqueIndex)
    {
        var columns = uniqueIndex.Columns.SelectMany(static ic => ic.DependentColumns).ToList();
        return new PostgreSqlDatabaseKey(uniqueIndex.Name, DatabaseKeyType.Unique, columns);
    }

    /// <summary>
    /// Creates a numeric precision given a base.
    /// </summary>
    /// <param name="precision">The numeric precision.</param>
    /// <param name="radix">The radix.</param>
    /// <returns>A numeric precision object.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="radix"/> is less than zero.</exception>
    protected static int CreatePrecisionFromBase(int precision, int radix)
    {
        if (precision <= 0)
            return 0;
        ArgumentOutOfRangeException.ThrowIfNegative(radix);

        var newPrecision = Convert.ToInt64(Math.Pow(precision, radix));
        var newPrecisionStr = newPrecision.ToString(CultureInfo.InvariantCulture);

        return newPrecisionStr.Length;
    }

    /// <summary>
    /// Creates a numeric precision with scale, given a base.
    /// </summary>
    /// <param name="precision">The numeric precision.</param>
    /// <param name="scale">The numeric scale.</param>
    /// <param name="radix">The radix.</param>
    /// <returns>A numeric precision object.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="precision"/> or <paramref name="scale"/> or <paramref name="radix"/> are less than zero.</exception>
    protected static INumericPrecision CreatePrecisionWithScaleFromBase(int precision, int scale, int radix)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(precision);
        ArgumentOutOfRangeException.ThrowIfNegative(scale);
        ArgumentOutOfRangeException.ThrowIfNegative(radix);

        var newPrecision = Convert.ToInt64(Math.Pow(precision, radix));
        var newPrecisionStr = newPrecision.ToString(CultureInfo.InvariantCulture);

        var newScale = Convert.ToInt64(Math.Pow(scale, radix));
        var newScaleStr = newScale.ToString(CultureInfo.InvariantCulture);

        return new NumericPrecision(newPrecisionStr.Length, newScaleStr.Length);
    }

    /// <summary>
    /// A mapping from the referential actions as described in PostgreSQL, to a <see cref="ReferentialAction"/> instance.
    /// </summary>
    /// <value>A mapping dictionary.</value>
    protected IReadOnlyDictionary<string, ReferentialAction> ReferentialActionMapping { get; } = new Dictionary<string, ReferentialAction>(StringComparer.Ordinal)
    {
        ["a"] = ReferentialAction.NoAction,
        ["r"] = ReferentialAction.Restrict,
        ["c"] = ReferentialAction.Cascade,
        ["n"] = ReferentialAction.SetNull,
        ["d"] = ReferentialAction.SetDefault,
    };

    /// <summary>
    /// A mapping from the foreign key match types as described in PostgreSQL, to a <see cref="ForeignKeyMatchType"/> instance.
    /// </summary>
    /// <value>A mapping dictionary.</value>
    protected IReadOnlyDictionary<string, ForeignKeyMatchType> MatchTypeMapping { get; } = new Dictionary<string, ForeignKeyMatchType>(StringComparer.Ordinal)
    {
        ["s"] = ForeignKeyMatchType.Simple,
        ["p"] = ForeignKeyMatchType.Partial,
        ["f"] = ForeignKeyMatchType.Full,
    };

    /// <summary>
    /// Qualifies the name of a table, using known identifier defaults.
    /// </summary>
    /// <param name="tableName">A table name to qualify.</param>
    /// <returns>A table name that is at least as qualified as its input.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="tableName"/> is <see langword="null" />.</exception>
    protected Identifier QualifyTableName(Identifier tableName)
    {
        ArgumentNullException.ThrowIfNull(tableName);

        var schema = tableName.Schema ?? IdentifierDefaults.Schema;
        return Identifier.CreateQualifiedIdentifier(IdentifierDefaults.Server, IdentifierDefaults.Database, schema, tableName.LocalName);
    }

    /// <summary>
    /// A set of constants used to test results of queries.
    /// </summary>
    protected static class Constants
    {
        /// <summary>
        /// Used to check whether a trigger event is a <c>DELETE</c> event.
        /// </summary>
        public const string Delete = "DELETE";

        /// <summary>
        /// Determines whether a trigger is enabled.
        /// </summary>
        public const string DisabledFlag = "D";

        /// <summary>
        /// Used to check whether a trigger event is an <c>INSERT</c> event.
        /// </summary>
        public const string Insert = "INSERT";

        /// <summary>
        /// The built-in system schema.
        /// </summary>
        public const string PgCatalog = "pg_catalog";

        /// <summary>
        /// Determines whether a key type is a primary key.
        /// </summary>
        public const string PrimaryKeyType = "p";

        /// <summary>
        /// Used to check whether a trigger fires once per row, rather than once per statement.
        /// </summary>
        public const string Row = "ROW";

        /// <summary>
        /// Used to check whether a trigger event is a <c>TRUNCATE</c> event.
        /// </summary>
        public const string Truncate = "TRUNCATE";

        /// <summary>
        /// Used to check whether a trigger event is an <c>UPDATE</c> event.
        /// </summary>
        public const string Update = "UPDATE";

        /// <summary>
        /// Some queries return yes/no, this handles the yes case.
        /// </summary>
        public const string Yes = "YES";

        /// <summary>
        /// Determines whether a column is generated.
        /// </summary>
        public const string Always = "ALWAYS";

        /// <summary>
        /// The <c>pg_attribute.attgenerated</c> value given to a generated column that is computed on read.
        /// </summary>
        public const string VirtualGenerated = "v";
    }

    // pg_am.amname values for the access methods shipped with PostgreSQL. Anything else, e.g. an
    // access method provided by an extension, is reported as IndexType.Other.
    private static readonly IReadOnlyDictionary<string, IndexType> IndexTypeMapping = new Dictionary<string, IndexType>(StringComparer.OrdinalIgnoreCase)
    {
        ["btree"] = IndexType.BTree,
        ["hash"] = IndexType.Hash,
        ["gin"] = IndexType.Gin,
        ["gist"] = IndexType.Gist,
        ["brin"] = IndexType.Brin,
    };

    /// <summary>
    /// A query cache provider for PostgreSQL tables. Ensures that a given query only occurs at most once for a given query context.
    /// </summary>
    protected class PostgreSqlTableQueryCache
    {
        private readonly AsyncCache<Identifier, Option<Identifier>, PostgreSqlTableQueryCache> _tableNames;
        private readonly AsyncCache<Identifier, IReadOnlyList<IDatabaseColumn>, PostgreSqlTableQueryCache> _columns;
        private readonly AsyncCache<Identifier, IReadOnlyDictionary<Identifier, IDatabaseColumn>, PostgreSqlTableQueryCache> _columnLookups;
        private readonly AsyncCache<Identifier, Option<IDatabaseKey>, PostgreSqlTableQueryCache> _primaryKeys;
        private readonly AsyncCache<Identifier, IReadOnlyCollection<IDatabaseKey>, PostgreSqlTableQueryCache> _uniqueKeys;
        private readonly AsyncCache<Identifier, IReadOnlyCollection<IDatabaseIndex>, PostgreSqlTableQueryCache> _indexes;
        private readonly AsyncCache<Identifier, IReadOnlyCollection<IDatabaseRelationalKey>, PostgreSqlTableQueryCache> _foreignKeys;

        /// <summary>
        /// Initializes a new instance of the <see cref="PostgreSqlTableQueryCache"/> class.
        /// </summary>
        /// <param name="tableNameLoader">A table name cache.</param>
        /// <param name="columnLoader">A column cache.</param>
        /// <param name="columnLookupLoader">A column lookup cache.</param>
        /// <param name="primaryKeyLoader">A primary key cache.</param>
        /// <param name="uniqueKeyLoader">A unique key cache.</param>
        /// <param name="indexLoader">An index cache.</param>
        /// <param name="foreignKeyLoader">A foreign key cache.</param>
        /// <exception cref="ArgumentNullException">Thrown when any of <paramref name="tableNameLoader"/>, <paramref name="columnLoader"/>, <paramref name="columnLookupLoader"/>, <paramref name="primaryKeyLoader"/>, <paramref name="uniqueKeyLoader"/>, <paramref name="indexLoader"/> or <paramref name="foreignKeyLoader"/> are <see langword="null" />.</exception>
        public PostgreSqlTableQueryCache(
            AsyncCache<Identifier, Option<Identifier>, PostgreSqlTableQueryCache> tableNameLoader,
            AsyncCache<Identifier, IReadOnlyList<IDatabaseColumn>, PostgreSqlTableQueryCache> columnLoader,
            AsyncCache<Identifier, IReadOnlyDictionary<Identifier, IDatabaseColumn>, PostgreSqlTableQueryCache> columnLookupLoader,
            AsyncCache<Identifier, Option<IDatabaseKey>, PostgreSqlTableQueryCache> primaryKeyLoader,
            AsyncCache<Identifier, IReadOnlyCollection<IDatabaseKey>, PostgreSqlTableQueryCache> uniqueKeyLoader,
            AsyncCache<Identifier, IReadOnlyCollection<IDatabaseIndex>, PostgreSqlTableQueryCache> indexLoader,
            AsyncCache<Identifier, IReadOnlyCollection<IDatabaseRelationalKey>, PostgreSqlTableQueryCache> foreignKeyLoader
        )
        {
            _tableNames = tableNameLoader ?? throw new ArgumentNullException(nameof(tableNameLoader));
            _columns = columnLoader ?? throw new ArgumentNullException(nameof(columnLoader));
            _columnLookups = columnLookupLoader ?? throw new ArgumentNullException(nameof(columnLookupLoader));
            _primaryKeys = primaryKeyLoader ?? throw new ArgumentNullException(nameof(primaryKeyLoader));
            _uniqueKeys = uniqueKeyLoader ?? throw new ArgumentNullException(nameof(uniqueKeyLoader));
            _indexes = indexLoader ?? throw new ArgumentNullException(nameof(indexLoader));
            _foreignKeys = foreignKeyLoader ?? throw new ArgumentNullException(nameof(foreignKeyLoader));
        }

        /// <summary>
        /// Retrieves the table name from the cache, querying the database when not populated.
        /// </summary>
        /// <param name="tableName">A table name.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A table name, if matched in the database.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="tableName"/> is <see langword="null" />.</exception>
        public Task<Option<Identifier>> GetTableNameAsync(Identifier tableName, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(tableName);

            return _tableNames.GetByKeyAsync(tableName, this, cancellationToken);
        }

        /// <summary>
        /// Retrieves a table's columns from the cache, querying the database when not populated.
        /// </summary>
        /// <param name="tableName">A table name.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A collection of columns.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="tableName"/> is <see langword="null" />.</exception>
        public Task<IReadOnlyList<IDatabaseColumn>> GetColumnsAsync(Identifier tableName, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(tableName);

            return _columns.GetByKeyAsync(tableName, this, cancellationToken);
        }

        /// <summary>
        /// Retrieves a table's column lookup from the cache, querying the database when not populated.
        /// </summary>
        /// <param name="tableName">A table name.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A dictionary whose keys are column names, and the values are the columns associated with those names.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="tableName"/> is <see langword="null" />.</exception>
        public Task<IReadOnlyDictionary<Identifier, IDatabaseColumn>> GetColumnLookupAsync(Identifier tableName, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(tableName);

            return _columnLookups.GetByKeyAsync(tableName, this, cancellationToken);
        }

        /// <summary>
        /// Retrieves a table's primary key from the cache, querying the database when not populated.
        /// </summary>
        /// <param name="tableName">A table name.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A primary key, if available.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="tableName"/> is <see langword="null" />.</exception>
        public Task<Option<IDatabaseKey>> GetPrimaryKeyAsync(Identifier tableName, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(tableName);

            return _primaryKeys.GetByKeyAsync(tableName, this, cancellationToken);
        }

        /// <summary>
        /// Retrieves a table's unique keys from the cache, querying the database when not populated.
        /// </summary>
        /// <param name="tableName">A table name.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A collection of unique keys.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="tableName"/> is <see langword="null" />.</exception>
        public Task<IReadOnlyCollection<IDatabaseKey>> GetUniqueKeysAsync(Identifier tableName, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(tableName);

            return _uniqueKeys.GetByKeyAsync(tableName, this, cancellationToken);
        }

        /// <summary>
        /// Retrieves a table's indexes from the cache, querying the database when not populated.
        /// </summary>
        /// <param name="tableName">A table name.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A collection of indexes.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="tableName"/> is <see langword="null" />.</exception>
        public Task<IReadOnlyCollection<IDatabaseIndex>> GetIndexesAsync(Identifier tableName, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(tableName);

            return _indexes.GetByKeyAsync(tableName, this, cancellationToken);
        }

        /// <summary>
        /// Retrieves a table's foreign keys from the cache, querying the database when not populated.
        /// </summary>
        /// <param name="tableName">A table name.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A collection of foreign keys.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="tableName"/> is <see langword="null" />.</exception>
        public Task<IReadOnlyCollection<IDatabaseRelationalKey>> GetForeignKeysAsync(Identifier tableName, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(tableName);

            return _foreignKeys.GetByKeyAsync(tableName, this, cancellationToken);
        }
    }
}