using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using SJP.Schematic.MySql.Query;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using System.Threading;
using SJP.Schematic.Core.Utilities;

namespace SJP.Schematic.MySql
{
    public class MySqlRelationalDatabaseView : IRelationalDatabaseView
    {
        public MySqlRelationalDatabaseView(IDbConnection connection, IRelationalDatabase database, Identifier viewName, IEqualityComparer<Identifier> comparer = null)
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

        public bool IsIndexed => Indexes.Any();

        public IReadOnlyDictionary<Identifier, IDatabaseViewIndex> Index => new Dictionary<Identifier, IDatabaseViewIndex>(Comparer);

        public Task<IReadOnlyDictionary<Identifier, IDatabaseViewIndex>> IndexAsync(CancellationToken cancellationToken = default(CancellationToken)) => Task.FromResult(Index);

        public IReadOnlyCollection<IDatabaseViewIndex> Indexes => Array.Empty<IDatabaseViewIndex>();

        public Task<IReadOnlyCollection<IDatabaseViewIndex>> IndexesAsync(CancellationToken cancellationToken = default(CancellationToken)) => _emptyIndexes;

        private readonly static Task<IReadOnlyCollection<IDatabaseViewIndex>> _emptyIndexes = Task.FromResult<IReadOnlyCollection<IDatabaseViewIndex>>(
            Array.Empty<IDatabaseViewIndex>()
        );

        public IReadOnlyDictionary<Identifier, IDatabaseViewColumn> Column => LoadColumnLookupSync();

        public Task<IReadOnlyDictionary<Identifier, IDatabaseViewColumn>> ColumnAsync(CancellationToken cancellationToken = default(CancellationToken)) => LoadColumnLookupAsync(cancellationToken);

        public IReadOnlyList<IDatabaseViewColumn> Columns => LoadColumnsSync();

        public Task<IReadOnlyList<IDatabaseViewColumn>> ColumnsAsync(CancellationToken cancellationToken = default(CancellationToken)) => LoadColumnsAsync(cancellationToken);

        protected virtual IReadOnlyDictionary<Identifier, IDatabaseViewColumn> LoadColumnLookupSync()
        {
            var result = new Dictionary<Identifier, IDatabaseViewColumn>(Comparer);

            foreach (var column in Columns.Where(c => c.Name != null))
                result[column.Name.LocalName] = column;

            return result.AsReadOnlyDictionary();
        }

        protected virtual async Task<IReadOnlyDictionary<Identifier, IDatabaseViewColumn>> LoadColumnLookupAsync(CancellationToken cancellationToken)
        {
            var result = new Dictionary<Identifier, IDatabaseViewColumn>(Comparer);

            var columns = await ColumnsAsync(cancellationToken).ConfigureAwait(false);
            foreach (var column in columns.Where(c => c.Name != null))
                result[column.Name.LocalName] = column;

            return result.AsReadOnlyDictionary();
        }

        protected virtual IReadOnlyList<IDatabaseViewColumn> LoadColumnsSync()
        {
            const string sql = @"
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
where table_schema = @SchemaName and table_name = @ViewName
order by ordinal_position";

            var query = Connection.Query<ColumnData>(sql, new { SchemaName = Name.Schema, ViewName = Name.LocalName });
            var result = new List<IDatabaseViewColumn>();

            foreach (var row in query)
            {
                var precision = row.DateTimePrecision > 0
                    ? new NumericPrecision(row.DateTimePrecision, 0)
                    : new NumericPrecision(row.Precision, row.Scale);

                var typeMetadata = new ColumnTypeMetadata
                {
                    TypeName = new Identifier(row.DataTypeName),
                    Collation = row.Collation.IsNullOrWhiteSpace() ? null : new Identifier(row.Collation),
                    MaxLength = row.CharacterMaxLength,
                    NumericPrecision = precision
                };
                var columnType = TypeProvider.CreateColumnType(typeMetadata);

                var columnName = new Identifier(row.ColumnName);
                var isAutoIncrement = row.ExtraInformation.Contains("auto_increment", StringComparison.OrdinalIgnoreCase);
                var autoIncrement = isAutoIncrement ? new AutoIncrement(1, 1) : (IAutoIncrement)null;
                var isNullable = !string.Equals(row.IsNullable, "NO", StringComparison.OrdinalIgnoreCase);

                var column = new MySqlDatabaseViewColumn(this, columnName, columnType, isNullable, row.DefaultValue, autoIncrement);
                result.Add(column);
            }

            return result.AsReadOnly();
        }

        protected virtual async Task<IReadOnlyList<IDatabaseViewColumn>> LoadColumnsAsync(CancellationToken cancellationToken)
        {
            const string sql = @"
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
where table_schema = @SchemaName and table_name = @ViewName
order by ordinal_position";

            var query = await Connection.QueryAsync<ColumnData>(sql, new { SchemaName = Name.Schema, ViewName = Name.LocalName }).ConfigureAwait(false);
            var result = new List<IDatabaseViewColumn>();

            foreach (var row in query)
            {
                var precision = row.DateTimePrecision > 0
                    ? new NumericPrecision(row.DateTimePrecision, 0)
                    : new NumericPrecision(row.Precision, row.Scale);

                var typeMetadata = new ColumnTypeMetadata
                {
                    TypeName = new Identifier(row.DataTypeName),
                    Collation = row.Collation.IsNullOrWhiteSpace() ? null : new Identifier(row.Collation),
                    MaxLength = row.CharacterMaxLength,
                    NumericPrecision = precision
                };
                var columnType = TypeProvider.CreateColumnType(typeMetadata);

                var columnName = new Identifier(row.ColumnName);
                var isAutoIncrement = row.ExtraInformation.Contains("auto_increment", StringComparison.OrdinalIgnoreCase);
                var autoIncrement = isAutoIncrement ? new AutoIncrement(1, 1) : (IAutoIncrement)null;
                var isNullable = !string.Equals(row.IsNullable, "NO", StringComparison.OrdinalIgnoreCase);

                var column = new MySqlDatabaseViewColumn(this, columnName, columnType, isNullable, row.DefaultValue, autoIncrement);
                result.Add(column);
            }

            return result.AsReadOnly();
        }
    }
}
