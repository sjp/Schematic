using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using SJP.Schematic.SqlServer.Query;
using SJP.Schematic.Core;

namespace SJP.Schematic.SqlServer
{
    public class SqlServerRelationalDatabaseView : IRelationalDatabaseView
    {
        public SqlServerRelationalDatabaseView(IDbConnection connection, IRelationalDatabase database, Identifier viewName, IEqualityComparer<Identifier> comparer = null)
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

            Comparer = comparer ?? new IdentifierComparer(StringComparer.Ordinal, serverName, databaseName, schemaName);

            Name = new Identifier(serverName, databaseName, schemaName, viewName.LocalName);
        }

        public Identifier Name { get; }

        public IRelationalDatabase Database { get; }

        protected IDbTypeProvider TypeProvider { get; }

        protected IDbConnection Connection { get; }

        protected IEqualityComparer<Identifier> Comparer { get; }

        public string Definition => LoadDefinitionSync();

        public Task<string> DefinitionAsync() => LoadDefinitionAsync();

        protected virtual string LoadDefinitionSync()
        {
            const string sql = @"
select sm.definition
from sys.sql_modules sm
inner join sys.views v on sm.object_id = v.object_id
where schema_name(v.schema_id) = @SchemaName and v.name = @ViewName";

            return Connection.ExecuteScalar<string>(sql, new { SchemaName = Name.Schema, ViewName = Name.LocalName });
        }

        protected virtual Task<string> LoadDefinitionAsync()
        {
            const string sql = @"
select sm.definition
from sys.sql_modules sm
inner join sys.views v on sm.object_id = v.object_id
where schema_name(v.schema_id) = @SchemaName and v.name = @ViewName";

            return Connection.ExecuteScalarAsync<string>(sql, new { SchemaName = Name.Schema, ViewName = Name.LocalName });
        }

        public bool IsIndexed => Indexes.Any();

        public IReadOnlyDictionary<Identifier, IDatabaseViewIndex> Index => LoadIndexLookupSync();

        public Task<IReadOnlyDictionary<Identifier, IDatabaseViewIndex>> IndexAsync() => LoadIndexLookupAsync();

        public IEnumerable<IDatabaseViewIndex> Indexes => LoadIndexesSync();

        public Task<IEnumerable<IDatabaseViewIndex>> IndexesAsync() => LoadIndexesAsync();

        protected virtual IEnumerable<IDatabaseViewIndex> LoadIndexesSync()
        {
            const string sql = @"
select i.name as IndexName, i.is_unique as IsUnique, ic.key_ordinal as KeyOrdinal, ic.index_column_id as IndexColumnId, ic.is_included_column as IsIncludedColumn, ic.is_descending_key as IsDescending, c.name as ColumnName, i.is_disabled as IsDisabled
from sys.views v
inner join sys.indexes i on v.object_id = i.object_id
inner join sys.index_columns ic on i.object_id = ic.object_id and i.index_id = ic.index_id
inner join sys.columns c on ic.object_id = c.object_id and ic.column_id = c.column_id
where schema_name(v.schema_id) = @SchemaName and v.name = @ViewName
    and i.is_primary_key = 0 and i.is_unique_constraint = 0
    order by ic.index_id, ic.key_ordinal, ic.index_column_id";

            var queryResult = Connection.Query<IndexColumns>(sql, new { SchemaName = Name.Schema, ViewName = Name.LocalName });
            if (queryResult.Empty())
                return Enumerable.Empty<IDatabaseViewIndex>();

            var indexColumns = queryResult.GroupBy(row => new { row.IndexName, row.IsUnique, row.IsDisabled }).ToList();
            if (indexColumns.Count == 0)
                return Enumerable.Empty<IDatabaseViewIndex>();

            var viewColumns = Column;
            var result = new List<IDatabaseViewIndex>(indexColumns.Count);
            foreach (var indexInfo in indexColumns)
            {
                var isUnique = indexInfo.Key.IsUnique;
                var indexName = new Identifier(indexInfo.Key.IndexName);
                var isEnabled = !indexInfo.Key.IsDisabled;

                var indexCols = indexInfo
                    .Where(row => !row.IsIncludedColumn)
                    .OrderBy(row => row.KeyOrdinal)
                    .ThenBy(row => row.IndexColumnId)
                    .Select(row => new { row.IsDescending, Column = viewColumns[row.ColumnName] })
                    .Select(row => new SqlServerDatabaseIndexColumn(row.Column, row.IsDescending ? IndexColumnOrder.Descending : IndexColumnOrder.Ascending))
                    .ToList();

                var includedCols = indexInfo
                    .Where(row => row.IsIncludedColumn)
                    .OrderBy(row => row.KeyOrdinal)
                    .ThenBy(row => row.IndexColumnId)
                    .Select(row => viewColumns[row.ColumnName])
                    .ToList();

                // TODO: Merge index definitions so that views and tables can be shared (but typed)
                //       Use generics for Parent<T> ?
                var index = new SqlServerDatabaseViewIndex(this, indexName, isUnique, indexCols, includedCols, isEnabled);
                result.Add(index);
            }

            return result;
        }

        protected virtual async Task<IEnumerable<IDatabaseViewIndex>> LoadIndexesAsync()
        {
            const string sql = @"
select i.name as IndexName, i.is_unique as IsUnique, ic.key_ordinal as KeyOrdinal, ic.index_column_id as IndexColumnId, ic.is_included_column as IsIncludedColumn, ic.is_descending_key as IsDescending, c.name as ColumnName, i.is_disabled as IsDisabled
from sys.views v
inner join sys.indexes i on v.object_id = i.object_id
inner join sys.index_columns ic on i.object_id = ic.object_id and i.index_id = ic.index_id
inner join sys.columns c on ic.object_id = c.object_id and ic.column_id = c.column_id
where schema_name(v.schema_id) = @SchemaName and v.name = @ViewName
    and i.is_primary_key = 0 and i.is_unique_constraint = 0
    order by ic.index_id, ic.key_ordinal, ic.index_column_id";

            var queryResult = await Connection.QueryAsync<IndexColumns>(sql, new { SchemaName = Name.Schema, ViewName = Name.LocalName }).ConfigureAwait(false);
            if (queryResult.Empty())
                return Enumerable.Empty<IDatabaseViewIndex>();

            var indexColumns = queryResult.GroupBy(row => new { row.IndexName, row.IsUnique, row.IsDisabled }).ToList();
            if (indexColumns.Count == 0)
                return Enumerable.Empty<IDatabaseViewIndex>();

            var viewColumns = await ColumnAsync().ConfigureAwait(false);
            var result = new List<IDatabaseViewIndex>(indexColumns.Count);
            foreach (var indexInfo in indexColumns)
            {
                var isUnique = indexInfo.Key.IsUnique;
                var indexName = new Identifier(indexInfo.Key.IndexName);
                var isEnabled = !indexInfo.Key.IsDisabled;

                var indexCols = indexInfo
                    .Where(row => !row.IsIncludedColumn)
                    .OrderBy(row => row.KeyOrdinal)
                    .ThenBy(row => row.IndexColumnId)
                    .Select(row => new { row.IsDescending, Column = viewColumns[row.ColumnName] })
                    .Select(row => new SqlServerDatabaseIndexColumn(row.Column, row.IsDescending ? IndexColumnOrder.Descending : IndexColumnOrder.Ascending))
                    .ToList();

                var includedCols = indexInfo
                    .Where(row => row.IsIncludedColumn)
                    .OrderBy(row => row.KeyOrdinal)
                    .ThenBy(row => row.IndexColumnId)
                    .Select(row => viewColumns[row.ColumnName])
                    .ToList();

                // TODO: Merge index definitions so that views and tables can be shared (but typed)
                //       Use generics for Parent<T> ?
                var index = new SqlServerDatabaseViewIndex(this, indexName, isUnique, indexCols, includedCols, isEnabled);
                result.Add(index);
            }

            return result;
        }

        protected virtual IReadOnlyDictionary<Identifier, IDatabaseViewIndex> LoadIndexLookupSync()
        {
            var result = new Dictionary<Identifier, IDatabaseViewIndex>(Comparer);

            foreach (var index in Indexes)
                result[index.Name.LocalName] = index;

            return result.AsReadOnlyDictionary();
        }

        protected virtual async Task<IReadOnlyDictionary<Identifier, IDatabaseViewIndex>> LoadIndexLookupAsync()
        {
            var result = new Dictionary<Identifier, IDatabaseViewIndex>(Comparer);

            var indexes = await IndexesAsync().ConfigureAwait(false);
            foreach (var index in indexes)
                result[index.Name.LocalName] = index;

            return result.AsReadOnlyDictionary();
        }

        public IReadOnlyDictionary<Identifier, IDatabaseViewColumn> Column => LoadColumnLookupSync();

        public Task<IReadOnlyDictionary<Identifier, IDatabaseViewColumn>> ColumnAsync() => LoadColumnLookupAsync();

        public IReadOnlyList<IDatabaseViewColumn> Columns => LoadColumnsSync();

        public Task<IReadOnlyList<IDatabaseViewColumn>> ColumnsAsync() => LoadColumnsAsync();

        protected virtual IReadOnlyDictionary<Identifier, IDatabaseViewColumn> LoadColumnLookupSync()
        {
            var result = new Dictionary<Identifier, IDatabaseViewColumn>(Comparer);

            foreach (var column in Columns.Where(c => c.Name != null))
                result[column.Name.LocalName] = column;

            return result.AsReadOnlyDictionary();
        }

        protected virtual async Task<IReadOnlyDictionary<Identifier, IDatabaseViewColumn>> LoadColumnLookupAsync()
        {
            var result = new Dictionary<Identifier, IDatabaseViewColumn>(Comparer);

            var columns = await ColumnsAsync().ConfigureAwait(false);
            foreach (var column in columns.Where(c => c.Name != null))
                result[column.Name.LocalName] = column;

            return result.AsReadOnlyDictionary();
        }

        protected virtual IReadOnlyList<IDatabaseViewColumn> LoadColumnsSync()
        {
            const string sql = @"
select
    c.name as ColumnName,
    schema_name(st.schema_id) as ColumnTypeSchema,
    st.name as ColumnTypeName,
    c.max_length as MaxLength,
    c.precision as Precision,
    c.scale as Scale,
    c.collation_name as Collation,
    c.is_computed as IsComputed,
    c.is_nullable as IsNullable,
    dc.definition as DefaultValue,
    cc.definition as ComputedColumnDefinition,
    (convert(bigint, ic.seed_value)) as IdentitySeed,
    (convert(bigint, ic.increment_value)) as IdentityIncrement
from sys.views v
inner join sys.columns c on v.object_id = c.object_id
left join sys.default_constraints dc on c.object_id = dc.parent_object_id and c.column_id = dc.parent_column_id
left join sys.computed_columns cc on c.object_id = cc.object_id and c.column_id = cc.column_id
left join sys.identity_columns ic on c.object_id = ic.object_id and c.column_id = ic.column_id
left join sys.types st on c.user_type_id = st.user_type_id
where schema_name(v.schema_id) = @SchemaName
    and v.name = @ViewName
    order by c.column_id";

            var query = Connection.Query<ColumnData>(sql, new { SchemaName = Name.Schema, ViewName = Name.LocalName });
            var result = new List<IDatabaseViewColumn>();

            foreach (var row in query)
            {
                var typeMetadata = new ColumnTypeMetadata
                {
                    TypeName = new Identifier(row.ColumnTypeSchema, row.ColumnTypeName),
                    Collation = row.Collation.IsNullOrWhiteSpace() ? null : new Identifier(row.Collation),
                    MaxLength = row.MaxLength,
                    NumericPrecision = new NumericPrecision(row.Precision, row.Scale)
                };
                var columnType = TypeProvider.CreateColumnType(typeMetadata);

                var columnName = new Identifier(row.ColumnName);
                var isAutoIncrement = row.IdentitySeed.HasValue && row.IdentityIncrement.HasValue;
                var autoIncrement = isAutoIncrement
                    ? new AutoIncrement(row.IdentitySeed.Value, row.IdentityIncrement.Value)
                    : (IAutoIncrement)null;

                var column = new SqlServerDatabaseViewColumn(this, columnName, columnType, row.IsNullable, row.DefaultValue, autoIncrement);

                result.Add(column);
            }

            return result.AsReadOnly();
        }

        protected virtual async Task<IReadOnlyList<IDatabaseViewColumn>> LoadColumnsAsync()
        {
            const string sql = @"
select
    c.name as ColumnName,
    schema_name(st.schema_id) as ColumnTypeSchema,
    st.name as ColumnTypeName,
    c.max_length as MaxLength,
    c.precision as Precision,
    c.scale as Scale,
    c.collation_name as Collation,
    c.is_computed as IsComputed,
    c.is_nullable as IsNullable,
    dc.definition as DefaultValue,
    cc.definition as ComputedColumnDefinition,
    (convert(bigint, ic.seed_value)) as IdentitySeed,
    (convert(bigint, ic.increment_value)) as IdentityIncrement
from sys.views v
inner join sys.columns c on v.object_id = c.object_id
left join sys.default_constraints dc on c.object_id = dc.parent_object_id and c.column_id = dc.parent_column_id
left join sys.computed_columns cc on c.object_id = cc.object_id and c.column_id = cc.column_id
left join sys.identity_columns ic on c.object_id = ic.object_id and c.column_id = ic.column_id
left join sys.types st on c.user_type_id = st.user_type_id
where schema_name(v.schema_id) = @SchemaName
    and v.name = @ViewName
    order by c.column_id";

            var query = await Connection.QueryAsync<ColumnData>(sql, new { SchemaName = Name.Schema, ViewName = Name.LocalName }).ConfigureAwait(false);
            var result = new List<IDatabaseViewColumn>();

            foreach (var row in query)
            {
                var typeMetadata = new ColumnTypeMetadata
                {
                    TypeName = new Identifier(row.ColumnTypeSchema, row.ColumnTypeName),
                    Collation = row.Collation.IsNullOrWhiteSpace() ? null : new Identifier(row.Collation),
                    MaxLength = row.MaxLength,
                    NumericPrecision = new NumericPrecision(row.Precision, row.Scale)
                };
                var columnType = TypeProvider.CreateColumnType(typeMetadata);

                var columnName = new Identifier(row.ColumnName);
                var isAutoIncrement = row.IdentitySeed.HasValue && row.IdentityIncrement.HasValue;
                var autoIncrement = isAutoIncrement
                    ? new AutoIncrement(row.IdentitySeed.Value, row.IdentityIncrement.Value)
                    : (IAutoIncrement)null;

                var column = new SqlServerDatabaseViewColumn(this, columnName, columnType, row.IsNullable, row.DefaultValue, autoIncrement);

                result.Add(column);
            }

            return result.AsReadOnly();
        }
    }
}
