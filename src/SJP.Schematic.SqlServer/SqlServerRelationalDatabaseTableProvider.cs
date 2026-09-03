using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LanguageExt;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Core.Utilities;
using SJP.Schematic.SqlServer.Queries;

namespace SJP.Schematic.SqlServer;

/// <summary>
/// A database table provider for SQL Server.
/// </summary>
/// <seealso cref="IRelationalDatabaseTableProvider" />
public class SqlServerRelationalDatabaseTableProvider : IRelationalDatabaseTableProvider
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SqlServerRelationalDatabaseTableProvider"/> class.
    /// </summary>
    /// <param name="connection">A schematic connection.</param>
    /// <param name="identifierDefaults">Database identifier defaults.</param>
    /// <exception cref="ArgumentNullException"><paramref name="connection"/> or <paramref name="identifierDefaults"/> are <see langword="null" />.</exception>
    public SqlServerRelationalDatabaseTableProvider(ISchematicConnection connection, IIdentifierDefaults identifierDefaults)
    {
        Connection = connection ?? throw new ArgumentNullException(nameof(connection));
        IdentifierDefaults = identifierDefaults ?? throw new ArgumentNullException(nameof(identifierDefaults));
    }

    /// <summary>
    /// A database connection that is specific to a given SQL Server database.
    /// </summary>
    /// <value>A database connection.</value>
    protected ISchematicConnection Connection { get; }

    /// <summary>
    /// Identifier defaults for the associated database.
    /// </summary>
    /// <value>Identifier defaults.</value>
    protected IIdentifierDefaults IdentifierDefaults { get; }

    /// <summary>
    /// A database connection factory.
    /// </summary>
    /// <value>A database connection factory.</value>
    protected IDbConnectionFactory DbConnection => Connection.ConnectionFactory;

    /// <summary>
    /// The dialect for the associated database.
    /// </summary>
    /// <value>A database dialect.</value>
    protected IDatabaseDialect Dialect => Connection.Dialect;

    /// <summary>
    /// Creates a query cache for a given query context
    /// </summary>
    /// <returns>A query cache.</returns>
    protected SqlServerTableQueryCache CreateQueryCache() => new(
        new AsyncCache<Identifier, Option<Identifier>, SqlServerTableQueryCache>((tableName, _, token) => GetResolvedTableName(tableName, token)),
        new AsyncCache<Identifier, IReadOnlyList<IDatabaseColumn>, SqlServerTableQueryCache>((tableName, _, token) => LoadColumnsAsync(tableName, token)),
        new AsyncCache<Identifier, Option<IDatabaseKey>, SqlServerTableQueryCache>(LoadPrimaryKeyAsync),
        new AsyncCache<Identifier, IReadOnlyCollection<IDatabaseKey>, SqlServerTableQueryCache>(LoadUniqueKeysAsync),
        new AsyncCache<Identifier, IReadOnlyCollection<IDatabaseIndex>, SqlServerTableQueryCache>(LoadIndexesAsync),
        new AsyncCache<Identifier, IReadOnlyCollection<IDatabaseRelationalKey>, SqlServerTableQueryCache>(LoadParentKeysAsync),
        new AsyncCache<Identifier, IReadOnlyDictionary<Identifier, IDatabaseColumn>, SqlServerTableQueryCache>(
            async (tableName, cache, token) => GetColumnLookup(await cache.GetColumnsAsync(tableName, token)))
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

        var candidateTableName = QualifyTableName(tableName);
        return LoadTable(candidateTableName, CreateQueryCache(), cancellationToken);
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

        tableName = QualifyTableName(tableName);
        var qualifiedTableName = DbConnection.QueryFirstOrNone(
            GetTableName.Sql,
            new GetTableName.Query { SchemaName = tableName.Schema!, TableName = tableName.LocalName },
            cancellationToken
        );

        return qualifiedTableName
            .Map(name => Identifier.CreateQualifiedIdentifier(tableName.Server, tableName.Database, name.SchemaName, name.TableName))
            .ToOption();
    }

    /// <summary>
    /// Retrieves a table from the database, if available.
    /// </summary>
    /// <param name="tableName">A table name.</param>
    /// <param name="queryCache">The query cache.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A table, if available.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="tableName"/> or <paramref name="queryCache"/> is <see langword="null" />.</exception>
    protected OptionAsync<IRelationalDatabaseTable> LoadTable(Identifier tableName, SqlServerTableQueryCache queryCache, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(tableName);
        ArgumentNullException.ThrowIfNull(queryCache);

        var candidateTableName = QualifyTableName(tableName);
        return GetResolvedTableName(candidateTableName, cancellationToken)
            .MapAsync(name => LoadTableAsyncCore(name, queryCache, cancellationToken));
    }

    private async Task<IRelationalDatabaseTable> LoadTableAsyncCore(Identifier tableName, SqlServerTableQueryCache queryCache, CancellationToken cancellationToken)
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

    /// <summary>
    /// Retrieves the primary key for the given table, if available.
    /// </summary>
    /// <param name="tableName">A table name.</param>
    /// <param name="queryCache">A query cache for the given context.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A primary key, if available.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="tableName"/> or <paramref name="queryCache"/> are <see langword="null" />.</exception>
    protected Task<Option<IDatabaseKey>> LoadPrimaryKeyAsync(Identifier tableName, SqlServerTableQueryCache queryCache, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(tableName);
        ArgumentNullException.ThrowIfNull(queryCache);

        return LoadPrimaryKeyAsyncCore(tableName, queryCache, cancellationToken);
    }

    private async Task<Option<IDatabaseKey>> LoadPrimaryKeyAsyncCore(Identifier tableName, SqlServerTableQueryCache queryCache, CancellationToken cancellationToken)
    {
        var primaryKeyColumns = await DbConnection.QueryAsync(
            GetTablePrimaryKey.Sql,
            new GetTablePrimaryKey.Query { SchemaName = tableName.Schema!, TableName = tableName.LocalName },
            cancellationToken
        );

        if (primaryKeyColumns.Empty())
            return Option<IDatabaseKey>.None;

        var groupedByName = primaryKeyColumns.GroupAsDictionary(static row => new { row.ConstraintName, row.IsDisabled });
        var firstRow = groupedByName.First();
        var constraintName = firstRow.Key.ConstraintName;
        if (constraintName == null)
            return Option<IDatabaseKey>.None;

        var isEnabled = !firstRow.Key.IsDisabled;

        var columnLookup = await queryCache.GetColumnLookupAsync(tableName, cancellationToken);

        var keyColumns = groupedByName
            .Where(row => string.Equals(row.Key.ConstraintName, constraintName, StringComparison.Ordinal))
            .SelectMany(g => ResolveColumns(g.Value.Select(static row => (Identifier)row.ColumnName), columnLookup))
            .ToList();

        var indexes = await queryCache.GetIndexesAsync(tableName, cancellationToken);
        var backingIndex = GetBackingIndex(indexes, constraintName);

        var primaryKey = new SqlServerDatabaseKey(constraintName, DatabaseKeyType.Primary, keyColumns, isEnabled, backingIndex);
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
    protected Task<IReadOnlyCollection<IDatabaseIndex>> LoadIndexesAsync(Identifier tableName, SqlServerTableQueryCache queryCache, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(tableName);
        ArgumentNullException.ThrowIfNull(queryCache);

        return LoadIndexesAsyncCore(tableName, queryCache, cancellationToken);
    }

    private async Task<IReadOnlyCollection<IDatabaseIndex>> LoadIndexesAsyncCore(Identifier tableName, SqlServerTableQueryCache queryCache, CancellationToken cancellationToken)
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
                row.IsDisabled,
                row.IsFiltered,
                row.FilterDefinition,
                row.IndexType,
                row.FillFactor,
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
            var isEnabled = !indexInfo.Key.IsDisabled;

            var indexCols = indexInfo.Value
                .Where(static row => !row.IsIncludedColumn)
                .OrderBy(static row => row.KeyOrdinal)
                .ThenBy(static row => row.IndexColumnId)
                .Select(row =>
                {
                    columnLookup.TryGetValue(row.ColumnName, out var column);
                    return new { row.IsDescending, Column = column };
                })
                .Where(static row => row.Column != null)
                .Select(row =>
                {
                    var order = row.IsDescending ? IndexColumnOrder.Descending : IndexColumnOrder.Ascending;
                    var column = row.Column!;
                    var expression = Dialect.QuoteName(column.Name);
                    return new DatabaseIndexColumn(expression, column, order);
                })
                .ToList();

            var includedCols = ResolveColumns(
                indexInfo.Value
                    .Where(static row => row.IsIncludedColumn)
                    .OrderBy(static row => row.KeyOrdinal)
                    .ThenBy(static row => row.ColumnName, StringComparer.Ordinal) // matches SSMS behaviour
                    .Select(static row => (Identifier)row.ColumnName),
                columnLookup
            ).ToList();

            var filterDefinition = indexInfo.Key.IsFiltered && !indexInfo.Key.FilterDefinition.IsNullOrWhiteSpace()
                ? Option<string>.Some(indexInfo.Key.FilterDefinition)
                : Option<string>.None;

            var indexType = IndexTypeMapping.TryGetValue(indexInfo.Key.IndexType, out var mappedIndexType)
                ? mappedIndexType
                : IndexType.Unknown;

            // a fill factor of zero means that the server default is used, i.e. no fill factor was set
            var fillFactor = indexInfo.Key.FillFactor > 0
                ? Option<int>.Some(indexInfo.Key.FillFactor)
                : Option<int>.None;

            var index = new DatabaseIndex(
                indexName,
                isUnique,
                indexCols,
                includedCols,
                isEnabled,
                filterDefinition,
                indexType,
                fillFactor,
                true, // SQL Server has no notion of an invalid index, only a disabled one
                true // nor of an invisible index
            );
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
    protected Task<IReadOnlyCollection<IDatabaseKey>> LoadUniqueKeysAsync(Identifier tableName, SqlServerTableQueryCache queryCache, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(tableName);
        ArgumentNullException.ThrowIfNull(queryCache);

        return LoadUniqueKeysAsyncCore(tableName, queryCache, cancellationToken);
    }

    private async Task<IReadOnlyCollection<IDatabaseKey>> LoadUniqueKeysAsyncCore(Identifier tableName, SqlServerTableQueryCache queryCache, CancellationToken cancellationToken)
    {
        var uniqueKeyColumns = await DbConnection.QueryAsync(
            GetTableUniqueKeys.Sql,
            new GetTableUniqueKeys.Query { SchemaName = tableName.Schema!, TableName = tableName.LocalName },
            cancellationToken
        );

        if (uniqueKeyColumns.Empty())
            return [];

        var columnLookup = await queryCache.GetColumnLookupAsync(tableName, cancellationToken);

        var groupedByName = uniqueKeyColumns.GroupAsDictionary(static row => new { row.ConstraintName, row.IsDisabled });
        var constraintColumns = groupedByName
            .Select(g => new
            {
                g.Key.ConstraintName,
                Columns = ResolveColumns(g.Value.Select(static row => (Identifier)row.ColumnName), columnLookup).ToList(),
                IsEnabled = !g.Key.IsDisabled,
            })
            .ToList();
        if (constraintColumns.Empty())
            return [];

        var indexes = await queryCache.GetIndexesAsync(tableName, cancellationToken);

        var result = new List<IDatabaseKey>(constraintColumns.Count);
        foreach (var uk in constraintColumns)
        {
            var backingIndex = GetBackingIndex(indexes, uk.ConstraintName);
            var uniqueKey = new SqlServerDatabaseKey(uk.ConstraintName, DatabaseKeyType.Unique, uk.Columns, uk.IsEnabled, backingIndex);
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
    protected Task<IReadOnlyCollection<IDatabaseRelationalKey>> LoadChildKeysAsync(Identifier tableName, SqlServerTableQueryCache queryCache, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(tableName);
        ArgumentNullException.ThrowIfNull(queryCache);

        return LoadChildKeysAsyncCore(tableName, queryCache, cancellationToken);
    }

    private async Task<IReadOnlyCollection<IDatabaseRelationalKey>> LoadChildKeysAsyncCore(Identifier tableName, SqlServerTableQueryCache queryCache, CancellationToken cancellationToken)
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

        // Several child-key rows can share the same child table (e.g. it may hold more than one FK back
        // to this table), so its foreign-key lookup is memoised here rather than rebuilt per row.
        var childForeignKeyLookups = new Dictionary<Identifier, IReadOnlyDictionary<Identifier, IDatabaseKey>>();

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
            var childTableNameOption = await queryCache.GetTableNameAsync(candidateChildTableName, cancellationToken);

            await childTableNameOption
                .BindAsync(async childTableName =>
                {
                    var childKeyName = Identifier.CreateQualifiedIdentifier(groupedChildKey.Key.ChildKeyName);

                    if (!childForeignKeyLookups.TryGetValue(childTableName, out var parentKeyLookup))
                    {
                        var parentKeys = await queryCache.GetForeignKeysAsync(childTableName, cancellationToken);
                        parentKeyLookup = GetDatabaseKeyLookup(parentKeys.Select(fk => fk.ChildKey).ToList());
                        childForeignKeyLookups[childTableName] = parentKeyLookup;
                    }

                    if (!parentKeyLookup.TryGetValue(childKeyName, out var childKey))
                        return OptionAsync<IDatabaseRelationalKey>.None;

                    var deleteAction = ReferentialActionMapping[groupedChildKey.Key.DeleteAction];
                    var updateAction = ReferentialActionMapping[groupedChildKey.Key.UpdateAction];
                    var relationalKey = new DatabaseRelationalKey(childTableName, childKey, tableName, parentKey, deleteAction, updateAction);
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
    protected async Task<IReadOnlyCollection<IDatabaseCheckConstraint>> LoadChecksAsync(Identifier tableName, CancellationToken cancellationToken)
    {
        return await DbConnection.QueryEnumerableAsync(
                GetTableChecks.Sql,
                new GetTableChecks.Query { SchemaName = tableName.Schema!, TableName = tableName.LocalName },
                cancellationToken
            )
            .Where(checkRow => checkRow.ConstraintName != null && checkRow.Definition != null)
            .Select(checkRow =>
            {
                var constraintName = Identifier.CreateQualifiedIdentifier(checkRow.ConstraintName);
                var definition = checkRow.Definition;
                var isEnabled = !checkRow.IsDisabled;
                var isValidated = !checkRow.IsNotTrusted;

                return new DatabaseCheckConstraint(constraintName, definition, isEnabled, isValidated, ConstraintDeferrability.NotDeferrable);
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
    protected Task<IReadOnlyCollection<IDatabaseRelationalKey>> LoadParentKeysAsync(Identifier tableName, SqlServerTableQueryCache queryCache, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(tableName);
        ArgumentNullException.ThrowIfNull(queryCache);

        return LoadParentKeysAsyncCore(tableName, queryCache, cancellationToken);
    }

    private async Task<IReadOnlyCollection<IDatabaseRelationalKey>> LoadParentKeysAsyncCore(Identifier tableName, SqlServerTableQueryCache queryCache, CancellationToken cancellationToken)
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
            row.ParentTableSchema,
            row.ParentTableName,
            row.ParentKeyName,
            KeyType = row.ParentKeyType,
            row.DeleteAction,
            row.UpdateAction,
            row.IsDisabled,
            row.IsNotTrusted,
        }).ToList();
        if (foreignKeys.Empty())
            return [];

        var columnLookup = await queryCache.GetColumnLookupAsync(tableName, cancellationToken);

        // Several foreign keys can reference the same parent table, so its unique-key/unique-index
        // lookups are memoised here rather than rebuilt for every foreign key.
        var parentUniqueKeyLookups = new Dictionary<Identifier, IReadOnlyDictionary<Identifier, IDatabaseKey>>();
        var parentUniqueIndexLookups = new Dictionary<Identifier, IReadOnlyDictionary<Identifier, IDatabaseIndex>>();

        var result = new List<IDatabaseRelationalKey>(foreignKeys.Count);
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
                        var primaryKey = await queryCache.GetPrimaryKeyAsync(parentTableName, cancellationToken);
                        return primaryKey.ToAsync();
                    }

                    var parentKeyName = Identifier.CreateQualifiedIdentifier(fkey.Key.ParentKeyName);

                    if (!parentUniqueKeyLookups.TryGetValue(parentTableName, out var parentUniqueKeyLookup))
                    {
                        var parentUniqueKeys = await queryCache.GetUniqueKeysAsync(parentTableName, cancellationToken);
                        parentUniqueKeyLookup = GetDatabaseKeyLookup(parentUniqueKeys);
                        parentUniqueKeyLookups[parentTableName] = parentUniqueKeyLookup;
                    }

                    if (parentUniqueKeyLookup.TryGetValue(parentKeyName.LocalName, out var uniqueKey))
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
                    var childKeyName = Identifier.CreateQualifiedIdentifier(fkey.Key.ChildKeyName);
                    var childKeyColumns = ResolveColumns(
                        fkey.Value
                            .OrderBy(static row => row.ConstraintColumnId)
                            .Select(static row => (Identifier)row.ColumnName),
                        columnLookup
                    ).ToList();

                    var isEnabled = !fkey.Key.IsDisabled;
                    var isValidated = !fkey.Key.IsNotTrusted;
                    var childKey = new SqlServerDatabaseKey(childKeyName, DatabaseKeyType.Foreign, childKeyColumns, isEnabled, Option<IDatabaseIndex>.None, isValidated);

                    var deleteAction = ReferentialActionMapping[fkey.Key.DeleteAction];
                    var updateAction = ReferentialActionMapping[fkey.Key.UpdateAction];

                    return new DatabaseRelationalKey(tableName, childKey, resolvedParentTableName!, parentKey, deleteAction, updateAction);
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
    protected Task<IReadOnlyList<IDatabaseColumn>> LoadColumnsAsync(Identifier tableName, CancellationToken cancellationToken)
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
                var typeMetadata = new ColumnTypeMetadata
                {
                    TypeName = Identifier.CreateQualifiedIdentifier(row.ColumnTypeSchema, row.ColumnTypeName),
                    Collation = !row.Collation.IsNullOrWhiteSpace()
                    ? Option<Identifier>.Some(Identifier.CreateQualifiedIdentifier(row.Collation))
                    : Option<Identifier>.None,
                    MaxLength = row.MaxLength,
                    NumericPrecision = new NumericPrecision(row.Precision, row.Scale),
                };
                var columnType = Dialect.TypeProvider.CreateColumnType(typeMetadata);

                var columnName = Identifier.CreateQualifiedIdentifier(row.ColumnName);
                // sys.identity_columns reports the seed and increment as sql_variant, which can be
                // null for a column whose identity was defined without them; SQL Server's own
                // defaults are 1 and 1.
                var identitySeed = row.IdentitySeed ?? 1;
                var identityIncrement = row.IdentityIncrement is long incr && incr != 0 ? incr : 1;
                var autoIncrement = row.IsIdentity
                    ? Option<IAutoIncrement>.Some(new AutoIncrement(identitySeed, identityIncrement))
                    : Option<IAutoIncrement>.None;
                var defaultValue = row.HasDefaultValue && row.DefaultValue != null
                    ? Option<string>.Some(row.DefaultValue)
                    : Option<string>.None;
                var computedColumnDefinition = !row.ComputedColumnDefinition.IsNullOrWhiteSpace()
                    ? Option<string>.Some(row.ComputedColumnDefinition)
                    : Option<string>.None;

                // sys.computed_columns.is_persisted separates a computed column whose value is
                // written with the row from one evaluated on every read.
                var computedStorage = row.ComputedColumnIsPersisted == true
                    ? ComputedColumnStorage.Stored
                    : ComputedColumnStorage.Virtual;

                return new DatabaseColumn(
                    columnName,
                    columnType,
                    row.IsNullable,
                    defaultValue,
                    autoIncrement,
                    row.IsComputed,
                    computedColumnDefinition,
                    computedStorage);
            })
            .ToListAsync(cancellationToken);
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

        var triggerRows = queryResult.ToList();

        var result = new List<IDatabaseTrigger>(triggerRows.Count);
        foreach (var trig in triggerRows)
        {
            var triggerName = Identifier.CreateQualifiedIdentifier(trig.TriggerName);
            var queryTiming = trig.IsInsteadOfTrigger ? TriggerQueryTiming.InsteadOf : TriggerQueryTiming.After;
            var definition = trig.Definition;
            var isEnabled = !trig.IsDisabled;

            var events = TriggerEvent.None;
            if (trig.IsInsertTrigger)
                events |= TriggerEvent.Insert;
            if (trig.IsUpdateTrigger)
                events |= TriggerEvent.Update;
            if (trig.IsDeleteTrigger)
                events |= TriggerEvent.Delete;
            if (trig.IsOtherTrigger)
                events |= TriggerEvent.Other;

            // SQL Server DML triggers always fire once per statement; there is no FOR EACH ROW form.
            // Neither is there a WHEN clause or an UPDATE OF column list -- a trigger body tests
            // update(column) at runtime instead, which the catalog does not record.
            var trigger = new DatabaseTrigger(
                triggerName,
                definition,
                queryTiming,
                events,
                isEnabled,
                TriggerGranularity.Statement,
                Option<string>.None,
                []
            );
            result.Add(trigger);
        }

        return result;
    }

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
    /// A mapping from the referential actions as described in SQL Server, to a <see cref="ReferentialAction"/> instance.
    /// </summary>
    /// <value>A mapping dictionary.</value>
    protected IReadOnlyDictionary<int, ReferentialAction> ReferentialActionMapping { get; } = new Dictionary<int, ReferentialAction>
    {
        [0] = ReferentialAction.NoAction,
        [1] = ReferentialAction.Cascade,
        [2] = ReferentialAction.SetNull,
        [3] = ReferentialAction.SetDefault,
    };

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

    private static IReadOnlyDictionary<Identifier, IDatabaseKey> GetDatabaseKeyLookup(IReadOnlyCollection<IDatabaseKey> keys)
    {
        ArgumentNullException.ThrowIfNull(keys);

        var result = new Dictionary<Identifier, IDatabaseKey>(keys.Count);

        foreach (var key in keys)
            key.Name.IfSome(name => result[name.LocalName] = key);

        return result;
    }

    // SQL Server names the index enforcing a primary or unique key constraint after the constraint itself,
    // so the two are matched by name.
    private static Option<IDatabaseIndex> GetBackingIndex(IReadOnlyCollection<IDatabaseIndex> indexes, Identifier constraintName)
    {
        var backingIndex = indexes.FirstOrDefault(index => index.Name == constraintName);
        return backingIndex != null
            ? Option<IDatabaseIndex>.Some(backingIndex)
            : Option<IDatabaseIndex>.None;
    }

    // A foreign key can reference a unique index that has no backing UNIQUE constraint, so the referenced key
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
        return new SqlServerDatabaseKey(uniqueIndex.Name, DatabaseKeyType.Unique, columns, uniqueIndex.IsEnabled);
    }

    private static class Constants
    {
        public const string PrimaryKeyType = "PK";
    }

    // sys.indexes.type values, see https://learn.microsoft.com/en-us/sql/relational-databases/system-catalog-views/sys-indexes-transact-sql
    private static readonly IReadOnlyDictionary<int, IndexType> IndexTypeMapping = new Dictionary<int, IndexType>
    {
        [1] = IndexType.Clustered,
        [2] = IndexType.BTree,
        [3] = IndexType.Xml,
        [4] = IndexType.Spatial,
        [5] = IndexType.ColumnStore,
        [6] = IndexType.ColumnStore,
        [7] = IndexType.Hash,
    };

    /// <summary>
    /// A query cache provider for SQL Server tables. Ensures that a given query only occurs at most once for a given query context.
    /// </summary>
    protected class SqlServerTableQueryCache
    {
        private readonly AsyncCache<Identifier, Option<Identifier>, SqlServerTableQueryCache> _tableNames;
        private readonly AsyncCache<Identifier, IReadOnlyList<IDatabaseColumn>, SqlServerTableQueryCache> _columns;
        private readonly AsyncCache<Identifier, Option<IDatabaseKey>, SqlServerTableQueryCache> _primaryKeys;
        private readonly AsyncCache<Identifier, IReadOnlyCollection<IDatabaseKey>, SqlServerTableQueryCache> _uniqueKeys;
        private readonly AsyncCache<Identifier, IReadOnlyCollection<IDatabaseIndex>, SqlServerTableQueryCache> _indexes;
        private readonly AsyncCache<Identifier, IReadOnlyCollection<IDatabaseRelationalKey>, SqlServerTableQueryCache> _foreignKeys;
        private readonly AsyncCache<Identifier, IReadOnlyDictionary<Identifier, IDatabaseColumn>, SqlServerTableQueryCache> _columnLookups;

        /// <summary>
        /// Initializes a new instance of the <see cref="SqlServerTableQueryCache"/> class.
        /// </summary>
        /// <param name="tableNameLoader">A table name cache.</param>
        /// <param name="columnLoader">A column cache.</param>
        /// <param name="primaryKeyLoader">A primary key cache.</param>
        /// <param name="uniqueKeyLoader">A unique key cache.</param>
        /// <param name="indexLoader">An index cache.</param>
        /// <param name="foreignKeyLoader">A foreign key cache.</param>
        /// <param name="columnLookupLoader">A column lookup cache.</param>
        /// <exception cref="ArgumentNullException">Thrown when any of <paramref name="tableNameLoader"/>, <paramref name="columnLoader"/>, <paramref name="primaryKeyLoader"/>, <paramref name="uniqueKeyLoader"/>, <paramref name="indexLoader"/>, <paramref name="foreignKeyLoader"/> or <paramref name="columnLookupLoader"/> are <see langword="null" />.</exception>
        public SqlServerTableQueryCache(
            AsyncCache<Identifier, Option<Identifier>, SqlServerTableQueryCache> tableNameLoader,
            AsyncCache<Identifier, IReadOnlyList<IDatabaseColumn>, SqlServerTableQueryCache> columnLoader,
            AsyncCache<Identifier, Option<IDatabaseKey>, SqlServerTableQueryCache> primaryKeyLoader,
            AsyncCache<Identifier, IReadOnlyCollection<IDatabaseKey>, SqlServerTableQueryCache> uniqueKeyLoader,
            AsyncCache<Identifier, IReadOnlyCollection<IDatabaseIndex>, SqlServerTableQueryCache> indexLoader,
            AsyncCache<Identifier, IReadOnlyCollection<IDatabaseRelationalKey>, SqlServerTableQueryCache> foreignKeyLoader,
            AsyncCache<Identifier, IReadOnlyDictionary<Identifier, IDatabaseColumn>, SqlServerTableQueryCache> columnLookupLoader
        )
        {
            _tableNames = tableNameLoader ?? throw new ArgumentNullException(nameof(tableNameLoader));
            _columns = columnLoader ?? throw new ArgumentNullException(nameof(columnLoader));
            _primaryKeys = primaryKeyLoader ?? throw new ArgumentNullException(nameof(primaryKeyLoader));
            _uniqueKeys = uniqueKeyLoader ?? throw new ArgumentNullException(nameof(uniqueKeyLoader));
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

        /// <summary>
        /// Retrieves a table's column lookup from the cache, querying the database when not populated.
        /// </summary>
        /// <param name="tableName">A table name.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A lookup of a table's columns, keyed by column name.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="tableName"/> is <see langword="null" />.</exception>
        public Task<IReadOnlyDictionary<Identifier, IDatabaseColumn>> GetColumnLookupAsync(Identifier tableName, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(tableName);

            return _columnLookups.GetByKeyAsync(tableName, this, cancellationToken);
        }
    }
}