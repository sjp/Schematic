using Dapper;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using SJP.Schema.SqlServer.Query;
using SJP.Schema.Core;
using SJP.Schema.Core.Utilities;

namespace SJP.Schema.SqlServer
{
    public class SqlServerRelationalDatabaseView : IRelationalDatabaseView
    {
        public SqlServerRelationalDatabaseView(IDbConnection connection, IRelationalDatabase database, Identifier viewName)
        {
            Connection = connection ?? throw new ArgumentNullException(nameof(connection));
            Database = database ?? throw new ArgumentNullException(nameof(database));
            Name = viewName ?? throw new ArgumentNullException(nameof(viewName));

            _dependencies = new AsyncLazy<IEnumerable<Identifier>>(LoadDependenciesAsync);
            _dependents = new AsyncLazy<IEnumerable<Identifier>>(LoadDependentsAsync);

            _columnList = new AsyncLazy<IList<IDatabaseViewColumn>>(LoadColumnsAsync);
            _columnLookup = new AsyncLazy<IReadOnlyDictionary<string, IDatabaseViewColumn>>(LoadColumnLookupAsync);

            _indexes = new AsyncLazy<IEnumerable<IDatabaseViewIndex>>(LoadIndexesAsync);
            _indexLookup = new AsyncLazy<IReadOnlyDictionary<string, IDatabaseViewIndex>>(LoadIndexLookupAsync);
        }

        private async Task<IEnumerable<Identifier>> LoadDependentsAsync()
        {
            const string sql = @"
select schema_name(o1.schema_id) as ReferencingSchemaName, o1.name as ReferencingObjectName, o1.type_desc as ReferencingObjectType,
    schema_name(o2.schema_id) as ReferencedSchemaName, o2.name as ReferencedObjectName, o2.type_desc as ReferencedObjectType
from sys.sql_expression_dependencies sed
inner join sys.objects o1 on sed.referencing_id = o1.object_id
inner join sys.objects o2 on sed.referenced_id = o2.object_id
where o2.schema_id = schema_id(@SchemaName) and o2.name = @ViewName
    and o2.type = 'V' and sed.referenced_minor_id = 0";

            var dependents = await Connection.QueryAsync<DependencyData>(sql, new { SchemaName = Name.Schema, ViewName = Name.LocalName });
            if (dependents.Empty())
                return null;

            var results = dependents
                .Select(d => new Identifier(d.ReferencingSchemaName, d.ReferencingObjectName))
                .ToList();

            return results.ToImmutableList();
        }

        private async Task<IEnumerable<Identifier>> LoadDependenciesAsync()
        {
            const string sql = @"
select schema_name(o1.schema_id) as ReferencingSchemaName, o1.name as ReferencingObjectName, o1.type_desc as ReferencingObjectType,
    schema_name(o2.schema_id) as ReferencedSchemaName, o2.name as ReferencedObjectName, o2.type_desc as ReferencedObjectType
from sys.sql_expression_dependencies sed
inner join sys.objects o1 on sed.referencing_id = o1.object_id
inner join sys.objects o2 on sed.referenced_id = o2.object_id
where o1.schema_id = schema_id(@SchemaName) and o1.name = @TableName
    and o1.type = 'V' and sed.referencing_minor_id = 0";

            var dependencies = await Connection.QueryAsync<DependencyData>(sql, new { SchemaName = Name.Schema, ViewName = Name.LocalName });
            if (dependencies.Empty())
                return null;

            var results = dependencies
                .Select(d => new Identifier(d.ReferencedSchemaName, d.ReferencedObjectName))
                .ToList();

            return results.ToImmutableList();
        }

        public Identifier Name { get; }

        public IRelationalDatabase Database { get; }

        protected IDbConnection Connection { get; }

        public IList<IDatabaseViewColumn> Columns => _columnList.Task.Result;

        public Task<IList<IDatabaseViewColumn>> ColumnsAsync() => _columnList.Task;

        public IReadOnlyDictionary<string, IDatabaseViewColumn> Column => _columnLookup.Task.Result;

        public bool IsIndexed => Indexes.Any();

        public IReadOnlyDictionary<string, IDatabaseViewIndex> Index
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public IEnumerable<IDatabaseViewIndex> Indexes
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public IEnumerable<Identifier> Dependencies => _dependencies.Task.Result;

        public IEnumerable<Identifier> Dependents => _dependents.Task.Result;

        public Task<IReadOnlyDictionary<string, IDatabaseViewIndex>> IndexAsync()
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<IDatabaseViewIndex>> IndexesAsync()
        {
            throw new NotImplementedException();
        }

        private async Task<IEnumerable<IDatabaseViewIndex>> LoadIndexesAsync()
        {
            const string sql = @"
select i.name as IndexName, i.is_unique as IsUnique, ic.key_ordinal as KeyOrdinal, ic.index_column_id as IndexColumnId, ic.is_included_column as IsIncludedColumn, ic.is_descending_key as IsDescending, c.name as ColumnName
from sys.views v
inner join sys.indexes i on v.object_id = i.object_id
inner join sys.index_columns ic on i.object_id = ic.object_id and i.index_id = ic.index_id
inner join sys.columns c on ic.object_id = c.object_id and ic.column_id = c.column_id
where schema_name(v.schema_id) = @SchemaName and v.name = @ViewName
    and i.is_primary_key = 0 and i.is_unique_constraint = 0
    order by ic.index_id, ic.key_ordinal, ic.index_column_id";

            var queryResult = await Connection.QueryAsync<IndexColumns>(sql, new { SchemaName = Name.Schema, ViewName = Name.LocalName });
            if (queryResult.Empty())
                return Enumerable.Empty<IDatabaseViewIndex>();

            var indexColumns = queryResult.GroupBy(row => new { IndexName = row.IndexName, IsUnique = row.IsUnique });

            var viewColumns = await ColumnAsync();
            var result = new List<IDatabaseViewIndex>();
            foreach (var indexInfo in indexColumns)
            {
                var isUnique = indexInfo.Key.IsUnique;
                var indexName = new LocalIdentifier(indexInfo.Key.IndexName);
                var indexCols = indexInfo
                    .Where(row => !row.IsIncludedColumn)
                    .OrderBy(row => row.KeyOrdinal)
                    .ThenBy(row => row.IndexColumnId)
                    .Select(row => new { IsDescending = row.IsDescending, Column = viewColumns[row.ColumnName] })
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
                var index = new SqlServerDatabaseViewIndex(null, indexName, isUnique, indexCols, includedCols);
                result.Add(index);
            }

            return result.ToImmutableList();
        }

        private async Task<IReadOnlyDictionary<string, IDatabaseViewIndex>> LoadIndexLookupAsync()
        {
            var result = new Dictionary<string, IDatabaseViewIndex>();

            var indexes = await IndexesAsync();
            foreach (var index in indexes)
                result[index.Name.LocalName] = index;

            return result.ToImmutableDictionary();
        }

        public Task<IReadOnlyDictionary<string, IDatabaseViewColumn>> ColumnAsync() => _columnLookup.Task;

        private async Task<IReadOnlyDictionary<string, IDatabaseViewColumn>> LoadColumnLookupAsync()
        {
            var result = new Dictionary<string, IDatabaseViewColumn>();

            var columns = await ColumnsAsync();
            foreach (var column in columns.Where(c => c.Name != null))
                result[column.Name.LocalName] = column;

            return result.ToImmutableDictionary();
        }

        private async Task<IList<IDatabaseViewColumn>> LoadColumnsAsync()
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

            var query = await Connection.QueryAsync<ColumnData>(sql, new { SchemaName = Name.Schema, ViewName = Name.LocalName });
            var result = new List<IDatabaseViewColumn>();

            foreach (var row in query)
            {
                var columnTypeName = new Identifier(row.ColumnTypeSchema, row.ColumnTypeName);

                IDbType dbType;
                var columnType = new SqlServerColumnDataType(columnTypeName);
                if (columnType.IsNumericType)
                    dbType = new SqlServerNumericColumnDataType(columnTypeName, row.Precision, row.Scale); // new SqlServerDatabaseNumericColumnType(columnTypeName, row.MaxLength, row.Precision, row.Scale);
                else if (columnType.IsStringType)
                    dbType = new SqlServerStringColumnDataType(columnTypeName, row.MaxLength, row.Collation);
                else
                    dbType = columnType;

                var columnName = new LocalIdentifier(row.ColumnName);
                var isAutoIncrement = row.IdentitySeed.HasValue && row.IdentityIncrement.HasValue;

                var column = new SqlServerDatabaseViewColumn(this, columnName, columnType, row.IsNullable, row.DefaultValue, isAutoIncrement);

                result.Add(column);
            }

            return result.ToImmutableList();
        }

        public Task<IEnumerable<Identifier>> DependenciesAsync() => _dependencies.Task;

        public Task<IEnumerable<Identifier>> DependentsAsync() => _dependents.Task;

        private readonly AsyncLazy<IEnumerable<Identifier>> _dependencies;
        private readonly AsyncLazy<IEnumerable<Identifier>> _dependents;

        private readonly AsyncLazy<IList<IDatabaseViewColumn>> _columnList;
        private readonly AsyncLazy<IReadOnlyDictionary<string, IDatabaseViewColumn>> _columnLookup;

        private readonly AsyncLazy<IEnumerable<IDatabaseViewIndex>> _indexes;
        private readonly AsyncLazy<IReadOnlyDictionary<string, IDatabaseViewIndex>> _indexLookup;
    }
}
