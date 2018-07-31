using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using SJP.Schematic.PostgreSql.Query;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using System.Threading;

namespace SJP.Schematic.PostgreSql
{
    public class PostgreSqlRelationalDatabaseView : IRelationalDatabaseView
    {
        public PostgreSqlRelationalDatabaseView(IDbConnection connection, IRelationalDatabase database, Identifier viewName, IEqualityComparer<Identifier> comparer = null)
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

        public Task<string> DefinitionAsync(CancellationToken cancellationToken = default(CancellationToken)) => LoadDefinitionAsync(cancellationToken);

        protected virtual string LoadDefinitionSync()
        {
            const string sql = @"
select view_definition
from information_schema.views
where table_schema = @SchemaName and table_name = @ViewName";

            return Connection.ExecuteScalar<string>(sql, new { SchemaName = Name.Schema, ViewName = Name.LocalName });
        }

        protected virtual Task<string> LoadDefinitionAsync(CancellationToken cancellationToken)
        {
            const string sql = @"
select view_definition
from information_schema.views
where table_schema = @SchemaName and table_name = @ViewName";

            return Connection.ExecuteScalarAsync<string>(sql, new { SchemaName = Name.Schema, ViewName = Name.LocalName });
        }

        public bool IsIndexed => Indexes.Count > 0;

        public IReadOnlyDictionary<Identifier, IDatabaseViewIndex> Index => LoadIndexLookupSync();

        public Task<IReadOnlyDictionary<Identifier, IDatabaseViewIndex>> IndexAsync(CancellationToken cancellationToken = default(CancellationToken)) => LoadIndexLookupAsync(cancellationToken);

        public IReadOnlyCollection<IDatabaseViewIndex> Indexes => Array.Empty<IDatabaseViewIndex>();

        public Task<IReadOnlyCollection<IDatabaseViewIndex>> IndexesAsync(CancellationToken cancellationToken = default(CancellationToken)) => _emptyIndexes;

        protected virtual IReadOnlyDictionary<Identifier, IDatabaseViewIndex> LoadIndexLookupSync() => _emptyIndexLookup;

        protected virtual Task<IReadOnlyDictionary<Identifier, IDatabaseViewIndex>> LoadIndexLookupAsync(CancellationToken cancellationToken) => _emptyIndexLookupTask;

        private readonly static Task<IReadOnlyCollection<IDatabaseViewIndex>> _emptyIndexes = Task.FromResult<IReadOnlyCollection<IDatabaseViewIndex>>(Array.Empty<IDatabaseViewIndex>());
        private readonly static IReadOnlyDictionary<Identifier, IDatabaseViewIndex> _emptyIndexLookup = new Dictionary<Identifier, IDatabaseViewIndex>();
        private readonly static Task<IReadOnlyDictionary<Identifier, IDatabaseViewIndex>> _emptyIndexLookupTask = Task.FromResult(_emptyIndexLookup);

        public IReadOnlyDictionary<Identifier, IDatabaseViewColumn> Column => LoadColumnLookupSync();

        public Task<IReadOnlyDictionary<Identifier, IDatabaseViewColumn>> ColumnAsync(CancellationToken cancellationToken = default(CancellationToken)) => LoadColumnLookupAsync(cancellationToken);

        public IReadOnlyList<IDatabaseViewColumn> Columns => LoadColumnsSync();

        public Task<IReadOnlyList<IDatabaseViewColumn>> ColumnsAsync(CancellationToken cancellationToken = default(CancellationToken)) => LoadColumnsAsync(cancellationToken);

        protected virtual IReadOnlyDictionary<Identifier, IDatabaseViewColumn> LoadColumnLookupSync()
        {
            var columns = Columns;
            var result = new Dictionary<Identifier, IDatabaseViewColumn>(columns.Count, Comparer);

            foreach (var column in columns.Where(c => c.Name != null))
                result[column.Name.LocalName] = column;

            return result;
        }

        protected virtual async Task<IReadOnlyDictionary<Identifier, IDatabaseViewColumn>> LoadColumnLookupAsync(CancellationToken cancellationToken)
        {
            var columns = await ColumnsAsync(cancellationToken).ConfigureAwait(false);
            var result = new Dictionary<Identifier, IDatabaseViewColumn>(columns.Count, Comparer);

            foreach (var column in columns.Where(c => c.Name != null))
                result[column.Name.LocalName] = column;

            return result;
        }

        protected virtual IReadOnlyList<IDatabaseViewColumn> LoadColumnsSync()
        {
            const string sql = @"
select
    column_name,
    ordinal_position,
    column_default,
    is_nullable,
    data_type,
    character_maximum_length,
    character_octet_length,
    numeric_precision,
    numeric_precision_radix,
    numeric_scale,
    datetime_precision,
    interval_type,
    collation_catalog,
    collation_schema,
    collation_name,
    domain_catalog,
    domain_schema,
    domain_name,
    udt_catalog,
    udt_schema,
    udt_name,
    dtd_identifier
from information_schema.columns
where table_schema = @SchemaName and table_name = @ViewName
order by ordinal_position";

            var query = Connection.Query<ColumnData>(sql, new { SchemaName = Name.Schema, ViewName = Name.LocalName });
            var result = new List<IDatabaseViewColumn>();

            foreach (var row in query)
            {
                var typeMetadata = new ColumnTypeMetadata
                {
                    TypeName = new Identifier(row.data_type),
                    Collation = row.collation_name.IsNullOrWhiteSpace() ? null : new Identifier(row.collation_catalog, row.collation_schema, row.collation_name),
                    //TODO -- need to fix max length as it's different for char-like objects and numeric
                    //MaxLength = row.,
                    // TODO: numeric_precision has a base, can be either binary or decimal, need to use the correct one
                    NumericPrecision = new NumericPrecision(row.numeric_precision, row.numeric_scale)
                };

                var columnType = TypeProvider.CreateColumnType(typeMetadata);
                var columnName = new Identifier(row.column_name);
                IAutoIncrement autoIncrement = null;

                var column = new PostgreSqlDatabaseViewColumn(this, columnName, columnType, row.is_nullable == "YES", row.column_default, autoIncrement);
                result.Add(column);
            }

            return result.AsReadOnly();
        }

        protected virtual async Task<IReadOnlyList<IDatabaseViewColumn>> LoadColumnsAsync(CancellationToken cancellationToken)
        {
            const string sql = @"
select
    column_name,
    ordinal_position,
    column_default,
    is_nullable,
    data_type,
    character_maximum_length,
    character_octet_length,
    numeric_precision,
    numeric_precision_radix,
    numeric_scale,
    datetime_precision,
    interval_type,
    collation_catalog,
    collation_schema,
    collation_name,
    domain_catalog,
    domain_schema,
    domain_name,
    udt_catalog,
    udt_schema,
    udt_name,
    dtd_identifier
from information_schema.columns
where table_schema = @SchemaName and table_name = @ViewName
order by ordinal_position";

            var query = await Connection.QueryAsync<ColumnData>(sql, new { SchemaName = Name.Schema, ViewName = Name.LocalName }).ConfigureAwait(false);
            var result = new List<IDatabaseViewColumn>();

            foreach (var row in query)
            {
                var typeMetadata = new ColumnTypeMetadata
                {
                    TypeName = new Identifier(row.data_type),
                    Collation = row.collation_name.IsNullOrWhiteSpace() ? null : new Identifier(row.collation_catalog, row.collation_schema, row.collation_name),
                    //TODO -- need to fix max length as it's different for char-like objects and numeric
                    //MaxLength = row.,
                    // TODO: numeric_precision has a base, can be either binary or decimal, need to use the correct one
                    NumericPrecision = new NumericPrecision(row.numeric_precision, row.numeric_scale)
                };

                var columnType = TypeProvider.CreateColumnType(typeMetadata);
                var columnName = new Identifier(row.column_name);
                IAutoIncrement autoIncrement = null;

                var column = new PostgreSqlDatabaseViewColumn(this, columnName, columnType, row.is_nullable == "YES", row.column_default, autoIncrement);
                result.Add(column);
            }

            return result.AsReadOnly();
        }
    }
}
