using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LanguageExt;
using Nito.AsyncEx;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Exceptions;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Core.Utilities;
using SJP.Schematic.MySql.Queries;

namespace SJP.Schematic.MySql;

/// <summary>
/// A database table provider for MySQL.
/// </summary>
/// <seealso cref="IRelationalDatabaseTableProvider" />
public class MySqlRelationalDatabaseTableProvider : IRelationalDatabaseTableProvider
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MySqlRelationalDatabaseTableProvider"/> class.
    /// </summary>
    /// <param name="connection">A database connection.</param>
    /// <param name="identifierDefaults">Identifier defaults for the given database.</param>
    /// <exception cref="ArgumentNullException"><paramref name="connection"/> or <paramref name="identifierDefaults"/> are <see langword="null" />.</exception>
    public MySqlRelationalDatabaseTableProvider(ISchematicConnection connection, IIdentifierDefaults identifierDefaults)
    {
        Connection = connection ?? throw new ArgumentNullException(nameof(connection));
        IdentifierDefaults = identifierDefaults ?? throw new ArgumentNullException(nameof(identifierDefaults));

        _supportsChecks = new AsyncLazy<bool>(LoadHasCheckSupport);
    }

    /// <summary>
    /// A database connection that is specific to a given MySQL database.
    /// </summary>
    /// <value>A database connection.</value>
    protected ISchematicConnection Connection { get; }

    /// <summary>
    /// Identifier defaults for the associated database.
    /// </summary>
    /// <value>Identifier defaults.</value>
    protected IIdentifierDefaults IdentifierDefaults { get; }

    /// <summary>
    /// A database connection factory to query the database.
    /// </summary>
    /// <value>A connection factory.</value>
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
    protected MySqlTableQueryCache CreateQueryCache() => new(
        new AsyncCache<Identifier, Option<Identifier>, MySqlTableQueryCache>((tableName, _, token) => GetResolvedTableName(tableName, token)),
        new AsyncCache<Identifier, IReadOnlyList<IDatabaseColumn>, MySqlTableQueryCache>((tableName, _, token) => LoadColumnsAsync(tableName, token)),
        new AsyncCache<Identifier, Option<IDatabaseKey>, MySqlTableQueryCache>(LoadPrimaryKeyAsync),
        new AsyncCache<Identifier, IReadOnlyCollection<IDatabaseKey>, MySqlTableQueryCache>(LoadUniqueKeysAsync),
        new AsyncCache<Identifier, IReadOnlyCollection<IDatabaseRelationalKey>, MySqlTableQueryCache>(LoadParentKeysAsync)
    );

    /// <summary>
    /// Enumerates all database tables.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A collection of database tables.</returns>
    public IAsyncEnumerable<IRelationalDatabaseTable> GetAllTables(CancellationToken cancellationToken = default)
    {
        var queryCache = CreateQueryCache();

        return DbConnection.QueryEnumerableAsync(
                GetAllTableNames.Sql,
                new GetAllTableNames.Query { SchemaName = IdentifierDefaults.Schema! },
                cancellationToken
            )
            .Select(dto => Identifier.CreateQualifiedIdentifier(dto.SchemaName, dto.TableName))
            .Select(QualifyTableName)
            .SelectAwait(tableName => LoadTableAsyncCore(tableName, queryCache, cancellationToken).ToValue());
    }

    /// <summary>
    /// Gets all database tables.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A collection of database tables.</returns>
    public async Task<IReadOnlyCollection<IRelationalDatabaseTable>> GetAllTables2(CancellationToken cancellationToken = default)
    {
        var queryCache = CreateQueryCache();

        var tableNames = await DbConnection.QueryEnumerableAsync(
                GetAllTableNames.Sql,
                new GetAllTableNames.Query { SchemaName = IdentifierDefaults.Schema! },
                cancellationToken
            )
            .Select(dto => Identifier.CreateQualifiedIdentifier(dto.SchemaName, dto.TableName))
            .Select(QualifyTableName)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var tableTasks = tableNames
            .Select(tableName => LoadTableAsyncCore(tableName, queryCache, cancellationToken))
            .ToArray();

        await Task.WhenAll(tableTasks).ConfigureAwait(false);

        return tableTasks
            .Select(t => t.Result)
            .ToArray();
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
    protected OptionAsync<IRelationalDatabaseTable> LoadTable(Identifier tableName, MySqlTableQueryCache queryCache, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(tableName);
        ArgumentNullException.ThrowIfNull(queryCache);

        var candidateTableName = QualifyTableName(tableName);
        return GetResolvedTableName(candidateTableName, cancellationToken)
            .MapAsync(name => LoadTableAsyncCore(name, queryCache, cancellationToken));
    }

    private async Task<IRelationalDatabaseTable> LoadTableAsyncCore(Identifier tableName, MySqlTableQueryCache queryCache, CancellationToken cancellationToken)
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
            LoadIndexesAsync(tableName, queryCache, cancellationToken),
            queryCache.GetPrimaryKeyAsync(tableName, cancellationToken),
            queryCache.GetUniqueKeysAsync(tableName, cancellationToken),
            queryCache.GetForeignKeysAsync(tableName, cancellationToken),
            LoadChildKeysAsync(tableName, queryCache, cancellationToken)
        ).WhenAll().ConfigureAwait(false);

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
    protected Task<Option<IDatabaseKey>> LoadPrimaryKeyAsync(Identifier tableName, MySqlTableQueryCache queryCache, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(tableName);
        ArgumentNullException.ThrowIfNull(queryCache);

        return LoadPrimaryKeyAsyncCore(tableName, queryCache, cancellationToken);
    }

    private async Task<Option<IDatabaseKey>> LoadPrimaryKeyAsyncCore(Identifier tableName, MySqlTableQueryCache queryCache, CancellationToken cancellationToken)
    {
        var primaryKeyColumns = await DbConnection.QueryAsync(
            GetTablePrimaryKey.Sql,
            new GetTablePrimaryKey.Query { SchemaName = tableName.Schema!, TableName = tableName.LocalName },
            cancellationToken
        ).ConfigureAwait(false);

        if (primaryKeyColumns.Empty())
            return Option<IDatabaseKey>.None;

        var columns = await queryCache.GetColumnsAsync(tableName, cancellationToken).ConfigureAwait(false);
        var columnLookup = GetColumnLookup(columns);

        var groupedByName = primaryKeyColumns.GroupAsDictionary(static row => new { row.ConstraintName });
        var firstRow = groupedByName.First();
        var constraintName = firstRow.Key.ConstraintName;

        var keyColumns = groupedByName
            .Where(row => string.Equals(row.Key.ConstraintName, constraintName, StringComparison.Ordinal))
            .SelectMany(g => g.Value.ConvertAll(row => columnLookup[row.ColumnName!]))
            .ToList();

        var primaryKey = new MySqlDatabasePrimaryKey(keyColumns);
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
    protected Task<IReadOnlyCollection<IDatabaseIndex>> LoadIndexesAsync(Identifier tableName, MySqlTableQueryCache queryCache, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(tableName);
        ArgumentNullException.ThrowIfNull(queryCache);

        return LoadIndexesAsyncCore(tableName, queryCache, cancellationToken);
    }

    private async Task<IReadOnlyCollection<IDatabaseIndex>> LoadIndexesAsyncCore(Identifier tableName, MySqlTableQueryCache queryCache, CancellationToken cancellationToken)
    {
        var queryResult = await DbConnection.QueryAsync(
            GetTableIndexes.Sql,
            new GetTableIndexes.Query { SchemaName = tableName.Schema!, TableName = tableName.LocalName },
            cancellationToken
        ).ConfigureAwait(false);

        if (queryResult.Empty())
            return [];

        var indexColumns = queryResult.GroupAsDictionary(static row => new { row.IndexName, row.IsNonUnique }).ToList();
        if (indexColumns.Empty())
            return [];

        var columns = await queryCache.GetColumnsAsync(tableName, cancellationToken).ConfigureAwait(false);
        var columnLookup = GetColumnLookup(columns);

        var result = new List<IDatabaseIndex>(indexColumns.Count);

        foreach (var indexInfo in indexColumns)
        {
            var isUnique = !indexInfo.Key.IsNonUnique;
            var indexName = Identifier.CreateQualifiedIdentifier(indexInfo.Key.IndexName);

            var indexCols = indexInfo.Value
                .OrderBy(static row => row.ColumnOrdinal)
                .Select(row => columnLookup[row.ColumnName])
                .Select(col =>
                {
                    var expression = Dialect.QuoteName(col.Name);
                    return new MySqlDatabaseIndexColumn(expression, col);
                })
                .ToList();

            var index = new MySqlDatabaseIndex(indexName, isUnique, indexCols);
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
    protected Task<IReadOnlyCollection<IDatabaseKey>> LoadUniqueKeysAsync(Identifier tableName, MySqlTableQueryCache queryCache, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(tableName);
        ArgumentNullException.ThrowIfNull(queryCache);

        return LoadUniqueKeysAsyncCore(tableName, queryCache, cancellationToken);
    }

    private async Task<IReadOnlyCollection<IDatabaseKey>> LoadUniqueKeysAsyncCore(Identifier tableName, MySqlTableQueryCache queryCache, CancellationToken cancellationToken)
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

        var groupedByName = uniqueKeyColumns.GroupAsDictionary(static row => new { row.ConstraintName });
        var constraintColumns = groupedByName
            .Select(g => new
            {
                g.Key.ConstraintName,
                Columns = g.Value.ConvertAll(row => columnLookup[row.ColumnName!]),
            })
            .ToList();
        if (constraintColumns.Empty())
            return [];

        var result = new List<IDatabaseKey>(constraintColumns.Count);
        foreach (var uk in constraintColumns)
        {
            var uniqueKey = new MySqlDatabaseKey(uk.ConstraintName, DatabaseKeyType.Unique, uk.Columns);
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
    protected Task<IReadOnlyCollection<IDatabaseRelationalKey>> LoadChildKeysAsync(Identifier tableName, MySqlTableQueryCache queryCache, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(tableName);
        ArgumentNullException.ThrowIfNull(queryCache);

        return LoadChildKeysAsyncCore(tableName, queryCache, cancellationToken);
    }

    private async Task<IReadOnlyCollection<IDatabaseRelationalKey>> LoadChildKeysAsyncCore(Identifier tableName, MySqlTableQueryCache queryCache, CancellationToken cancellationToken)
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

        var uniqueKeys = await queryCache.GetUniqueKeysAsync(tableName, cancellationToken).ConfigureAwait(false);
        var uniqueKeyLookup = GetDatabaseKeyLookup(uniqueKeys);

        var result = new List<IDatabaseRelationalKey>(groupedChildKeys.Count);

        foreach (var groupedChildKey in groupedChildKeys.Select(ck => ck.Key))
        {
            // ensure we have a key to begin with
            IDatabaseKey? parentKey = null;
            if (string.Equals(groupedChildKey.ParentKeyType, Constants.PrimaryKey, StringComparison.Ordinal))
            {
                var pk = await queryCache.GetPrimaryKeyAsync(tableName, cancellationToken).ConfigureAwait(false);
                pk.IfSome(k => parentKey = k);
            }
            else if (uniqueKeyLookup.ContainsKey(groupedChildKey.ParentKeyName))
            {
                parentKey = uniqueKeyLookup[groupedChildKey.ParentKeyName];
            }

            if (parentKey == null)
                continue;

            var candidateChildTableName = Identifier.CreateQualifiedIdentifier(groupedChildKey.ChildTableSchema, groupedChildKey.ChildTableName);
            var resolvedName = await queryCache.GetTableNameAsync(candidateChildTableName, cancellationToken).ConfigureAwait(false);

            await resolvedName
                .BindAsync(async name =>
                {
                    var childKeyName = Identifier.CreateQualifiedIdentifier(groupedChildKey.ChildKeyName);

                    var parentKeys = await queryCache.GetForeignKeysAsync(name, cancellationToken).ConfigureAwait(false);
                    var parentKeyLookup = GetDatabaseKeyLookup(parentKeys.Select(fk => fk.ChildKey).ToList());
                    if (!parentKeyLookup.TryGetValue(childKeyName, out var childKey))
                        return OptionAsync<IDatabaseRelationalKey>.None;

                    var deleteAction = ReferentialActionMapping[groupedChildKey.DeleteAction];
                    var updateAction = ReferentialActionMapping[groupedChildKey.UpdateAction];
                    var relationalKey = new MySqlRelationalKey(name, childKey, tableName, parentKey, deleteAction, updateAction);

                    return OptionAsync<IDatabaseRelationalKey>.Some(relationalKey);
                })
                .IfSome(key => result.Add(key))
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
    /// <exception cref="ArgumentNullException"><paramref name="tableName"/> is <see langword="null" />.</exception>
    protected Task<IReadOnlyCollection<IDatabaseCheckConstraint>> LoadChecksAsync(Identifier tableName, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(tableName);

        return LoadChecksAsyncCore(tableName, cancellationToken);
    }

    private async Task<IReadOnlyCollection<IDatabaseCheckConstraint>> LoadChecksAsyncCore(Identifier tableName, CancellationToken cancellationToken)
    {
        var hasCheckSupport = await _supportsChecks.ConfigureAwait(false);
        if (!hasCheckSupport)
            return [];

        return await DbConnection.QueryEnumerableAsync(
                GetTableCheckConstraints.Sql,
                new GetTableCheckConstraints.Query { SchemaName = tableName.Schema!, TableName = tableName.LocalName },
                cancellationToken
            )
            .Select(row =>
            {
                var checkName = Identifier.CreateQualifiedIdentifier(row.ConstraintName);
                var isEnabled = string.Equals("YES", row.Enforced, StringComparison.OrdinalIgnoreCase);
                return new MySqlCheckConstraint(checkName, row.Definition, isEnabled);
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
    protected Task<IReadOnlyCollection<IDatabaseRelationalKey>> LoadParentKeysAsync(Identifier tableName, MySqlTableQueryCache queryCache, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(tableName);
        ArgumentNullException.ThrowIfNull(queryCache);

        return LoadParentKeysAsyncCore(tableName, queryCache, cancellationToken);
    }

    private async Task<IReadOnlyCollection<IDatabaseRelationalKey>> LoadParentKeysAsyncCore(Identifier tableName, MySqlTableQueryCache queryCache, CancellationToken cancellationToken)
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
        }).ToList();
        if (foreignKeys.Empty())
            return [];

        var columns = await queryCache.GetColumnsAsync(tableName, cancellationToken).ConfigureAwait(false);
        var columnLookup = GetColumnLookup(columns);

        var result = new List<IDatabaseRelationalKey>(foreignKeys.Count);
        foreach (var fkey in foreignKeys)
        {
            var candidateParentTableName = Identifier.CreateQualifiedIdentifier(fkey.Key.ParentTableSchema, fkey.Key.ParentTableName);
            var resolvedName = await queryCache.GetTableNameAsync(candidateParentTableName, cancellationToken).ConfigureAwait(false);

            Identifier? parentTableName = null;

            await resolvedName
                .BindAsync(async name =>
                {
                    parentTableName = name;
                    if (string.Equals(fkey.Key.KeyType, Constants.PrimaryKey, StringComparison.Ordinal))
                    {
                        var primaryKey = await queryCache.GetPrimaryKeyAsync(name, cancellationToken).ConfigureAwait(false);
                        return primaryKey.ToAsync();
                    }

                    var parentKeyName = Identifier.CreateQualifiedIdentifier(fkey.Key.ParentKeyName);

                    var parentUniqueKeys = await queryCache.GetUniqueKeysAsync(name, cancellationToken).ConfigureAwait(false);
                    var parentUniqueKeyLookup = GetDatabaseKeyLookup(parentUniqueKeys);
                    return parentUniqueKeyLookup.ContainsKey(parentKeyName.LocalName)
                        ? OptionAsync<IDatabaseKey>.Some(parentUniqueKeyLookup[parentKeyName.LocalName])
                        : OptionAsync<IDatabaseKey>.None;
                })
                .IfSome(key =>
                {
                    var childKeyName = Identifier.CreateQualifiedIdentifier(fkey.Key.ChildKeyName);
                    var childKeyColumns = fkey.Value
                        .OrderBy(static row => row.ConstraintColumnId)
                        .Select(row => columnLookup[row.ColumnName!])
                        .ToList();

                    var childKey = new MySqlDatabaseKey(childKeyName, DatabaseKeyType.Foreign, childKeyColumns);

                    var deleteAction = ReferentialActionMapping[fkey.Key.DeleteAction];
                    var updateAction = ReferentialActionMapping[fkey.Key.UpdateAction];

                    var relationalKey = new DatabaseRelationalKey(tableName, childKey, parentTableName!, key, deleteAction, updateAction);
                    result.Add(relationalKey);
                })
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
                    TypeName = Identifier.CreateQualifiedIdentifier(row.DataTypeName),
                    Collation = !row.Collation.IsNullOrWhiteSpace()
                    ? Option<Identifier>.Some(Identifier.CreateQualifiedIdentifier(row.Collation))
                    : Option<Identifier>.None,
                    MaxLength = row.CharacterMaxLength,
                    NumericPrecision = new NumericPrecision(row.Precision, row.Scale),
                };
                var columnType = Dialect.TypeProvider.CreateColumnType(typeMetadata);

                var columnName = Identifier.CreateQualifiedIdentifier(row.ColumnName);
                var isAutoIncrement = row.ExtraInformation?.Contains(Constants.AutoIncrement, StringComparison.OrdinalIgnoreCase) == true;
                var autoIncrement = isAutoIncrement
                    ? Option<IAutoIncrement>.Some(new AutoIncrement(1, 1))
                    : Option<IAutoIncrement>.None;
                var isComputed = !row.ComputedColumnDefinition.IsNullOrWhiteSpace();
                var isNullable = !string.Equals(row.IsNullable, Constants.No, StringComparison.OrdinalIgnoreCase);
                var defaultValue = !row.DefaultValue.IsNullOrWhiteSpace()
                    ? Option<string>.Some(row.DefaultValue)
                    : Option<string>.None;
                var computedColumnDefinition = isComputed
                    ? Option<string>.Some(row.ComputedColumnDefinition!)
                    : Option<string>.None;

                return isComputed
                    ? new DatabaseComputedColumn(columnName, columnType, isNullable, defaultValue, computedColumnDefinition)
                    : new DatabaseColumn(columnName, columnType, isNullable, defaultValue, autoIncrement) as IDatabaseColumn;
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
            row.Timing,
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
            foreach (var trigEvent in trig.Value.Select(tr => tr.TriggerEvent))
            {
                if (string.Equals(trigEvent, Constants.Insert, StringComparison.Ordinal))
                    events |= TriggerEvent.Insert;
                else if (string.Equals(trigEvent, Constants.Update, StringComparison.Ordinal))
                    events |= TriggerEvent.Update;
                else if (string.Equals(trigEvent, Constants.Delete, StringComparison.Ordinal))
                    events |= TriggerEvent.Delete;
                else
                    throw new UnsupportedTriggerEventException(tableName, trigEvent ?? string.Empty);
            }

            var trigger = new MySqlDatabaseTrigger(triggerName, definition, queryTiming, events);
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
    /// A mapping from the referential actions as described in MySQL, to a <see cref="ReferentialAction"/> instance.
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
        {
            key.Name.IfSome(name => result[name.LocalName] = key);
        }

        return result;
    }

    private Task<bool> LoadHasCheckSupport()
    {
        const string sql = "select count(*) from information_schema.tables where table_schema = 'information_schema' and table_name = 'CHECK_CONSTRAINTS'";
        return DbConnection.ExecuteScalarAsync<bool>(sql, CancellationToken.None);
    }

    private readonly AsyncLazy<bool> _supportsChecks;

    private static class Constants
    {
        public const string AutoIncrement = "auto_increment";

        public const string Delete = "DELETE";

        public const string Insert = "INSERT";

        public const string No = "NO";

        public const string PrimaryKey = "PRIMARY KEY";

        public const string Update = "UPDATE";
    }

    /// <summary>
    /// A query cache provider for MySQL tables. Ensures that a given query only occurs at most once for a given query context.
    /// </summary>
    protected class MySqlTableQueryCache
    {
        private readonly AsyncCache<Identifier, Option<Identifier>, MySqlTableQueryCache> _tableNames;
        private readonly AsyncCache<Identifier, IReadOnlyList<IDatabaseColumn>, MySqlTableQueryCache> _columns;
        private readonly AsyncCache<Identifier, Option<IDatabaseKey>, MySqlTableQueryCache> _primaryKeys;
        private readonly AsyncCache<Identifier, IReadOnlyCollection<IDatabaseKey>, MySqlTableQueryCache> _uniqueKeys;
        private readonly AsyncCache<Identifier, IReadOnlyCollection<IDatabaseRelationalKey>, MySqlTableQueryCache> _foreignKeys;

        /// <summary>
        /// Initializes a new instance of the <see cref="MySqlTableQueryCache"/> class.
        /// </summary>
        /// <param name="tableNameLoader">A table name cache.</param>
        /// <param name="columnLoader">A column cache.</param>
        /// <param name="primaryKeyLoader">A primary key cache.</param>
        /// <param name="uniqueKeyLoader">A unique key cache.</param>
        /// <param name="foreignKeyLoader">A foreign key cache.</param>
        /// <exception cref="ArgumentNullException">Thrown when any of <paramref name="tableNameLoader"/>, <paramref name="columnLoader"/>, <paramref name="primaryKeyLoader"/>, <paramref name="uniqueKeyLoader"/> or <paramref name="foreignKeyLoader"/> are <see langword="null" />.</exception>
        public MySqlTableQueryCache(
            AsyncCache<Identifier, Option<Identifier>, MySqlTableQueryCache> tableNameLoader,
            AsyncCache<Identifier, IReadOnlyList<IDatabaseColumn>, MySqlTableQueryCache> columnLoader,
            AsyncCache<Identifier, Option<IDatabaseKey>, MySqlTableQueryCache> primaryKeyLoader,
            AsyncCache<Identifier, IReadOnlyCollection<IDatabaseKey>, MySqlTableQueryCache> uniqueKeyLoader,
            AsyncCache<Identifier, IReadOnlyCollection<IDatabaseRelationalKey>, MySqlTableQueryCache> foreignKeyLoader
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