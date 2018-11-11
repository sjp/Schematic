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
using SJP.Schematic.Core.Utilities;

namespace SJP.Schematic.PostgreSql
{
    public class PostgreSqlRelationalDatabaseView : IRelationalDatabaseView
    {
        public PostgreSqlRelationalDatabaseView(IDbConnection connection, IDbTypeProvider typeProvider, Identifier viewName, IIdentifierResolutionStrategy identifierResolver)
        {
            Connection = connection ?? throw new ArgumentNullException(nameof(connection));
            TypeProvider = typeProvider ?? throw new ArgumentNullException(nameof(typeProvider));
            Name = viewName ?? throw new ArgumentNullException(nameof(viewName));
            IdentifierResolver = identifierResolver ?? throw new ArgumentNullException(nameof(identifierResolver));
        }

        public Identifier Name { get; }

        protected IDbTypeProvider TypeProvider { get; }

        protected IDbConnection Connection { get; }

        protected IIdentifierResolutionStrategy IdentifierResolver { get; }

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

        public IReadOnlyDictionary<Identifier, IDatabaseIndex> Index => LoadIndexLookupSync();

        public Task<IReadOnlyDictionary<Identifier, IDatabaseIndex>> IndexAsync(CancellationToken cancellationToken = default(CancellationToken)) => LoadIndexLookupAsync(cancellationToken);

        public IReadOnlyCollection<IDatabaseIndex> Indexes { get; } = Array.Empty<IDatabaseIndex>();

        public Task<IReadOnlyCollection<IDatabaseIndex>> IndexesAsync(CancellationToken cancellationToken = default(CancellationToken)) => _emptyIndexes;

        protected virtual IReadOnlyDictionary<Identifier, IDatabaseIndex> LoadIndexLookupSync() => _emptyIndexLookup;

        protected virtual Task<IReadOnlyDictionary<Identifier, IDatabaseIndex>> LoadIndexLookupAsync(CancellationToken cancellationToken) => _emptyIndexLookupTask;

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
            var query = Connection.Query<ColumnData>(ColumnsQuery, new { SchemaName = Name.Schema, ViewName = Name.LocalName });
            var result = new List<IDatabaseColumn>();

            foreach (var row in query)
            {
                var typeMetadata = new ColumnTypeMetadata
                {
                    TypeName = Identifier.CreateQualifiedIdentifier(row.data_type),
                    Collation = row.collation_name.IsNullOrWhiteSpace() ? null : Identifier.CreateQualifiedIdentifier(row.collation_catalog, row.collation_schema, row.collation_name),
                    //TODO -- need to fix max length as it's different for char-like objects and numeric
                    //MaxLength = row.,
                    // TODO: numeric_precision has a base, can be either binary or decimal, need to use the correct one
                    NumericPrecision = new NumericPrecision(row.numeric_precision, row.numeric_scale)
                };

                var columnType = TypeProvider.CreateColumnType(typeMetadata);
                var columnName = Identifier.CreateQualifiedIdentifier(row.column_name);
                IAutoIncrement autoIncrement = null;

                var column = new DatabaseColumn(columnName, columnType, row.is_nullable == "YES", row.column_default, autoIncrement);
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
                var typeMetadata = new ColumnTypeMetadata
                {
                    TypeName = Identifier.CreateQualifiedIdentifier(row.data_type),
                    Collation = row.collation_name.IsNullOrWhiteSpace() ? null : Identifier.CreateQualifiedIdentifier(row.collation_catalog, row.collation_schema, row.collation_name),
                    //TODO -- need to fix max length as it's different for char-like objects and numeric
                    //MaxLength = row.,
                    // TODO: numeric_precision has a base, can be either binary or decimal, need to use the correct one
                    NumericPrecision = new NumericPrecision(row.numeric_precision, row.numeric_scale)
                };

                var columnType = TypeProvider.CreateColumnType(typeMetadata);
                var columnName = Identifier.CreateQualifiedIdentifier(row.column_name);
                IAutoIncrement autoIncrement = null;

                var column = new DatabaseColumn(columnName, columnType, row.is_nullable == "YES", row.column_default, autoIncrement);
                result.Add(column);
            }

            return result.AsReadOnly();
        }

        protected virtual string ColumnsQuery => ColumnsQuerySql;

        private const string ColumnsQuerySql = @"
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
    }
}
