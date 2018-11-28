using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using SJP.Schematic.Core;
using SJP.Schematic.MySql.Query;
using SJP.Schematic.Core.Extensions;
using System.Threading;

namespace SJP.Schematic.MySql
{
    public class MySqlRelationalDatabaseTable : IRelationalDatabaseTable
    {
        public MySqlRelationalDatabaseTable(IDbConnection connection, IRelationalDatabase database, IDbTypeProvider typeProvider, Identifier tableName)
        {
            Connection = connection ?? throw new ArgumentNullException(nameof(connection));
            Database = database ?? throw new ArgumentNullException(nameof(database));
            TypeProvider = typeProvider ?? throw new ArgumentNullException(nameof(typeProvider));

            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));
            if (tableName.Schema == null)
                throw new ArgumentException("The given table name is missing a required schema name.", nameof(tableName));

            Name = tableName;
        }

        public Identifier Name { get; }

        protected IRelationalDatabase Database { get; }

        protected IDbTypeProvider TypeProvider { get; }

        protected IDbConnection Connection { get; }

        public IDatabaseKey PrimaryKey => LoadPrimaryKeySync();

        public Task<IDatabaseKey> PrimaryKeyAsync(CancellationToken cancellationToken = default(CancellationToken)) => LoadPrimaryKeyAsync(cancellationToken);

        protected virtual IDatabaseKey LoadPrimaryKeySync()
        {
            var primaryKeyColumns = Connection.Query<ConstraintColumnMapping>(PrimaryKeyQuery, new { SchemaName = Name.Schema, TableName = Name.LocalName });
            if (primaryKeyColumns.Empty())
                return null;

            var groupedByName = primaryKeyColumns.GroupBy(row => new { row.ConstraintName });
            var firstRow = groupedByName.First();
            var constraintName = firstRow.Key.ConstraintName;

            var tableColumns = this.GetColumnLookup();
            var columns = groupedByName
                .Where(row => row.Key.ConstraintName == constraintName)
                .SelectMany(g => g.Select(row => tableColumns[row.ColumnName]))
                .ToList();

            return new MySqlDatabasePrimaryKey(columns);
        }

        protected virtual async Task<IDatabaseKey> LoadPrimaryKeyAsync(CancellationToken cancellationToken)
        {
            var primaryKeyColumns = await Connection.QueryAsync<ConstraintColumnMapping>(PrimaryKeyQuery, new { SchemaName = Name.Schema, TableName = Name.LocalName }).ConfigureAwait(false);
            if (primaryKeyColumns.Empty())
                return null;

            var groupedByName = primaryKeyColumns.GroupBy(row => new { row.ConstraintName });
            var firstRow = groupedByName.First();
            var constraintName = firstRow.Key.ConstraintName;

            var tableColumns = await this.GetColumnLookupAsync(cancellationToken).ConfigureAwait(false);
            var columns = groupedByName
                .Where(row => row.Key.ConstraintName == constraintName)
                .SelectMany(g => g.Select(row => tableColumns[row.ColumnName]))
                .ToList();

            return new MySqlDatabasePrimaryKey(columns);
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

        public IReadOnlyCollection<IDatabaseIndex> Indexes => LoadIndexesSync();

        public Task<IReadOnlyCollection<IDatabaseIndex>> IndexesAsync(CancellationToken cancellationToken = default(CancellationToken)) => LoadIndexesAsync(cancellationToken);

        protected virtual IReadOnlyCollection<IDatabaseIndex> LoadIndexesSync()
        {
            var queryResult = Connection.Query<IndexColumns>(IndexesQuery, new { SchemaName = Name.Schema, TableName = Name.LocalName });
            if (queryResult.Empty())
                return Array.Empty<IDatabaseIndex>();

            var indexColumns = queryResult.GroupBy(row => new { row.IndexName, row.IsNonUnique }).ToList();
            if (indexColumns.Count == 0)
                return Array.Empty<IDatabaseIndex>();

            var tableColumns = this.GetColumnLookup();
            var result = new List<IDatabaseIndex>(indexColumns.Count);
            foreach (var indexInfo in indexColumns)
            {
                var isUnique = !indexInfo.Key.IsNonUnique;
                var indexName = Identifier.CreateQualifiedIdentifier(indexInfo.Key.IndexName);

                var indexCols = indexInfo
                    .OrderBy(row => row.ColumnOrdinal)
                    .Select(row => tableColumns[row.ColumnName])
                    .Select(col => new MySqlDatabaseIndexColumn(col))
                    .ToList();

                var index = new MySqlDatabaseIndex(indexName, isUnique, indexCols);
                result.Add(index);
            }

            return result;
        }

        protected virtual async Task<IReadOnlyCollection<IDatabaseIndex>> LoadIndexesAsync(CancellationToken cancellationToken)
        {
            var queryResult = await Connection.QueryAsync<IndexColumns>(IndexesQuery, new { SchemaName = Name.Schema, TableName = Name.LocalName }).ConfigureAwait(false);
            if (queryResult.Empty())
                return Array.Empty<IDatabaseIndex>();

            var indexColumns = queryResult.GroupBy(row => new { row.IndexName, row.IsNonUnique }).ToList();
            if (indexColumns.Count == 0)
                return Array.Empty<IDatabaseIndex>();

            var tableColumns = await this.GetColumnLookupAsync(cancellationToken).ConfigureAwait(false);
            var result = new List<IDatabaseIndex>(indexColumns.Count);

            foreach (var indexInfo in indexColumns)
            {
                var isUnique = !indexInfo.Key.IsNonUnique;
                var indexName = Identifier.CreateQualifiedIdentifier(indexInfo.Key.IndexName);

                var indexCols = indexInfo
                    .OrderBy(row => row.ColumnOrdinal)
                    .Select(row => tableColumns[row.ColumnName])
                    .Select(col => new MySqlDatabaseIndexColumn(col))
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

        public IReadOnlyCollection<IDatabaseKey> UniqueKeys => LoadUniqueKeysSync();

        public Task<IReadOnlyCollection<IDatabaseKey>> UniqueKeysAsync(CancellationToken cancellationToken = default(CancellationToken)) => LoadUniqueKeysAsync(cancellationToken);

        protected virtual IReadOnlyCollection<IDatabaseKey> LoadUniqueKeysSync()
        {
            var uniqueKeyColumns = Connection.Query<ConstraintColumnMapping>(UniqueKeysQuery, new { SchemaName = Name.Schema, TableName = Name.LocalName });
            if (uniqueKeyColumns.Empty())
                return Array.Empty<IDatabaseKey>();

            var groupedByName = uniqueKeyColumns.GroupBy(row => new { row.ConstraintName });
            var tableColumns = this.GetColumnLookup();
            var constraintColumns = groupedByName
                .Select(g => new
                {
                    g.Key.ConstraintName,
                    Columns = g.Select(row => tableColumns[row.ColumnName]).ToList(),
                })
                .ToList();
            if (constraintColumns.Count == 0)
                return Array.Empty<IDatabaseKey>();

            var result = new List<IDatabaseKey>(constraintColumns.Count);
            foreach (var uk in constraintColumns)
            {
                var uniqueKey = new MySqlDatabaseKey(uk.ConstraintName, DatabaseKeyType.Unique, uk.Columns);
                result.Add(uniqueKey);
            }
            return result;
        }

        protected virtual async Task<IReadOnlyCollection<IDatabaseKey>> LoadUniqueKeysAsync(CancellationToken cancellationToken)
        {
            var uniqueKeyColumns = await Connection.QueryAsync<ConstraintColumnMapping>(UniqueKeysQuery, new { SchemaName = Name.Schema, TableName = Name.LocalName }).ConfigureAwait(false);
            if (uniqueKeyColumns.Empty())
                return Array.Empty<IDatabaseKey>();

            var groupedByName = uniqueKeyColumns.GroupBy(row => new { row.ConstraintName });
            var tableColumns = await this.GetColumnLookupAsync(cancellationToken).ConfigureAwait(false);
            var constraintColumns = groupedByName
                .Select(g => new
                {
                    g.Key.ConstraintName,
                    Columns = g.Select(row => tableColumns[row.ColumnName]).ToList(),
                })
                .ToList();
            if (constraintColumns.Count == 0)
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

        public IReadOnlyCollection<IDatabaseRelationalKey> ChildKeys => LoadChildKeysSync();

        public Task<IReadOnlyCollection<IDatabaseRelationalKey>> ChildKeysAsync(CancellationToken cancellationToken = default(CancellationToken)) => LoadChildKeysAsync(cancellationToken);

        protected virtual IReadOnlyCollection<IDatabaseRelationalKey> LoadChildKeysSync()
        {
            var queryResult = Connection.Query<ChildKeyData>(ChildKeysQuery, new { SchemaName = Name.Schema, TableName = Name.LocalName });
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
            if (groupedChildKeys.Count == 0)
                return Array.Empty<IDatabaseRelationalKey>();

            var result = new List<IDatabaseRelationalKey>(groupedChildKeys.Count);
            foreach (var groupedChildKey in groupedChildKeys)
            {
                var childKeyName = Identifier.CreateQualifiedIdentifier(groupedChildKey.Key.ChildKeyName);

                var childTableName = Identifier.CreateQualifiedIdentifier(groupedChildKey.Key.ChildTableSchema, groupedChildKey.Key.ChildTableName);
                var childOption = Database.GetTable(childTableName);
                if (childOption.IsNone)
                    throw new Exception("Could not find child table with name: " + childTableName.ToString());

                var childTable = childOption.UnwrapSome();
                var parentKeyLookup = childTable.GetParentKeyLookup();

                var childKey = parentKeyLookup[childKeyName.LocalName].ChildKey;

                IDatabaseKey parentKey;
                if (groupedChildKey.Key.ParentKeyType == "PRIMARY KEY")
                {
                    parentKey = PrimaryKey;
                }
                else
                {
                    var uniqueKeyLookup = this.GetUniqueKeyLookup();
                    parentKey = uniqueKeyLookup[groupedChildKey.Key.ParentKeyName];
                }

                var deleteRule = RelationalRuleMapping[groupedChildKey.Key.DeleteRule];
                var updateRule = RelationalRuleMapping[groupedChildKey.Key.UpdateRule];

                var relationalKey = new MySqlRelationalKey(childTableName, childKey, Name, parentKey, deleteRule, updateRule);
                result.Add(relationalKey);
            }

            return result;
        }

        protected virtual async Task<IReadOnlyCollection<IDatabaseRelationalKey>> LoadChildKeysAsync(CancellationToken cancellationToken)
        {
            var queryResult = await Connection.QueryAsync<ChildKeyData>(ChildKeysQuery, new { SchemaName = Name.Schema, TableName = Name.LocalName }).ConfigureAwait(false);
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
            if (groupedChildKeys.Count == 0)
                return Array.Empty<IDatabaseRelationalKey>();

            var result = new List<IDatabaseRelationalKey>(groupedChildKeys.Count);
            foreach (var groupedChildKey in groupedChildKeys)
            {
                var childKeyName = Identifier.CreateQualifiedIdentifier(groupedChildKey.Key.ChildKeyName);

                var childTableName = Identifier.CreateQualifiedIdentifier(groupedChildKey.Key.ChildTableSchema, groupedChildKey.Key.ChildTableName);
                var childOption = Database.GetTableAsync(childTableName, cancellationToken);
                var childIsNone = await childOption.IsNone.ConfigureAwait(false);
                if (childIsNone)
                    throw new Exception("Could not find child table with name: " + childTableName.ToString());

                var childTable = await childOption.UnwrapSomeAsync().ConfigureAwait(false);
                var parentKeyLookup = await childTable.GetParentKeyLookupAsync(cancellationToken).ConfigureAwait(false);

                var childKey = parentKeyLookup[childKeyName.LocalName].ChildKey;

                IDatabaseKey parentKey;
                if (groupedChildKey.Key.ParentKeyType == "PRIMARY KEY")
                {
                    parentKey = await PrimaryKeyAsync(cancellationToken).ConfigureAwait(false);
                }
                else
                {
                    var uniqueKeyLookup = await this.GetUniqueKeyLookupAsync(cancellationToken).ConfigureAwait(false);
                    parentKey = uniqueKeyLookup[groupedChildKey.Key.ParentKeyName];
                }

                var deleteRule = RelationalRuleMapping[groupedChildKey.Key.DeleteRule];
                var updateRule = RelationalRuleMapping[groupedChildKey.Key.UpdateRule];
                var relationalKey = new MySqlRelationalKey(childTableName, childKey, Name, parentKey, deleteRule, updateRule);

                result.Add(relationalKey);
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

        // checks are parsed but not supported by MySQL
        public IReadOnlyCollection<IDatabaseCheckConstraint> Checks { get; } = Array.Empty<IDatabaseCheckConstraint>();

        public Task<IReadOnlyCollection<IDatabaseCheckConstraint>> ChecksAsync(CancellationToken cancellationToken = default(CancellationToken)) => _emptyChecks;

        private readonly static Task<IReadOnlyCollection<IDatabaseCheckConstraint>> _emptyChecks = Task.FromResult<IReadOnlyCollection<IDatabaseCheckConstraint>>(Array.Empty<IDatabaseCheckConstraint>());

        public IReadOnlyCollection<IDatabaseRelationalKey> ParentKeys => LoadParentKeysSync();

        public Task<IReadOnlyCollection<IDatabaseRelationalKey>> ParentKeysAsync(CancellationToken cancellationToken = default(CancellationToken)) => LoadParentKeysAsync(cancellationToken);

        protected virtual IReadOnlyCollection<IDatabaseRelationalKey> LoadParentKeysSync()
        {
            var queryResult = Connection.Query<ForeignKeyData>(ParentKeysQuery, new { SchemaName = Name.Schema, TableName = Name.LocalName });
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
                row.UpdateRule
            }).ToList();
            if (foreignKeys.Count == 0)
                return Array.Empty<IDatabaseRelationalKey>();

            var result = new List<IDatabaseRelationalKey>(foreignKeys.Count);
            foreach (var fkey in foreignKeys)
            {
                var parentTableName = Identifier.CreateQualifiedIdentifier(fkey.Key.ParentTableSchema, fkey.Key.ParentTableName);
                var parentOption = Database.GetTable(parentTableName);
                if (parentOption.IsNone)
                    throw new Exception("Could not find parent table with name: " + parentTableName.ToString());

                var parentTable = parentOption.UnwrapSome();
                var parentKeyName = Identifier.CreateQualifiedIdentifier(fkey.Key.ParentKeyName);

                IDatabaseKey parentKey;
                if (fkey.Key.KeyType == "PRIMARY KEY")
                {
                    parentKey = parentTable.PrimaryKey;
                }
                else
                {
                    var uniqueKeys = parentTable.GetUniqueKeyLookup();
                    parentKey = uniqueKeys[parentKeyName.LocalName];
                }

                var childKeyName = Identifier.CreateQualifiedIdentifier(fkey.Key.ChildKeyName);
                var childKeyColumnLookup = this.GetColumnLookup();
                var childKeyColumns = fkey
                    .OrderBy(row => row.ConstraintColumnId)
                    .Select(row => childKeyColumnLookup[row.ColumnName])
                    .ToList();

                var childKey = new MySqlDatabaseKey(childKeyName, DatabaseKeyType.Foreign, childKeyColumns);

                var deleteRule = RelationalRuleMapping[fkey.Key.DeleteRule];
                var updateRule = RelationalRuleMapping[fkey.Key.UpdateRule];

                var relationalKey = new MySqlRelationalKey(Name, childKey, parentTableName, parentKey, deleteRule, updateRule);
                result.Add(relationalKey);
            }

            return result;
        }

        protected virtual async Task<IReadOnlyCollection<IDatabaseRelationalKey>> LoadParentKeysAsync(CancellationToken cancellationToken)
        {
            var queryResult = await Connection.QueryAsync<ForeignKeyData>(ParentKeysQuery, new { SchemaName = Name.Schema, TableName = Name.LocalName }).ConfigureAwait(false);
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
            if (foreignKeys.Count == 0)
                return Array.Empty<IDatabaseRelationalKey>();

            var result = new List<IDatabaseRelationalKey>(foreignKeys.Count);
            foreach (var fkey in foreignKeys)
            {
                var parentTableName = Identifier.CreateQualifiedIdentifier(fkey.Key.ParentTableSchema, fkey.Key.ParentTableName);
                var parentOption = Database.GetTableAsync(parentTableName, cancellationToken);
                var parentIsNone = await parentOption.IsNone.ConfigureAwait(false);
                if (parentIsNone)
                    throw new Exception("Could not find parent table with name: " + parentTableName.ToString());

                var parentTable = await parentOption.UnwrapSomeAsync().ConfigureAwait(false);
                var parentKeyName = Identifier.CreateQualifiedIdentifier(fkey.Key.ParentKeyName);

                IDatabaseKey parentKey;
                if (fkey.Key.KeyType == "PRIMARY KEY")
                {
                    parentKey = parentTable.PrimaryKey;
                }
                else
                {
                    var uniqueKeys = await parentTable.GetUniqueKeyLookupAsync(cancellationToken).ConfigureAwait(false);
                    parentKey = uniqueKeys[parentKeyName.LocalName];
                }

                var childKeyName = Identifier.CreateQualifiedIdentifier(fkey.Key.ChildKeyName);
                var childKeyColumnLookup = await this.GetColumnLookupAsync(cancellationToken).ConfigureAwait(false);
                var childKeyColumns = fkey
                    .OrderBy(row => row.ConstraintColumnId)
                    .Select(row => childKeyColumnLookup[row.ColumnName])
                    .ToList();

                var childKey = new MySqlDatabaseKey(childKeyName, DatabaseKeyType.Foreign, childKeyColumns);

                var deleteRule = RelationalRuleMapping[fkey.Key.DeleteRule];
                var updateRule = RelationalRuleMapping[fkey.Key.UpdateRule];

                var relationalKey = new MySqlRelationalKey(Name, childKey, parentTableName, parentKey, deleteRule, updateRule);
                result.Add(relationalKey);
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

        public IReadOnlyList<IDatabaseColumn> Columns => LoadColumnsSync();

        public Task<IReadOnlyList<IDatabaseColumn>> ColumnsAsync(CancellationToken cancellationToken = default(CancellationToken)) => LoadColumnsAsync(cancellationToken);

        protected virtual IReadOnlyList<IDatabaseColumn> LoadColumnsSync()
        {
            var query = Connection.Query<ColumnData>(ColumnsQuery, new { SchemaName = Name.Schema, TableName = Name.LocalName });
            var result = new List<IDatabaseColumn>();

            foreach (var row in query)
            {
                var precision = row.DateTimePrecision > 0
                    ? new NumericPrecision(row.DateTimePrecision, 0)
                    : new NumericPrecision(row.Precision, row.Scale);

                var typeMetadata = new ColumnTypeMetadata
                {
                    TypeName = Identifier.CreateQualifiedIdentifier(row.DataTypeName),
                    Collation = row.Collation.IsNullOrWhiteSpace() ? null : Identifier.CreateQualifiedIdentifier(row.Collation),
                    MaxLength = row.CharacterMaxLength,
                    NumericPrecision = precision
                };
                var columnType = TypeProvider.CreateColumnType(typeMetadata);

                var columnName = Identifier.CreateQualifiedIdentifier(row.ColumnName);
                var isAutoIncrement = row.ExtraInformation.Contains("auto_increment", StringComparison.OrdinalIgnoreCase);
                var autoIncrement = isAutoIncrement ? new AutoIncrement(1, 1) : (IAutoIncrement)null;

                var isComputed = !row.ComputedColumnDefinition.IsNullOrWhiteSpace();
                var isNullable = !string.Equals(row.IsNullable, "NO", StringComparison.OrdinalIgnoreCase);

                var column = isComputed
                    ? new DatabaseComputedColumn(columnName, columnType, isNullable, row.DefaultValue, row.ComputedColumnDefinition)
                    : new DatabaseColumn(columnName, columnType, isNullable, row.DefaultValue, autoIncrement);

                result.Add(column);
            }

            return result.AsReadOnly();
        }

        protected virtual async Task<IReadOnlyList<IDatabaseColumn>> LoadColumnsAsync(CancellationToken cancellationToken)
        {
            var query = await Connection.QueryAsync<ColumnData>(ColumnsQuery, new { SchemaName = Name.Schema, TableName = Name.LocalName }).ConfigureAwait(false);
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
                var isAutoIncrement = row.ExtraInformation.Contains("auto_increment", StringComparison.OrdinalIgnoreCase);
                var autoIncrement = isAutoIncrement ? new AutoIncrement(1, 1) : (IAutoIncrement)null;

                var isComputed = !row.ComputedColumnDefinition.IsNullOrWhiteSpace();
                var isNullable = !string.Equals(row.IsNullable, "NO", StringComparison.OrdinalIgnoreCase);

                var column = isComputed
                    ? new DatabaseComputedColumn(columnName, columnType, isNullable, row.DefaultValue, row.ComputedColumnDefinition)
                    : new DatabaseColumn(columnName, columnType, isNullable, row.DefaultValue, autoIncrement);

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

        public IReadOnlyCollection<IDatabaseTrigger> Triggers => LoadTriggersSync();

        public Task<IReadOnlyCollection<IDatabaseTrigger>> TriggersAsync(CancellationToken cancellationToken = default(CancellationToken)) => LoadTriggersAsync(cancellationToken);

        protected virtual IReadOnlyCollection<IDatabaseTrigger> LoadTriggersSync()
        {
            var queryResult = Connection.Query<TriggerData>(TriggersQuery, new { SchemaName = Name.Schema, TableName = Name.LocalName });
            if (queryResult.Empty())
                return Array.Empty<IDatabaseTrigger>();

            var triggers = queryResult.GroupBy(row => new
            {
                row.TriggerName,
                row.Definition,
                row.Timing
            }).ToList();
            if (triggers.Count == 0)
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
                    if (trigEvent.TriggerEvent == "INSERT")
                        events |= TriggerEvent.Insert;
                    else if (trigEvent.TriggerEvent == "UPDATE")
                        events |= TriggerEvent.Update;
                    else if (trigEvent.TriggerEvent == "DELETE")
                        events |= TriggerEvent.Delete;
                    else
                        throw new Exception("Found an unsupported trigger event name. Expected one of INSERT, UPDATE, DELETE, got: " + trigEvent.TriggerEvent);
                }

                var trigger = new MySqlDatabaseTrigger(triggerName, definition, queryTiming, events);
                result.Add(trigger);
            }

            return result;
        }

        protected virtual async Task<IReadOnlyCollection<IDatabaseTrigger>> LoadTriggersAsync(CancellationToken cancellationToken)
        {
            var queryResult = await Connection.QueryAsync<TriggerData>(TriggersQuery, new { SchemaName = Name.Schema, TableName = Name.LocalName }).ConfigureAwait(false);
            if (queryResult.Empty())
                return Array.Empty<IDatabaseTrigger>();

            var triggers = queryResult.GroupBy(row => new
            {
                row.TriggerName,
                row.Definition,
                row.Timing
            }).ToList();
            if (triggers.Count == 0)
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
                    if (trigEvent.TriggerEvent == "INSERT")
                        events |= TriggerEvent.Insert;
                    else if (trigEvent.TriggerEvent == "UPDATE")
                        events |= TriggerEvent.Update;
                    else if (trigEvent.TriggerEvent == "DELETE")
                        events |= TriggerEvent.Delete;
                    else
                        throw new Exception("Found an unsupported trigger event name. Expected one of INSERT, UPDATE, DELETE, got: " + trigEvent.TriggerEvent);
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

        protected IReadOnlyDictionary<string, Rule> RelationalRuleMapping { get; } = new Dictionary<string, Rule>(StringComparer.OrdinalIgnoreCase)
        {
            ["NO ACTION"] = Rule.None,
            ["RESTRICT"] = Rule.None,
            ["CASCADE"] = Rule.Cascade,
            ["SET NULL"] = Rule.SetNull,
            ["SET DEFAULT"] = Rule.SetDefault
        };
    }
}
