using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LanguageExt;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Exceptions;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Core.Utilities;
using SJP.Schematic.MySql.Query;

namespace SJP.Schematic.MySql
{
    public class MySqlRelationalDatabaseTableProvider : IRelationalDatabaseTableProvider
    {
        public MySqlRelationalDatabaseTableProvider(IDbConnection connection, IIdentifierDefaults identifierDefaults, IDbTypeProvider typeProvider)
        {
            Connection = connection ?? throw new ArgumentNullException(nameof(connection));
            IdentifierDefaults = identifierDefaults ?? throw new ArgumentNullException(nameof(identifierDefaults));
            TypeProvider = typeProvider ?? throw new ArgumentNullException(nameof(typeProvider));
            Dialect = new MySqlDialect(connection);

            _supportsChecks = new AsyncLazy<bool>(LoadHasCheckSupport);
        }

        protected IDbConnection Connection { get; }

        protected IIdentifierDefaults IdentifierDefaults { get; }

        protected IDbTypeProvider TypeProvider { get; }

        protected IDatabaseDialect Dialect { get; }

        protected Task<bool> HasCheckSupport => _supportsChecks.Task;

        public virtual async Task<IReadOnlyCollection<IRelationalDatabaseTable>> GetAllTables(CancellationToken cancellationToken = default(CancellationToken))
        {
            var queryResults = await Connection.QueryAsync<QualifiedName>(TablesQuery, new { SchemaName = IdentifierDefaults.Schema }, cancellationToken).ConfigureAwait(false);
            var tableNames = queryResults
                .Select(dto => Identifier.CreateQualifiedIdentifier(dto.SchemaName, dto.ObjectName))
                .Select(QualifyTableName)
                .ToList();

            var tables = new List<IRelationalDatabaseTable>();

            foreach (var tableName in tableNames)
            {
                var table = await LoadTableAsyncCore(tableName, cancellationToken).ConfigureAwait(false);
                tables.Add(table);
            }

            return tables;
        }

        protected virtual string TablesQuery => TablesQuerySql;

        private const string TablesQuerySql = @"
select
    TABLE_SCHEMA as SchemaName,
    TABLE_NAME as ObjectName
from information_schema.tables
where TABLE_SCHEMA = @SchemaName order by TABLE_NAME";

        public OptionAsync<IRelationalDatabaseTable> GetTable(Identifier tableName, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));

            var candidateTableName = QualifyTableName(tableName);
            return LoadTable(candidateTableName, cancellationToken);
        }

        protected OptionAsync<Identifier> GetResolvedTableName(Identifier tableName, CancellationToken cancellationToken)
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));

            tableName = QualifyTableName(tableName);
            var qualifiedTableName = Connection.QueryFirstOrNone<QualifiedName>(
                TableNameQuery,
                new { SchemaName = tableName.Schema, TableName = tableName.LocalName },
                cancellationToken
            );

            return qualifiedTableName.Map(name => Identifier.CreateQualifiedIdentifier(tableName.Server, tableName.Database, name.SchemaName, name.ObjectName));
        }

        protected virtual string TableNameQuery => TableNameQuerySql;

        private const string TableNameQuerySql = @"
select table_schema as SchemaName, table_name as ObjectName
from information_schema.tables
where table_schema = @SchemaName and table_name = @TableName
limit 1";

        protected virtual OptionAsync<IRelationalDatabaseTable> LoadTable(Identifier tableName, CancellationToken cancellationToken)
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));

            var candidateTableName = QualifyTableName(tableName);
            return GetResolvedTableName(candidateTableName, cancellationToken)
                .MapAsync(name => LoadTableAsyncCore(name, cancellationToken));
        }

        private async Task<IRelationalDatabaseTable> LoadTableAsyncCore(Identifier tableName, CancellationToken cancellationToken)
        {
            var columns = await LoadColumnsAsync(tableName, cancellationToken).ConfigureAwait(false);
            var columnLookup = GetColumnLookup(columns);
            var checks = await LoadChecksAsync(tableName, cancellationToken).ConfigureAwait(false);
            var triggers = await LoadTriggersAsync(tableName, cancellationToken).ConfigureAwait(false);

            var primaryKey = await LoadPrimaryKeyAsync(tableName, columnLookup, cancellationToken).ConfigureAwait(false);
            var uniqueKeys = await LoadUniqueKeysAsync(tableName, columnLookup, cancellationToken).ConfigureAwait(false);
            var uniqueKeyLookup = GetDatabaseKeyLookup(uniqueKeys);
            var indexes = await LoadIndexesAsync(tableName, columnLookup, cancellationToken).ConfigureAwait(false);

            var childKeys = await LoadChildKeysAsync(tableName, columnLookup, primaryKey, uniqueKeyLookup, cancellationToken).ConfigureAwait(false);
            var parentKeys = await LoadParentKeysAsync(tableName, columnLookup, cancellationToken).ConfigureAwait(false);

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

        protected virtual Task<Option<IDatabaseKey>> LoadPrimaryKeyAsync(Identifier tableName, IReadOnlyDictionary<Identifier, IDatabaseColumn> columns, CancellationToken cancellationToken)
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));
            if (columns == null)
                throw new ArgumentNullException(nameof(columns));

            return LoadPrimaryKeyAsyncCore(tableName, columns, cancellationToken);
        }

        private async Task<Option<IDatabaseKey>> LoadPrimaryKeyAsyncCore(Identifier tableName, IReadOnlyDictionary<Identifier, IDatabaseColumn> columns, CancellationToken cancellationToken)
        {
            var primaryKeyColumns = await Connection.QueryAsync<ConstraintColumnMapping>(
                PrimaryKeyQuery,
                new { SchemaName = tableName.Schema, TableName = tableName.LocalName },
                cancellationToken
            ).ConfigureAwait(false);

            if (primaryKeyColumns.Empty())
                return Option<IDatabaseKey>.None;

            var groupedByName = primaryKeyColumns.GroupBy(row => new { row.ConstraintName });
            var firstRow = groupedByName.First();
            var constraintName = firstRow.Key.ConstraintName;

            var keyColumns = groupedByName
                .Where(row => row.Key.ConstraintName == constraintName)
                .SelectMany(g => g.Select(row => columns[row.ColumnName]))
                .ToList();

            var primaryKey = new MySqlDatabasePrimaryKey(keyColumns);
            return Option<IDatabaseKey>.Some(primaryKey);
        }

        protected virtual string PrimaryKeyQuery => PrimaryKeyQuerySql;

        private const string PrimaryKeyQuerySql = @"
select
    kc.constraint_name as ConstraintName,
    kc.column_name as ColumnName
from information_schema.tables t
inner join information_schema.table_constraints tc on t.table_schema = tc.table_schema and t.table_name = tc.table_name
inner join information_schema.key_column_usage kc
    on t.table_schema = kc.table_schema
    and t.table_name = kc.table_name
    and tc.constraint_schema = kc.constraint_schema
    and tc.constraint_name = kc.constraint_name
where t.table_schema = @SchemaName and t.table_name = @TableName
    and tc.constraint_type = 'PRIMARY KEY'
order by kc.ordinal_position";

        protected virtual Task<IReadOnlyCollection<IDatabaseIndex>> LoadIndexesAsync(Identifier tableName, IReadOnlyDictionary<Identifier, IDatabaseColumn> columns, CancellationToken cancellationToken)
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));
            if (columns == null)
                throw new ArgumentNullException(nameof(columns));

            return LoadIndexesAsyncCore(tableName, columns, cancellationToken);
        }

        private async Task<IReadOnlyCollection<IDatabaseIndex>> LoadIndexesAsyncCore(Identifier tableName, IReadOnlyDictionary<Identifier, IDatabaseColumn> columns, CancellationToken cancellationToken)
        {
            var queryResult = await Connection.QueryAsync<IndexColumns>(
                IndexesQuery,
                new { SchemaName = tableName.Schema, TableName = tableName.LocalName },
                cancellationToken
            ).ConfigureAwait(false);

            if (queryResult.Empty())
                return Array.Empty<IDatabaseIndex>();

            var indexColumns = queryResult.GroupBy(row => new { row.IndexName, row.IsNonUnique }).ToList();
            if (indexColumns.Empty())
                return Array.Empty<IDatabaseIndex>();

            var result = new List<IDatabaseIndex>(indexColumns.Count);

            foreach (var indexInfo in indexColumns)
            {
                var isUnique = !indexInfo.Key.IsNonUnique;
                var indexName = Identifier.CreateQualifiedIdentifier(indexInfo.Key.IndexName);

                var indexCols = indexInfo
                    .OrderBy(row => row.ColumnOrdinal)
                    .Select(row => columns[row.ColumnName])
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

        protected virtual string IndexesQuery => IndexesQuerySql;

        private const string IndexesQuerySql = @"
select
    index_name as IndexName,
    non_unique as IsNonUnique,
    seq_in_index as ColumnOrdinal,
    column_name as ColumnName
from information_schema.statistics
where table_schema = @SchemaName and table_name = @TableName";

        protected virtual Task<IReadOnlyCollection<IDatabaseKey>> LoadUniqueKeysAsync(Identifier tableName, IReadOnlyDictionary<Identifier, IDatabaseColumn> columns, CancellationToken cancellationToken)
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));
            if (columns == null)
                throw new ArgumentNullException(nameof(columns));

            return LoadUniqueKeysAsyncCore(tableName, columns, cancellationToken);
        }

        private async Task<IReadOnlyCollection<IDatabaseKey>> LoadUniqueKeysAsyncCore(Identifier tableName, IReadOnlyDictionary<Identifier, IDatabaseColumn> columns, CancellationToken cancellationToken)
        {
            var uniqueKeyColumns = await Connection.QueryAsync<ConstraintColumnMapping>(
                UniqueKeysQuery,
                new { SchemaName = tableName.Schema, TableName = tableName.LocalName },
                cancellationToken
            ).ConfigureAwait(false);

            if (uniqueKeyColumns.Empty())
                return Array.Empty<IDatabaseKey>();

            var groupedByName = uniqueKeyColumns.GroupBy(row => new { row.ConstraintName });
            var constraintColumns = groupedByName
                .Select(g => new
                {
                    g.Key.ConstraintName,
                    Columns = g.Select(row => columns[row.ColumnName]).ToList(),
                })
                .ToList();
            if (constraintColumns.Empty())
                return Array.Empty<IDatabaseKey>();

            var result = new List<IDatabaseKey>(constraintColumns.Count);
            foreach (var uk in constraintColumns)
            {
                var uniqueKey = new MySqlDatabaseKey(uk.ConstraintName, DatabaseKeyType.Unique, uk.Columns);
                result.Add(uniqueKey);
            }
            return result;
        }

        protected virtual string UniqueKeysQuery => UniqueKeysQuerySql;

        private const string UniqueKeysQuerySql = @"
select
    kc.constraint_name as ConstraintName,
    kc.column_name as ColumnName
from information_schema.tables t
inner join information_schema.table_constraints tc on t.table_schema = tc.table_schema and t.table_name = tc.table_name
inner join information_schema.key_column_usage kc
    on t.table_schema = kc.table_schema
    and t.table_name = kc.table_name
    and tc.constraint_schema = kc.constraint_schema
    and tc.constraint_name = kc.constraint_name
where t.table_schema = @SchemaName and t.table_name = @TableName
    and tc.constraint_type = 'UNIQUE'
order by kc.ordinal_position";

        protected virtual Task<IReadOnlyCollection<IDatabaseRelationalKey>> LoadChildKeysAsync(Identifier tableName, IReadOnlyDictionary<Identifier, IDatabaseColumn> columns, Option<IDatabaseKey> primaryKey, IReadOnlyDictionary<Identifier, IDatabaseKey> uniqueKeys, CancellationToken cancellationToken)
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));
            if (columns == null)
                throw new ArgumentNullException(nameof(columns));
            if (uniqueKeys == null)
                throw new ArgumentNullException(nameof(uniqueKeys));

            return LoadChildKeysAsyncCore(tableName, columns, primaryKey, uniqueKeys, cancellationToken);
        }

        private async Task<IReadOnlyCollection<IDatabaseRelationalKey>> LoadChildKeysAsyncCore(Identifier tableName, IReadOnlyDictionary<Identifier, IDatabaseColumn> columns, Option<IDatabaseKey> primaryKey, IReadOnlyDictionary<Identifier, IDatabaseKey> uniqueKeys, CancellationToken cancellationToken)
        {
            var queryResult = await Connection.QueryAsync<ChildKeyData>(
                ChildKeysQuery,
                new { SchemaName = tableName.Schema, TableName = tableName.LocalName },
                cancellationToken
            ).ConfigureAwait(false);

            if (queryResult.Empty())
                return Array.Empty<IDatabaseRelationalKey>();

            var groupedChildKeys = queryResult.GroupBy(row =>
            new
            {
                row.ChildTableSchema,
                row.ChildTableName,
                row.ChildKeyName,
                row.ParentKeyName,
                row.ParentKeyType,
                row.DeleteRule,
                row.UpdateRule
            }).ToList();
            if (groupedChildKeys.Empty())
                return Array.Empty<IDatabaseRelationalKey>();

            var tableNameCache = new Dictionary<Identifier, Identifier> { [Identifier.CreateQualifiedIdentifier(tableName.Schema, tableName.LocalName)] = tableName };
            var columnLookupsCache = new Dictionary<Identifier, IReadOnlyDictionary<Identifier, IDatabaseColumn>> { [tableName] = columns };
            var foreignKeyLookupCache = new Dictionary<Identifier, IReadOnlyDictionary<Identifier, IDatabaseKey>>();

            var result = new List<IDatabaseRelationalKey>(groupedChildKeys.Count);

            foreach (var groupedChildKey in groupedChildKeys)
            {
                // ensure we have a key to begin with
                IDatabaseKey parentKey = null;
                if (groupedChildKey.Key.ParentKeyType == Constants.PrimaryKey)
                    primaryKey.IfSome(k => parentKey = k);
                else if (uniqueKeys.ContainsKey(groupedChildKey.Key.ParentKeyName))
                    parentKey = uniqueKeys[groupedChildKey.Key.ParentKeyName];
                if (parentKey == null)
                    continue;

                var candidateChildTableName = Identifier.CreateQualifiedIdentifier(groupedChildKey.Key.ChildTableSchema, groupedChildKey.Key.ChildTableName);
                var resolvedName = tableNameCache.ContainsKey(candidateChildTableName)
                    ? OptionAsync<Identifier>.Some(tableNameCache[candidateChildTableName])
                    : GetResolvedTableName(candidateChildTableName, cancellationToken);

                await resolvedName
                    .BindAsync(async name =>
                    {
                        tableNameCache[candidateChildTableName] = name;
                        var childKeyName = Identifier.CreateQualifiedIdentifier(groupedChildKey.Key.ChildKeyName);

                        if (!columnLookupsCache.TryGetValue(name, out var childKeyColumnLookup))
                        {
                            var childKeyColumns = await LoadColumnsAsync(name, cancellationToken).ConfigureAwait(false);
                            childKeyColumnLookup = GetColumnLookup(childKeyColumns);
                            columnLookupsCache[tableName] = childKeyColumnLookup;
                        }

                        if (!foreignKeyLookupCache.TryGetValue(name, out var parentKeyLookup))
                        {
                            var parentKeys = await LoadParentKeysAsync(name, childKeyColumnLookup, cancellationToken).ConfigureAwait(false);
                            parentKeyLookup = GetDatabaseKeyLookup(parentKeys.Select(fk => fk.ChildKey).ToList());
                            foreignKeyLookupCache[tableName] = parentKeyLookup;
                        }

                        if (!parentKeyLookup.TryGetValue(childKeyName, out var childKey))
                            return OptionAsync<IDatabaseRelationalKey>.None;

                        var deleteRule = RelationalRuleMapping[groupedChildKey.Key.DeleteRule];
                        var updateRule = RelationalRuleMapping[groupedChildKey.Key.UpdateRule];
                        var relationalKey = new MySqlRelationalKey(name, childKey, tableName, parentKey, deleteRule, updateRule);

                        return OptionAsync<IDatabaseRelationalKey>.Some(relationalKey);
                    })
                    .IfSome(key => result.Add(key))
                    .ConfigureAwait(false);
            }

            return result;
        }

        protected virtual string ChildKeysQuery => ChildKeysQuerySql;

        private const string ChildKeysQuerySql = @"
select
    t.table_schema as ChildTableSchema,
    t.table_name as ChildTableName,
    pt.table_schema as ParentTableSchema,
    pt.table_name as ParentTableName,
    rc.constraint_name as ChildKeyName,
    rc.unique_constraint_name as ParentKeyName,
    ptc.constraint_type as ParentKeyType,
    rc.delete_rule as DeleteRule,
    rc.update_rule as UpdateRule
from information_schema.tables t
inner join information_schema.referential_constraints rc on t.table_schema = rc.constraint_schema and t.table_name = rc.table_name
inner join information_schema.key_column_usage kc on t.table_schema = kc.table_schema and t.table_name = kc.table_name
inner join information_schema.tables pt on pt.table_schema = rc.unique_constraint_schema and pt.table_name = rc.referenced_table_name
inner join information_schema.table_constraints ptc on pt.table_schema = ptc.table_schema and pt.table_name = ptc.table_name and ptc.constraint_name = rc.unique_constraint_name
where pt.table_schema = @SchemaName and pt.table_name = @TableName";

        protected virtual Task<IReadOnlyCollection<IDatabaseCheckConstraint>> LoadChecksAsync(Identifier tableName, CancellationToken cancellationToken)
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));

            return Empty.Checks;
        }

        protected async Task<IReadOnlyCollection<IDatabaseCheckConstraint>> LoadChecksAsyncCore(Identifier tableName, CancellationToken cancellationToken)
        {
            var hasCheckSupport = await HasCheckSupport.ConfigureAwait(false);
            if (!hasCheckSupport)
                return Array.Empty<IDatabaseCheckConstraint>();

            var queryResult = await Connection.QueryAsync<CheckData>(
                ChecksQuery,
                new { SchemaName = tableName.Schema, TableName = tableName.LocalName },
                cancellationToken
            ).ConfigureAwait(false);
            if (queryResult.Empty())
                return Array.Empty<IDatabaseCheckConstraint>();

            var result = new List<IDatabaseCheckConstraint>();

            foreach (var row in queryResult)
            {
                var checkName = Identifier.CreateQualifiedIdentifier(row.ConstraintName);
                var isEnabled = string.Equals("YES", row.Enforced, StringComparison.OrdinalIgnoreCase);
                var check = new MySqlCheckConstraint(checkName, row.Definition, isEnabled);
                result.Add(check);
            }

            return result;
        }

        protected virtual string ChecksQuery => ChecksQuerySql;

        private const string ChecksQuerySql = @"
select
    cc.constraint_name as ConstraintName,
    cc.check_clause as Definition,
    tc.enforced as Enforced
from information_schema.table_constraints tc
inner join information_schema.check_constraints cc on tc.table_schema = cc.constraint_schema and tc.constraint_name = cc.constraint_name
where tc.table_schema = @SchemaName and tc.table_name = @TableName and tc.constraint_type = 'CHECK'";

        protected virtual Task<IReadOnlyCollection<IDatabaseRelationalKey>> LoadParentKeysAsync(Identifier tableName, IReadOnlyDictionary<Identifier, IDatabaseColumn> columns, CancellationToken cancellationToken)
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));
            if (columns == null)
                throw new ArgumentNullException(nameof(columns));

            return LoadParentKeysAsyncCore(tableName, columns, cancellationToken);
        }

        private async Task<IReadOnlyCollection<IDatabaseRelationalKey>> LoadParentKeysAsyncCore(Identifier tableName, IReadOnlyDictionary<Identifier, IDatabaseColumn> columns, CancellationToken cancellationToken)
        {
            var queryResult = await Connection.QueryAsync<ForeignKeyData>(
                ParentKeysQuery,
                new { SchemaName = tableName.Schema, TableName = tableName.LocalName },
                cancellationToken
            ).ConfigureAwait(false);

            if (queryResult.Empty())
                return Array.Empty<IDatabaseRelationalKey>();

            var foreignKeys = queryResult.GroupBy(row => new
            {
                row.ChildKeyName,
                row.ParentTableSchema,
                row.ParentTableName,
                row.ParentKeyName,
                KeyType = row.ParentKeyType,
                row.DeleteRule,
                row.UpdateRule,
            }).ToList();
            if (foreignKeys.Empty())
                return Array.Empty<IDatabaseRelationalKey>();

            var tableNameCache = new Dictionary<Identifier, Identifier> { [Identifier.CreateQualifiedIdentifier(tableName.Schema, tableName.LocalName)] = tableName };
            var columnLookupsCache = new Dictionary<Identifier, IReadOnlyDictionary<Identifier, IDatabaseColumn>> { [tableName] = columns };
            var primaryKeyCache = new Dictionary<Identifier, Option<IDatabaseKey>>();
            var uniqueKeyLookupCache = new Dictionary<Identifier, IReadOnlyDictionary<Identifier, IDatabaseKey>>();

            var result = new List<IDatabaseRelationalKey>(foreignKeys.Count);
            foreach (var fkey in foreignKeys)
            {
                var candidateParentTableName = Identifier.CreateQualifiedIdentifier(fkey.Key.ParentTableSchema, fkey.Key.ParentTableName);
                var resolvedName = tableNameCache.ContainsKey(candidateParentTableName)
                    ? OptionAsync<Identifier>.Some(tableNameCache[candidateParentTableName])
                    : GetResolvedTableName(candidateParentTableName, cancellationToken);

                Identifier parentTableName = null;

                await resolvedName
                    .BindAsync(async name =>
                    {
                        parentTableName = name;
                        if (fkey.Key.KeyType == Constants.PrimaryKey)
                        {
                            if (primaryKeyCache.TryGetValue(name, out var pk))
                            {
                                return pk.ToAsync();
                            }
                            else
                            {
                                if (!columnLookupsCache.TryGetValue(name, out var parentColumnLookup))
                                {
                                    var parentColumns = await LoadColumnsAsync(name, cancellationToken).ConfigureAwait(false);
                                    parentColumnLookup = GetColumnLookup(parentColumns);
                                    columnLookupsCache[name] = parentColumnLookup;
                                }

                                var parentKeyOption = await LoadPrimaryKeyAsync(name, parentColumnLookup, cancellationToken).ConfigureAwait(false);
                                primaryKeyCache[name] = parentKeyOption;
                                return parentKeyOption.ToAsync();
                            }
                        }
                        else
                        {
                            var parentKeyName = Identifier.CreateQualifiedIdentifier(fkey.Key.ParentKeyName);
                            if (uniqueKeyLookupCache.TryGetValue(name, out var uks) && uks.ContainsKey(parentKeyName.LocalName))
                            {
                                return OptionAsync<IDatabaseKey>.Some(uks[parentKeyName.LocalName]);
                            }
                            else
                            {
                                if (!columnLookupsCache.TryGetValue(name, out var parentColumnLookup))
                                {
                                    var parentColumns = await LoadColumnsAsync(name, cancellationToken).ConfigureAwait(false);
                                    parentColumnLookup = GetColumnLookup(parentColumns);
                                    columnLookupsCache[name] = parentColumnLookup;
                                }

                                var parentUniqueKeys = await LoadUniqueKeysAsync(name, parentColumnLookup, cancellationToken).ConfigureAwait(false);
                                var parentUniqueKeyLookup = GetDatabaseKeyLookup(parentUniqueKeys);
                                uniqueKeyLookupCache[name] = parentUniqueKeyLookup;

                                return parentUniqueKeyLookup.ContainsKey(parentKeyName.LocalName)
                                    ? OptionAsync<IDatabaseKey>.Some(parentUniqueKeyLookup[parentKeyName.LocalName])
                                    : OptionAsync<IDatabaseKey>.None;
                            }
                        }
                    })
                    .IfSome(key =>
                    {
                        var childKeyName = Identifier.CreateQualifiedIdentifier(fkey.Key.ChildKeyName);
                        var childKeyColumns = fkey
                            .OrderBy(row => row.ConstraintColumnId)
                            .Select(row => columns[row.ColumnName])
                            .ToList();

                        var childKey = new MySqlDatabaseKey(childKeyName, DatabaseKeyType.Foreign, childKeyColumns);

                        var deleteRule = RelationalRuleMapping[fkey.Key.DeleteRule];
                        var updateRule = RelationalRuleMapping[fkey.Key.UpdateRule];

                        var relationalKey = new DatabaseRelationalKey(tableName, childKey, parentTableName, key, deleteRule, updateRule);
                        result.Add(relationalKey);
                    })
                    .ConfigureAwait(false);
            }

            return result;
        }

        protected virtual string ParentKeysQuery => ParentKeysQuerySql;

        private const string ParentKeysQuerySql = @"
select
    pt.table_schema as ParentTableSchema,
    pt.table_name as ParentTableName,
    rc.constraint_name as ChildKeyName,
    rc.unique_constraint_name as ParentKeyName,
    kc.column_name as ColumnName,
    kc.ordinal_position as ConstraintColumnId,
    ptc.constraint_type as ParentKeyType,
    rc.delete_rule as DeleteRule,
    rc.update_rule as UpdateRule
from information_schema.tables t
inner join information_schema.referential_constraints rc on t.table_schema = rc.constraint_schema and t.table_name = rc.table_name
inner join information_schema.key_column_usage kc on t.table_schema = kc.table_schema and t.table_name = kc.table_name
inner join information_schema.tables pt on pt.table_schema = rc.unique_constraint_schema and pt.table_name = rc.referenced_table_name
inner join information_schema.table_constraints ptc on pt.table_schema = ptc.table_schema and pt.table_name = ptc.table_name and ptc.constraint_name = rc.unique_constraint_name
where t.table_schema = @SchemaName and t.table_name = @TableName";

        protected virtual Task<IReadOnlyList<IDatabaseColumn>> LoadColumnsAsync(Identifier tableName, CancellationToken cancellationToken)
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));

            return LoadColumnsAsyncCore(tableName, cancellationToken);
        }

        private async Task<IReadOnlyList<IDatabaseColumn>> LoadColumnsAsyncCore(Identifier tableName, CancellationToken cancellationToken)
        {
            var query = await Connection.QueryAsync<ColumnData>(
                ColumnsQuery,
                new { SchemaName = tableName.Schema, TableName = tableName.LocalName },
                cancellationToken
            ).ConfigureAwait(false);

            var result = new List<IDatabaseColumn>();

            foreach (var row in query)
            {
                var typeMetadata = new ColumnTypeMetadata
                {
                    TypeName = Identifier.CreateQualifiedIdentifier(row.DataTypeName),
                    Collation = row.Collation.IsNullOrWhiteSpace() ? null : Identifier.CreateQualifiedIdentifier(row.Collation),
                    MaxLength = row.CharacterMaxLength,
                    NumericPrecision = new NumericPrecision(row.Precision, row.Scale)
                };
                var columnType = TypeProvider.CreateColumnType(typeMetadata);

                var columnName = Identifier.CreateQualifiedIdentifier(row.ColumnName);
                var isAutoIncrement = row.ExtraInformation.Contains(Constants.AutoIncrement, StringComparison.OrdinalIgnoreCase);
                var autoIncrement = isAutoIncrement
                    ? Option<IAutoIncrement>.Some(new AutoIncrement(1, 1))
                    : Option<IAutoIncrement>.None;
                var isComputed = !row.ComputedColumnDefinition.IsNullOrWhiteSpace();
                var isNullable = !string.Equals(row.IsNullable, Constants.No, StringComparison.OrdinalIgnoreCase);
                var defaultValue = !row.DefaultValue.IsNullOrWhiteSpace()
                    ? Option<string>.Some(row.DefaultValue)
                    : Option<string>.None;
                var computedColumnDefinition = isComputed
                    ? Option<string>.Some(row.ComputedColumnDefinition)
                    : Option<string>.None;

                var column = isComputed
                    ? new DatabaseComputedColumn(columnName, columnType, isNullable, defaultValue, computedColumnDefinition)
                    : new DatabaseColumn(columnName, columnType, isNullable, defaultValue, autoIncrement);

                result.Add(column);
            }

            return result.AsReadOnly();
        }

        protected virtual string ColumnsQuery => ColumnsQuerySql;

        private const string ColumnsQuerySql = @"
select
    column_name as ColumnName,
    data_type as DataTypeName,
    character_maximum_length as CharacterMaxLength,
    numeric_precision as `Precision`,
    numeric_scale as `Scale`,
    datetime_precision as `DateTimePrecision`,
    collation_name as Collation,
    is_nullable as IsNullable,
    column_default as DefaultValue,
    generation_expression as ComputedColumnDefinition,
    extra as ExtraInformation
from information_schema.columns
where table_schema = @SchemaName and table_name = @TableName
order by ordinal_position";

        protected virtual Task<IReadOnlyCollection<IDatabaseTrigger>> LoadTriggersAsync(Identifier tableName, CancellationToken cancellationToken)
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(cancellationToken));

            return LoadTriggersAsyncCore(tableName, cancellationToken);
        }

        private async Task<IReadOnlyCollection<IDatabaseTrigger>> LoadTriggersAsyncCore(Identifier tableName, CancellationToken cancellationToken)
        {
            var queryResult = await Connection.QueryAsync<TriggerData>(
                TriggersQuery,
                new { SchemaName = tableName.Schema, TableName = tableName.LocalName },
                cancellationToken
            ).ConfigureAwait(false);

            if (queryResult.Empty())
                return Array.Empty<IDatabaseTrigger>();

            var triggers = queryResult.GroupBy(row => new
            {
                row.TriggerName,
                row.Definition,
                row.Timing
            }).ToList();
            if (triggers.Empty())
                return Array.Empty<IDatabaseTrigger>();

            var result = new List<IDatabaseTrigger>(triggers.Count);
            foreach (var trig in triggers)
            {
                var triggerName = Identifier.CreateQualifiedIdentifier(trig.Key.TriggerName);
                var queryTiming = Enum.TryParse(trig.Key.Timing, true, out TriggerQueryTiming timing) ? timing : TriggerQueryTiming.Before;
                var definition = trig.Key.Definition;

                var events = TriggerEvent.None;
                foreach (var trigEvent in trig)
                {
                    if (trigEvent.TriggerEvent == Constants.Insert)
                        events |= TriggerEvent.Insert;
                    else if (trigEvent.TriggerEvent == Constants.Update)
                        events |= TriggerEvent.Update;
                    else if (trigEvent.TriggerEvent == Constants.Delete)
                        events |= TriggerEvent.Delete;
                    else
                        throw new UnsupportedTriggerEventException(tableName, trigEvent.TriggerEvent);
                }

                var trigger = new MySqlDatabaseTrigger(triggerName, definition, queryTiming, events);
                result.Add(trigger);
            }

            return result;
        }

        protected virtual string TriggersQuery => TriggersQuerySql;

        private const string TriggersQuerySql = @"
select
    tr.trigger_name as TriggerName,
    tr.action_statement as Definition,
    tr.action_timing as Timing,
    tr.event_manipulation as TriggerEvent
from information_schema.triggers tr
where tr.event_object_schema = @SchemaName and tr.event_object_table = @TableName";

        protected Identifier QualifyTableName(Identifier tableName)
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));

            var schema = tableName.Schema ?? IdentifierDefaults.Schema;
            return Identifier.CreateQualifiedIdentifier(IdentifierDefaults.Server, IdentifierDefaults.Database, schema, tableName.LocalName);
        }

        protected IReadOnlyDictionary<string, Rule> RelationalRuleMapping { get; } = new Dictionary<string, Rule>(StringComparer.OrdinalIgnoreCase)
        {
            ["NO ACTION"] = Rule.None,
            ["RESTRICT"] = Rule.None,
            ["CASCADE"] = Rule.Cascade,
            ["SET NULL"] = Rule.SetNull,
            ["SET DEFAULT"] = Rule.SetDefault
        };

        private static IReadOnlyDictionary<Identifier, IDatabaseColumn> GetColumnLookup(IReadOnlyCollection<IDatabaseColumn> columns)
        {
            if (columns == null)
                throw new ArgumentNullException(nameof(columns));

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
            if (keys == null)
                throw new ArgumentNullException(nameof(keys));

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
            return Connection.ExecuteScalarAsync<bool>(sql, CancellationToken.None);
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
    }
}
