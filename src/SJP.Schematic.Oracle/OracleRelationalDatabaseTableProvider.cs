using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using LanguageExt;
using SJP.Schematic.Core;
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
    /// Creates a query cache for a given query context.
    /// </summary>
    /// <param name="cancellationToken">A token that cancels every query the cache has started. Pass the token of the operation that owns the cache, since the cached queries are shared by every part of that operation.</param>
    /// <returns>A query cache.</returns>
    protected OracleTableQueryCache CreateQueryCache(CancellationToken cancellationToken) => new(
        new AsyncCache<Identifier, Option<Identifier>, OracleTableQueryCache>((tableName, _, token) => GetResolvedTableName(tableName, token), cancellationToken),
        new AsyncCache<Identifier, IReadOnlyList<IDatabaseColumn>, OracleTableQueryCache>(LoadColumnsAsync, cancellationToken),
        new AsyncCache<Identifier, TableConstraints, OracleTableQueryCache>(LoadConstraintsAsync, cancellationToken),
        new AsyncCache<Identifier, IReadOnlyCollection<IDatabaseIndex>, OracleTableQueryCache>(LoadIndexesAsync, cancellationToken),
        new AsyncCache<Identifier, IReadOnlyCollection<IDatabaseRelationalKey>, OracleTableQueryCache>(LoadParentKeysAsync, cancellationToken),
        new AsyncCache<Identifier, IReadOnlyDictionary<Identifier, IDatabaseColumn>, OracleTableQueryCache>(LoadColumnLookupAsync, cancellationToken)
    );

    /// <summary>
    /// Enumerates all database tables.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A collection of database tables.</returns>
    public async IAsyncEnumerable<IRelationalDatabaseTable> EnumerateAllTables([EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var queryCache = CreateQueryCache(cancellationToken);

        var tableNames = await DbConnection.QueryEnumerableAsync<GetAllTableNames.Result>(GetAllTableNames.Sql, cancellationToken)
            .Select(static dto => Identifier.CreateQualifiedIdentifier(dto.SchemaName, dto.TableName))
            .Select(QualifyTableName)
            .ToListAsync(cancellationToken);

        var tables = tableNames.SelectOrderedPrefetchAsync(
            (tableName, ct) => LoadTableAsyncCore(tableName, queryCache, ct),
            Math.Max(1, DbConnection.MaxConcurrentQueries),
            cancellationToken);

        await foreach (var table in tables.WithCancellation(cancellationToken))
            yield return table;
    }

    /// <summary>
    /// Gets all database tables.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A collection of database tables.</returns>
    public async Task<IReadOnlyCollection<IRelationalDatabaseTable>> GetAllTables(CancellationToken cancellationToken = default)
    {
        var queryCache = CreateQueryCache(cancellationToken);

        var tableNames = await DbConnection.QueryEnumerableAsync<GetAllTableNames.Result>(GetAllTableNames.Sql, cancellationToken)
            .Select(static dto => Identifier.CreateQualifiedIdentifier(dto.SchemaName, dto.TableName))
            .Select(QualifyTableName)
            .ToListAsync(cancellationToken);

        return await tableNames.SelectBoundedAsync(
            (tableName, ct) => LoadTableAsyncCore(tableName, queryCache, ct),
            Math.Max(1, DbConnection.MaxConcurrentQueries),
            cancellationToken);
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

        var queryCache = CreateQueryCache(cancellationToken);
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
            childKeys,
            options
        ) = await (
            queryCache.GetColumnsAsync(tableName, cancellationToken),
            LoadChecksAsync(tableName, queryCache, cancellationToken),
            LoadTriggersAsync(tableName, cancellationToken),
            queryCache.GetIndexesAsync(tableName, cancellationToken),
            queryCache.GetPrimaryKeyAsync(tableName, cancellationToken),
            queryCache.GetUniqueKeysAsync(tableName, cancellationToken),
            queryCache.GetForeignKeysAsync(tableName, cancellationToken),
            LoadChildKeysAsync(tableName, queryCache, cancellationToken),
            LoadTableOptionsAsync(tableName, queryCache, cancellationToken)
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
            triggers,
            options.Kind,
            options.Partitioning,
            options.SystemVersioning,
            options.IsLogged,
            options.Collation
        );
    }

    private async Task<TableOptions> LoadTableOptionsAsync(Identifier tableName, OracleTableQueryCache queryCache, CancellationToken cancellationToken)
    {
        var optionsResult = await DbConnection.QueryAsync(
            GetTableOptions.Sql,
            new GetTableOptions.Query { SchemaName = tableName.Schema!, TableName = tableName.LocalName },
            cancellationToken
        );

        var options = optionsResult.FirstOrDefault();
        if (options == null)
            return TableOptions.Default;

        var isPartitioned = string.Equals(options.IsPartitioned, "YES", StringComparison.OrdinalIgnoreCase);
        var partitioning = Option<ITablePartitioning>.None;
        if (isPartitioned)
            partitioning = Option<ITablePartitioning>.Some(await LoadPartitioningAsync(tableName, options, queryCache, cancellationToken));

        var isExternal = string.Equals(options.IsExternal, "Y", StringComparison.OrdinalIgnoreCase);
        var isTemporary = string.Equals(options.IsTemporary, "Y", StringComparison.OrdinalIgnoreCase);
        var isIndexOrganized = string.Equals(options.IotType, "IOT", StringComparison.OrdinalIgnoreCase);

        var kind = isExternal
            ? TableKind.External
            : isTemporary
                ? TableKind.Temporary
                : isPartitioned
                    ? TableKind.PartitionParent
                    : isIndexOrganized
                        ? TableKind.IndexOrganized
                        : TableKind.Regular;

        // LOGGING is null for a partitioned table, where the setting belongs to each partition, so
        // only an explicit NO is treated as unlogged.
        var isLogged = !string.Equals(options.Logging, "NO", StringComparison.OrdinalIgnoreCase);

        var collation = !options.DefaultCollation.IsNullOrWhiteSpace()
            ? Option<Identifier>.Some(Identifier.CreateQualifiedIdentifier(options.DefaultCollation))
            : Option<Identifier>.None;

        // Oracle has no system-versioned tables; Flashback Data Archive is a separate feature that
        // does not expose a period on the table itself.
        return new TableOptions(kind, partitioning, Option<ITableSystemVersioning>.None, isLogged, collation);
    }

    private async Task<ITablePartitioning> LoadPartitioningAsync(Identifier tableName, GetTableOptions.Result options, OracleTableQueryCache queryCache, CancellationToken cancellationToken)
    {
        var (partitionColumnNames, partitionNames, columnLookup) = await (
            DbConnection.QueryAsync(
                GetTablePartitionColumns.Sql,
                new GetTablePartitionColumns.Query { SchemaName = tableName.Schema!, TableName = tableName.LocalName },
                cancellationToken
            ),
            DbConnection.QueryAsync(
                GetTablePartitions.Sql,
                new GetTablePartitions.Query { SchemaName = tableName.Schema!, TableName = tableName.LocalName },
                cancellationToken
            ),
            queryCache.GetColumnLookupAsync(tableName, cancellationToken)
        ).WhenAll();

        var partitionColumns = partitionColumnNames
            .Select(columnName => columnLookup.TryGetValue(Identifier.CreateQualifiedIdentifier(columnName), out var column) ? column : null)
            .Where(static column => column != null)
            .Select(static column => column!)
            .ToList();
        var partitions = partitionNames
            .Select(static partitionName => Identifier.CreateQualifiedIdentifier(partitionName))
            .ToList();

        var strategy = !options.PartitioningType.IsNullOrWhiteSpace()
            ? options.PartitioningType
            : "UNKNOWN";

        return new TablePartitioning(strategy, partitionColumns, partitions);
    }

    private sealed record TableOptions(
        TableKind Kind,
        Option<ITablePartitioning> Partitioning,
        Option<ITableSystemVersioning> SystemVersioning,
        bool IsLogged,
        Option<Identifier> Collation
    )
    {
        public static TableOptions Default { get; } = new(
            TableKind.Regular,
            Option<ITablePartitioning>.None,
            Option<ITableSystemVersioning>.None,
            true,
            Option<Identifier>.None
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
    /// Retrieves the primary key, unique keys and foreign keys declared on the given table.
    /// </summary>
    /// <param name="tableName">A table name.</param>
    /// <param name="queryCache">A query cache for the given context.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The table's key constraints. The key each foreign key references is identified but not loaded.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="tableName"/> or <paramref name="queryCache"/> are <see langword="null" />.</exception>
    protected Task<TableConstraints> LoadConstraintsAsync(Identifier tableName, OracleTableQueryCache queryCache, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(tableName);
        ArgumentNullException.ThrowIfNull(queryCache);

        return LoadConstraintsAsyncCore(tableName, queryCache, cancellationToken);
    }

    // Primary, unique and foreign keys are all read from ALL_CONSTRAINTS with one query. The key a foreign
    // key references belongs to another table, so it is only identified here, and is resolved when the
    // table's parent keys are loaded.
    private async Task<TableConstraints> LoadConstraintsAsyncCore(Identifier tableName, OracleTableQueryCache queryCache, CancellationToken cancellationToken)
    {
        var constraintRows = await DbConnection.QueryAsync(
            GetTableConstraints.Sql,
            new GetTableConstraints.Query { SchemaName = tableName.Schema!, TableName = tableName.LocalName },
            cancellationToken
        );

        var rows = constraintRows.ToList();
        var primaryKeyRows = rows
            .Where(static row => string.Equals(row.ConstraintType, Constants.PrimaryKeyType, StringComparison.Ordinal))
            .ToList();
        var uniqueKeyRows = rows
            .Where(static row => string.Equals(row.ConstraintType, Constants.UniqueKeyType, StringComparison.Ordinal))
            .ToList();
        // The parent constraint is left joined so that one query serves every key type, which means a
        // foreign key referencing anything other than a primary or unique key comes back with null
        // parent-table columns. Such a foreign key cannot be resolved to a parent key, so it is dropped.
        var foreignKeyRows = rows
            .Where(static row => string.Equals(row.ConstraintType, Constants.ForeignKeyType, StringComparison.Ordinal)
                && row.ParentTableSchema != null
                && row.ParentTableName != null)
            .ToList();

        var hasKeys = primaryKeyRows.Count > 0 || uniqueKeyRows.Count > 0;
        if (!hasKeys && foreignKeyRows.Count == 0)
            return NoConstraints;

        var columnLookup = await queryCache.GetColumnLookupAsync(tableName, cancellationToken);
        var indexes = hasKeys
            ? await queryCache.GetIndexesAsync(tableName, cancellationToken)
            : [];

        return new TableConstraints(
            CreatePrimaryKey(primaryKeyRows, columnLookup, indexes),
            CreateUniqueKeys(uniqueKeyRows, columnLookup, indexes),
            CreateForeignKeyReferences(foreignKeyRows, columnLookup)
        );
    }

    private static Option<IDatabaseKey> CreatePrimaryKey(
        IReadOnlyCollection<GetTableConstraints.Result> rows,
        IReadOnlyDictionary<Identifier, IDatabaseColumn> columnLookup,
        IReadOnlyCollection<IDatabaseIndex> indexes)
    {
        if (rows.Count == 0)
            return Option<IDatabaseKey>.None;

        var groupedByName = rows.GroupAsDictionary(static row => new { row.ConstraintName, row.EnabledStatus, row.ValidatedStatus, row.Deferrable, row.Deferred, row.IndexName });
        var firstRow = groupedByName.First();
        var constraintName = firstRow.Key.ConstraintName;
        if (constraintName == null)
            return Option<IDatabaseKey>.None;

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

        var backingIndex = GetBackingIndex(indexes, firstRow.Key.IndexName);

        var primaryKey = new OracleDatabaseKey(constraintName, DatabaseKeyType.Primary, keyColumns, isEnabled, backingIndex, isValidated, deferrability);
        return Option<IDatabaseKey>.Some(primaryKey);
    }

    private static IReadOnlyCollection<IDatabaseKey> CreateUniqueKeys(
        IReadOnlyCollection<GetTableConstraints.Result> rows,
        IReadOnlyDictionary<Identifier, IDatabaseColumn> columnLookup,
        IReadOnlyCollection<IDatabaseIndex> indexes)
    {
        var groupedByName = rows
            .Where(static row => row.ConstraintName != null)
            .GroupAsDictionary(static row => new { ConstraintName = row.ConstraintName!, row.EnabledStatus, row.ValidatedStatus, row.Deferrable, row.Deferred, row.IndexName });
        if (groupedByName.Count == 0)
            return [];

        var result = new List<IDatabaseKey>(groupedByName.Count);
        foreach (var uk in groupedByName)
        {
            var columns = ResolveColumns(
                uk.Value
                    .Where(static row => row.ColumnName != null)
                    .OrderBy(static row => row.ColumnPosition)
                    .Select(static row => (Identifier)row.ColumnName!),
                columnLookup
            ).ToList();
            var isEnabled = string.Equals(uk.Key.EnabledStatus, Constants.Enabled, StringComparison.Ordinal);
            var isValidated = string.Equals(uk.Key.ValidatedStatus, Constants.Validated, StringComparison.Ordinal);
            var deferrability = GetDeferrability(uk.Key.Deferrable, uk.Key.Deferred);
            var backingIndex = GetBackingIndex(indexes, uk.Key.IndexName);

            result.Add(new OracleDatabaseKey(uk.Key.ConstraintName, DatabaseKeyType.Unique, columns, isEnabled, backingIndex, isValidated, deferrability));
        }

        return result;
    }

    private static IReadOnlyCollection<ForeignKeyReference> CreateForeignKeyReferences(
        IReadOnlyCollection<GetTableConstraints.Result> rows,
        IReadOnlyDictionary<Identifier, IDatabaseColumn> columnLookup)
    {
        var foreignKeys = rows.GroupAsDictionary(static row => new
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
            row.ParentKeyType,
        });
        if (foreignKeys.Count == 0)
            return [];

        var result = new List<ForeignKeyReference>(foreignKeys.Count);
        foreach (var fkey in foreignKeys)
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

            var parentKeyType = string.Equals(fkey.Key.ParentKeyType, Constants.PrimaryKeyType, StringComparison.Ordinal)
                ? DatabaseKeyType.Primary
                : DatabaseKeyType.Unique;

            result.Add(new ForeignKeyReference(
                childKey,
                Identifier.CreateQualifiedIdentifier(fkey.Key.ParentTableSchema, fkey.Key.ParentTableName),
                Identifier.CreateQualifiedIdentifier(fkey.Key.ParentConstraintName),
                parentKeyType,
                OracleCatalogMapper.GetReferentialAction(fkey.Key.DeleteAction)
            ));
        }

        return result;
    }

    private static readonly TableConstraints NoConstraints = new(Option<IDatabaseKey>.None, [], []);

    /// <summary>
    /// The primary key, unique keys and foreign keys declared on a table, which are read from the catalog together.
    /// </summary>
    /// <param name="PrimaryKey">The table's primary key, if it has one.</param>
    /// <param name="UniqueKeys">The table's unique keys.</param>
    /// <param name="ForeignKeys">The table's foreign keys, each identifying the key it references without loading it.</param>
    protected sealed record TableConstraints(
        Option<IDatabaseKey> PrimaryKey,
        IReadOnlyCollection<IDatabaseKey> UniqueKeys,
        IReadOnlyCollection<ForeignKeyReference> ForeignKeys
    );

    /// <summary>
    /// A foreign key declared on a table, together with the name of the primary or unique key it references.
    /// </summary>
    /// <param name="ChildKey">The foreign key, over the columns of the table declaring it.</param>
    /// <param name="ParentTableName">The schema-qualified name of the referenced table, as recorded in the catalog.</param>
    /// <param name="ParentKeyName">The name of the referenced key.</param>
    /// <param name="ParentKeyType">Whether the referenced key is a primary key or a unique key.</param>
    /// <param name="DeleteAction">The action taken on the child rows when a referenced row is deleted.</param>
    protected sealed record ForeignKeyReference(
        IDatabaseKey ChildKey,
        Identifier ParentTableName,
        Identifier ParentKeyName,
        DatabaseKeyType ParentKeyType,
        ReferentialAction DeleteAction
    );

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

        if (queryResult.Empty())
            return [];

        var columnLookup = await queryCache.GetColumnLookupAsync(tableName, cancellationToken);

        return OracleCatalogMapper.MapIndexes(queryResult, columnLookup, Dialect);
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

                    var deleteAction = OracleCatalogMapper.GetReferentialAction(childKeyRow.DeleteAction);

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
        var checkRows = await DbConnection.QueryAsync(
            GetTableChecks.Sql,
            new GetTableChecks.Query { SchemaName = tableName.Schema!, TableName = tableName.LocalName },
            cancellationToken
        );

        var checks = checkRows.ToList();
        if (checks.Empty())
            return [];

        var columnLookup = await queryCache.GetColumnLookupAsync(tableName, cancellationToken);

        // An inline NOT NULL is stored as a system-named check constraint, and is reported through
        // the column's IsNullable instead. Only constraints named by Oracle whose effect the column
        // already carries are dropped -- a user-named constraint, or one that has been disabled so
        // that the column reads as nullable, would otherwise be invisible.
        var columnNotNullConstraints = columnLookup
            .Where(static kv => !kv.Value.IsNullable)
            .Select(static kv => GenerateNotNullDefinition(kv.Key.LocalName))
            .ToHashSet(StringComparer.Ordinal);

        var result = new List<IDatabaseCheckConstraint>();

        foreach (var checkRow in checks)
        {
            var definition = checkRow.Definition;
            if (definition == null)
                continue;

            var isGeneratedName = string.Equals(checkRow.NameGeneration, Constants.GeneratedName, StringComparison.Ordinal);
            if (isGeneratedName && columnNotNullConstraints.Contains(definition))
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
        var constraints = await queryCache.GetConstraintsAsync(tableName, cancellationToken);
        if (constraints.ForeignKeys.Count == 0)
            return [];

        var result = new List<IDatabaseRelationalKey>(constraints.ForeignKeys.Count);
        // Memoized per parent table name so that multiple foreign keys referencing unique keys on the
        // same parent table don't rebuild the same lookup dictionary once per foreign key.
        var parentUniqueKeyLookups = new Dictionary<Identifier, IReadOnlyDictionary<Identifier, IDatabaseKey>>();
        foreach (var foreignKey in constraints.ForeignKeys)
        {
            var parentTableNameOption = await queryCache.GetTableNameAsync(foreignKey.ParentTableName, cancellationToken);
            Identifier? resolvedParentTableName = null;

            await parentTableNameOption
                .BindAsync(async parentTableName =>
                {
                    resolvedParentTableName = parentTableName;
                    if (foreignKey.ParentKeyType == DatabaseKeyType.Primary)
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

                    return uniqueKeyLookup.TryGetValue(foreignKey.ParentKeyName.LocalName, out var uniqueParentKey)
                        ? OptionAsync<IDatabaseKey>.Some(uniqueParentKey)
                        : OptionAsync<IDatabaseKey>.None;
                })
                .Map(parentKey => new OracleRelationalKey(tableName, foreignKey.ChildKey, resolvedParentTableName!, parentKey, foreignKey.DeleteAction))
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

        return LoadColumnsAsyncCore(tableName, cancellationToken);
    }

    private async Task<IReadOnlyList<IDatabaseColumn>> LoadColumnsAsyncCore(Identifier tableName, CancellationToken cancellationToken)
    {
        var query = await DbConnection.QueryAsync(
            GetTableColumns.Sql,
            new GetTableColumns.Query { SchemaName = tableName.Schema!, TableName = tableName.LocalName },
            cancellationToken
        );

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

            var isNullable = !string.Equals(row.IsNullable, Constants.N, StringComparison.Ordinal);
            var isComputed = string.Equals(row.IsComputed, Constants.Yes, StringComparison.Ordinal);
            // only a column the user declared INVISIBLE reaches this point; the query filters out
            // the system-generated hidden columns that back function-based indexes and object types
            var isHidden = string.Equals(row.IsHidden, Constants.Yes, StringComparison.Ordinal);
            var columnName = Identifier.CreateQualifiedIdentifier(row.ColumnName);
            var computedColumnDefinition = isComputed && !row.DefaultValue.IsNullOrWhiteSpace()
                ? Option<string>.Some(row.DefaultValue)
                : Option<string>.None;
            var defaultValue = OracleDefaultValueParser.Parse(row.DefaultValue);

            // Oracle evaluates a virtual column whenever it is read, so it is never stored.
            var column = new OracleDatabaseColumn(
                columnName,
                columnType,
                isNullable,
                defaultValue,
                isComputed ? Option<IAutoIncrement>.None : BuildAutoIncrement(row, tableName),
                isComputed,
                computedColumnDefinition,
                ComputedColumnStorage.Virtual,
                isHidden);

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

        return OracleCatalogMapper.MapTriggers(queryResult);
    }

    /// <summary>
    /// Creates a not null constraint definition, used to determine whether a constraint is a system-generated <c>NOT NULL</c> constraint.
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
    protected IReadOnlyDictionary<string, ReferentialAction> ReferentialActionMapping { get; } = OracleCatalogMapper.ReferentialActionMapping;

    /// <summary>
    /// A mapping from the trigger query timings as described in Oracle, to a <see cref="TriggerQueryTiming"/> instance.
    /// </summary>
    /// <value>A mapping dictionary.</value>
    protected IReadOnlyDictionary<string, TriggerQueryTiming> TimingMapping { get; } = OracleCatalogMapper.TimingMapping;

    /// <summary>
    /// A mapping from the trigger types as described in Oracle, to a <see cref="TriggerGranularity"/> instance.
    /// A compound trigger has sections at both granularities, so it is reported as
    /// <see cref="TriggerGranularity.Unknown"/> rather than picking one of them.
    /// </summary>
    /// <value>A mapping dictionary.</value>
    protected IReadOnlyDictionary<string, TriggerGranularity> GranularityMapping { get; } = OracleCatalogMapper.GranularityMapping;

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

    private static class Constants
    {

        public const string Enabled = "ENABLED";

        public const string Validated = "VALIDATED";

        public const string Deferrable = "DEFERRABLE";

        public const string Deferred = "DEFERRED";

        public const string PrimaryKeyType = "P";

        public const string UniqueKeyType = "U";

        public const string ForeignKeyType = "R";

        public const string Y = "Y";

        public const string N = "N";

        public const string GeneratedName = "GENERATED NAME";

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
        private readonly AsyncCache<Identifier, TableConstraints, OracleTableQueryCache> _constraints;
        private readonly AsyncCache<Identifier, IReadOnlyCollection<IDatabaseIndex>, OracleTableQueryCache> _indexes;
        private readonly AsyncCache<Identifier, IReadOnlyCollection<IDatabaseRelationalKey>, OracleTableQueryCache> _foreignKeys;
        private readonly AsyncCache<Identifier, IReadOnlyDictionary<Identifier, IDatabaseColumn>, OracleTableQueryCache> _columnLookups;

        /// <summary>
        /// Initializes a new instance of the <see cref="OracleTableQueryCache"/> class.
        /// </summary>
        /// <param name="tableNameLoader">A table name cache.</param>
        /// <param name="columnLoader">A column cache.</param>
        /// <param name="constraintLoader">A primary, unique and foreign key constraint cache.</param>
        /// <param name="indexLoader">An index cache.</param>
        /// <param name="foreignKeyLoader">A foreign key cache.</param>
        /// <param name="columnLookupLoader">A column lookup cache.</param>
        /// <exception cref="ArgumentNullException">Thrown when any of <paramref name="tableNameLoader"/>, <paramref name="columnLoader"/>, <paramref name="constraintLoader"/>, <paramref name="indexLoader"/>, <paramref name="foreignKeyLoader"/> or <paramref name="columnLookupLoader"/> are <see langword="null" />.</exception>
        public OracleTableQueryCache(
            AsyncCache<Identifier, Option<Identifier>, OracleTableQueryCache> tableNameLoader,
            AsyncCache<Identifier, IReadOnlyList<IDatabaseColumn>, OracleTableQueryCache> columnLoader,
            AsyncCache<Identifier, TableConstraints, OracleTableQueryCache> constraintLoader,
            AsyncCache<Identifier, IReadOnlyCollection<IDatabaseIndex>, OracleTableQueryCache> indexLoader,
            AsyncCache<Identifier, IReadOnlyCollection<IDatabaseRelationalKey>, OracleTableQueryCache> foreignKeyLoader,
            AsyncCache<Identifier, IReadOnlyDictionary<Identifier, IDatabaseColumn>, OracleTableQueryCache> columnLookupLoader
        )
        {
            _tableNames = tableNameLoader ?? throw new ArgumentNullException(nameof(tableNameLoader));
            _columns = columnLoader ?? throw new ArgumentNullException(nameof(columnLoader));
            _constraints = constraintLoader ?? throw new ArgumentNullException(nameof(constraintLoader));
            _indexes = indexLoader ?? throw new ArgumentNullException(nameof(indexLoader));
            _foreignKeys = foreignKeyLoader ?? throw new ArgumentNullException(nameof(foreignKeyLoader));
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
        /// Retrieves a table's primary, unique and foreign key constraints from the cache, querying the database when not populated.
        /// </summary>
        /// <param name="tableName">A table name.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The table's key constraints.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="tableName"/> is <see langword="null" />.</exception>
        public Task<TableConstraints> GetConstraintsAsync(Identifier tableName, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(tableName);

            return _constraints.GetByKeyAsync(tableName, this, cancellationToken);
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

            return GetPrimaryKeyAsyncCore(tableName, cancellationToken);
        }

        private async Task<Option<IDatabaseKey>> GetPrimaryKeyAsyncCore(Identifier tableName, CancellationToken cancellationToken)
        {
            var constraints = await _constraints.GetByKeyAsync(tableName, this, cancellationToken);
            return constraints.PrimaryKey;
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

            return GetUniqueKeysAsyncCore(tableName, cancellationToken);
        }

        private async Task<IReadOnlyCollection<IDatabaseKey>> GetUniqueKeysAsyncCore(Identifier tableName, CancellationToken cancellationToken)
        {
            var constraints = await _constraints.GetByKeyAsync(tableName, this, cancellationToken);
            return constraints.UniqueKeys;
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