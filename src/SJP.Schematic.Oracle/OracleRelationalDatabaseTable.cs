using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Core.Utilities;
using SJP.Schematic.Oracle.Query;

namespace SJP.Schematic.Oracle
{
    public class OracleRelationalDatabaseTable : IRelationalDatabaseTable
    {
        public OracleRelationalDatabaseTable(IDbConnection connection, IRelationalDatabase database, IDbTypeProvider typeProvider, Identifier tableName, IIdentifierResolutionStrategy identifierResolver = null)
        {
            Connection = connection ?? throw new ArgumentNullException(nameof(connection));
            Database = database ?? throw new ArgumentNullException(nameof(database));
            TypeProvider = typeProvider ?? throw new ArgumentNullException(nameof(typeProvider));
            Name = tableName ?? throw new ArgumentNullException(nameof(tableName));
            IdentifierResolver = identifierResolver ?? new DefaultOracleIdentifierResolutionStrategy();
        }

        public Identifier Name { get; }

        protected IRelationalDatabase Database { get; }

        protected IDbTypeProvider TypeProvider { get; }

        protected IDbConnection Connection { get; }

        protected IIdentifierResolutionStrategy IdentifierResolver { get; }

        public IDatabaseKey PrimaryKey => LoadPrimaryKeySync();

        public Task<IDatabaseKey> PrimaryKeyAsync(CancellationToken cancellationToken = default(CancellationToken)) => LoadPrimaryKeyAsync(cancellationToken);

        protected virtual IDatabaseKey LoadPrimaryKeySync()
        {
            var primaryKeyColumns = Connection.Query<ConstraintColumnMapping>(PrimaryKeyQuery, new { SchemaName = Name.Schema, TableName = Name.LocalName });
            if (primaryKeyColumns.Empty())
                return null;

            var groupedByName = primaryKeyColumns.GroupBy(row => new { row.ConstraintName, row.EnabledStatus } );
            var firstRow = groupedByName.First();
            var constraintName = firstRow.Key.ConstraintName;
            var isEnabled = firstRow.Key.EnabledStatus == "ENABLED";

            var tableColumns = Column;
            var columns = firstRow
                .OrderBy(row => row.ColumnPosition)
                .Select(row => tableColumns[row.ColumnName])
                .ToList();

            return new OracleDatabaseKey(constraintName, DatabaseKeyType.Primary, columns, isEnabled);
        }

        protected virtual async Task<IDatabaseKey> LoadPrimaryKeyAsync(CancellationToken cancellationToken)
        {
            var primaryKeyColumns = await Connection.QueryAsync<ConstraintColumnMapping>(PrimaryKeyQuery, new { SchemaName = Name.Schema, TableName = Name.LocalName }).ConfigureAwait(false);
            if (primaryKeyColumns.Empty())
                return null;

            var groupedByName = primaryKeyColumns.GroupBy(row => new { row.ConstraintName, row.EnabledStatus });
            var firstRow = groupedByName.First();
            var constraintName = firstRow.Key.ConstraintName;
            var isEnabled = firstRow.Key.EnabledStatus == "ENABLED";

            var tableColumns = await ColumnAsync(cancellationToken).ConfigureAwait(false);
            var columns = firstRow
                .OrderBy(row => row.ColumnPosition)
                .Select(row => tableColumns[row.ColumnName])
                .ToList();

            return new OracleDatabaseKey(constraintName, DatabaseKeyType.Primary, columns, isEnabled);
        }

        protected virtual string PrimaryKeyQuery => PrimaryKeyQuerySql;

        private const string PrimaryKeyQuerySql = @"
select
    ac.CONSTRAINT_NAME as ConstraintName,
    ac.STATUS as EnabledStatus,
    acc.COLUMN_NAME as ColumnName,
    acc.POSITION as ColumnPosition
from ALL_CONSTRAINTS ac
inner join ALL_CONS_COLUMNS acc on ac.OWNER = acc.OWNER and ac.CONSTRAINT_NAME = acc.CONSTRAINT_NAME and ac.TABLE_NAME = acc.TABLE_NAME
where ac.OWNER = :SchemaName and ac.TABLE_NAME = :TableName and ac.CONSTRAINT_TYPE = 'P'";

        public IReadOnlyDictionary<Identifier, IDatabaseIndex> Index => LoadIndexLookupSync();

        public Task<IReadOnlyDictionary<Identifier, IDatabaseIndex>> IndexAsync(CancellationToken cancellationToken = default(CancellationToken)) => LoadIndexLookupAsync(cancellationToken);

        public IReadOnlyCollection<IDatabaseIndex> Indexes => LoadIndexesSync();

        public Task<IReadOnlyCollection<IDatabaseIndex>> IndexesAsync(CancellationToken cancellationToken = default(CancellationToken)) => LoadIndexesAsync(cancellationToken);

        protected virtual IReadOnlyDictionary<Identifier, IDatabaseIndex> LoadIndexLookupSync()
        {
            var indexes = Indexes;
            var result = new Dictionary<Identifier, IDatabaseIndex>(indexes.Count);

            foreach (var index in indexes)
                result[index.Name.LocalName] = index;

            return new IdentifierResolvingDictionary<IDatabaseIndex>(result, IdentifierResolver);
        }

        protected virtual async Task<IReadOnlyDictionary<Identifier, IDatabaseIndex>> LoadIndexLookupAsync(CancellationToken cancellationToken)
        {
            var indexes = await IndexesAsync(cancellationToken).ConfigureAwait(false);
            var result = new Dictionary<Identifier, IDatabaseIndex>(indexes.Count);

            foreach (var index in indexes)
                result[index.Name.LocalName] = index;

            return new IdentifierResolvingDictionary<IDatabaseIndex>(result, IdentifierResolver);
        }

        protected virtual IReadOnlyCollection<IDatabaseIndex> LoadIndexesSync()
        {
            var queryResult = Connection.Query<IndexColumns>(IndexesQuery, new { SchemaName = Name.Schema, TableName = Name.LocalName });
            if (queryResult.Empty())
                return Array.Empty<IDatabaseIndex>();

            var indexColumns = queryResult.GroupBy(row => new { row.IndexName, row.IndexProperty, row.Uniqueness }).ToList();
            if (indexColumns.Count == 0)
                return Array.Empty<IDatabaseIndex>();

            var tableColumns = Column;
            var result = new List<IDatabaseIndex>(indexColumns.Count);
            foreach (var indexInfo in indexColumns)
            {
                var indexProperties = (OracleIndexProperties)indexInfo.Key.IndexProperty;
                var isUnique = indexInfo.Key.Uniqueness == "UNIQUE";
                var indexName = Identifier.CreateQualifiedIdentifier(indexInfo.Key.IndexName);

                var indexCols = indexInfo
                    .OrderBy(row => row.ColumnPosition)
                    .Select(row => new { row.IsDescending, Column = tableColumns[row.ColumnName] })
                    .Select(row => new OracleDatabaseIndexColumn(row.Column, row.IsDescending == "Y" ? IndexColumnOrder.Descending : IndexColumnOrder.Ascending))
                    .ToList();

                var index = new OracleDatabaseIndex(indexName, isUnique, indexCols, indexProperties);
                result.Add(index);
            }

            return result;
        }

        protected virtual async Task<IReadOnlyCollection<IDatabaseIndex>> LoadIndexesAsync(CancellationToken cancellationToken)
        {
            var queryResult = await Connection.QueryAsync<IndexColumns>(IndexesQuery, new { SchemaName = Name.Schema, TableName = Name.LocalName }).ConfigureAwait(false);
            if (queryResult.Empty())
                return Array.Empty<IDatabaseIndex>();

            var indexColumns = queryResult.GroupBy(row => new { row.IndexName, row.IndexProperty, row.Uniqueness }).ToList();
            if (indexColumns.Count == 0)
                return Array.Empty<IDatabaseIndex>();

            var tableColumns = await ColumnAsync(cancellationToken).ConfigureAwait(false);
            var result = new List<IDatabaseIndex>(indexColumns.Count);
            foreach (var indexInfo in indexColumns)
            {
                var indexProperties = (OracleIndexProperties)indexInfo.Key.IndexProperty;
                var isUnique = indexInfo.Key.Uniqueness == "UNIQUE";
                var indexName = Identifier.CreateQualifiedIdentifier(indexInfo.Key.IndexName);

                var indexCols = indexInfo
                    .OrderBy(row => row.ColumnPosition)
                    .Select(row => new { row.IsDescending, Column = tableColumns[row.ColumnName] })
                    .Select(row => new OracleDatabaseIndexColumn(row.Column, row.IsDescending == "Y" ? IndexColumnOrder.Descending : IndexColumnOrder.Ascending))
                    .ToList();

                var index = new OracleDatabaseIndex(indexName, isUnique, indexCols, indexProperties);
                result.Add(index);
            }

            return result;
        }

        protected virtual string IndexesQuery => IndexesQuerySql;

        private const string IndexesQuerySql = @"
select
    ai.OWNER as IndexOwner,
    ai.INDEX_NAME as IndexName,
    ai.UNIQUENESS as Uniqueness,
    ind.PROPERTY as IndexProperty,
    aic.COLUMN_NAME as ColumnName,
    aic.COLUMN_POSITION as ColumnPosition,
    aic.DESCEND as IsDescending
from ALL_INDEXES ai
inner join ALL_OBJECTS ao on ai.OWNER = ao.OWNER and ai.INDEX_NAME = ao.OBJECT_NAME
inner join SYS.IND$ ind on ao.OBJECT_ID = ind.OBJ#
inner join ALL_IND_COLUMNS aic
    on ai.OWNER = aic.INDEX_OWNER and ai.INDEX_NAME = aic.INDEX_NAME
where ai.TABLE_OWNER = :SchemaName and ai.TABLE_NAME = :TableName
    and aic.TABLE_OWNER = :SchemaName and aic.TABLE_NAME = :TableName
    and ao.OBJECT_TYPE = 'INDEX'
order by aic.COLUMN_POSITION";

        public IReadOnlyDictionary<Identifier, IDatabaseKey> UniqueKey => LoadUniqueKeyLookupSync();

        public Task<IReadOnlyDictionary<Identifier, IDatabaseKey>> UniqueKeyAsync(CancellationToken cancellationToken = default(CancellationToken)) => LoadUniqueKeyLookupAsync(cancellationToken);

        public IReadOnlyCollection<IDatabaseKey> UniqueKeys => LoadUniqueKeysSync();

        public Task<IReadOnlyCollection<IDatabaseKey>> UniqueKeysAsync(CancellationToken cancellationToken = default(CancellationToken)) => LoadUniqueKeysAsync(cancellationToken);

        protected virtual IReadOnlyDictionary<Identifier, IDatabaseKey> LoadUniqueKeyLookupSync()
        {
            var uniqueKeys = UniqueKeys;
            var result = new Dictionary<Identifier, IDatabaseKey>(uniqueKeys.Count);

            foreach (var uk in uniqueKeys)
                result[uk.Name.LocalName] = uk;

            return new IdentifierResolvingDictionary<IDatabaseKey>(result, IdentifierResolver);
        }

        protected virtual async Task<IReadOnlyDictionary<Identifier, IDatabaseKey>> LoadUniqueKeyLookupAsync(CancellationToken cancellationToken)
        {
            var uniqueKeys = await UniqueKeysAsync(cancellationToken).ConfigureAwait(false);
            var result = new Dictionary<Identifier, IDatabaseKey>(uniqueKeys.Count);

            foreach (var uk in uniqueKeys)
                result[uk.Name.LocalName] = uk;

            return new IdentifierResolvingDictionary<IDatabaseKey>(result, IdentifierResolver);
        }

        protected virtual IReadOnlyCollection<IDatabaseKey> LoadUniqueKeysSync()
        {
            var uniqueKeyColumns = Connection.Query<ConstraintColumnMapping>(UniqueKeysQuery, new { SchemaName = Name.Schema, TableName = Name.LocalName });
            if (uniqueKeyColumns.Empty())
                return Array.Empty<IDatabaseKey>();

            var groupedByName = uniqueKeyColumns.GroupBy(row => new { row.ConstraintName, row.EnabledStatus });
            var tableColumns = Column;
            var constraintColumns = groupedByName
                .Select(g => new
                {
                    g.Key.ConstraintName,
                    Columns = g.OrderBy(row => row.ColumnPosition)
                        .Select(row => tableColumns[row.ColumnName])
                        .ToList(),
                    IsEnabled = g.Key.EnabledStatus == "ENABLED"
                })
                .ToList();
            if (constraintColumns.Count == 0)
                return Array.Empty<IDatabaseKey>();

            var result = new List<IDatabaseKey>(constraintColumns.Count);
            foreach (var uk in constraintColumns)
            {
                var uniqueKey = new OracleDatabaseKey(uk.ConstraintName, DatabaseKeyType.Unique, uk.Columns, uk.IsEnabled);
                result.Add(uniqueKey);
            }
            return result;
        }

        protected virtual async Task<IReadOnlyCollection<IDatabaseKey>> LoadUniqueKeysAsync(CancellationToken cancellationToken)
        {
            var uniqueKeyColumns = await Connection.QueryAsync<ConstraintColumnMapping>(UniqueKeysQuery, new { SchemaName = Name.Schema, TableName = Name.LocalName }).ConfigureAwait(false);
            if (uniqueKeyColumns.Empty())
                return Array.Empty<IDatabaseKey>();

            var groupedByName = uniqueKeyColumns.GroupBy(row => new { row.ConstraintName, row.EnabledStatus });
            var tableColumns = await ColumnAsync(cancellationToken).ConfigureAwait(false);
            var constraintColumns = groupedByName
                .Select(g => new
                {
                    g.Key.ConstraintName,
                    Columns = g.OrderBy(row => row.ColumnPosition)
                        .Select(row => tableColumns[row.ColumnName])
                        .ToList(),
                    IsEnabled = g.Key.EnabledStatus == "ENABLED"
                })
                .ToList();
            if (constraintColumns.Count == 0)
                return Array.Empty<IDatabaseKey>();

            var result = new List<IDatabaseKey>(constraintColumns.Count);
            foreach (var uk in constraintColumns)
            {
                var uniqueKey = new OracleDatabaseKey(uk.ConstraintName, DatabaseKeyType.Unique, uk.Columns, uk.IsEnabled);
                result.Add(uniqueKey);
            }
            return result;
        }

        protected virtual string UniqueKeysQuery => UniqueKeysQuerySql;

        private const string UniqueKeysQuerySql = @"
select
    ac.CONSTRAINT_NAME as ConstraintName,
    ac.STATUS as EnabledStatus,
    acc.COLUMN_NAME as ColumnName,
    acc.POSITION as ColumnPosition
from ALL_CONSTRAINTS ac
inner join ALL_CONS_COLUMNS acc on ac.OWNER = acc.OWNER and ac.CONSTRAINT_NAME = acc.CONSTRAINT_NAME and ac.TABLE_NAME = acc.TABLE_NAME
where ac.OWNER = :SchemaName and ac.TABLE_NAME = :TableName and ac.CONSTRAINT_TYPE = 'U'";

        public IReadOnlyCollection<IDatabaseRelationalKey> ChildKeys => LoadChildKeysSync();

        public Task<IReadOnlyCollection<IDatabaseRelationalKey>> ChildKeysAsync(CancellationToken cancellationToken = default(CancellationToken)) => LoadChildKeysAsync(cancellationToken);

        protected virtual IReadOnlyCollection<IDatabaseRelationalKey> LoadChildKeysSync()
        {
            var queryResult = Connection.Query<ChildKeyData>(ChildKeysQuery, new { SchemaName = Name.Schema, TableName = Name.LocalName });
            if (queryResult.Empty())
                return Array.Empty<IDatabaseRelationalKey>();

            var childKeyRows = queryResult.ToList();
            if (childKeyRows.Count == 0)
                return Array.Empty<IDatabaseRelationalKey>();

            var result = new List<IDatabaseRelationalKey>(childKeyRows.Count);
            foreach (var childKeyRow in childKeyRows)
            {
                var childKeyName = Identifier.CreateQualifiedIdentifier(childKeyRow.ChildKeyName);
                var childTableName = Identifier.CreateQualifiedIdentifier(childKeyRow.ChildTableSchema, childKeyRow.ChildTableName);
                var childTable = Database.GetTable(childTableName);
                var parentKeyLookup = childTable.ParentKey;

                var childKey = parentKeyLookup[childKeyName.LocalName].ChildKey;

                IDatabaseKey parentKey;
                if (childKeyRow.ParentKeyType == "P")
                {
                    parentKey = PrimaryKey;
                }
                else
                {
                    var uniqueKeyLookup = UniqueKey;
                    parentKey = uniqueKeyLookup[childKeyRow.ParentKeyName];
                }

                var deleteRule = RelationalRuleMapping[childKeyRow.DeleteRule];

                var relationalKey = new OracleRelationalKey(childTableName, childKey, Name, parentKey, deleteRule);
                result.Add(relationalKey);
            }

            return result;
        }

        protected virtual async Task<IReadOnlyCollection<IDatabaseRelationalKey>> LoadChildKeysAsync(CancellationToken cancellationToken)
        {
            var queryResult = await Connection.QueryAsync<ChildKeyData>(ChildKeysQuery, new { SchemaName = Name.Schema, TableName = Name.LocalName }).ConfigureAwait(false);
            if (queryResult.Empty())
                return Array.Empty<IDatabaseRelationalKey>();

            var childKeyRows = queryResult.ToList();
            if (childKeyRows.Count == 0)
                return Array.Empty<IDatabaseRelationalKey>();

            var result = new List<IDatabaseRelationalKey>(childKeyRows.Count);
            foreach (var childKeyRow in childKeyRows)
            {
                var childKeyName = Identifier.CreateQualifiedIdentifier(childKeyRow.ChildKeyName);
                var childTableName = Identifier.CreateQualifiedIdentifier(childKeyRow.ChildTableSchema, childKeyRow.ChildTableName);
                var childTable = await Database.GetTableAsync(childTableName, cancellationToken).ConfigureAwait(false);
                var parentKeyLookup = await childTable.ParentKeyAsync(cancellationToken).ConfigureAwait(false);

                var childKey = parentKeyLookup[childKeyName.LocalName].ChildKey;

                IDatabaseKey parentKey;
                if (childKeyRow.ParentKeyType == "P")
                {
                    parentKey = await PrimaryKeyAsync(cancellationToken).ConfigureAwait(false);
                }
                else
                {
                    var uniqueKeyLookup = await UniqueKeyAsync(cancellationToken).ConfigureAwait(false);
                    parentKey = uniqueKeyLookup[childKeyRow.ParentKeyName];
                }

                var deleteRule = RelationalRuleMapping[childKeyRow.DeleteRule];
                var relationalKey = new OracleRelationalKey(childTableName, childKey, Name, parentKey, deleteRule);

                result.Add(relationalKey);
            }

            return result;
        }

        protected virtual string ChildKeysQuery => ChildKeysQuerySql;

        private const string ChildKeysQuerySql = @"
select
    ac.OWNER as ChildTableSchema,
    ac.TABLE_NAME as ChildTableName,
    ac.CONSTRAINT_NAME as ChildKeyName,
    ac.STATUS as EnabledStatus,
    ac.DELETE_RULE as DeleteRule,
    pac.CONSTRAINT_NAME as ParentKeyName,
    pac.CONSTRAINT_TYPE as ParentKeyType
from ALL_CONSTRAINTS ac
inner join ALL_CONSTRAINTS pac on pac.OWNER = ac.R_OWNER and pac.CONSTRAINT_NAME = ac.R_CONSTRAINT_NAME
where pac.OWNER = :SchemaName and pac.TABLE_NAME = :TableName and ac.CONSTRAINT_TYPE = 'R' and pac.CONSTRAINT_TYPE in ('P', 'U')";

        public IReadOnlyDictionary<Identifier, IDatabaseCheckConstraint> Check => LoadCheckLookupSync();

        public Task<IReadOnlyDictionary<Identifier, IDatabaseCheckConstraint>> CheckAsync(CancellationToken cancellationToken = default(CancellationToken)) => LoadCheckLookupAsync(cancellationToken);

        public IReadOnlyCollection<IDatabaseCheckConstraint> Checks => LoadChecksSync();

        public Task<IReadOnlyCollection<IDatabaseCheckConstraint>> ChecksAsync(CancellationToken cancellationToken = default(CancellationToken)) => LoadChecksAsync(cancellationToken);

        protected virtual IReadOnlyDictionary<Identifier, IDatabaseCheckConstraint> LoadCheckLookupSync()
        {
            var checks = Checks;
            var result = new Dictionary<Identifier, IDatabaseCheckConstraint>(checks.Count);

            foreach (var check in checks)
                result[check.Name.LocalName] = check;

            return new IdentifierResolvingDictionary<IDatabaseCheckConstraint>(result, IdentifierResolver);
        }

        protected virtual async Task<IReadOnlyDictionary<Identifier, IDatabaseCheckConstraint>> LoadCheckLookupAsync(CancellationToken cancellationToken)
        {
            var checks = await ChecksAsync(cancellationToken).ConfigureAwait(false);
            var result = new Dictionary<Identifier, IDatabaseCheckConstraint>(checks.Count);

            foreach (var check in checks)
                result[check.Name.LocalName] = check;

            return new IdentifierResolvingDictionary<IDatabaseCheckConstraint>(result, IdentifierResolver);
        }

        protected virtual IReadOnlyCollection<IDatabaseCheckConstraint> LoadChecksSync()
        {
            var checks = Connection.Query<CheckConstraintData>(ChecksQuery, new { SchemaName = Name.Schema, TableName = Name.LocalName });
            if (checks.Empty())
                return Array.Empty<IDatabaseCheckConstraint>();

            var columnNotNullConstraints = Column.Keys
                .Select(k => k.LocalName)
                .Select(GenerateNotNullDefinition)
                .ToList();

            var result = new List<IDatabaseCheckConstraint>();

            foreach (var checkRow in checks)
            {
                if (columnNotNullConstraints.Contains(checkRow.Definition))
                    continue;

                var constraintName = Identifier.CreateQualifiedIdentifier(checkRow.ConstraintName);
                var definition = checkRow.Definition;
                var isEnabled = checkRow.EnabledStatus == "ENABLED";

                var check = new OracleCheckConstraint(constraintName, definition, isEnabled);
                result.Add(check);
            }

            return result;
        }

        protected virtual async Task<IReadOnlyCollection<IDatabaseCheckConstraint>> LoadChecksAsync(CancellationToken cancellationToken)
        {
            var checks = await Connection.QueryAsync<CheckConstraintData>(ChecksQuery, new { SchemaName = Name.Schema, TableName = Name.LocalName }).ConfigureAwait(false);
            if (checks.Empty())
                return Array.Empty<IDatabaseCheckConstraint>();

            var columnLookup = await ColumnAsync().ConfigureAwait(false);
            var columnNotNullConstraints = columnLookup.Keys
                .Select(k => k.LocalName)
                .Select(GenerateNotNullDefinition)
                .ToList();

            var result = new List<IDatabaseCheckConstraint>();

            foreach (var checkRow in checks)
            {
                if (columnNotNullConstraints.Contains(checkRow.Definition))
                    continue;

                var constraintName = Identifier.CreateQualifiedIdentifier(checkRow.ConstraintName);
                var definition = checkRow.Definition;
                var isEnabled = checkRow.EnabledStatus == "ENABLED";

                var check = new OracleCheckConstraint(constraintName, definition, isEnabled);
                result.Add(check);
            }

            return result;
        }

        protected virtual string ChecksQuery => ChecksQuerySql;

        private const string ChecksQuerySql = @"
select
    CONSTRAINT_NAME as ConstraintName,
    SEARCH_CONDITION as Definition,
    STATUS as EnabledStatus
from ALL_CONSTRAINTS
where OWNER = :SchemaName and TABLE_NAME = :TableName and CONSTRAINT_TYPE = 'C'";

        public IReadOnlyDictionary<Identifier, IDatabaseRelationalKey> ParentKey => LoadParentKeyLookupSync();

        public Task<IReadOnlyDictionary<Identifier, IDatabaseRelationalKey>> ParentKeyAsync(CancellationToken cancellationToken = default(CancellationToken)) => LoadParentKeyLookupAsync(cancellationToken);

        public IReadOnlyCollection<IDatabaseRelationalKey> ParentKeys => LoadParentKeysSync();

        public Task<IReadOnlyCollection<IDatabaseRelationalKey>> ParentKeysAsync(CancellationToken cancellationToken = default(CancellationToken)) => LoadParentKeysAsync(cancellationToken);

        protected virtual IReadOnlyDictionary<Identifier, IDatabaseRelationalKey> LoadParentKeyLookupSync()
        {
            var parentKeys = ParentKeys;
            var result = new Dictionary<Identifier, IDatabaseRelationalKey>(parentKeys.Count);

            foreach (var parentKey in parentKeys)
                result[parentKey.ChildKey.Name.LocalName] = parentKey;

            return new IdentifierResolvingDictionary<IDatabaseRelationalKey>(result, IdentifierResolver);
        }

        protected virtual async Task<IReadOnlyDictionary<Identifier, IDatabaseRelationalKey>> LoadParentKeyLookupAsync(CancellationToken cancellationToken)
        {
            var parentKeys = await ParentKeysAsync(cancellationToken).ConfigureAwait(false);
            var result = new Dictionary<Identifier, IDatabaseRelationalKey>(parentKeys.Count);

            foreach (var parentKey in parentKeys)
                result[parentKey.ChildKey.Name.LocalName] = parentKey;

            return new IdentifierResolvingDictionary<IDatabaseRelationalKey>(result, IdentifierResolver);
        }

        protected virtual IReadOnlyCollection<IDatabaseRelationalKey> LoadParentKeysSync()
        {
            var queryResult = Connection.Query<ForeignKeyData>(ParentKeysQuery, new { SchemaName = Name.Schema, TableName = Name.LocalName });
            if (queryResult.Empty())
                return Array.Empty<IDatabaseRelationalKey>();

            var foreignKeys = queryResult.GroupBy(row => new
            {
                row.ConstraintName,
                row.EnabledStatus,
                row.DeleteRule,
                row.ParentTableSchema,
                row.ParentTableName,
                row.ParentConstraintName,
                KeyType = row.ParentKeyType
            }).ToList();
            if (foreignKeys.Count == 0)
                return Array.Empty<IDatabaseRelationalKey>();

            var result = new List<IDatabaseRelationalKey>(foreignKeys.Count);
            foreach (var fkey in foreignKeys)
            {
                var parentTableName = Identifier.CreateQualifiedIdentifier(fkey.Key.ParentTableSchema, fkey.Key.ParentTableName);
                var parentTable = Database.GetTable(parentTableName);
                var parentKeyName = Identifier.CreateQualifiedIdentifier(fkey.Key.ParentConstraintName);

                IDatabaseKey parentKey;
                if (fkey.Key.KeyType == "P")
                {
                    parentKey = parentTable.PrimaryKey;
                }
                else
                {
                    var uniqueKeys = parentTable.UniqueKey;
                    parentKey = uniqueKeys[parentKeyName.LocalName];
                }

                var childKeyName = Identifier.CreateQualifiedIdentifier(fkey.Key.ConstraintName);
                var childKeyColumnLookup = Column;
                var childKeyColumns = fkey
                    .OrderBy(row => row.ColumnPosition)
                    .Select(row => childKeyColumnLookup[row.ColumnName])
                    .ToList();

                var isEnabled = fkey.Key.EnabledStatus == "ENABLED";
                var childKey = new OracleDatabaseKey(childKeyName, DatabaseKeyType.Foreign, childKeyColumns, isEnabled);

                var deleteRule = RelationalRuleMapping[fkey.Key.DeleteRule];
                var relationalKey = new OracleRelationalKey(Name, childKey, parentTableName, parentKey, deleteRule);
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
                row.ConstraintName,
                row.EnabledStatus,
                row.DeleteRule,
                row.ParentTableSchema,
                row.ParentTableName,
                row.ParentConstraintName,
                KeyType = row.ParentKeyType,
            }).ToList();
            if (foreignKeys.Count == 0)
                return Array.Empty<IDatabaseRelationalKey>();

            var result = new List<IDatabaseRelationalKey>(foreignKeys.Count);
            foreach (var fkey in foreignKeys)
            {
                var parentTableName = Identifier.CreateQualifiedIdentifier(fkey.Key.ParentTableSchema, fkey.Key.ParentTableName);
                var parentTable = await Database.GetTableAsync(parentTableName, cancellationToken).ConfigureAwait(false);
                var parentKeyName = Identifier.CreateQualifiedIdentifier(fkey.Key.ParentConstraintName);

                IDatabaseKey parentKey;
                if (fkey.Key.KeyType == "P")
                {
                    parentKey = parentTable.PrimaryKey;
                }
                else
                {
                    var uniqueKeys = await parentTable.UniqueKeyAsync(cancellationToken).ConfigureAwait(false);
                    parentKey = uniqueKeys[parentKeyName.LocalName];
                }

                var childKeyName = Identifier.CreateQualifiedIdentifier(fkey.Key.ConstraintName);
                var childKeyColumnLookup = await ColumnAsync(cancellationToken).ConfigureAwait(false);
                var childKeyColumns = fkey
                    .OrderBy(row => row.ColumnPosition)
                    .Select(row => childKeyColumnLookup[row.ColumnName])
                    .ToList();

                var isEnabled = fkey.Key.EnabledStatus == "ENABLED";
                var childKey = new OracleDatabaseKey(childKeyName, DatabaseKeyType.Foreign, childKeyColumns, isEnabled);

                var deleteRule = RelationalRuleMapping[fkey.Key.DeleteRule];
                var relationalKey = new OracleRelationalKey(Name, childKey, parentTableName, parentKey, deleteRule);
                result.Add(relationalKey);
            }

            return result;
        }

        protected virtual string ParentKeysQuery => ParentKeysQuerySql;

        private const string ParentKeysQuerySql = @"
select
    ac.CONSTRAINT_NAME as ConstraintName,
    ac.STATUS as EnabledStatus,
    ac.DELETE_RULE as DeleteRule,
    pac.OWNER as ParentTableSchema,
    pac.TABLE_NAME as ParentTableName,
    pac.CONSTRAINT_NAME as ParentConstraintName,
    pac.CONSTRAINT_TYPE as ParentKeyType,
    acc.COLUMN_NAME as ColumnName,
    acc.POSITION as ColumnPosition
from ALL_CONSTRAINTS ac
inner join ALL_CONS_COLUMNS acc on ac.OWNER = acc.OWNER and ac.CONSTRAINT_NAME = acc.CONSTRAINT_NAME and ac.TABLE_NAME = acc.TABLE_NAME
inner join ALL_CONSTRAINTS pac on pac.OWNER = ac.R_OWNER and pac.CONSTRAINT_NAME = ac.R_CONSTRAINT_NAME
where ac.OWNER = :SchemaName and ac.TABLE_NAME = :TableName and ac.CONSTRAINT_TYPE = 'R' and pac.CONSTRAINT_TYPE in ('P', 'U')";

        public IReadOnlyList<IDatabaseColumn> Columns => LoadColumnsSync();

        public Task<IReadOnlyList<IDatabaseColumn>> ColumnsAsync(CancellationToken cancellationToken = default(CancellationToken)) => LoadColumnsAsync(cancellationToken);

        public IReadOnlyDictionary<Identifier, IDatabaseColumn> Column => LoadColumnLookupSync();

        public Task<IReadOnlyDictionary<Identifier, IDatabaseColumn>> ColumnAsync(CancellationToken cancellationToken = default(CancellationToken)) => LoadColumnLookupAsync(cancellationToken);

        protected virtual IReadOnlyDictionary<Identifier, IDatabaseColumn> LoadColumnLookupSync()
        {
            var columns = Columns;
            var result = new Dictionary<Identifier, IDatabaseColumn>(columns.Count);

            foreach (var column in columns.Where(c => c.Name != null))
                result[column.Name.LocalName] = column;

            return new IdentifierResolvingDictionary<IDatabaseColumn>(result, IdentifierResolver);
        }

        protected virtual async Task<IReadOnlyDictionary<Identifier, IDatabaseColumn>> LoadColumnLookupAsync(CancellationToken cancellationToken)
        {
            var columns = await ColumnsAsync(cancellationToken).ConfigureAwait(false);
            var result = new Dictionary<Identifier, IDatabaseColumn>(columns.Count);

            foreach (var column in columns.Where(c => c.Name != null))
                result[column.Name.LocalName] = column;

            return new IdentifierResolvingDictionary<IDatabaseColumn>(result, IdentifierResolver);
        }

        protected virtual IReadOnlyList<IDatabaseColumn> LoadColumnsSync()
        {
            var query = Connection.Query<ColumnData>(ColumnsQuery, new { SchemaName = Name.Schema, TableName = Name.LocalName });

            var columnNames = query.Select(row => row.ColumnName).ToList();
            var notNullableColumnNames = GetNotNullConstrainedColumns(columnNames);
            var result = new List<IDatabaseColumn>();

            foreach (var row in query)
            {
                var typeMetadata = new ColumnTypeMetadata
                {
                    TypeName = Identifier.CreateQualifiedIdentifier(row.ColumnTypeSchema, row.ColumnTypeName),
                    Collation = row.Collation.IsNullOrWhiteSpace() ? null : Identifier.CreateQualifiedIdentifier(row.Collation),
                    MaxLength = row.DataLength,
                    NumericPrecision = new NumericPrecision(row.Precision, row.Scale)
                };
                var columnType = TypeProvider.CreateColumnType(typeMetadata);

                var isNullable = !notNullableColumnNames.Contains(row.ColumnName);
                var columnName = Identifier.CreateQualifiedIdentifier(row.ColumnName);

                var column = row.IsComputed == "YES"
                    ? new OracleDatabaseComputedColumn(columnName, columnType, isNullable, row.DefaultValue)
                    : new OracleDatabaseColumn(columnName, columnType, isNullable, row.DefaultValue);

                result.Add(column);
            }

            return result.AsReadOnly();
        }

        protected virtual async Task<IReadOnlyList<IDatabaseColumn>> LoadColumnsAsync(CancellationToken cancellationToken)
        {
            var query = await Connection.QueryAsync<ColumnData>(ColumnsQuery, new { SchemaName = Name.Schema, TableName = Name.LocalName }).ConfigureAwait(false);

            var columnNames = query.Select(row => row.ColumnName).ToList();
            var notNullableColumnNames = await GetNotNullConstrainedColumnsAsync(columnNames).ConfigureAwait(false);
            var result = new List<IDatabaseColumn>();

            foreach (var row in query)
            {
                var typeMetadata = new ColumnTypeMetadata
                {
                    TypeName = Identifier.CreateQualifiedIdentifier(row.ColumnTypeSchema, row.ColumnTypeName),
                    Collation = row.Collation.IsNullOrWhiteSpace() ? null : Identifier.CreateQualifiedIdentifier(row.Collation),
                    MaxLength = row.DataLength,
                    NumericPrecision = new NumericPrecision(row.Precision, row.Scale)
                };
                var columnType = TypeProvider.CreateColumnType(typeMetadata);

                var isNullable = !notNullableColumnNames.Contains(row.ColumnName);
                var columnName = Identifier.CreateQualifiedIdentifier(row.ColumnName);

                var column = row.IsComputed == "YES"
                    ? new OracleDatabaseComputedColumn(columnName, columnType, isNullable, row.DefaultValue)
                    : new OracleDatabaseColumn(columnName, columnType, isNullable, row.DefaultValue);

                result.Add(column);
            }

            return result.AsReadOnly();
        }

        protected virtual string ColumnsQuery => ColumnsQuerySql;

        private const string ColumnsQuerySql = @"
select
    COLUMN_NAME as ColumnName,
    DATA_TYPE_OWNER as ColumnTypeSchema,
    DATA_TYPE as ColumnTypeName,
    DATA_LENGTH as DataLength,
    DATA_PRECISION as Precision,
    DATA_SCALE as Scale,
    DATA_DEFAULT as DefaultValue,
    CHAR_LENGTH as CharacterLength,
    CHARACTER_SET_NAME as Collation,
    VIRTUAL_COLUMN as IsComputed
from ALL_TAB_COLS
where OWNER = :SchemaName and TABLE_NAME = :TableName
order by COLUMN_ID";

        public IReadOnlyDictionary<Identifier, IDatabaseTrigger> Trigger => LoadTriggerLookupSync();

        public IReadOnlyCollection<IDatabaseTrigger> Triggers => LoadTriggersSync();

        public Task<IReadOnlyDictionary<Identifier, IDatabaseTrigger>> TriggerAsync(CancellationToken cancellationToken = default(CancellationToken)) => LoadTriggerLookupAsync(cancellationToken);

        public Task<IReadOnlyCollection<IDatabaseTrigger>> TriggersAsync(CancellationToken cancellationToken = default(CancellationToken)) => LoadTriggersAsync(cancellationToken);

        protected virtual IReadOnlyDictionary<Identifier, IDatabaseTrigger> LoadTriggerLookupSync()
        {
            var triggers = Triggers;
            var result = new Dictionary<Identifier, IDatabaseTrigger>(triggers.Count);

            foreach (var trigger in triggers)
                result[trigger.Name.LocalName] = trigger;

            return new IdentifierResolvingDictionary<IDatabaseTrigger>(result, IdentifierResolver);
        }

        protected virtual async Task<IReadOnlyDictionary<Identifier, IDatabaseTrigger>> LoadTriggerLookupAsync(CancellationToken cancellationToken)
        {
            var triggers = await TriggersAsync(cancellationToken).ConfigureAwait(false);
            var result = new Dictionary<Identifier, IDatabaseTrigger>(triggers.Count);

            foreach (var trigger in triggers)
                result[trigger.Name.LocalName] = trigger;

            return new IdentifierResolvingDictionary<IDatabaseTrigger>(result, IdentifierResolver);
        }

        protected virtual IReadOnlyCollection<IDatabaseTrigger> LoadTriggersSync()
        {
            var queryResult = Connection.Query<TriggerData>(TriggersQuery, new { SchemaName = Name.Schema, TableName = Name.LocalName });
            if (queryResult.Empty())
                return Array.Empty<IDatabaseTrigger>();

            var triggers = queryResult.ToList();
            if (triggers.Count == 0)
                return Array.Empty<IDatabaseTrigger>();

            var result = new List<IDatabaseTrigger>(triggers.Count);
            foreach (var triggerRow in triggers)
            {
                var triggerName = Identifier.CreateQualifiedIdentifier(triggerRow.TriggerSchema, triggerRow.TriggerName);
                var queryTiming = TimingMapping[triggerRow.TriggerType];
                var definition = triggerRow.Definition;
                var isEnabled = triggerRow.EnabledStatus == "ENABLED";

                var events = TriggerEvent.None;
                var triggerEventPieces = triggerRow.TriggerEvent.Split(new[] { " OR " }, StringSplitOptions.RemoveEmptyEntries);

                foreach (var triggerEventPiece in triggerEventPieces)
                {
                    if (triggerEventPiece == "INSERT")
                        events |= TriggerEvent.Insert;
                    else if (triggerEventPiece == "UPDATE")
                        events |= TriggerEvent.Update;
                    else if (triggerEventPiece == "DELETE")
                        events |= TriggerEvent.Delete;
                    else
                        throw new Exception("Found an unsupported trigger event name. Expected one of INSERT, UPDATE, DELETE, got: " + triggerEventPiece);
                }

                var trigger = new OracleDatabaseTrigger(triggerName, definition, queryTiming, events, isEnabled);
                result.Add(trigger);
            }

            return result;
        }

        protected virtual async Task<IReadOnlyCollection<IDatabaseTrigger>> LoadTriggersAsync(CancellationToken cancellationToken)
        {
            var queryResult = await Connection.QueryAsync<TriggerData>(TriggersQuery, new { SchemaName = Name.Schema, TableName = Name.LocalName }).ConfigureAwait(false);
            if (queryResult.Empty())
                return Array.Empty<IDatabaseTrigger>();

            var triggers = queryResult.ToList();
            if (triggers.Count == 0)
                return Array.Empty<IDatabaseTrigger>();

            var result = new List<IDatabaseTrigger>(triggers.Count);
            foreach (var triggerRow in triggers)
            {
                var triggerName = Identifier.CreateQualifiedIdentifier(triggerRow.TriggerSchema, triggerRow.TriggerName);
                var queryTiming = TimingMapping[triggerRow.TriggerType];
                var definition = triggerRow.Definition;
                var isEnabled = triggerRow.EnabledStatus == "ENABLED";

                var events = TriggerEvent.None;
                var triggerEventPieces = triggerRow.TriggerEvent.Split(new[] { " OR " }, StringSplitOptions.RemoveEmptyEntries);

                foreach (var triggerEventPiece in triggerEventPieces)
                {
                    if (triggerEventPiece == "INSERT")
                        events |= TriggerEvent.Insert;
                    else if (triggerEventPiece == "UPDATE")
                        events |= TriggerEvent.Update;
                    else if (triggerEventPiece == "DELETE")
                        events |= TriggerEvent.Delete;
                    else
                        throw new Exception("Found an unsupported trigger event name. Expected one of INSERT, UPDATE, DELETE, got: " + triggerEventPiece);
                }

                var trigger = new OracleDatabaseTrigger(triggerName, definition, queryTiming, events, isEnabled);
                result.Add(trigger);
            }

            return result;
        }

        protected virtual string TriggersQuery => TriggersQuerySql;

        private const string TriggersQuerySql = @"
select
    OWNER as TriggerSchema,
    TRIGGER_NAME as TriggerName,
    TRIGGER_TYPE as TriggerType,
    TRIGGERING_EVENT as TriggerEvent,
    TRIGGER_BODY as Definition,
    STATUS as EnabledStatus
from ALL_TRIGGERS
where TABLE_OWNER = :SchemaName and TABLE_NAME = :TableName and BASE_OBJECT_TYPE = 'TABLE'";

        protected IEnumerable<string> GetNotNullConstrainedColumns(IEnumerable<string> columnNames)
        {
            if (columnNames == null)
                throw new ArgumentNullException(nameof(columnNames));

            var checks = Connection.Query<CheckConstraintData>(ChecksQuery, new { SchemaName = Name.Schema, TableName = Name.LocalName });
            if (checks.Empty())
                return Array.Empty<string>();

            var columnNotNullConstraints = columnNames
                .Select(name => new KeyValuePair<string, string>(GenerateNotNullDefinition(name), name))
                .ToDictionary();

            return checks
                .Where(c => columnNotNullConstraints.ContainsKey(c.Definition) && c.EnabledStatus == "ENABLED")
                .Select(c => columnNotNullConstraints[c.Definition])
                .ToList();
        }

        protected Task<IEnumerable<string>> GetNotNullConstrainedColumnsAsync(IEnumerable<string> columnNames)
        {
            if (columnNames == null)
                throw new ArgumentNullException(nameof(columnNames));

            return GetNotNullConstrainedColumnsAsyncCore(columnNames);
        }

        private async Task<IEnumerable<string>> GetNotNullConstrainedColumnsAsyncCore(IEnumerable<string> columnNames)
        {
            var checks = await Connection.QueryAsync<CheckConstraintData>(ChecksQuery, new { SchemaName = Name.Schema, TableName = Name.LocalName }).ConfigureAwait(false);
            if (checks.Empty())
                return Array.Empty<string>();

            var columnNotNullConstraints = columnNames
                .Select(name => new KeyValuePair<string, string>(GenerateNotNullDefinition(name), name))
                .ToDictionary();

            return checks
                .Where(c => columnNotNullConstraints.ContainsKey(c.Definition) && c.EnabledStatus == "ENABLED")
                .Select(c => columnNotNullConstraints[c.Definition])
                .ToList();
        }

        private static string GenerateNotNullDefinition(string columnName)
        {
            if (columnName.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(columnName));

            return "\"" + columnName + "\" IS NOT NULL";
        }

        protected IReadOnlyDictionary<string, Rule> RelationalRuleMapping { get; } = new Dictionary<string, Rule>(StringComparer.OrdinalIgnoreCase)
        {
            ["NO ACTION"] = Rule.None,
            ["CASCADE"] = Rule.Cascade,
            ["SET NULL"] = Rule.SetNull,
            ["SET DEFAULT"] = Rule.SetDefault
        };

        protected IReadOnlyDictionary<string, TriggerQueryTiming> TimingMapping { get; } = new Dictionary<string, TriggerQueryTiming>(StringComparer.OrdinalIgnoreCase)
        {
            ["BEFORE STATEMENT"] = TriggerQueryTiming.Before,
            ["BEFORE EACH ROW"] = TriggerQueryTiming.Before,
            ["AFTER STATEMENT"] = TriggerQueryTiming.After,
            ["AFTER EACH ROW"] = TriggerQueryTiming.After,
            ["INSTEAD OF"] = TriggerQueryTiming.InsteadOf,
            ["COMPOUND"] = TriggerQueryTiming.InsteadOf
        };
    }
}
