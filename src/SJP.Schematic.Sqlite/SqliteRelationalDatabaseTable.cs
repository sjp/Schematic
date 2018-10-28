using Dapper;
using Superpower;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using SJP.Schematic.Core;
using SJP.Schematic.Sqlite.Query;
using SJP.Schematic.Sqlite.Parsing;
using SJP.Schematic.Sqlite.Pragma;
using System.Threading;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Sqlite
{
    public class SqliteRelationalDatabaseTable : IRelationalDatabaseTable
    {
        public SqliteRelationalDatabaseTable(IDbConnection connection, IRelationalDatabase database, Identifier tableName)
        {
            Connection = connection ?? throw new ArgumentNullException(nameof(connection));

            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));
            if (tableName.Schema == null)
                throw new ArgumentException("The given table name is missing a required schema name.", nameof(tableName));

            Name = tableName.Database == null
                ? tableName
                : Identifier.CreateQualifiedIdentifier(tableName.Schema, tableName.LocalName);

            Database = database ?? throw new ArgumentNullException(nameof(database));
            if (database.Dialect == null)
                throw new ArgumentException("The given database object does not contain a dialect.", nameof(database));
            Dialect = database.Dialect;

            Comparer = new IdentifierComparer(StringComparer.OrdinalIgnoreCase, defaultSchema: Database.DefaultSchema);
            Pragma = new DatabasePragma(Dialect, connection, tableName.Schema);
        }

        protected IRelationalDatabase Database { get; }

        protected IDatabaseDialect Dialect { get; }

        protected IDbConnection Connection { get; }

        protected IEqualityComparer<Identifier> Comparer { get; }

        protected ISqliteDatabasePragma Pragma { get; }

        public Identifier Name { get; }

        public IDatabaseKey PrimaryKey => LoadPrimaryKeySync();

        public Task<IDatabaseKey> PrimaryKeyAsync(CancellationToken cancellationToken = default(CancellationToken)) => LoadPrimaryKeyAsync(cancellationToken);

        protected virtual IDatabaseKey LoadPrimaryKeySync()
        {
            var tableInfos = Pragma.TableInfo(Name);
            if (tableInfos.Empty())
                return null;

            var pkColumns = tableInfos
                .Where(ti => ti.pk > 0)
                .OrderBy(ti => ti.pk)
                .ToList();
            if (pkColumns.Count == 0)
                return null;

            var tableColumn = Column;
            var columns = pkColumns.Select(c => tableColumn[c.name]).ToList();

            var parser = ParsedDefinition;
            var pkConstraint = parser.PrimaryKey;

            var pkStringName = pkConstraint?.Name;
            var primaryKeyName = !pkStringName.IsNullOrWhiteSpace() ? Identifier.CreateQualifiedIdentifier(pkStringName) : null;
            return new SqliteDatabaseKey(primaryKeyName, DatabaseKeyType.Primary, columns);
        }

        protected virtual async Task<IDatabaseKey> LoadPrimaryKeyAsync(CancellationToken cancellationToken)
        {
            var tableInfos = await Pragma.TableInfoAsync(Name, cancellationToken).ConfigureAwait(false);
            if (tableInfos.Empty())
                return null;

            var pkColumns = tableInfos
                .Where(ti => ti.pk > 0)
                .OrderBy(ti => ti.pk)
                .ToList();
            if (pkColumns.Count == 0)
                return null;

            var tableColumn = await ColumnAsync(cancellationToken).ConfigureAwait(false);
            var columns = pkColumns.Select(c => tableColumn[c.name]).ToList();

            var parser = await ParsedDefinitionAsync(cancellationToken).ConfigureAwait(false);
            var pkConstraint = parser.PrimaryKey;

            var pkStringName = pkConstraint?.Name;
            var primaryKeyName = !pkStringName.IsNullOrWhiteSpace() ? Identifier.CreateQualifiedIdentifier(pkStringName) : null;
            return new SqliteDatabaseKey(primaryKeyName, DatabaseKeyType.Primary, columns);
        }

        public IReadOnlyDictionary<Identifier, IDatabaseIndex> Index => LoadIndexLookupSync();

        public Task<IReadOnlyDictionary<Identifier, IDatabaseIndex>> IndexAsync(CancellationToken cancellationToken = default(CancellationToken)) => LoadIndexLookupAsync(cancellationToken);

        public IReadOnlyCollection<IDatabaseIndex> Indexes => LoadIndexesSync();

        public Task<IReadOnlyCollection<IDatabaseIndex>> IndexesAsync(CancellationToken cancellationToken = default(CancellationToken)) => LoadIndexesAsync(cancellationToken);

        protected virtual IReadOnlyDictionary<Identifier, IDatabaseIndex> LoadIndexLookupSync()
        {
            var indexes = Indexes;
            var result = new Dictionary<Identifier, IDatabaseIndex>(indexes.Count, Comparer);

            var namedIndexes = indexes.Where(i => i.Name != null);
            foreach (var index in namedIndexes)
                result[index.Name.LocalName] = index;

            return result;
        }

        protected virtual async Task<IReadOnlyDictionary<Identifier, IDatabaseIndex>> LoadIndexLookupAsync(CancellationToken cancellationToken)
        {
            var indexes = await IndexesAsync(cancellationToken).ConfigureAwait(false);
            var result = new Dictionary<Identifier, IDatabaseIndex>(indexes.Count, Comparer);

            var namedIndexes = indexes.Where(i => i.Name != null);
            foreach (var index in namedIndexes)
                result[index.Name.LocalName] = index;

            return result;
        }

        protected virtual IReadOnlyCollection<IDatabaseIndex> LoadIndexesSync()
        {
            var indexLists = Pragma.IndexList(Name);
            if (indexLists.Empty())
                return Array.Empty<IDatabaseIndex>();

            var nonConstraintIndexLists = indexLists.Where(i => i.origin == "c").ToList();
            if (nonConstraintIndexLists.Count == 0)
                return Array.Empty<IDatabaseIndex>();

            var result = new List<IDatabaseIndex>(nonConstraintIndexLists.Count);

            foreach (var indexList in nonConstraintIndexLists)
            {
                var indexInfo = Pragma.IndexXInfo(indexList.name);
                var indexColumns = indexInfo
                    .Where(i => i.key && i.cid >= 0)
                    .OrderBy(i => i.seqno)
                    .Select(i => new SqliteDatabaseIndexColumn(Column[i.name], i.desc ? IndexColumnOrder.Descending : IndexColumnOrder.Ascending))
                    .ToList();

                var includedColumns = indexInfo
                    .Where(i => !i.key && i.cid >= 0)
                    .OrderBy(i => i.name)
                    .Select(i => Column[i.name])
                    .ToList();

                var index = new SqliteDatabaseIndex(indexList.name, indexList.unique, indexColumns, includedColumns);
                result.Add(index);
            }

            return result;
        }

        protected virtual async Task<IReadOnlyCollection<IDatabaseIndex>> LoadIndexesAsync(CancellationToken cancellationToken)
        {
            var indexLists = await Pragma.IndexListAsync(Name, cancellationToken).ConfigureAwait(false);
            if (indexLists.Empty())
                return Array.Empty<IDatabaseIndex>();

            var nonConstraintIndexLists = indexLists.Where(i => i.origin == "c").ToList();
            if (nonConstraintIndexLists.Count == 0)
                return Array.Empty<IDatabaseIndex>();

            var result = new List<IDatabaseIndex>(nonConstraintIndexLists.Count);

            foreach (var indexList in nonConstraintIndexLists)
            {
                var indexInfo = await Pragma.IndexXInfoAsync(indexList.name, cancellationToken).ConfigureAwait(false);
                var indexColumns = indexInfo
                    .Where(i => i.key && i.cid >= 0)
                    .OrderBy(i => i.seqno)
                    .Select(i => new SqliteDatabaseIndexColumn(Column[i.name], i.desc ? IndexColumnOrder.Descending : IndexColumnOrder.Ascending))
                    .ToList();

                var includedColumns = indexInfo
                    .Where(i => !i.key && i.cid >= 0)
                    .OrderBy(i => i.name)
                    .Select(i => Column[i.name])
                    .ToList();

                var index = new SqliteDatabaseIndex(indexList.name, indexList.unique, indexColumns, includedColumns);
                result.Add(index);
            }

            return result;
        }

        public IReadOnlyDictionary<Identifier, IDatabaseKey> UniqueKey => LoadUniqueKeyLookupSync();

        public Task<IReadOnlyDictionary<Identifier, IDatabaseKey>> UniqueKeyAsync(CancellationToken cancellationToken = default(CancellationToken)) => LoadUniqueKeyLookupAsync(cancellationToken);

        public IReadOnlyCollection<IDatabaseKey> UniqueKeys => LoadUniqueKeysSync();

        public Task<IReadOnlyCollection<IDatabaseKey>> UniqueKeysAsync(CancellationToken cancellationToken = default(CancellationToken)) => LoadUniqueKeysAsync(cancellationToken);

        protected virtual IReadOnlyDictionary<Identifier, IDatabaseKey> LoadUniqueKeyLookupSync()
        {
            var uniqueKeys = UniqueKeys;
            var result = new Dictionary<Identifier, IDatabaseKey>(uniqueKeys.Count, Comparer);

            var namedUniqueKeys = uniqueKeys.Where(uk => uk.Name != null);
            foreach (var uk in namedUniqueKeys)
                result[uk.Name.LocalName] = uk;

            return result;
        }

        protected virtual async Task<IReadOnlyDictionary<Identifier, IDatabaseKey>> LoadUniqueKeyLookupAsync(CancellationToken cancellationToken)
        {
            var uniqueKeys = await UniqueKeysAsync(cancellationToken).ConfigureAwait(false);
            var result = new Dictionary<Identifier, IDatabaseKey>(uniqueKeys.Count, Comparer);

            var namedUniqueKeys = uniqueKeys.Where(uk => uk.Name != null);
            foreach (var uk in namedUniqueKeys)
                result[uk.Name.LocalName] = uk;

            return result;
        }

        protected virtual IReadOnlyCollection<IDatabaseKey> LoadUniqueKeysSync()
        {
            var indexLists = Pragma.IndexList(Name);
            if (indexLists.Empty())
                return Array.Empty<IDatabaseKey>();

            var ukIndexLists = indexLists
                .Where(i => i.origin == "u" && i.unique)
                .ToList();
            if (ukIndexLists.Count == 0)
                return Array.Empty<IDatabaseKey>();

            var result = new List<IDatabaseKey>(ukIndexLists.Count);

            var parser = ParsedDefinition;
            var parsedUniqueConstraints = parser.UniqueKeys;

            var tableColumn = Column;
            foreach (var ukIndexList in ukIndexLists)
            {
                var indexXInfos = Pragma.IndexXInfo(ukIndexList.name);
                var orderedColumns = indexXInfos
                    .Where(i => i.key && i.cid >= 0)
                    .OrderBy(i => i.seqno)
                    .ToList();
                var columnNames = orderedColumns.Select(i => i.name).ToList();
                var columns = orderedColumns.Select(i => tableColumn[i.name]).ToList();

                var uniqueConstraint = parsedUniqueConstraints
                    .FirstOrDefault(constraint => constraint.Columns.Select(c => c.Name).SequenceEqual(columnNames));
                var stringConstraintName = uniqueConstraint?.Name;

                var keyName = !stringConstraintName.IsNullOrWhiteSpace() ? Identifier.CreateQualifiedIdentifier(stringConstraintName) : null;
                var uniqueKey = new SqliteDatabaseKey(keyName, DatabaseKeyType.Unique, columns);
                result.Add(uniqueKey);
            }

            return result;
        }

        protected virtual async Task<IReadOnlyCollection<IDatabaseKey>> LoadUniqueKeysAsync(CancellationToken cancellationToken)
        {
            var indexLists = await Pragma.IndexListAsync(Name, cancellationToken).ConfigureAwait(false);
            if (indexLists.Empty())
                return Array.Empty<IDatabaseKey>();

            var ukIndexLists = indexLists
                .Where(i => i.origin == "u" && i.unique)
                .ToList();
            if (ukIndexLists.Count == 0)
                return Array.Empty<IDatabaseKey>();

            var result = new List<IDatabaseKey>(ukIndexLists.Count);

            var parser = await ParsedDefinitionAsync(cancellationToken).ConfigureAwait(false);
            var parsedUniqueConstraints = parser.UniqueKeys;

            var tableColumn = await ColumnAsync(cancellationToken).ConfigureAwait(false);
            foreach (var ukIndexList in ukIndexLists)
            {
                var indexXInfos = await Pragma.IndexXInfoAsync(ukIndexList.name, cancellationToken).ConfigureAwait(false);
                var orderedColumns = indexXInfos
                    .Where(i => i.key && i.cid >= 0)
                    .OrderBy(i => i.seqno)
                    .ToList();
                var columnNames = orderedColumns.Select(i => i.name).ToList();
                var columns = orderedColumns.Select(i => tableColumn[i.name]).ToList();

                var uniqueConstraint = parsedUniqueConstraints
                    .FirstOrDefault(constraint => constraint.Columns.Select(c => c.Name).SequenceEqual(columnNames));
                var stringConstraintName = uniqueConstraint?.Name;

                var keyName = !stringConstraintName.IsNullOrWhiteSpace() ? Identifier.CreateQualifiedIdentifier(stringConstraintName) : null;
                var uniqueKey = new SqliteDatabaseKey(keyName, DatabaseKeyType.Unique, columns);
                result.Add(uniqueKey);
            }

            return result;
        }

        public IReadOnlyCollection<IDatabaseRelationalKey> ChildKeys => LoadChildKeysSync();

        public Task<IReadOnlyCollection<IDatabaseRelationalKey>> ChildKeysAsync(CancellationToken cancellationToken = default(CancellationToken)) => LoadChildKeysAsync(cancellationToken);

        protected virtual IReadOnlyCollection<IDatabaseRelationalKey> LoadChildKeysSync()
        {
            return Database.Tables
                .Where(t => string.Equals(t.Name.Schema, Name.Schema, StringComparison.OrdinalIgnoreCase))
                .SelectMany(t => t.ParentKeys)
                .Where(fk => Comparer.Equals(Name, fk.ParentTable))
                .ToList();
        }

        protected virtual async Task<IReadOnlyCollection<IDatabaseRelationalKey>> LoadChildKeysAsync(CancellationToken cancellationToken)
        {
            var dbTableCollections = await Database.TablesAsync(cancellationToken).ConfigureAwait(false);
            var dbTables = await Task.WhenAll(dbTableCollections).ConfigureAwait(false);

            var parentKeyCollectionTasks = dbTables
                .Where(t => string.Equals(t.Name.Schema, Name.Schema, StringComparison.OrdinalIgnoreCase))
                .Select(t => t.ParentKeysAsync())
                .ToList();
            var parentKeyCollections = await Task.WhenAll(parentKeyCollectionTasks).ConfigureAwait(false);

            var childKeys = parentKeyCollections
                .SelectMany(pkc => pkc)
                .Where(fk => Comparer.Equals(Name, fk.ParentTable))
                .ToList();

            return childKeys;
        }

        public IReadOnlyDictionary<Identifier, IDatabaseCheckConstraint> Check => LoadCheckLookupSync();

        public Task<IReadOnlyDictionary<Identifier, IDatabaseCheckConstraint>> CheckAsync(CancellationToken cancellationToken = default(CancellationToken)) => LoadCheckLookupAsync(cancellationToken);

        public IReadOnlyCollection<IDatabaseCheckConstraint> Checks => LoadChecksSync();

        public Task<IReadOnlyCollection<IDatabaseCheckConstraint>> ChecksAsync(CancellationToken cancellationToken = default(CancellationToken)) => LoadChecksAsync(cancellationToken);

        protected virtual IReadOnlyDictionary<Identifier, IDatabaseCheckConstraint> LoadCheckLookupSync()
        {
            var checks = Checks;
            var result = new Dictionary<Identifier, IDatabaseCheckConstraint>(checks.Count, Comparer);

            var namedChecks = checks.Where(c => c.Name != null);
            foreach (var check in namedChecks)
                result[check.Name.LocalName] = check;

            return result;
        }

        protected virtual async Task<IReadOnlyDictionary<Identifier, IDatabaseCheckConstraint>> LoadCheckLookupAsync(CancellationToken cancellationToken)
        {
            var checks = await ChecksAsync(cancellationToken).ConfigureAwait(false);
            var result = new Dictionary<Identifier, IDatabaseCheckConstraint>(checks.Count, Comparer);

            var namedChecks = checks.Where(c => c.Name != null);
            foreach (var check in namedChecks)
                result[check.Name.LocalName] = check;

            return result;
        }

        protected virtual IReadOnlyCollection<IDatabaseCheckConstraint> LoadChecksSync()
        {
            var parser = ParsedDefinition;
            var checks = parser.Checks.ToList();
            if (checks.Count == 0)
                return Array.Empty<IDatabaseCheckConstraint>();

            var result = new List<IDatabaseCheckConstraint>(checks.Count);

            foreach (var ck in checks)
            {
                var startIndex = ck.Definition.First().Position.Absolute;
                var lastToken = ck.Definition.Last();
                var endIndex = lastToken.Position.Absolute + lastToken.ToStringValue().Length;

                var definition = parser.Definition.Substring(startIndex, endIndex - startIndex);
                var check = new SqliteCheckConstraint(ck.Name, definition);
                result.Add(check);
            }

            return result;
        }

        protected virtual async Task<IReadOnlyCollection<IDatabaseCheckConstraint>> LoadChecksAsync(CancellationToken cancellationToken)
        {
            var parser = await ParsedDefinitionAsync(cancellationToken).ConfigureAwait(false);
            var checks = parser.Checks.ToList();
            if (checks.Count == 0)
                return Array.Empty<IDatabaseCheckConstraint>();

            var result = new List<IDatabaseCheckConstraint>(checks.Count);

            foreach (var ck in checks)
            {
                var startIndex = ck.Definition.First().Position.Absolute;
                var lastToken = ck.Definition.Last();
                var endIndex = lastToken.Position.Absolute + lastToken.ToStringValue().Length;

                var definition = parser.Definition.Substring(startIndex, endIndex - startIndex);
                var check = new SqliteCheckConstraint(ck.Name, definition);
                result.Add(check);
            }

            return result;
        }

        public IReadOnlyDictionary<Identifier, IDatabaseRelationalKey> ParentKey => LoadParentKeyLookupSync();

        public Task<IReadOnlyDictionary<Identifier, IDatabaseRelationalKey>> ParentKeyAsync(CancellationToken cancellationToken = default(CancellationToken)) => LoadParentKeyLookupAsync(cancellationToken);

        public IReadOnlyCollection<IDatabaseRelationalKey> ParentKeys => LoadParentKeysSync();

        public Task<IReadOnlyCollection<IDatabaseRelationalKey>> ParentKeysAsync(CancellationToken cancellationToken = default(CancellationToken)) => LoadParentKeysAsync(cancellationToken);

        protected virtual IReadOnlyDictionary<Identifier, IDatabaseRelationalKey> LoadParentKeyLookupSync()
        {
            var parentKeys = ParentKeys;
            var result = new Dictionary<Identifier, IDatabaseRelationalKey>(parentKeys.Count, Comparer);

            var namedParentKeys = parentKeys.Where(fk => fk.ChildKey.Name != null);
            foreach (var parentKey in namedParentKeys)
                result[parentKey.ChildKey.Name.LocalName] = parentKey;

            return result;
        }

        protected virtual async Task<IReadOnlyDictionary<Identifier, IDatabaseRelationalKey>> LoadParentKeyLookupAsync(CancellationToken cancellationToken)
        {
            var parentKeys = await ParentKeysAsync(cancellationToken).ConfigureAwait(false);
            var result = new Dictionary<Identifier, IDatabaseRelationalKey>(parentKeys.Count, Comparer);

            var namedParentKeys = parentKeys.Where(fk => fk.ChildKey.Name != null);
            foreach (var parentKey in namedParentKeys)
                result[parentKey.ChildKey.Name.LocalName] = parentKey;

            return result;
        }

        protected virtual IReadOnlyCollection<IDatabaseRelationalKey> LoadParentKeysSync()
        {
            var queryResult = Pragma.ForeignKeyList(Name);
            if (queryResult.Empty())
                return Array.Empty<IDatabaseRelationalKey>();

            var foreignKeys = queryResult.GroupBy(row => new { ForeignKeyId = row.id, ParentTableName = row.table, OnDelete = row.on_delete, OnUpdate = row.on_update }).ToList();
            if (foreignKeys.Count == 0)
                return Array.Empty<IDatabaseRelationalKey>();

            var parser = ParsedDefinition;
            var fkConstraints = parser.ParentKeys;

            var result = new List<IDatabaseRelationalKey>(foreignKeys.Count);
            foreach (var fkey in foreignKeys)
            {
                var rows = fkey.OrderBy(row => row.seq);

                var parentTableName = Identifier.CreateQualifiedIdentifier(Name.Schema, fkey.Key.ParentTableName);
                var parentTable = Database.GetTable(parentTableName);

                var parentColumns = parentTable.Columns;
                parentColumns = rows.Select(row => parentTable.Column[row.to]).ToList();

                var parentPrimaryKey = parentTable.PrimaryKey;
                var pkColumnsEqual = parentPrimaryKey.Columns.Select(col => col.Name)
                    .SequenceEqual(parentColumns.Select(col => col.Name));

                IDatabaseKey parentConstraint;
                if (pkColumnsEqual)
                {
                    parentConstraint = parentPrimaryKey;
                }
                else
                {
                    var uniqueKeys = parentTable.UniqueKeys;
                    parentConstraint = uniqueKeys.FirstOrDefault(uk =>
                        uk.Columns.Select(ukCol => ukCol.Name)
                            .SequenceEqual(parentColumns.Select(pc => pc.Name)));
                }

                // don't need to check for the parent schema as cross-schema references are not supported
                var parsedConstraint = fkConstraints
                    .Where(fkc => string.Equals(fkc.ParentTable.LocalName, fkey.Key.ParentTableName, StringComparison.OrdinalIgnoreCase))
                    .FirstOrDefault(fkc => fkc.ParentColumns.SequenceEqual(rows.Select(row => row.to), StringComparer.OrdinalIgnoreCase));
                var constraintStringName = parsedConstraint?.Name;

                var childKeyName = !constraintStringName.IsNullOrWhiteSpace() ? Identifier.CreateQualifiedIdentifier(constraintStringName) : null;
                var childKeyColumnLookup = Column;
                var childKeyColumns = rows.Select(row => childKeyColumnLookup[row.from]).ToList();

                var childKey = new SqliteDatabaseKey(childKeyName, DatabaseKeyType.Foreign, childKeyColumns);

                var deleteRule = GetRelationalUpdateRule(fkey.Key.OnDelete);
                var updateRule = GetRelationalUpdateRule(fkey.Key.OnUpdate);

                var relationalKey = new SqliteRelationalKey(Name, childKey, parentTableName, parentConstraint, deleteRule, updateRule);
                result.Add(relationalKey);
            }

            return result;
        }

        protected virtual async Task<IReadOnlyCollection<IDatabaseRelationalKey>> LoadParentKeysAsync(CancellationToken cancellationToken)
        {
            var queryResult = await Pragma.ForeignKeyListAsync(Name, cancellationToken).ConfigureAwait(false);
            if (queryResult.Empty())
                return Array.Empty<IDatabaseRelationalKey>();

            var foreignKeys = queryResult.GroupBy(row => new { ForeignKeyId = row.id, ParentTableName = row.table, OnDelete = row.on_delete, OnUpdate = row.on_update }).ToList();
            if (foreignKeys.Count == 0)
                return Array.Empty<IDatabaseRelationalKey>();

            var parser = await ParsedDefinitionAsync(cancellationToken).ConfigureAwait(false);
            var fkConstraints = parser.ParentKeys;

            var result = new List<IDatabaseRelationalKey>(foreignKeys.Count);
            foreach (var fkey in foreignKeys)
            {
                var rows = fkey.OrderBy(row => row.seq);

                var parentTableName = Identifier.CreateQualifiedIdentifier(Name.Schema, fkey.Key.ParentTableName);
                var parentTable = await Database.GetTableAsync(parentTableName, cancellationToken).ConfigureAwait(false);

                var parentColumns = await parentTable.ColumnsAsync(cancellationToken).ConfigureAwait(false);
                parentColumns = rows.Select(row => parentTable.Column[row.to]).ToList();

                var parentPrimaryKey = await parentTable.PrimaryKeyAsync(cancellationToken).ConfigureAwait(false);
                var pkColumnsEqual = parentPrimaryKey.Columns.Select(col => col.Name)
                    .SequenceEqual(parentColumns.Select(col => col.Name));

                IDatabaseKey parentConstraint;
                if (pkColumnsEqual)
                {
                    parentConstraint = parentPrimaryKey;
                }
                else
                {
                    var uniqueKeys = await parentTable.UniqueKeysAsync(cancellationToken).ConfigureAwait(false);
                    parentConstraint = uniqueKeys.FirstOrDefault(uk =>
                        uk.Columns.Select(ukCol => ukCol.Name)
                            .SequenceEqual(parentColumns.Select(pc => pc.Name)));
                }

                // don't need to check for the parent schema as cross-schema references are not supported
                var parsedConstraint = fkConstraints
                    .Where(fkc => string.Equals(fkc.ParentTable.LocalName, fkey.Key.ParentTableName, StringComparison.OrdinalIgnoreCase))
                    .FirstOrDefault(fkc => fkc.ParentColumns.SequenceEqual(rows.Select(row => row.to), StringComparer.OrdinalIgnoreCase));
                var constraintStringName = parsedConstraint?.Name;

                var childKeyName = !constraintStringName.IsNullOrWhiteSpace() ? Identifier.CreateQualifiedIdentifier(constraintStringName) : null;
                var childKeyColumnLookup = await ColumnAsync(cancellationToken).ConfigureAwait(false);
                var childKeyColumns = rows.Select(row => childKeyColumnLookup[row.from]).ToList();

                var childKey = new SqliteDatabaseKey(childKeyName, DatabaseKeyType.Foreign, childKeyColumns);

                var deleteRule = GetRelationalUpdateRule(fkey.Key.OnDelete);
                var updateRule = GetRelationalUpdateRule(fkey.Key.OnUpdate);

                var relationalKey = new SqliteRelationalKey(Name, childKey, parentTableName, parentConstraint, deleteRule, updateRule);
                result.Add(relationalKey);
            }

            return result;
        }

        public IReadOnlyList<IDatabaseColumn> Columns => LoadColumnsSync();

        public Task<IReadOnlyList<IDatabaseColumn>> ColumnsAsync(CancellationToken cancellationToken = default(CancellationToken)) => LoadColumnsAsync(cancellationToken);

        public IReadOnlyDictionary<Identifier, IDatabaseColumn> Column => LoadColumnLookupSync();

        public Task<IReadOnlyDictionary<Identifier, IDatabaseColumn>> ColumnAsync(CancellationToken cancellationToken = default(CancellationToken)) => LoadColumnLookupAsync(cancellationToken);

        protected virtual IReadOnlyDictionary<Identifier, IDatabaseColumn> LoadColumnLookupSync()
        {
            var columns = Columns;
            var result = new Dictionary<Identifier, IDatabaseColumn>(columns.Count, Comparer);

            var namedColumns = columns.Where(c => c.Name != null);
            foreach (var column in namedColumns)
                result[column.Name.LocalName] = column;

            return result;
        }

        protected virtual async Task<IReadOnlyDictionary<Identifier, IDatabaseColumn>> LoadColumnLookupAsync(CancellationToken cancellationToken)
        {
            var columns = await ColumnsAsync(cancellationToken).ConfigureAwait(false);
            var result = new Dictionary<Identifier, IDatabaseColumn>(columns.Count, Comparer);

            var namedColumns = columns.Where(c => c.Name != null);
            foreach (var column in namedColumns)
                result[column.Name.LocalName] = column;

            return result;
        }

        protected virtual IReadOnlyList<IDatabaseColumn> LoadColumnsSync()
        {
            var tableInfos = Pragma.TableInfo(Name);
            if (tableInfos.Empty())
                return Array.Empty<IDatabaseColumn>();

            var result = new List<IDatabaseColumn>();

            var parser = ParsedDefinition;
            var parsedColumns = parser.Columns;

            foreach (var tableInfo in tableInfos)
            {
                var parsedColumnInfo = parsedColumns.FirstOrDefault(col => string.Equals(col.Name, tableInfo.name, StringComparison.OrdinalIgnoreCase));
                var columnTypeName = tableInfo.type;

                var affinity = _affinityParser.ParseTypeName(columnTypeName);
                var columnType = new SqliteColumnType(affinity);

                var isAutoIncrement = parsedColumnInfo.IsAutoIncrement;
                var autoIncrement = isAutoIncrement
                    ? new AutoIncrement(1, 1)
                    : (IAutoIncrement)null;

                var column = new SqliteDatabaseColumn(tableInfo.name, columnType, !tableInfo.notnull, tableInfo.dflt_value, autoIncrement);
                result.Add(column);
            }

            return result.AsReadOnly();
        }

        protected virtual async Task<IReadOnlyList<IDatabaseColumn>> LoadColumnsAsync(CancellationToken cancellationToken)
        {
            var tableInfos = await Pragma.TableInfoAsync(Name, cancellationToken).ConfigureAwait(false);
            if (tableInfos.Empty())
                return Array.Empty<IDatabaseColumn>();

            var result = new List<IDatabaseColumn>();

            var parser = await ParsedDefinitionAsync(cancellationToken).ConfigureAwait(false);
            var parsedColumns = parser.Columns;

            foreach (var tableInfo in tableInfos)
            {
                var parsedColumnInfo = parsedColumns.FirstOrDefault(col => string.Equals(col.Name, tableInfo.name, StringComparison.OrdinalIgnoreCase));
                var columnTypeName = tableInfo.type;

                var affinity = _affinityParser.ParseTypeName(columnTypeName);
                var columnType = new SqliteColumnType(affinity);

                var isAutoIncrement = parsedColumnInfo.IsAutoIncrement;
                var autoIncrement = isAutoIncrement
                    ? new AutoIncrement(1, 1)
                    : (IAutoIncrement)null;

                var column = new SqliteDatabaseColumn(tableInfo.name, columnType, !tableInfo.notnull, tableInfo.dflt_value, autoIncrement);
                result.Add(column);
            }

            return result.AsReadOnly();
        }

        public IReadOnlyDictionary<Identifier, IDatabaseTrigger> Trigger => LoadTriggerLookupSync();

        public Task<IReadOnlyDictionary<Identifier, IDatabaseTrigger>> TriggerAsync(CancellationToken cancellationToken = default(CancellationToken)) => LoadTriggerLookupAsync(cancellationToken);

        public IReadOnlyCollection<IDatabaseTrigger> Triggers => LoadTriggersSync();

        public Task<IReadOnlyCollection<IDatabaseTrigger>> TriggersAsync(CancellationToken cancellationToken = default(CancellationToken)) => LoadTriggersAsync(cancellationToken);

        protected virtual IReadOnlyDictionary<Identifier, IDatabaseTrigger> LoadTriggerLookupSync()
        {
            var triggers = Triggers;
            var result = new Dictionary<Identifier, IDatabaseTrigger>(triggers.Count, Comparer);

            foreach (var trigger in triggers)
                result[trigger.Name.LocalName] = trigger;

            return result;
        }

        protected virtual async Task<IReadOnlyDictionary<Identifier, IDatabaseTrigger>> LoadTriggerLookupAsync(CancellationToken cancellationToken)
        {
            var triggers = await TriggersAsync(cancellationToken).ConfigureAwait(false);
            var result = new Dictionary<Identifier, IDatabaseTrigger>(triggers.Count, Comparer);

            foreach (var trigger in triggers)
                result[trigger.Name.LocalName] = trigger;

            return result;
        }

        protected virtual IReadOnlyCollection<IDatabaseTrigger> LoadTriggersSync()
        {
            var triggerInfos = Connection.Query<SqliteMaster>(TriggerDefinitionQuery, new { TableName = Name.LocalName });

            var result = new List<IDatabaseTrigger>();

            foreach (var triggerInfo in triggerInfos)
            {
                var tokenizer = new SqliteTokenizer();
                var tokenizeResult = tokenizer.TryTokenize(triggerInfo.sql);
                if (!tokenizeResult.HasValue)
                    throw new Exception("Unable to parse the TRIGGER statement: " + triggerInfo.sql);

                var tokens = tokenizeResult.Value;
                var parser = new SqliteTriggerParser(tokens);

                var trigger = new SqliteDatabaseTrigger(triggerInfo.name, triggerInfo.sql, parser.Timing, parser.Event);
                result.Add(trigger);
            }

            return result;
        }

        protected virtual async Task<IReadOnlyCollection<IDatabaseTrigger>> LoadTriggersAsync(CancellationToken cancellationToken)
        {
            var triggerInfos = await Connection.QueryAsync<SqliteMaster>(TriggerDefinitionQuery, new { TableName = Name.LocalName }).ConfigureAwait(false);

            var result = new List<IDatabaseTrigger>();

            foreach (var triggerInfo in triggerInfos)
            {
                var tokenizer = new SqliteTokenizer();
                var tokenizeResult = tokenizer.TryTokenize(triggerInfo.sql);
                if (!tokenizeResult.HasValue)
                    throw new Exception("Unable to parse the TRIGGER statement: " + triggerInfo.sql);

                var tokens = tokenizeResult.Value;
                var parser = new SqliteTriggerParser(tokens);

                var trigger = new SqliteDatabaseTrigger(triggerInfo.name, triggerInfo.sql, parser.Timing, parser.Event);
                result.Add(trigger);
            }

            return result;
        }

        protected virtual string TriggerDefinitionQuery => $"select * from { Dialect.QuoteIdentifier(Name.Schema) }.sqlite_master where type = 'trigger' and tbl_name = @TableName";

        protected SqliteTableParser ParsedDefinition => LoadTableParserSync();

        protected Task<SqliteTableParser> ParsedDefinitionAsync(CancellationToken cancellationToken) => LoadTableParserAsync(cancellationToken);

        protected SqliteTableParser LoadTableParserSync()
        {
            string tableSql = null;

            try
            {
                _rwLock.EnterReadLock();
                tableSql = Connection.ExecuteScalar<string>(TableDefinitionQuery, new { TableName = Name.LocalName });
                if (tableSql == _createTableSql)
                    return _parser;
            }
            finally
            {
                _rwLock.ExitReadLock();
            }

            try
            {
                _rwLock.EnterWriteLock();
                _createTableSql = tableSql;

                var tokenizer = new SqliteTokenizer();
                var tokenizeResult = tokenizer.TryTokenize(_createTableSql);
                if (!tokenizeResult.HasValue)
                    throw new Exception("Unable to parse the CREATE TABLE statement: " + _createTableSql);

                var tokens = tokenizeResult.Value;
                _parser = new SqliteTableParser(tokens, tableSql);
                return _parser;
            }
            finally
            {
                _rwLock.ExitWriteLock();
            }
        }

        protected async Task<SqliteTableParser> LoadTableParserAsync(CancellationToken cancellationToken)
        {
            string tableSql = null;

            try
            {
                _rwLock.EnterReadLock();
                tableSql = await Connection.ExecuteScalarAsync<string>(TableDefinitionQuery, new { TableName = Name.LocalName }).ConfigureAwait(false);
                if (tableSql == _createTableSql)
                    return _parser;
            }
            finally
            {
                _rwLock.ExitReadLock();
            }

            try
            {
                _rwLock.EnterWriteLock();
                _createTableSql = tableSql;

                var tokenizer = new SqliteTokenizer();
                var tokenizeResult = tokenizer.TryTokenize(_createTableSql);
                if (!tokenizeResult.HasValue)
                    throw new Exception("Unable to parse the CREATE TABLE statement: " + _createTableSql);

                var tokens = tokenizeResult.Value;
                _parser = new SqliteTableParser(tokens, tableSql);
                return _parser;
            }
            finally
            {
                _rwLock.ExitWriteLock();
            }
        }

        protected virtual string TableDefinitionQuery => $"select sql from { Dialect.QuoteIdentifier(Name.Schema) }.sqlite_master where type = 'table' and tbl_name = @TableName";

        protected static Rule GetRelationalUpdateRule(string pragmaUpdateRule)
        {
            if (pragmaUpdateRule.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(pragmaUpdateRule));

            return _relationalUpdateMapping.ContainsKey(pragmaUpdateRule)
                ? _relationalUpdateMapping[pragmaUpdateRule]
                : Rule.None;
        }

        private readonly static IReadOnlyDictionary<string, Rule> _relationalUpdateMapping = new Dictionary<string, Rule>(StringComparer.OrdinalIgnoreCase)
        {
            ["NO ACTION"] = Rule.None,
            ["RESTRICT"] = Rule.None,
            ["SET NULL"] = Rule.SetNull,
            ["SET DEFAULT"] = Rule.SetDefault,
            ["CASCADE"] = Rule.Cascade
        };

        private string _createTableSql;
        private SqliteTableParser _parser;
        private readonly ReaderWriterLockSlim _rwLock = new ReaderWriterLockSlim();

        private readonly static SqliteTypeAffinityParser _affinityParser = new SqliteTypeAffinityParser();
    }
}
