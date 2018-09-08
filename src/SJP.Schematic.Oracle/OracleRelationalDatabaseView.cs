using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using SJP.Schematic.Oracle.Query;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using System.Threading;
using SJP.Schematic.Oracle.Utilities;

namespace SJP.Schematic.Oracle
{
    public class OracleRelationalDatabaseView : IRelationalDatabaseView
    {
        public OracleRelationalDatabaseView(IDbConnection connection, IRelationalDatabase database, Identifier viewName, INameResolverStrategy strategy = null)
        {
            if (viewName == null)
                throw new ArgumentNullException(nameof(viewName));

            Connection = connection ?? throw new ArgumentNullException(nameof(connection));
            Database = database ?? throw new ArgumentNullException(nameof(database));

            var dialect = database.Dialect;
            if (dialect == null)
                throw new ArgumentException("The given database does not contain a valid dialect.", nameof(database));

            var typeProvider = dialect.TypeProvider;
            TypeProvider = typeProvider ?? throw new ArgumentException("The given database's dialect does not have a valid type provider.", nameof(database));

            var serverName = viewName.Server ?? database.ServerName;
            var databaseName = viewName.Database ?? database.DatabaseName;
            var schemaName = viewName.Schema ?? database.DefaultSchema;

            NameResolver = strategy ?? new DefaultOracleNameResolverStrategy();

            Name = Identifier.CreateQualifiedIdentifier(serverName, databaseName, schemaName, viewName.LocalName);
        }

        public Identifier Name { get; }

        public IRelationalDatabase Database { get; }

        protected IDbTypeProvider TypeProvider { get; }

        protected IDbConnection Connection { get; }

        protected INameResolverStrategy NameResolver { get; }

        public string Definition => LoadDefinitionSync();

        public Task<string> DefinitionAsync(CancellationToken cancellationToken = default(CancellationToken)) => LoadDefinitionAsync(cancellationToken);

        protected virtual string LoadDefinitionSync() => Connection.ExecuteScalar<string>(DefinitionQuery, new { SchemaName = Name.Schema, ViewName = Name.LocalName });

        protected virtual Task<string> LoadDefinitionAsync(CancellationToken cancellationToken) => Connection.ExecuteScalarAsync<string>(DefinitionQuery, new { SchemaName = Name.Schema, ViewName = Name.LocalName });

        protected virtual string DefinitionQuery => DefinitionQuerySql;

        private const string DefinitionQuerySql = @"
select TEXT
from ALL_VIEWS
where OWNER = :SchemaName and VIEW_NAME = :ViewName";

        public bool IsIndexed => Indexes.Count > 0;

        public IReadOnlyDictionary<Identifier, IDatabaseViewIndex> Index => LoadIndexLookupSync();

        public Task<IReadOnlyDictionary<Identifier, IDatabaseViewIndex>> IndexAsync(CancellationToken cancellationToken = default(CancellationToken)) => LoadIndexLookupAsync(cancellationToken);

        public IReadOnlyCollection<IDatabaseViewIndex> Indexes => LoadIndexesSync();

        public Task<IReadOnlyCollection<IDatabaseViewIndex>> IndexesAsync(CancellationToken cancellationToken = default(CancellationToken)) => LoadIndexesAsync(cancellationToken);

        protected virtual IReadOnlyCollection<IDatabaseViewIndex> LoadIndexesSync()
        {
            var queryResult = Connection.Query<IndexColumns>(IndexesQuery, new { SchemaName = Name.Schema, ViewName = Name.LocalName });
            if (queryResult.Empty())
                return Array.Empty<IDatabaseViewIndex>();

            var indexColumns = queryResult.GroupBy(row => new { row.IndexName, row.IndexProperty, row.Uniqueness }).ToList();
            if (indexColumns.Count == 0)
                return Array.Empty<IDatabaseViewIndex>();

            var viewColumns = Column;
            var result = new List<IDatabaseViewIndex>(indexColumns.Count);
            foreach (var indexInfo in indexColumns)
            {
                var indexProperties = (OracleIndexProperties)indexInfo.Key.IndexProperty;
                var isUnique = indexInfo.Key.Uniqueness == "UNIQUE";
                var indexName = Identifier.CreateQualifiedIdentifier(indexInfo.Key.IndexName);

                var indexCols = indexInfo
                    .OrderBy(row => row.ColumnPosition)
                    .Select(row => new { row.IsDescending, Column = viewColumns[row.ColumnName] })
                    .Select(row => new OracleDatabaseIndexColumn(row.Column, row.IsDescending == "Y" ? IndexColumnOrder.Descending : IndexColumnOrder.Ascending))
                    .ToList();

                // TODO: Merge index definitions so that views and tables can be shared (but typed)
                //       Use generics for Parent<T> ?
                var index = new OracleDatabaseViewIndex(this, indexName, isUnique, indexCols, indexProperties);
                result.Add(index);
            }

            return result;
        }

        protected virtual async Task<IReadOnlyCollection<IDatabaseViewIndex>> LoadIndexesAsync(CancellationToken cancellationToken)
        {
            var queryResult = await Connection.QueryAsync<IndexColumns>(IndexesQuery, new { SchemaName = Name.Schema, ViewName = Name.LocalName }).ConfigureAwait(false);
            if (queryResult.Empty())
                return Array.Empty<IDatabaseViewIndex>();

            var indexColumns = queryResult.GroupBy(row => new { row.IndexName, row.IndexProperty, row.Uniqueness }).ToList();
            if (indexColumns.Count == 0)
                return Array.Empty<IDatabaseViewIndex>();

            var viewColumns = await ColumnAsync(cancellationToken).ConfigureAwait(false);
            var result = new List<IDatabaseViewIndex>(indexColumns.Count);
            foreach (var indexInfo in indexColumns)
            {
                var indexProperties = (OracleIndexProperties)indexInfo.Key.IndexProperty;
                var isUnique = indexInfo.Key.Uniqueness == "UNIQUE";
                var indexName = Identifier.CreateQualifiedIdentifier(indexInfo.Key.IndexName);

                var indexCols = indexInfo
                    .OrderBy(row => row.ColumnPosition)
                    .Select(row => new { row.IsDescending, Column = viewColumns[row.ColumnName] })
                    .Select(row => new OracleDatabaseIndexColumn(row.Column, row.IsDescending == "Y" ? IndexColumnOrder.Descending : IndexColumnOrder.Ascending))
                    .ToList();

                // TODO: Merge index definitions so that views and tables can be shared (but typed)
                //       Use generics for Parent<T> ?
                var index = new OracleDatabaseViewIndex(this, indexName, isUnique, indexCols, indexProperties);
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
where ai.TABLE_OWNER = :SchemaName and ai.TABLE_NAME = :ViewName
    and aic.TABLE_OWNER = :SchemaName and aic.TABLE_NAME = :ViewName
    and ao.OBJECT_TYPE = 'INDEX'
order by aic.COLUMN_POSITION";

        protected virtual IReadOnlyDictionary<Identifier, IDatabaseViewIndex> LoadIndexLookupSync()
        {
            var indexes = Indexes;
            var result = new Dictionary<Identifier, IDatabaseViewIndex>(indexes.Count);

            foreach (var index in indexes)
                result[index.Name.LocalName] = index;

            return new ResolvingKeyDictionary<IDatabaseViewIndex>(result, NameResolver);
        }

        protected virtual async Task<IReadOnlyDictionary<Identifier, IDatabaseViewIndex>> LoadIndexLookupAsync(CancellationToken cancellationToken)
        {
            var indexes = await IndexesAsync(cancellationToken).ConfigureAwait(false);
            var result = new Dictionary<Identifier, IDatabaseViewIndex>(indexes.Count);

            foreach (var index in indexes)
                result[index.Name.LocalName] = index;

            return new ResolvingKeyDictionary<IDatabaseViewIndex>(result, NameResolver);
        }

        public IReadOnlyDictionary<Identifier, IDatabaseViewColumn> Column => LoadColumnLookupSync();

        public Task<IReadOnlyDictionary<Identifier, IDatabaseViewColumn>> ColumnAsync(CancellationToken cancellationToken = default(CancellationToken)) => LoadColumnLookupAsync(cancellationToken);

        public IReadOnlyList<IDatabaseViewColumn> Columns => LoadColumnsSync();

        public Task<IReadOnlyList<IDatabaseViewColumn>> ColumnsAsync(CancellationToken cancellationToken = default(CancellationToken)) => LoadColumnsAsync(cancellationToken);

        protected virtual IReadOnlyDictionary<Identifier, IDatabaseViewColumn> LoadColumnLookupSync()
        {
            var columns = Columns;
            var result = new Dictionary<Identifier, IDatabaseViewColumn>(columns.Count);

            foreach (var column in columns.Where(c => c.Name != null))
                result[column.Name.LocalName] = column;

            return new ResolvingKeyDictionary<IDatabaseViewColumn>(result, NameResolver);
        }

        protected virtual async Task<IReadOnlyDictionary<Identifier, IDatabaseViewColumn>> LoadColumnLookupAsync(CancellationToken cancellationToken)
        {
            var columns = await ColumnsAsync(cancellationToken).ConfigureAwait(false);
            var result = new Dictionary<Identifier, IDatabaseViewColumn>(columns.Count);

            foreach (var column in columns.Where(c => c.Name != null))
                result[column.Name.LocalName] = column;

            return new ResolvingKeyDictionary<IDatabaseViewColumn>(result, NameResolver);
        }

        protected virtual IReadOnlyList<IDatabaseViewColumn> LoadColumnsSync()
        {
            var query = Connection.Query<ColumnData>(ColumnsQuery, new { SchemaName = Name.Schema, ViewName = Name.LocalName });

            var columnNames = query.Select(row => row.ColumnName).ToList();
            var notNullableColumnNames = GetNotNullConstrainedColumns(columnNames);
            var result = new List<IDatabaseViewColumn>();

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

                var column = new OracleDatabaseViewColumn(this, columnName, columnType, isNullable, row.DefaultValue);

                result.Add(column);
            }

            return result.AsReadOnly();
        }

        protected virtual async Task<IReadOnlyList<IDatabaseViewColumn>> LoadColumnsAsync(CancellationToken cancellationToken)
        {
            var query = await Connection.QueryAsync<ColumnData>(ColumnsQuery, new { SchemaName = Name.Schema, ViewName = Name.LocalName }).ConfigureAwait(false);

            var columnNames = query.Select(row => row.ColumnName).ToList();
            var notNullableColumnNames = await GetNotNullConstrainedColumnsAsync(columnNames).ConfigureAwait(false);
            var result = new List<IDatabaseViewColumn>();

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

                var column = new OracleDatabaseViewColumn(this, columnName, columnType, isNullable, row.DefaultValue);

                result.Add(column);
            }

            return result.AsReadOnly();
        }

        protected virtual string ColumnsQuery => ColumnsQuerySql;

        private const string ColumnsQuerySql = @"
select
    atc.COLUMN_NAME as ColumnName,
    atc.DATA_TYPE_OWNER as ColumnTypeSchema,
    atc.DATA_TYPE as ColumnTypeName,
    atc.DATA_LENGTH as DataLength,
    atc.DATA_PRECISION as Precision,
    atc.DATA_SCALE as Scale,
    atc.NULLABLE as IsNullable,
    atc.DATA_DEFAULT as DefaultValue,
    atc.CHAR_LENGTH as CharacterLength,
    atc.CHARACTER_SET_NAME as Collation,
    atc.VIRTUAL_COLUMN as IsComputed
from ALL_TAB_COLS atc
where OWNER = :SchemaName and TABLE_NAME = :ViewName
order by atc.COLUMN_ID";

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

        protected virtual string ChecksQuery => ChecksQuerySql;

        private const string ChecksQuerySql = @"
select
    CONSTRAINT_NAME as ConstraintName,
    SEARCH_CONDITION as Definition,
    STATUS as EnabledStatus
from ALL_CONSTRAINTS
where OWNER = :SchemaName and TABLE_NAME = :TableName and CONSTRAINT_TYPE = 'C'";

        private static string GenerateNotNullDefinition(string columnName)
        {
            if (columnName.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(columnName));

            return "\"" + columnName + "\" IS NOT NULL";
        }
    }
}
