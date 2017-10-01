using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using SJP.Schematic.Core;

namespace SJP.Schematic.Sqlite
{
    public class SqliteRelationalDatabaseView : IRelationalDatabaseView
    {
        public SqliteRelationalDatabaseView(IDbConnection connection, IRelationalDatabase database, Identifier viewName)
        {
            if (viewName == null || viewName.LocalName == null)
                throw new ArgumentNullException(nameof(viewName));

            Connection = connection ?? throw new ArgumentNullException(nameof(connection));
            Database = database ?? throw new ArgumentNullException(nameof(database));

            Name = viewName.LocalName;
        }

        public Identifier Name { get; }

        public IRelationalDatabase Database { get; }

        protected IDbConnection Connection { get; }

        public string Definition => LoadDefinitionSync();

        public Task<string> DefinitionAsync() => LoadDefinitionAsync();

        protected virtual string LoadDefinitionSync()
        {
            const string sql = "select sql from sqlite_master where type = 'view' and tbl_name = @ViewName";
            return Connection.ExecuteScalar<string>(sql, new { ViewName = Name.LocalName });
        }

        protected virtual Task<string> LoadDefinitionAsync()
        {
            const string sql = "select sql from sqlite_master where type = 'view' and tbl_name = @ViewName";
            return Connection.ExecuteScalarAsync<string>(sql, new { ViewName = Name.LocalName });
        }

        public bool IsIndexed => Indexes.Any();

        public IReadOnlyDictionary<Identifier, IDatabaseViewIndex> Index => LoadIndexLookupSync();

        public Task<IReadOnlyDictionary<Identifier, IDatabaseViewIndex>> IndexAsync() => LoadIndexLookupAsync();

        public IEnumerable<IDatabaseViewIndex> Indexes => LoadIndexesSync();

        public Task<IEnumerable<IDatabaseViewIndex>> IndexesAsync() => LoadIndexesAsync();

        protected virtual IEnumerable<IDatabaseViewIndex> LoadIndexesSync() => Enumerable.Empty<IDatabaseViewIndex>();

        protected virtual Task<IEnumerable<IDatabaseViewIndex>> LoadIndexesAsync() => Task.FromResult(LoadIndexesSync());

        protected virtual IReadOnlyDictionary<Identifier, IDatabaseViewIndex> LoadIndexLookupSync()
        {
            var result = new Dictionary<Identifier, IDatabaseViewIndex>();
            return result.AsReadOnlyDictionary();
        }

        protected virtual Task<IReadOnlyDictionary<Identifier, IDatabaseViewIndex>> LoadIndexLookupAsync() => Task.FromResult(LoadIndexLookupSync());

        public IReadOnlyDictionary<Identifier, IDatabaseViewColumn> Column => LoadColumnLookupSync();

        public Task<IReadOnlyDictionary<Identifier, IDatabaseViewColumn>> ColumnAsync() => LoadColumnLookupAsync();

        public IReadOnlyList<IDatabaseViewColumn> Columns => LoadColumnsSync();

        public Task<IReadOnlyList<IDatabaseViewColumn>> ColumnsAsync() => LoadColumnsAsync();

        protected virtual IReadOnlyDictionary<Identifier, IDatabaseViewColumn> LoadColumnLookupSync()
        {
            var result = new Dictionary<Identifier, IDatabaseViewColumn>();

            foreach (var column in Columns.Where(c => c.Name != null))
                result[column.Name.LocalName] = column;

            return result.AsReadOnlyDictionary();
        }

        protected virtual async Task<IReadOnlyDictionary<Identifier, IDatabaseViewColumn>> LoadColumnLookupAsync()
        {
            var result = new Dictionary<Identifier, IDatabaseViewColumn>();

            var columns = await ColumnsAsync().ConfigureAwait(false);
            foreach (var column in columns.Where(c => c.Name != null))
                result[column.Name.LocalName] = column;

            return result.AsReadOnlyDictionary();
        }

        protected virtual IReadOnlyList<IDatabaseViewColumn> LoadColumnsSync()
        {
            /*
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

            var query = Connection.Query<ColumnData>(sql, new { SchemaName = Name.Schema, ViewName = Name.LocalName });*/
            var result = new List<IDatabaseViewColumn>();
            /*
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
                var autoIncrement = isAutoIncrement
                    ? new AutoIncrement(row.IdentitySeed.Value, row.IdentityIncrement.Value)
                    : (IAutoIncrement)null;

                var column = new SqlServerDatabaseViewColumn(this, columnName, columnType, row.IsNullable, row.DefaultValue, autoIncrement);

                result.Add(column);
            }
            */
            return result.AsReadOnly();
        }

        protected virtual async Task<IReadOnlyList<IDatabaseViewColumn>> LoadColumnsAsync()
        {
            /*
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

            var query = await Connection.QueryAsync<ColumnData>(sql, new { SchemaName = Name.Schema, ViewName = Name.LocalName }).ConfigureAwait(false);*/
            var result = new List<IDatabaseViewColumn>();
            /*
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
                var autoIncrement = isAutoIncrement
                    ? new AutoIncrement(row.IdentitySeed.Value, row.IdentityIncrement.Value)
                    : (IAutoIncrement)null;

                var column = new SqlServerDatabaseViewColumn(this, columnName, columnType, row.IsNullable, row.DefaultValue, autoIncrement);

                result.Add(column);
            }
            */

            await Task.Delay(1).ConfigureAwait(false);
            return result.AsReadOnly();
        }
    }
}
