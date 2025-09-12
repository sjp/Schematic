using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LanguageExt;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Exceptions;
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
    protected IDbConnectionFactory DbConnection => Connection.DbConnection;

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
        new AsyncCache<Identifier, IReadOnlyCollection<IDatabaseRelationalKey>, SqlServerTableQueryCache>(LoadParentKeysAsync)
    );

    /// <summary>
    /// Gets all database tables.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A collection of database tables.</returns>
    public IAsyncEnumerable<IRelationalDatabaseTable> GetAllTables(CancellationToken cancellationToken = default)
    {
        var queryCache = CreateQueryCache();

        return DbConnection.QueryEnumerableAsync<GetAllTableNames.Result>(GetAllTableNames.Sql, cancellationToken)
            .Select(static dto => Identifier.CreateQualifiedIdentifier(dto.SchemaName, dto.TableName))
            .Select(QualifyTableName)
            .SelectAwait(tableName => LoadTableAsyncCore(tableName, queryCache, cancellationToken).ToValue());
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
        ) = await TaskUtilities.WhenAll(
            queryCache.GetColumnsAsync(tableName, cancellationToken),
            LoadChecksAsync(tableName, cancellationToken),
            LoadTriggersAsync(tableName, cancellationToken),
            LoadIndexesAsync(tableName, queryCache, cancellationToken),
            queryCache.GetPrimaryKeyAsync(tableName, cancellationToken),
            queryCache.GetUniqueKeysAsync(tableName, cancellationToken),
            queryCache.GetForeignKeysAsync(tableName, cancellationToken),
            LoadChildKeysAsync(tableName, queryCache, cancellationToken)
        ).ConfigureAwait(false);

        return new RelationalDatabaseTable(
            tableName,
            columns,
            primaryKey,
            uniqueKeys,
            parentKeys,
            childKeys,
            indexes,
            checks,
            triggers
        );
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
        ).ConfigureAwait(false);

        if (primaryKeyColumns.Empty())
            return Option<IDatabaseKey>.None;

        var groupedByName = primaryKeyColumns.GroupAsDictionary(static row => new { row.ConstraintName, row.IsDisabled });
        var firstRow = groupedByName.First();
        var constraintName = firstRow.Key.ConstraintName;
        if (constraintName == null)
            return Option<IDatabaseKey>.None;

        var isEnabled = !firstRow.Key.IsDisabled;

        var columns = await queryCache.GetColumnsAsync(tableName, cancellationToken).ConfigureAwait(false);
        var columnLookup = GetColumnLookup(columns);

        var keyColumns = groupedByName
            .Where(row => string.Equals(row.Key.ConstraintName, constraintName, StringComparison.Ordinal))
            .SelectMany(g => g.Value
                .Where(row => columnLookup.ContainsKey(row.ColumnName))
                .Select(row => columnLookup[row.ColumnName]))
            .ToList();

        var primaryKey = new SqlServerDatabaseKey(constraintName, DatabaseKeyType.Primary, keyColumns, isEnabled);
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
        ).ConfigureAwait(false);

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
            })
            .ToList();
        if (indexColumns.Empty())
            return [];

        var columns = await queryCache.GetColumnsAsync(tableName, cancellationToken).ConfigureAwait(false);
        var columnLookup = GetColumnLookup(columns);

        var result = new List<IDatabaseIndex>(indexColumns.Count);
        foreach (var indexInfo in indexColumns)
        {
            var isUnique = indexInfo.Key.IsUnique;
            var indexName = Identifier.CreateQualifiedIdentifier(indexInfo.Key.IndexName);
            var isEnabled = !indexInfo.Key.IsDisabled;

            var indexCols = indexInfo.Value
                .Where(row => !row.IsIncludedColumn && columnLookup.ContainsKey(row.ColumnName))
                .OrderBy(static row => row.KeyOrdinal)
                .ThenBy(static row => row.IndexColumnId)
                .Select(row => new { row.IsDescending, Column = columnLookup[row.ColumnName] })
                .Select(row =>
                {
                    var order = row.IsDescending ? IndexColumnOrder.Descending : IndexColumnOrder.Ascending;
                    var column = row.Column;
                    var expression = Dialect.QuoteName(column.Name);
                    return new DatabaseIndexColumn(expression, column, order);
                })
                .ToList();

            var includedCols = indexInfo.Value
                .Where(row => row.IsIncludedColumn && columnLookup.ContainsKey(row.ColumnName))
                .OrderBy(static row => row.KeyOrdinal)
                .ThenBy(static row => row.ColumnName, StringComparer.Ordinal) // matches SSMS behaviour
                .Select(row => columnLookup[row.ColumnName])
                .ToList();

            var filterDefinition = indexInfo.Key.IsFiltered && !indexInfo.Key.FilterDefinition.IsNullOrWhiteSpace()
                ? Option<string>.Some(indexInfo.Key.FilterDefinition)
                : Option<string>.None;

            var index = new DatabaseIndex(indexName, isUnique, indexCols, includedCols, isEnabled, filterDefinition);
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
        ).ConfigureAwait(false);

        if (uniqueKeyColumns.Empty())
            return [];

        var columns = await queryCache.GetColumnsAsync(tableName, cancellationToken).ConfigureAwait(false);
        var columnLookup = GetColumnLookup(columns);

        var groupedByName = uniqueKeyColumns.GroupAsDictionary(static row => new { row.ConstraintName, row.IsDisabled });
        var constraintColumns = groupedByName
            .Select(g => new
            {
                g.Key.ConstraintName,
                Columns = g.Value
                    .Where(row => columnLookup.ContainsKey(row.ColumnName))
                    .Select(row => columnLookup[row.ColumnName])
                    .ToList(),
                IsEnabled = !g.Key.IsDisabled,
            })
            .ToList();
        if (constraintColumns.Empty())
            return [];

        var result = new List<IDatabaseKey>(constraintColumns.Count);
        foreach (var uk in constraintColumns)
        {
            var uniqueKey = new SqlServerDatabaseKey(uk.ConstraintName, DatabaseKeyType.Unique, uk.Columns, uk.IsEnabled);
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
        ).ConfigureAwait(false);

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

        var primaryKey = await queryCache.GetPrimaryKeyAsync(tableName, cancellationToken).ConfigureAwait(false);
        var uniqueKeys = await queryCache.GetUniqueKeysAsync(tableName, cancellationToken).ConfigureAwait(false);
        var uniqueKeyLookup = GetDatabaseKeyLookup(uniqueKeys);

        var result = new List<IDatabaseRelationalKey>(groupedChildKeys.Count);

        foreach (var groupedChildKey in groupedChildKeys)
        {
            // ensure we have a key to begin with
            IDatabaseKey? parentKey = null;

            if (string.Equals(groupedChildKey.Key.ParentKeyType, Constants.PrimaryKeyType, StringComparison.Ordinal))
                await primaryKey.IfSomeAsync(k => parentKey = k).ConfigureAwait(false);
            else if (uniqueKeyLookup.ContainsKey(groupedChildKey.Key.ParentKeyName))
                parentKey = uniqueKeyLookup[groupedChildKey.Key.ParentKeyName];
            if (parentKey == null)
                continue;

            var candidateChildTableName = Identifier.CreateQualifiedIdentifier(groupedChildKey.Key.ChildTableSchema, groupedChildKey.Key.ChildTableName);
            var childTableNameOption = await queryCache.GetTableNameAsync(candidateChildTableName, cancellationToken).ConfigureAwait(false);

            await childTableNameOption
                .BindAsync(async childTableName =>
                {
                    var childKeyName = Identifier.CreateQualifiedIdentifier(groupedChildKey.Key.ChildKeyName);

                    var parentKeys = await queryCache.GetForeignKeysAsync(childTableName, cancellationToken).ConfigureAwait(false);
                    var parentKeyLookup = GetDatabaseKeyLookup(parentKeys.Select(fk => fk.ChildKey).ToList());

                    if (!parentKeyLookup.TryGetValue(childKeyName, out var childKey))
                        return OptionAsync<IDatabaseRelationalKey>.None;

                    var deleteAction = ReferentialActionMapping[groupedChildKey.Key.DeleteAction];
                    var updateAction = ReferentialActionMapping[groupedChildKey.Key.UpdateAction];
                    var relationalKey = new DatabaseRelationalKey(childTableName, childKey, tableName, parentKey, deleteAction, updateAction);
                    return OptionAsync<IDatabaseRelationalKey>.Some(relationalKey);
                })
                .IfSome(relationalKey => result.Add(relationalKey))
                .ConfigureAwait(false);
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

                return new DatabaseCheckConstraint(constraintName, definition, isEnabled);
            })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
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
        ).ConfigureAwait(false);

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
        }).ToList();
        if (foreignKeys.Empty())
            return [];

        var columns = await queryCache.GetColumnsAsync(tableName, cancellationToken).ConfigureAwait(false);
        var columnLookup = GetColumnLookup(columns);

        var result = new List<IDatabaseRelationalKey>(foreignKeys.Count);
        foreach (var fkey in foreignKeys)
        {
            var candidateParentTableName = Identifier.CreateQualifiedIdentifier(fkey.Key.ParentTableSchema, fkey.Key.ParentTableName);
            var parentTableNameOption = await queryCache.GetTableNameAsync(candidateParentTableName, cancellationToken).ConfigureAwait(false);
            Identifier? resolvedParentTableName = null;

            await parentTableNameOption
                .BindAsync(async parentTableName =>
                {
                    resolvedParentTableName = parentTableName;
                    if (string.Equals(fkey.Key.KeyType, Constants.PrimaryKeyType, StringComparison.Ordinal))
                    {
                        var primaryKey = await queryCache.GetPrimaryKeyAsync(parentTableName, cancellationToken).ConfigureAwait(false);
                        return primaryKey.ToAsync();
                    }

                    var parentKeyName = Identifier.CreateQualifiedIdentifier(fkey.Key.ParentKeyName);
                    var parentUniqueKeys = await queryCache.GetUniqueKeysAsync(parentTableName, cancellationToken).ConfigureAwait(false);
                    var parentUniqueKeyLookup = GetDatabaseKeyLookup(parentUniqueKeys);

                    return parentUniqueKeyLookup.ContainsKey(parentKeyName.LocalName)
                        ? OptionAsync<IDatabaseKey>.Some(parentUniqueKeyLookup[parentKeyName.LocalName])
                        : OptionAsync<IDatabaseKey>.None;
                })
                .Map(parentKey =>
                {
                    var childKeyName = Identifier.CreateQualifiedIdentifier(fkey.Key.ChildKeyName);
                    var childKeyColumns = fkey.Value
                        .Where(row => columnLookup.ContainsKey(row.ColumnName))
                        .OrderBy(static row => row.ConstraintColumnId)
                        .Select(row => columnLookup[row.ColumnName])
                        .ToList();

                    var isEnabled = !fkey.Key.IsDisabled;
                    var childKey = new SqlServerDatabaseKey(childKeyName, DatabaseKeyType.Foreign, childKeyColumns, isEnabled);

                    var deleteAction = ReferentialActionMapping[fkey.Key.DeleteAction];
                    var updateAction = ReferentialActionMapping[fkey.Key.UpdateAction];

                    return new DatabaseRelationalKey(tableName, childKey, resolvedParentTableName!, parentKey, deleteAction, updateAction);
                })
                .IfSome(relationalKey => result.Add(relationalKey))
                .ConfigureAwait(false);
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
                var autoIncrement = row.IdentityIncrement
                    .Match(
                        incr => row.IdentitySeed.Match(seed => new AutoIncrement(seed, incr), () => Option<IAutoIncrement>.None),
                        static () => Option<IAutoIncrement>.None
                    );
                var defaultValue = row.HasDefaultValue && row.DefaultValue != null
                    ? Option<string>.Some(row.DefaultValue)
                    : Option<string>.None;
                var computedColumnDefinition = !row.ComputedColumnDefinition.IsNullOrWhiteSpace()
                    ? Option<string>.Some(row.ComputedColumnDefinition)
                    : Option<string>.None;

                return row.IsComputed
                    ? new DatabaseComputedColumn(columnName, columnType, row.IsNullable, defaultValue, computedColumnDefinition)
                    : new DatabaseColumn(columnName, columnType, row.IsNullable, defaultValue, autoIncrement);
            })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
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
        ).ConfigureAwait(false);

        if (queryResult.Empty())
            return [];

        var triggers = queryResult.GroupAsDictionary(static row => new
        {
            row.TriggerName,
            row.Definition,
            row.IsInsteadOfTrigger,
            row.IsDisabled,
        }).ToList();
        if (triggers.Empty())
            return [];

        var result = new List<IDatabaseTrigger>(triggers.Count);
        foreach (var trig in triggers)
        {
            var triggerName = Identifier.CreateQualifiedIdentifier(trig.Key.TriggerName);
            var queryTiming = trig.Key.IsInsteadOfTrigger ? TriggerQueryTiming.InsteadOf : TriggerQueryTiming.After;
            var definition = trig.Key.Definition;
            var isEnabled = !trig.Key.IsDisabled;

            var events = TriggerEvent.None;
            foreach (var triggerEvent in trig.Value.Select(t => t.TriggerEvent))
            {
                if (string.Equals(triggerEvent, Constants.Insert, StringComparison.Ordinal))
                    events |= TriggerEvent.Insert;
                else if (string.Equals(triggerEvent, Constants.Update, StringComparison.Ordinal))
                    events |= TriggerEvent.Update;
                else if (string.Equals(triggerEvent, Constants.Delete, StringComparison.Ordinal))
                    events |= TriggerEvent.Delete;
                else
                    throw new UnsupportedTriggerEventException(tableName, triggerEvent);
            }

            var trigger = new DatabaseTrigger(triggerName, definition, queryTiming, events, isEnabled);
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
        public const string Delete = "DELETE";

        public const string Insert = "INSERT";

        public const string PrimaryKeyType = "PK";

        public const string Update = "UPDATE";
    }

    /// <summary>
    /// A query cache provider for SQL Server tables. Ensures that a given query only occurs at most once for a given query context.
    /// </summary>
    protected class SqlServerTableQueryCache
    {
        private readonly AsyncCache<Identifier, Option<Identifier>, SqlServerTableQueryCache> _tableNames;
        private readonly AsyncCache<Identifier, IReadOnlyList<IDatabaseColumn>, SqlServerTableQueryCache> _columns;
        private readonly AsyncCache<Identifier, Option<IDatabaseKey>, SqlServerTableQueryCache> _primaryKeys;
        private readonly AsyncCache<Identifier, IReadOnlyCollection<IDatabaseKey>, SqlServerTableQueryCache> _uniqueKeys;
        private readonly AsyncCache<Identifier, IReadOnlyCollection<IDatabaseRelationalKey>, SqlServerTableQueryCache> _foreignKeys;

        /// <summary>
        /// Initializes a new instance of the <see cref="SqlServerTableQueryCache"/> class.
        /// </summary>
        /// <param name="tableNameLoader">A table name cache.</param>
        /// <param name="columnLoader">A column cache.</param>
        /// <param name="primaryKeyLoader">A primary key cache.</param>
        /// <param name="uniqueKeyLoader">A unique key cache.</param>
        /// <param name="foreignKeyLoader">A foreign key cache.</param>
        /// <exception cref="ArgumentNullException">Thrown when any of <paramref name="tableNameLoader"/>, <paramref name="columnLoader"/>, <paramref name="primaryKeyLoader"/>, <paramref name="uniqueKeyLoader"/> or <paramref name="foreignKeyLoader"/> are <see langword="null" />.</exception>
        public SqlServerTableQueryCache(
            AsyncCache<Identifier, Option<Identifier>, SqlServerTableQueryCache> tableNameLoader,
            AsyncCache<Identifier, IReadOnlyList<IDatabaseColumn>, SqlServerTableQueryCache> columnLoader,
            AsyncCache<Identifier, Option<IDatabaseKey>, SqlServerTableQueryCache> primaryKeyLoader,
            AsyncCache<Identifier, IReadOnlyCollection<IDatabaseKey>, SqlServerTableQueryCache> uniqueKeyLoader,
            AsyncCache<Identifier, IReadOnlyCollection<IDatabaseRelationalKey>, SqlServerTableQueryCache> foreignKeyLoader
        )
        {
            _tableNames = tableNameLoader ?? throw new ArgumentNullException(nameof(tableNameLoader));
            _columns = columnLoader ?? throw new ArgumentNullException(nameof(columnLoader));
            _primaryKeys = primaryKeyLoader ?? throw new ArgumentNullException(nameof(primaryKeyLoader));
            _uniqueKeys = uniqueKeyLoader ?? throw new ArgumentNullException(nameof(uniqueKeyLoader));
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