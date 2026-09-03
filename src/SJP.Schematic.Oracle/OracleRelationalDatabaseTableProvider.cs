using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LanguageExt;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Exceptions;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Core.Utilities;
using SJP.Schematic.Oracle.Queries;

namespace SJP.Schematic.Oracle;

/// <summary>
/// A database table provider for Oracle.
/// </summary>
/// <seealso cref="IRelationalDatabaseTableProvider" />
public class OracleRelationalDatabaseTableProvider : IRelationalDatabaseTableProvider
{
    /// <summary>
    /// Initializes a new instance of the <see cref="OracleRelationalDatabaseTableProvider"/> class.
    /// </summary>
    /// <param name="connection">A schematic connection.</param>
    /// <param name="identifierDefaults">Database identifier defaults.</param>
    /// <param name="identifierResolver">An identifier resolver.</param>
    /// <exception cref="ArgumentNullException"><paramref name="connection"/> or <paramref name="identifierDefaults"/> or <paramref name="identifierResolver"/> are <see langword="null" />.</exception>
    public OracleRelationalDatabaseTableProvider(ISchematicConnection connection, IIdentifierDefaults identifierDefaults, IIdentifierResolutionStrategy identifierResolver)
    {
        Connection = connection ?? throw new ArgumentNullException(nameof(connection));
        IdentifierDefaults = identifierDefaults ?? throw new ArgumentNullException(nameof(identifierDefaults));
        IdentifierResolver = identifierResolver ?? throw new ArgumentNullException(nameof(identifierResolver));
    }

    /// <summary>
    /// A database connection that is specific to a given Oracle database.
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
    protected OracleTableQueryCache CreateQueryCache() => new(
        new AsyncCache<Identifier, Option<Identifier>, OracleTableQueryCache>((tableName, _, token) => GetResolvedTableName(tableName, token)),
        new AsyncCache<Identifier, IReadOnlyList<IDatabaseColumn>, OracleTableQueryCache>(LoadColumnsAsync),
        new AsyncCache<Identifier, Option<IDatabaseKey>, OracleTableQueryCache>(LoadPrimaryKeyAsync),
        new AsyncCache<Identifier, IReadOnlyCollection<IDatabaseKey>, OracleTableQueryCache>(LoadUniqueKeysAsync),
        new AsyncCache<Identifier, IReadOnlyCollection<IDatabaseIndex>, OracleTableQueryCache>(LoadIndexesAsync),
        new AsyncCache<Identifier, IReadOnlyCollection<IDatabaseRelationalKey>, OracleTableQueryCache>(LoadParentKeysAsync),
        new AsyncCache<Identifier, IReadOnlyCollection<GetTableChecks.Result>, OracleTableQueryCache>((tableName, _, token) => LoadCheckRowsAsync(tableName, token)),
        new AsyncCache<Identifier, IReadOnlyCollection<GetTableConstraints.Result>, OracleTableQueryCache>((tableName, _, token) => LoadConstraintRowsAsync(tableName, token)),
        new AsyncCache<Identifier, IReadOnlyDictionary<Identifier, IDatabaseColumn>, OracleTableQueryCache>(LoadColumnLookupAsync)
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
    protected Task<Option<Identifier>> GetResolvedTableName(Identifier tableName, CancellationToken cancellationToken)
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
    protected OptionAsync<IRelationalDatabaseTable> LoadTable(Identifier tableName, OracleTableQueryCache queryCache, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(tableName);
        ArgumentNullException.ThrowIfNull(queryCache);

        var candidateTableName = QualifyTableName(tableName);
        return GetResolvedTableName(candidateTableName, cancellationToken)
            .MapAsync(name => LoadTableAsyncCore(name, queryCache, cancellationToken));
    }

    private async Task<IRelationalDatabaseTable> LoadTableAsyncCore(Identifier tableName, OracleTableQueryCache queryCache, CancellationToken cancellationToken)
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
            LoadChecksAsync(tableName, queryCache, cancellationToken),
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

    // ALL_CONSTRAINTS.INDEX_NAME names the index enforcing a primary or unique key constraint,
    // which need not share the constraint's name.
    private static Option<IDatabaseIndex> GetBackingIndex(IReadOnlyCollection<IDatabaseIndex> indexes, string? indexName)
    {
        if (indexName.IsNullOrWhiteSpace())
            return Option<IDatabaseIndex>.None;

        var backingIndex = indexes.FirstOrDefault(index => index.Name.LocalName == indexName);
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
    protected Task<Option<IDatabaseKey>> LoadPrimaryKeyAsync(Identifier tableName, OracleTableQueryCache queryCache, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(tableName);
        ArgumentNullException.ThrowIfNull(queryCache);

        return LoadPrimaryKeyAsyncCore(tableName, queryCache, cancellationToken);
    }

    private async Task<Option<IDatabaseKey>> LoadPrimaryKeyAsyncCore(Identifier tableName, OracleTableQueryCache queryCache, CancellationToken cancellationToken)
    {
        var constraintRows = await queryCache.GetConstraintRowsAsync(tableName, cancellationToken);
        var primaryKeyColumns = constraintRows
            .Where(static row => string.Equals(row.ConstraintType, Constants.PrimaryKeyType, StringComparison.Ordinal))
            .ToList();

        if (primaryKeyColumns.Empty())
            return Option<IDatabaseKey>.None;

        var columnLookup = await queryCache.GetColumnLookupAsync(tableName, cancellationToken);

        var groupedByName = primaryKeyColumns.GroupAsDictionary(static row => new { row.ConstraintName, row.EnabledStatus, row.ValidatedStatus, row.Deferrable, row.Deferred, row.IndexName });
        var firstRow = groupedByName.First();
        var constraintName = firstRow.Key.ConstraintName;
        var isEnabled = string.Equals(firstRow.Key.EnabledStatus, Constants.Enabled, StringComparison.Ordinal);
        var isValidated = string.Equals(firstRow.Key.ValidatedStatus, Constants.Validated, StringComparison.Ordinal);
        var deferrability = GetDeferrability(firstRow.Key.Deferrable, firstRow.Key.Deferred);

        var keyColumns = ResolveColumns(
            firstRow.Value
                .Where(static row => row.ColumnName != null)
                .OrderBy(static row => row.ColumnPosition)
                .Select(static row => (Identifier)row.ColumnName!),
            columnLookup
        ).ToList();

        var indexes = await queryCache.GetIndexesAsync(tableName, cancellationToken);
        var backingIndex = GetBackingIndex(indexes, firstRow.Key.IndexName);

        var primaryKey = constraintName != null
            ? new OracleDatabaseKey(constraintName, DatabaseKeyType.Primary, keyColumns, isEnabled, backingIndex, isValidated, deferrability)
            : (IDatabaseKey?)null;
        return primaryKey != null
            ? Option<IDatabaseKey>.Some(primaryKey)
            : Option<IDatabaseKey>.None;
    }

    /// <summary>
    /// Retrieves indexes that relate to the given table.
    /// </summary>
    /// <param name="tableName">A table name.</param>
    /// <param name="queryCache">A query cache for the given context.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of indexes.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="tableName"/> or <paramref name="queryCache"/> are <see langword="null" />.</exception>
    protected Task<IReadOnlyCollection<IDatabaseIndex>> LoadIndexesAsync(Identifier tableName, OracleTableQueryCache queryCache, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(tableName);
        ArgumentNullException.ThrowIfNull(queryCache);

        return LoadIndexesAsyncCore(tableName, queryCache, cancellationToken);
    }

    private async Task<IReadOnlyCollection<IDatabaseIndex>> LoadIndexesAsyncCore(Identifier tableName, OracleTableQueryCache queryCache, CancellationToken cancellationToken)
    {
        var queryResult = await DbConnection.QueryAsync(
            GetTableIndexes.Sql,
            new GetTableIndexes.Query { SchemaName = tableName.Schema!, TableName = tableName.LocalName },
            cancellationToken
        );

        var indexColumns = queryResult
            .GroupAsDictionary(static row => new { row.IndexName, row.Uniqueness, row.IndexType, row.Status, row.Visibility })
            .ToList();
        if (indexColumns.Empty())
            return [];

        var columnLookup = await queryCache.GetColumnLookupAsync(tableName, cancellationToken);

        var result = new List<IDatabaseIndex>(indexColumns.Count);
        foreach (var indexInfo in indexColumns)
        {
            var isUnique = string.Equals(indexInfo.Key.Uniqueness, Constants.Unique, StringComparison.Ordinal);
            var indexName = Identifier.CreateQualifiedIdentifier(indexInfo.Key.IndexName);

            var indexCols = indexInfo.Value
                .Where(static row => row.ColumnName != null)
                .OrderBy(static row => row.ColumnPosition)
                .Select(static row => new { row.IsDescending, Column = row.ColumnName! })
                .Select(row =>
                {
                    var order = string.Equals(row.IsDescending, Constants.Y, StringComparison.Ordinal) ? IndexColumnOrder.Descending : IndexColumnOrder.Ascending;
                    var indexColumns = columnLookup.TryGetValue(row.Column, out var indexColumn)
                        ? [indexColumn]
                        : Array.Empty<IDatabaseColumn>();
                    var expression = Dialect.QuoteName(row.Column);
                    return new DatabaseIndexColumn(expression, indexColumns, order);
                })
                .ToList();

            var indexType = indexInfo.Key.IndexType != null && IndexTypeMapping.TryGetValue(indexInfo.Key.IndexType, out var mappedIndexType)
                ? mappedIndexType
                : IndexType.Unknown;
            var isValid = !string.Equals(indexInfo.Key.Status, Constants.Unusable, StringComparison.Ordinal);
            var isVisible = !string.Equals(indexInfo.Key.Visibility, Constants.Invisible, StringComparison.Ordinal);

            var index = new OracleDatabaseIndex(indexName, isUnique, indexCols, indexType, isValid, isVisible);
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
    protected Task<IReadOnlyCollection<IDatabaseKey>> LoadUniqueKeysAsync(Identifier tableName, OracleTableQueryCache queryCache, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(tableName);
        ArgumentNullException.ThrowIfNull(queryCache);

        return LoadUniqueKeysAsyncCore(tableName, queryCache, cancellationToken);
    }

    private async Task<IReadOnlyCollection<IDatabaseKey>> LoadUniqueKeysAsyncCore(Identifier tableName, OracleTableQueryCache queryCache, CancellationToken cancellationToken)
    {
        var constraintRows = await queryCache.GetConstraintRowsAsync(tableName, cancellationToken);
        var uniqueKeyColumns = constraintRows
            .Where(static row => string.Equals(row.ConstraintType, Constants.UniqueKeyType, StringComparison.Ordinal))
            .ToList();

        if (uniqueKeyColumns.Empty())
            return [];

        var columnLookup = await queryCache.GetColumnLookupAsync(tableName, cancellationToken);

        var groupedByName = uniqueKeyColumns
            .Where(static row => row.ConstraintName != null)
            .GroupAsDictionary(static row => new { ConstraintName = row.ConstraintName!, row.EnabledStatus, row.ValidatedStatus, row.Deferrable, row.Deferred, row.IndexName });
        var constraintColumns = groupedByName
            .Select(g => new
            {
                g.Key.ConstraintName,
                g.Key.IndexName,
                Columns = ResolveColumns(
                    g.Value
                        .Where(static row => row.ColumnName != null)
                        .OrderBy(static row => row.ColumnPosition)
                        .Select(static row => (Identifier)row.ColumnName!),
                    columnLookup
                ).ToList(),
                IsEnabled = string.Equals(g.Key.EnabledStatus, Constants.Enabled, StringComparison.Ordinal),
                IsValidated = string.Equals(g.Key.ValidatedStatus, Constants.Validated, StringComparison.Ordinal),
                Deferrability = GetDeferrability(g.Key.Deferrable, g.Key.Deferred),
            })
            .ToList();
        if (constraintColumns.Empty())
            return [];

        var indexes = await queryCache.GetIndexesAsync(tableName, cancellationToken);

        return constraintColumns
            .ConvertAll(uk => new OracleDatabaseKey(uk.ConstraintName, DatabaseKeyType.Unique, uk.Columns, uk.IsEnabled, GetBackingIndex(indexes, uk.IndexName), uk.IsValidated, uk.Deferrability));
    }

    /// <summary>
    /// Retrieves child keys that relate to the given table.
    /// </summary>
    /// <param name="tableName">A table name.</param>
    /// <param name="queryCache">A query cache for the given context.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of child keys.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="tableName"/> or <paramref name="queryCache"/> are <see langword="null" />.</exception>
    protected Task<IReadOnlyCollection<IDatabaseRelationalKey>> LoadChildKeysAsync(Identifier tableName, OracleTableQueryCache queryCache, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(tableName);
        ArgumentNullException.ThrowIfNull(queryCache);

        return LoadChildKeysAsyncCore(tableName, queryCache, cancellationToken);
    }

    private async Task<IReadOnlyCollection<IDatabaseRelationalKey>> LoadChildKeysAsyncCore(Identifier tableName, OracleTableQueryCache queryCache, CancellationToken cancellationToken)
    {
        var queryResult = await DbConnection.QueryAsync(
            GetTableChildKeys.Sql,
            new GetTableChildKeys.Query { SchemaName = tableName.Schema!, TableName = tableName.LocalName },
            cancellationToken
        );

        var childKeyRows = queryResult.ToList();
        if (childKeyRows.Empty())
            return [];

        var (primaryKey, uniqueKeys) = await (
            queryCache.GetPrimaryKeyAsync(tableName, cancellationToken),
            queryCache.GetUniqueKeysAsync(tableName, cancellationToken)
        ).WhenAll();
        var uniqueKeyLookup = GetDatabaseKeyLookup(uniqueKeys);

        var result = new List<IDatabaseRelationalKey>(childKeyRows.Count);
        // Memoized per child table name so that a child table referencing this table via more than one
        // foreign key does not rebuild the same lookup dictionary once per row.
        var childForeignKeyLookups = new Dictionary<Identifier, IReadOnlyDictionary<Identifier, IDatabaseKey>>();

        foreach (var childKeyRow in childKeyRows)
        {
            // ensure we have a key to begin with
            IDatabaseKey? parentKey = null;
            if (string.Equals(childKeyRow.ParentKeyType, Constants.PrimaryKeyType, StringComparison.Ordinal))
                primaryKey.IfSome(k => parentKey = k);
            else if (childKeyRow.ParentKeyName != null && uniqueKeyLookup.TryGetValue(childKeyRow.ParentKeyName, out var uniqueParentKey))
                parentKey = uniqueParentKey;
            if (parentKey == null)
                continue;

            var candidateChildTableName = Identifier.CreateQualifiedIdentifier(childKeyRow.ChildTableSchema, childKeyRow.ChildTableName);
            var childTableNameOption = await queryCache.GetTableNameAsync(candidateChildTableName, cancellationToken);

            await childTableNameOption
                .BindAsync(async childTableName =>
                {
                    if (!childForeignKeyLookups.TryGetValue(childTableName, out var parentKeyLookup))
                    {
                        var parentKeys = await queryCache.GetForeignKeysAsync(childTableName, cancellationToken);
                        parentKeyLookup = GetDatabaseKeyLookup(parentKeys.Select(static fk => fk.ChildKey).ToList());
                        childForeignKeyLookups[childTableName] = parentKeyLookup;
                    }

                    var childKeyName = Identifier.CreateQualifiedIdentifier(childKeyRow.ChildKeyName);
                    if (!parentKeyLookup.TryGetValue(childKeyName, out var childKey))
                        return OptionAsync<IDatabaseRelationalKey>.None;

                    var deleteAction = childKeyRow.DeleteAction != null && ReferentialActionMapping.TryGetValue(childKeyRow.DeleteAction, out var mappedAction)
                        ? mappedAction
                        : ReferentialAction.NoAction;

                    var relationalKey = new OracleRelationalKey(childTableName, childKey, tableName, parentKey, deleteAction);
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
    /// <param name="queryCache">A query cache for the given context.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of check constraints.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="tableName"/> or <paramref name="queryCache"/> is <see langword="null" />.</exception>
    protected Task<IReadOnlyCollection<IDatabaseCheckConstraint>> LoadChecksAsync(Identifier tableName, OracleTableQueryCache queryCache, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(tableName);
        ArgumentNullException.ThrowIfNull(queryCache);

        return LoadChecksAsyncCore(tableName, queryCache, cancellationToken);
    }

    private async Task<IReadOnlyCollection<IDatabaseCheckConstraint>> LoadChecksAsyncCore(Identifier tableName, OracleTableQueryCache queryCache, CancellationToken cancellationToken)
    {
        var checks = await queryCache.GetCheckRowsAsync(tableName, cancellationToken);

        if (checks.Empty())
            return [];

        var columnLookup = await queryCache.GetColumnLookupAsync(tableName, cancellationToken);

        var columnNotNullConstraints = columnLookup.Keys
            .Select(static k => k.LocalName)
            .Select(GenerateNotNullDefinition)
            .ToHashSet(StringComparer.Ordinal);

        var result = new List<IDatabaseCheckConstraint>();

        foreach (var checkRow in checks)
        {
            var definition = checkRow.Definition;
            if (definition == null || columnNotNullConstraints.Contains(definition))
                continue;

            var constraintName = Identifier.CreateQualifiedIdentifier(checkRow.ConstraintName);
            var isEnabled = string.Equals(checkRow.EnabledStatus, Constants.Enabled, StringComparison.Ordinal);
            var isValidated = string.Equals(checkRow.ValidatedStatus, Constants.Validated, StringComparison.Ordinal);
            var deferrability = GetDeferrability(checkRow.Deferrable, checkRow.Deferred);

            var check = new DatabaseCheckConstraint(constraintName, definition, isEnabled, isValidated, deferrability);
            result.Add(check);
        }

        return result;
    }

    /// <summary>
    /// Retrieves the raw check constraint rows defined on a given table. Shared by <see cref="LoadChecksAsync"/> and
    /// <see cref="GetNotNullConstrainedColumnsAsync"/> so that <c>GetTableChecks</c> is only queried once per table.
    /// </summary>
    /// <param name="tableName">A table name.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of raw check constraint rows.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="tableName"/> is <see langword="null" />.</exception>
    internal Task<IReadOnlyCollection<GetTableChecks.Result>> LoadCheckRowsAsync(Identifier tableName, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(tableName);

        return LoadCheckRowsAsyncCore(tableName, cancellationToken);
    }

    private async Task<IReadOnlyCollection<GetTableChecks.Result>> LoadCheckRowsAsyncCore(Identifier tableName, CancellationToken cancellationToken)
    {
        var checks = await DbConnection.QueryAsync(
            GetTableChecks.Sql,
            new GetTableChecks.Query { SchemaName = tableName.Schema!, TableName = tableName.LocalName },
            cancellationToken
        );

        return checks.ToList();
    }

    /// <summary>
    /// Retrieves the raw primary key, unique key and foreign key constraint rows defined on a given
    /// table. Shared by <see cref="LoadPrimaryKeyAsync"/>, <see cref="LoadUniqueKeysAsync"/> and
    /// <see cref="LoadParentKeysAsync"/> so that <c>GetTableConstraints</c> is only queried once per table.
    /// </summary>
    /// <param name="tableName">A table name.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of raw constraint rows.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="tableName"/> is <see langword="null" />.</exception>
    internal Task<IReadOnlyCollection<GetTableConstraints.Result>> LoadConstraintRowsAsync(Identifier tableName, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(tableName);

        return LoadConstraintRowsAsyncCore(tableName, cancellationToken);
    }

    private async Task<IReadOnlyCollection<GetTableConstraints.Result>> LoadConstraintRowsAsyncCore(Identifier tableName, CancellationToken cancellationToken)
    {
        var constraints = await DbConnection.QueryAsync(
            GetTableConstraints.Sql,
            new GetTableConstraints.Query { SchemaName = tableName.Schema!, TableName = tableName.LocalName },
            cancellationToken
        );

        return constraints.ToList();
    }

    /// <summary>
    /// Retrieves foreign keys that relate to the given table.
    /// </summary>
    /// <param name="tableName">A table name.</param>
    /// <param name="queryCache">A query cache for the given context.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of foreign keys.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="tableName"/> or <paramref name="queryCache"/> are <see langword="null" />.</exception>
    protected Task<IReadOnlyCollection<IDatabaseRelationalKey>> LoadParentKeysAsync(Identifier tableName, OracleTableQueryCache queryCache, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(tableName);
        ArgumentNullException.ThrowIfNull(queryCache);

        return LoadParentKeysAsyncCore(tableName, queryCache, cancellationToken);
    }

    private async Task<IReadOnlyCollection<IDatabaseRelationalKey>> LoadParentKeysAsyncCore(Identifier tableName, OracleTableQueryCache queryCache, CancellationToken cancellationToken)
    {
        var constraintRows = await queryCache.GetConstraintRowsAsync(tableName, cancellationToken);
        // A left join is used so that a single query can serve primary/unique/foreign key constraints
        // alike (see GetTableConstraints.Sql), so a foreign key referencing anything other than a
        // primary or unique key comes back with null parent-table columns. Drop those here to match the
        // previous inner-join behaviour, which excluded such rows entirely.
        var queryResult = constraintRows
            .Where(static row => string.Equals(row.ConstraintType, Constants.ForeignKeyType, StringComparison.Ordinal)
                && row.ParentTableSchema != null
                && row.ParentTableName != null)
            .ToList();

        var foreignKeys = queryResult.GroupAsDictionary(static row => new
        {
            row.ConstraintName,
            row.EnabledStatus,
            row.ValidatedStatus,
            row.Deferrable,
            row.Deferred,
            row.DeleteAction,
            row.ParentTableSchema,
            row.ParentTableName,
            row.ParentConstraintName,
            KeyType = row.ParentKeyType,
        }).ToList();
        if (foreignKeys.Empty())
            return [];

        var columnLookup = await queryCache.GetColumnLookupAsync(tableName, cancellationToken);

        var result = new List<IDatabaseRelationalKey>(foreignKeys.Count);
        // Memoized per parent table name so that multiple foreign keys referencing unique keys on the
        // same parent table don't rebuild the same lookup dictionary once per foreign key.
        var parentUniqueKeyLookups = new Dictionary<Identifier, IReadOnlyDictionary<Identifier, IDatabaseKey>>();
        foreach (var fkey in foreignKeys)
        {
            var candidateParentTableName = Identifier.CreateQualifiedIdentifier(fkey.Key.ParentTableSchema, fkey.Key.ParentTableName);
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

                    if (!parentUniqueKeyLookups.TryGetValue(parentTableName, out var uniqueKeyLookup))
                    {
                        var uniqueKeys = await queryCache.GetUniqueKeysAsync(parentTableName, cancellationToken);
                        uniqueKeyLookup = GetDatabaseKeyLookup(uniqueKeys);
                        parentUniqueKeyLookups[parentTableName] = uniqueKeyLookup;
                    }

                    var parentKeyName = Identifier.CreateQualifiedIdentifier(fkey.Key.ParentConstraintName);
                    return uniqueKeyLookup.TryGetValue(parentKeyName.LocalName, out var uniqueParentKey)
                        ? OptionAsync<IDatabaseKey>.Some(uniqueParentKey)
                        : OptionAsync<IDatabaseKey>.None;
                })
                .Map(parentKey =>
                {
                    var childKeyName = Identifier.CreateQualifiedIdentifier(fkey.Key.ConstraintName);
                    var childKeyColumns = ResolveColumns(
                        fkey.Value
                            .Where(static row => row.ColumnName != null)
                            .OrderBy(static row => row.ColumnPosition)
                            .Select(static row => (Identifier)row.ColumnName!),
                        columnLookup
                    ).ToList();

                    var isEnabled = string.Equals(fkey.Key.EnabledStatus, Constants.Enabled, StringComparison.Ordinal);
                    var isValidated = string.Equals(fkey.Key.ValidatedStatus, Constants.Validated, StringComparison.Ordinal);
                    var deferrability = GetDeferrability(fkey.Key.Deferrable, fkey.Key.Deferred);
                    var childKey = new OracleDatabaseKey(childKeyName, DatabaseKeyType.Foreign, childKeyColumns, isEnabled, Option<IDatabaseIndex>.None, isValidated, deferrability);

                    // DELETE_RULE is null for a foreign key whose parent row cannot be deleted at all,
                    // which is Oracle's NO ACTION behaviour. Matches the child key direction.
                    var deleteAction = fkey.Key.DeleteAction != null && ReferentialActionMapping.TryGetValue(fkey.Key.DeleteAction, out var mappedDeleteAction)
                        ? mappedDeleteAction
                        : ReferentialAction.NoAction;
                    return new OracleRelationalKey(tableName, childKey, resolvedParentTableName!, parentKey, deleteAction);
                })
                .IfSome(result.Add);
        }

        return result;
    }

    /// <summary>
    /// Retrieves the columns for a given table.
    /// </summary>
    /// <param name="tableName">A table name.</param>
    /// <param name="queryCache">A query cache for the given context.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>An ordered collection of columns.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="tableName"/> or <paramref name="queryCache"/> are <see langword="null" />.</exception>
    protected Task<IReadOnlyList<IDatabaseColumn>> LoadColumnsAsync(Identifier tableName, OracleTableQueryCache queryCache, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(tableName);
        ArgumentNullException.ThrowIfNull(queryCache);

        return LoadColumnsAsyncCore(tableName, queryCache, cancellationToken);
    }

    private async Task<IReadOnlyList<IDatabaseColumn>> LoadColumnsAsyncCore(Identifier tableName, OracleTableQueryCache queryCache, CancellationToken cancellationToken)
    {
        var query = await DbConnection.QueryAsync(
            GetTableColumns.Sql,
            new GetTableColumns.Query { SchemaName = tableName.Schema!, TableName = tableName.LocalName },
            cancellationToken
        );

        var columnNames = query
            .Where(static row => row.ColumnName != null)
            .Select(static row => row.ColumnName!)
            .ToList();
        var notNullableColumnNames = await GetNotNullConstrainedColumnsAsync(tableName, columnNames, queryCache, cancellationToken);
        var result = new List<IDatabaseColumn>();

        foreach (var row in query)
        {
            var typeMetadata = new ColumnTypeMetadata
            {
                TypeName = Identifier.CreateQualifiedIdentifier(row.ColumnTypeSchema, row.ColumnTypeName),
                Collation = !row.Collation.IsNullOrWhiteSpace()
                    ? Option<Identifier>.Some(Identifier.CreateQualifiedIdentifier(row.Collation))
                    : Option<Identifier>.None,
                MaxLength = row.DataLength,
                NumericPrecision = row.Precision > 0 || row.Scale > 0
                    ? Option<INumericPrecision>.Some(new NumericPrecision(row.Precision, row.Scale))
                    : Option<INumericPrecision>.None,
            };
            var columnType = TypeProvider.CreateColumnType(typeMetadata);

            var isNullable = row.ColumnName == null || !notNullableColumnNames.Contains(row.ColumnName);
            var isComputed = string.Equals(row.IsComputed, Constants.Yes, StringComparison.Ordinal);
            var columnName = Identifier.CreateQualifiedIdentifier(row.ColumnName);
            var computedColumnDefinition = isComputed && !row.DefaultValue.IsNullOrWhiteSpace()
                ? Option<string>.Some(row.DefaultValue)
                : Option<string>.None;
            var defaultValue = !row.DefaultValue.IsNullOrWhiteSpace()
                ? Option<string>.Some(row.DefaultValue)
                : Option<string>.None;

            var column = isComputed
                ? new OracleDatabaseComputedColumn(columnName, columnType, isNullable, computedColumnDefinition)
                : new OracleDatabaseColumn(columnName, columnType, isNullable, defaultValue, BuildAutoIncrement(row, tableName));

            result.Add(column);
        }

        return result;
    }

    private static Option<IAutoIncrement> BuildAutoIncrement(GetTableColumns.Result row, Identifier tableName)
    {
        if (!string.Equals(row.IsIdentity, Constants.Yes, StringComparison.Ordinal))
            return Option<IAutoIncrement>.None;

        var options = ParseIdentityOptions(row.IdentityOptions);

        // GENERATION_TYPE only distinguishes ALWAYS from BY DEFAULT; whether a supplied NULL is
        // replaced by a generated value is reported separately, on the column itself.
        var generation = string.Equals(row.GenerationType, Constants.Always, StringComparison.Ordinal)
            ? IdentityGeneration.Always
            : string.Equals(row.DefaultOnNull, Constants.Yes, StringComparison.Ordinal)
                ? IdentityGeneration.ByDefaultOnNull
                : IdentityGeneration.ByDefault;

        var sequenceName = !row.SequenceName.IsNullOrWhiteSpace()
            ? Option<Identifier>.Some(Identifier.CreateQualifiedIdentifier(tableName.Schema, row.SequenceName))
            : Option<Identifier>.None;

        return Option<IAutoIncrement>.Some(new AutoIncrement(
            GetIdentityOption(options, Constants.StartWith).IfNone(1),
            GetIdentityOption(options, Constants.IncrementBy).Match(static incr => incr != 0 ? incr : 1, static () => 1),
            generation,
            GetIdentityOption(options, Constants.MinValue),
            GetIdentityOption(options, Constants.MaxValue),
            options.TryGetValue(Constants.CycleFlag, out var cycleFlag) && string.Equals(cycleFlag, Constants.Y, StringComparison.OrdinalIgnoreCase),
            sequenceName
        ));
    }

    // IDENTITY_OPTIONS describes the backing sequence as a single comma-separated list of
    // 'NAME: VALUE' pairs, e.g.
    // START WITH: 1, INCREMENT BY: 1, MAX_VALUE: 9999999999999999999999999999, MIN_VALUE: 1, CYCLE_FLAG: N, CACHE_SIZE: 20, ORDER_FLAG: N
    private static IReadOnlyDictionary<string, string> ParseIdentityOptions(string? identityOptions)
    {
        var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        if (identityOptions.IsNullOrWhiteSpace())
            return result;

        foreach (var option in identityOptions.Split(','))
        {
            var separatorIndex = option.IndexOf(':', StringComparison.Ordinal);
            if (separatorIndex < 0)
                continue;

            var key = option[..separatorIndex].Trim();
            if (key.Length > 0)
                result[key] = option[(separatorIndex + 1)..].Trim();
        }

        return result;
    }

    private static Option<decimal> GetIdentityOption(IReadOnlyDictionary<string, string> options, string optionName)
    {
        return options.TryGetValue(optionName, out var value)
            && decimal.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result)
            ? Option<decimal>.Some(result)
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

        var triggers = queryResult.ToList();
        if (triggers.Empty())
            return [];

        var result = new List<IDatabaseTrigger>(triggers.Count);
        foreach (var triggerRow in triggers)
        {
            var triggerName = Identifier.CreateQualifiedIdentifier(triggerRow.TriggerSchema, triggerRow.TriggerName);
            var queryTiming = triggerRow.TriggerType != null && TimingMapping.TryGetValue(triggerRow.TriggerType, out var timing)
                ? timing
                : TriggerQueryTiming.After;
            var definition = triggerRow.Definition ?? string.Empty;
            var isEnabled = string.Equals(triggerRow.EnabledStatus, Constants.Enabled, StringComparison.Ordinal);

            var events = TriggerEvent.None;
            var triggerEventPieces = triggerRow.TriggerEvent != null
                ? triggerRow.TriggerEvent.Split([" OR "], StringSplitOptions.RemoveEmptyEntries)
                : [];

            foreach (var triggerEventPiece in triggerEventPieces)
            {
                if (string.Equals(triggerEventPiece, Constants.Insert, StringComparison.Ordinal))
                    events |= TriggerEvent.Insert;
                else if (string.Equals(triggerEventPiece, Constants.Update, StringComparison.Ordinal))
                    events |= TriggerEvent.Update;
                else if (string.Equals(triggerEventPiece, Constants.Delete, StringComparison.Ordinal))
                    events |= TriggerEvent.Delete;
                else
                    throw new UnsupportedTriggerEventException(tableName, triggerEventPiece);
            }

            var trigger = new DatabaseTrigger(triggerName, definition, queryTiming, events, isEnabled);
            result.Add(trigger);
        }

        return result;
    }

    /// <summary>
    /// Retrieves the names all of the not-null constrained columns in a given table.
    /// </summary>
    /// <param name="tableName">A table name.</param>
    /// <param name="columnNames">The column names for the given table.</param>
    /// <param name="queryCache">A query cache for the given context.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of not-null constrained column names.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="tableName"/> or <paramref name="columnNames"/> or <paramref name="queryCache"/> are <see langword="null" />.</exception>
    protected Task<IEnumerable<string>> GetNotNullConstrainedColumnsAsync(Identifier tableName, IEnumerable<string> columnNames, OracleTableQueryCache queryCache, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(tableName);
        ArgumentNullException.ThrowIfNull(columnNames);
        ArgumentNullException.ThrowIfNull(queryCache);

        return GetNotNullConstrainedColumnsAsyncCore(tableName, columnNames, queryCache, cancellationToken);
    }

    private async Task<IEnumerable<string>> GetNotNullConstrainedColumnsAsyncCore(Identifier tableName, IEnumerable<string> columnNames, OracleTableQueryCache queryCache, CancellationToken cancellationToken)
    {
        var checks = await queryCache.GetCheckRowsAsync(tableName, cancellationToken);

        if (checks.Empty())
            return [];

        var columnNotNullConstraints = columnNames
            .Select(name => new KeyValuePair<string, string>(GenerateNotNullDefinition(name), name))
            .ToReadOnlyDictionary();

        return checks
            .Where(c => c.Definition != null
                && string.Equals(c.EnabledStatus, Constants.Enabled, StringComparison.Ordinal)
                && columnNotNullConstraints.ContainsKey(c.Definition))
            .Select(c => columnNotNullConstraints[c.Definition!])
            .ToHashSet(StringComparer.Ordinal);
    }

    /// <summary>
    /// Creates a not null constraint definition, used to determine whether a constraint is a <c>NOT NULL</c> constraint.
    /// </summary>
    /// <param name="columnName">A column name.</param>
    /// <returns>A <c>NOT NULL</c> constraint definition for the given column.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="columnName"/> is <see langword="null" />.</exception>
    /// <exception cref="ArgumentException"><paramref name="columnName"/> is empty or whitespace.</exception>
    protected static string GenerateNotNullDefinition(string columnName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(columnName);

        return "\"" + columnName + "\" IS NOT NULL";
    }

    /// <summary>
    /// A mapping from the referential actions as described in Oracle, to a <see cref="ReferentialAction"/> instance.
    /// </summary>
    /// <value>A mapping dictionary.</value>
    protected IReadOnlyDictionary<string, ReferentialAction> ReferentialActionMapping { get; } = new Dictionary<string, ReferentialAction>(StringComparer.OrdinalIgnoreCase)
    {
        ["NO ACTION"] = ReferentialAction.NoAction,
        ["RESTRICT"] = ReferentialAction.Restrict,
        ["CASCADE"] = ReferentialAction.Cascade,
        ["SET NULL"] = ReferentialAction.SetNull,
        ["SET DEFAULT"] = ReferentialAction.SetDefault,
    };

    /// <summary>
    /// A mapping from the trigger query timings as described in Oracle, to a <see cref="TriggerQueryTiming"/> instance.
    /// </summary>
    /// <value>A mapping dictionary.</value>
    protected IReadOnlyDictionary<string, TriggerQueryTiming> TimingMapping { get; } = new Dictionary<string, TriggerQueryTiming>(StringComparer.OrdinalIgnoreCase)
    {
        ["BEFORE STATEMENT"] = TriggerQueryTiming.Before,
        ["BEFORE EACH ROW"] = TriggerQueryTiming.Before,
        ["AFTER STATEMENT"] = TriggerQueryTiming.After,
        ["AFTER EACH ROW"] = TriggerQueryTiming.After,
        ["INSTEAD OF"] = TriggerQueryTiming.InsteadOf,
        ["COMPOUND"] = TriggerQueryTiming.InsteadOf,
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

    private static IReadOnlyDictionary<Identifier, IDatabaseColumn> GetColumnLookup(IReadOnlyCollection<IDatabaseColumn> columns)
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

    /// <summary>
    /// Retrieves a table's column lookup, backed by the (cached) column list. Cached in its own right so
    /// that the <see cref="Dictionary{TKey,TValue}"/> built by <see cref="GetColumnLookup"/> is not
    /// rebuilt by every one of the several methods (primary key, unique keys, indexes, checks, parent
    /// keys) that need to resolve column names for a given table.
    /// </summary>
    /// <param name="tableName">A table name.</param>
    /// <param name="queryCache">A query cache for the given context.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A lookup of columns keyed by column name.</returns>
    private async Task<IReadOnlyDictionary<Identifier, IDatabaseColumn>> LoadColumnLookupAsync(Identifier tableName, OracleTableQueryCache queryCache, CancellationToken cancellationToken)
    {
        var columns = await queryCache.GetColumnsAsync(tableName, cancellationToken);
        return GetColumnLookup(columns);
    }

    /// <summary>
    /// Resolves an ordered sequence of column names against a column lookup, skipping any name that has
    /// no match (mirroring the previous <c>.Where(lookup.ContainsKey).Select(lookup[...])</c> chains this
    /// replaces, minus the double dictionary lookup).
    /// </summary>
    /// <param name="columnNames">Column names, in the order the resolved columns should appear.</param>
    /// <param name="columnLookup">A column lookup for the table the names belong to.</param>
    /// <returns>The resolved columns, in the same order as <paramref name="columnNames"/>.</returns>
    private static IEnumerable<IDatabaseColumn> ResolveColumns(IEnumerable<Identifier> columnNames, IReadOnlyDictionary<Identifier, IDatabaseColumn> columnLookup)
    {
        foreach (var name in columnNames)
        {
            if (columnLookup.TryGetValue(name, out var column))
                yield return column;
        }
    }

    // DEFERRABLE is 'DEFERRABLE' or 'NOT DEFERRABLE'; DEFERRED is 'DEFERRED' or 'IMMEDIATE'.
    private static ConstraintDeferrability GetDeferrability(string? deferrable, string? deferred)
    {
        if (!string.Equals(deferrable, Constants.Deferrable, StringComparison.Ordinal))
            return ConstraintDeferrability.NotDeferrable;

        return string.Equals(deferred, Constants.Deferred, StringComparison.Ordinal)
            ? ConstraintDeferrability.DeferrableInitiallyDeferred
            : ConstraintDeferrability.DeferrableInitiallyImmediate;
    }

    private static IReadOnlyDictionary<Identifier, IDatabaseKey> GetDatabaseKeyLookup(IReadOnlyCollection<IDatabaseKey> keys)
    {
        ArgumentNullException.ThrowIfNull(keys);

        var result = new Dictionary<Identifier, IDatabaseKey>(keys.Count);

        foreach (var key in keys)
            key.Name.IfSome(name => result[name.LocalName] = key);

        return result;
    }

    // ALL_INDEXES.INDEX_TYPE values, see the Oracle reference for ALL_INDEXES.
    private static readonly IReadOnlyDictionary<string, IndexType> IndexTypeMapping = new Dictionary<string, IndexType>(StringComparer.Ordinal)
    {
        ["NORMAL"] = IndexType.BTree,
        ["NORMAL/REV"] = IndexType.BTree,
        ["FUNCTION-BASED NORMAL"] = IndexType.BTree,
        ["FUNCTION-BASED NORMAL/REV"] = IndexType.BTree,
        ["BITMAP"] = IndexType.Bitmap,
        ["FUNCTION-BASED BITMAP"] = IndexType.Bitmap,
        ["IOT - TOP"] = IndexType.Clustered,
        ["DOMAIN"] = IndexType.Other,
        ["CLUSTER"] = IndexType.Other,
        ["LOB"] = IndexType.Other,
    };

    private static class Constants
    {
        public const string Delete = "DELETE";

        public const string Enabled = "ENABLED";

        public const string Validated = "VALIDATED";

        public const string Deferrable = "DEFERRABLE";

        public const string Deferred = "DEFERRED";

        public const string Insert = "INSERT";

        public const string PrimaryKeyType = "P";

        public const string UniqueKeyType = "U";

        public const string ForeignKeyType = "R";

        public const string Unique = "UNIQUE";

        public const string Unusable = "UNUSABLE";

        public const string Invisible = "INVISIBLE";

        public const string Update = "UPDATE";

        public const string Y = "Y";

        public const string Yes = "YES";

        public const string Always = "ALWAYS";

        public const string StartWith = "START WITH";

        public const string IncrementBy = "INCREMENT BY";

        public const string MinValue = "MIN_VALUE";

        public const string MaxValue = "MAX_VALUE";

        public const string CycleFlag = "CYCLE_FLAG";
    }

    /// <summary>
    /// A query cache provider for Oracle tables. Ensures that a given query only occurs at most once for a given query context.
    /// </summary>
    protected class OracleTableQueryCache
    {
        private readonly AsyncCache<Identifier, Option<Identifier>, OracleTableQueryCache> _tableNames;
        private readonly AsyncCache<Identifier, IReadOnlyList<IDatabaseColumn>, OracleTableQueryCache> _columns;
        private readonly AsyncCache<Identifier, Option<IDatabaseKey>, OracleTableQueryCache> _primaryKeys;
        private readonly AsyncCache<Identifier, IReadOnlyCollection<IDatabaseKey>, OracleTableQueryCache> _uniqueKeys;
        private readonly AsyncCache<Identifier, IReadOnlyCollection<IDatabaseIndex>, OracleTableQueryCache> _indexes;
        private readonly AsyncCache<Identifier, IReadOnlyCollection<IDatabaseRelationalKey>, OracleTableQueryCache> _foreignKeys;
        private readonly AsyncCache<Identifier, IReadOnlyCollection<GetTableChecks.Result>, OracleTableQueryCache> _checkRows;
        private readonly AsyncCache<Identifier, IReadOnlyCollection<GetTableConstraints.Result>, OracleTableQueryCache> _constraintRows;
        private readonly AsyncCache<Identifier, IReadOnlyDictionary<Identifier, IDatabaseColumn>, OracleTableQueryCache> _columnLookups;

        /// <summary>
        /// Initializes a new instance of the <see cref="OracleTableQueryCache"/> class.
        /// </summary>
        /// <param name="tableNameLoader">A table name cache.</param>
        /// <param name="columnLoader">A column cache.</param>
        /// <param name="primaryKeyLoader">A primary key cache.</param>
        /// <param name="uniqueKeyLoader">A unique key cache.</param>
        /// <param name="indexLoader">An index cache.</param>
        /// <param name="foreignKeyLoader">A foreign key cache.</param>
        /// <param name="checkRowsLoader">A raw check constraint row cache.</param>
        /// <param name="constraintRowsLoader">A raw primary/unique/foreign key constraint row cache.</param>
        /// <param name="columnLookupLoader">A column lookup cache.</param>
        /// <exception cref="ArgumentNullException">Thrown when any of <paramref name="tableNameLoader"/>, <paramref name="columnLoader"/>, <paramref name="primaryKeyLoader"/>, <paramref name="uniqueKeyLoader"/>, <paramref name="indexLoader"/>, <paramref name="foreignKeyLoader"/>, <paramref name="checkRowsLoader"/>, <paramref name="constraintRowsLoader"/> or <paramref name="columnLookupLoader"/> are <see langword="null" />.</exception>
        internal OracleTableQueryCache(
            AsyncCache<Identifier, Option<Identifier>, OracleTableQueryCache> tableNameLoader,
            AsyncCache<Identifier, IReadOnlyList<IDatabaseColumn>, OracleTableQueryCache> columnLoader,
            AsyncCache<Identifier, Option<IDatabaseKey>, OracleTableQueryCache> primaryKeyLoader,
            AsyncCache<Identifier, IReadOnlyCollection<IDatabaseKey>, OracleTableQueryCache> uniqueKeyLoader,
            AsyncCache<Identifier, IReadOnlyCollection<IDatabaseIndex>, OracleTableQueryCache> indexLoader,
            AsyncCache<Identifier, IReadOnlyCollection<IDatabaseRelationalKey>, OracleTableQueryCache> foreignKeyLoader,
            AsyncCache<Identifier, IReadOnlyCollection<GetTableChecks.Result>, OracleTableQueryCache> checkRowsLoader,
            AsyncCache<Identifier, IReadOnlyCollection<GetTableConstraints.Result>, OracleTableQueryCache> constraintRowsLoader,
            AsyncCache<Identifier, IReadOnlyDictionary<Identifier, IDatabaseColumn>, OracleTableQueryCache> columnLookupLoader
        )
        {
            _tableNames = tableNameLoader ?? throw new ArgumentNullException(nameof(tableNameLoader));
            _columns = columnLoader ?? throw new ArgumentNullException(nameof(columnLoader));
            _primaryKeys = primaryKeyLoader ?? throw new ArgumentNullException(nameof(primaryKeyLoader));
            _uniqueKeys = uniqueKeyLoader ?? throw new ArgumentNullException(nameof(uniqueKeyLoader));
            _indexes = indexLoader ?? throw new ArgumentNullException(nameof(indexLoader));
            _foreignKeys = foreignKeyLoader ?? throw new ArgumentNullException(nameof(foreignKeyLoader));
            _checkRows = checkRowsLoader ?? throw new ArgumentNullException(nameof(checkRowsLoader));
            _constraintRows = constraintRowsLoader ?? throw new ArgumentNullException(nameof(constraintRowsLoader));
            _columnLookups = columnLookupLoader ?? throw new ArgumentNullException(nameof(columnLookupLoader));
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
        /// <returns>A collection of indexes, including those enforcing key constraints.</returns>
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

        /// <summary>
        /// Retrieves a table's raw check constraint rows from the cache, querying the database when not populated.
        /// </summary>
        /// <param name="tableName">A table name.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A collection of raw check constraint rows.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="tableName"/> is <see langword="null" />.</exception>
        internal Task<IReadOnlyCollection<GetTableChecks.Result>> GetCheckRowsAsync(Identifier tableName, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(tableName);

            return _checkRows.GetByKeyAsync(tableName, this, cancellationToken);
        }

        /// <summary>
        /// Retrieves a table's raw primary/unique/foreign key constraint rows from the cache, querying the database when not populated.
        /// </summary>
        /// <param name="tableName">A table name.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A collection of raw constraint rows.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="tableName"/> is <see langword="null" />.</exception>
        internal Task<IReadOnlyCollection<GetTableConstraints.Result>> GetConstraintRowsAsync(Identifier tableName, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(tableName);

            return _constraintRows.GetByKeyAsync(tableName, this, cancellationToken);
        }

        /// <summary>
        /// Retrieves a table's column lookup from the cache, querying the database when not populated.
        /// </summary>
        /// <param name="tableName">A table name.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A lookup of columns keyed by column name.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="tableName"/> is <see langword="null" />.</exception>
        public Task<IReadOnlyDictionary<Identifier, IDatabaseColumn>> GetColumnLookupAsync(Identifier tableName, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(tableName);

            return _columnLookups.GetByKeyAsync(tableName, this, cancellationToken);
        }
    }
}