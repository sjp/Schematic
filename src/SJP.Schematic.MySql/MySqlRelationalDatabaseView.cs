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

namespace SJP.Schematic.MySql
{
    public class MySqlRelationalDatabaseView : IRelationalDatabaseView
    {
        public MySqlRelationalDatabaseView(IDbConnection connection, IDbTypeProvider typeProvider, Identifier viewName)
        {
            Connection = connection ?? throw new ArgumentNullException(nameof(connection));
            TypeProvider = typeProvider ?? throw new ArgumentNullException(nameof(typeProvider));
            Name = viewName ?? throw new ArgumentNullException(nameof(viewName));
        }

        public Identifier Name { get; }

        protected IDbTypeProvider TypeProvider { get; }

        protected IDbConnection Connection { get; }

        public string Definition => LoadDefinitionSync();

        public Task<string> DefinitionAsync(CancellationToken cancellationToken = default(CancellationToken)) => LoadDefinitionAsync(cancellationToken);

        protected virtual string LoadDefinitionSync() => Connection.ExecuteScalar<string>(DefinitionQuery, new { SchemaName = Name.Schema, ViewName = Name.LocalName });

        protected virtual Task<string> LoadDefinitionAsync(CancellationToken cancellationToken) => Connection.ExecuteScalarAsync<string>(DefinitionQuery, new { SchemaName = Name.Schema, ViewName = Name.LocalName });

        protected virtual string DefinitionQuery => DefinitionQuerySql;

        private const string DefinitionQuerySql = @"
select view_definition
from information_schema.views
where table_schema = @SchemaName and table_name = @ViewName";

        public bool IsIndexed => Indexes.Count > 0;

        public IReadOnlyDictionary<Identifier, IDatabaseIndex> Index => _emptyIndexLookup;

        public Task<IReadOnlyDictionary<Identifier, IDatabaseIndex>> IndexAsync(CancellationToken cancellationToken = default(CancellationToken)) => _emptyIndexLookupTask;

        public IReadOnlyCollection<IDatabaseIndex> Indexes { get; } = Array.Empty<IDatabaseIndex>();

        public Task<IReadOnlyCollection<IDatabaseIndex>> IndexesAsync(CancellationToken cancellationToken = default(CancellationToken)) => _emptyIndexes;

        private readonly static Task<IReadOnlyCollection<IDatabaseIndex>> _emptyIndexes = Task.FromResult<IReadOnlyCollection<IDatabaseIndex>>(Array.Empty<IDatabaseIndex>());
        private readonly static IReadOnlyDictionary<Identifier, IDatabaseIndex> _emptyIndexLookup = new Dictionary<Identifier, IDatabaseIndex>();
        private readonly static Task<IReadOnlyDictionary<Identifier, IDatabaseIndex>> _emptyIndexLookupTask = Task.FromResult(_emptyIndexLookup);

        public IReadOnlyDictionary<Identifier, IDatabaseColumn> Column => LoadColumnLookupSync();

        public Task<IReadOnlyDictionary<Identifier, IDatabaseColumn>> ColumnAsync(CancellationToken cancellationToken = default(CancellationToken)) => LoadColumnLookupAsync(cancellationToken);

        public IReadOnlyList<IDatabaseColumn> Columns => LoadColumnsSync();

        public Task<IReadOnlyList<IDatabaseColumn>> ColumnsAsync(CancellationToken cancellationToken = default(CancellationToken)) => LoadColumnsAsync(cancellationToken);

        protected virtual IReadOnlyDictionary<Identifier, IDatabaseColumn> LoadColumnLookupSync()
        {
            var columns = Columns;
            var result = new Dictionary<Identifier, IDatabaseColumn>(columns.Count);

            foreach (var column in columns.Where(c => c.Name != null))
                result[column.Name.LocalName] = column;

            return result;
        }

        protected virtual async Task<IReadOnlyDictionary<Identifier, IDatabaseColumn>> LoadColumnLookupAsync(CancellationToken cancellationToken)
        {
            var columns = await ColumnsAsync(cancellationToken).ConfigureAwait(false);
            var result = new Dictionary<Identifier, IDatabaseColumn>(columns.Count);

            foreach (var column in columns.Where(c => c.Name != null))
                result[column.Name.LocalName] = column;

            return result;
        }

        protected virtual IReadOnlyList<IDatabaseColumn> LoadColumnsSync()
        {
            var query = Connection.Query<ColumnData>(ColumnsQuery, new { SchemaName = Name.Schema, ViewName = Name.LocalName });
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
                var isNullable = !string.Equals(row.IsNullable, "NO", StringComparison.OrdinalIgnoreCase);

                var column = new DatabaseColumn(columnName, columnType, isNullable, row.DefaultValue, autoIncrement);
                result.Add(column);
            }

            return result.AsReadOnly();
        }

        protected virtual async Task<IReadOnlyList<IDatabaseColumn>> LoadColumnsAsync(CancellationToken cancellationToken)
        {
            var query = await Connection.QueryAsync<ColumnData>(ColumnsQuery, new { SchemaName = Name.Schema, ViewName = Name.LocalName }).ConfigureAwait(false);
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
                var isNullable = !string.Equals(row.IsNullable, "NO", StringComparison.OrdinalIgnoreCase);

                var column = new DatabaseColumn(columnName, columnType, isNullable, row.DefaultValue, autoIncrement);
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
where table_schema = @SchemaName and table_name = @ViewName
order by ordinal_position";
    }
}
