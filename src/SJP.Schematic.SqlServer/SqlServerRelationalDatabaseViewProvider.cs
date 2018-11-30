using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using LanguageExt;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Core.Utilities;
using SJP.Schematic.SqlServer.Query;

namespace SJP.Schematic.SqlServer
{
    public class SqlServerRelationalDatabaseViewProvider : IRelationalDatabaseViewProvider
    {
        public SqlServerRelationalDatabaseViewProvider(IDbConnection connection, IDatabaseIdentifierDefaults identifierDefaults, IDbTypeProvider typeProvider)
        {
            Connection = connection ?? throw new ArgumentNullException(nameof(connection));
            IdentifierDefaults = identifierDefaults ?? throw new ArgumentNullException(nameof(identifierDefaults));
            TypeProvider = typeProvider ?? throw new ArgumentNullException(nameof(typeProvider));
        }

        protected IDbConnection Connection { get; }

        protected IDatabaseIdentifierDefaults IdentifierDefaults { get; }

        protected IDbTypeProvider TypeProvider { get; }

        public IReadOnlyCollection<IRelationalDatabaseView> Views
        {
            get
            {
                var viewNames = Connection.Query<QualifiedName>(ViewsQuery)
                    .Select(dto => Identifier.CreateQualifiedIdentifier(dto.SchemaName, dto.ObjectName))
                    .ToList();

                var views = viewNames
                    .Select(LoadViewSync)
                    .Somes();
                return new ReadOnlyCollectionSlim<IRelationalDatabaseView>(viewNames.Count, views);
            }
        }

        public async Task<IReadOnlyCollection<IRelationalDatabaseView>> ViewsAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            var queryResult = await Connection.QueryAsync<QualifiedName>(ViewsQuery).ConfigureAwait(false);
            var viewNames = queryResult
                .Select(dto => Identifier.CreateQualifiedIdentifier(dto.SchemaName, dto.ObjectName))
                .ToList();

            var views = await viewNames
                .Select(name => LoadViewAsync(name, cancellationToken))
                .Somes()
                .ConfigureAwait(false);

            return views.ToList();
        }

        protected virtual string ViewsQuery => ViewsQuerySql;

        private const string ViewsQuerySql = "select schema_name(schema_id) as SchemaName, name as ObjectName from sys.views order by schema_name(schema_id), name";

        public Option<IRelationalDatabaseView> GetView(Identifier viewName)
        {
            if (viewName == null)
                throw new ArgumentNullException(nameof(viewName));

            var candidateViewName = QualifyViewName(viewName);
            return LoadViewSync(candidateViewName);
        }

        public OptionAsync<IRelationalDatabaseView> GetViewAsync(Identifier viewName, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (viewName == null)
                throw new ArgumentNullException(nameof(viewName));

            var candidateViewName = QualifyViewName(viewName);
            return LoadViewAsync(candidateViewName, cancellationToken);
        }

        protected Option<Identifier> GetResolvedViewName(Identifier viewName)
        {
            if (viewName == null)
                throw new ArgumentNullException(nameof(viewName));

            var candidateViewName = QualifyViewName(viewName);
            var qualifiedViewName = Connection.QueryFirstOrNone<QualifiedName>(
                ViewNameQuery,
                new { SchemaName = candidateViewName.Schema, ViewName = candidateViewName.LocalName }
            );

            return qualifiedViewName.Map(name => Identifier.CreateQualifiedIdentifier(candidateViewName.Server, candidateViewName.Database, name.SchemaName, name.ObjectName));
        }

        protected OptionAsync<Identifier> GetResolvedViewNameAsync(Identifier viewName, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (viewName == null)
                throw new ArgumentNullException(nameof(viewName));

            var candidateViewName = QualifyViewName(viewName);
            var qualifiedViewName = Connection.QueryFirstOrNoneAsync<QualifiedName>(
                ViewNameQuery,
                new { SchemaName = candidateViewName.Schema, ViewName = candidateViewName.LocalName }
            );

            return qualifiedViewName.Map(name => Identifier.CreateQualifiedIdentifier(candidateViewName.Server, candidateViewName.Database, name.SchemaName, name.ObjectName));
        }

        protected virtual string ViewNameQuery => ViewNameQuerySql;

        private const string ViewNameQuerySql = @"
select top 1 schema_name(schema_id) as SchemaName, name as ObjectName
from sys.views
where schema_id = schema_id(@SchemaName) and name = @ViewName";

        protected virtual Option<IRelationalDatabaseView> LoadViewSync(Identifier viewName)
        {
            if (viewName == null)
                throw new ArgumentNullException(nameof(viewName));

            var candidateViewName = QualifyViewName(viewName);
            var resolvedViewNameOption = GetResolvedViewName(candidateViewName);
            if (resolvedViewNameOption.IsNone)
                return Option<IRelationalDatabaseView>.None;

            var resolvedViewName = resolvedViewNameOption.UnwrapSome();

            var definition = LoadDefinitionSync(resolvedViewName);
            var columns = LoadColumnsSync(resolvedViewName);
            var columnLookup = GetColumnLookup(columns);
            var indexes = LoadIndexesSync(resolvedViewName, columnLookup);

            var view = new RelationalDatabaseView(resolvedViewName, definition, columns, indexes);
            return Option<IRelationalDatabaseView>.Some(view);
        }

        protected virtual OptionAsync<IRelationalDatabaseView> LoadViewAsync(Identifier viewName, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (viewName == null)
                throw new ArgumentNullException(nameof(viewName));

            var candidateViewName = QualifyViewName(viewName);
            return LoadViewAsyncCore(candidateViewName, cancellationToken).ToAsync();
        }

        private async Task<Option<IRelationalDatabaseView>> LoadViewAsyncCore(Identifier viewName, CancellationToken cancellationToken)
        {
            var candidateViewName = QualifyViewName(viewName);
            var resolvedViewNameOption = GetResolvedViewNameAsync(candidateViewName, cancellationToken);
            var resolvedViewNameOptionIsNone = await resolvedViewNameOption.IsNone.ConfigureAwait(false);
            if (resolvedViewNameOptionIsNone)
                return Option<IRelationalDatabaseView>.None;

            var resolvedViewName = await resolvedViewNameOption.UnwrapSomeAsync().ConfigureAwait(false);

            var columnsTask = LoadColumnsAsync(resolvedViewName, cancellationToken);
            var definitionTask = LoadDefinitionAsync(resolvedViewName, cancellationToken);
            await Task.WhenAll(columnsTask, definitionTask).ConfigureAwait(false);

            var columns = columnsTask.Result;
            var columnLookup = GetColumnLookup(columns);
            var definition = definitionTask.Result;
            var indexes = await LoadIndexesAsync(resolvedViewName, columnLookup, cancellationToken).ConfigureAwait(false);

            var view = new RelationalDatabaseView(resolvedViewName, definition, columns, indexes);
            return Option<IRelationalDatabaseView>.Some(view);
        }

        protected virtual string LoadDefinitionSync(Identifier viewName)
        {
            if (viewName == null)
                throw new ArgumentNullException(nameof(viewName));

            return Connection.ExecuteScalar<string>(DefinitionQuery, new { SchemaName = viewName.Schema, ViewName = viewName.LocalName });
        }

        protected virtual Task<string> LoadDefinitionAsync(Identifier viewName, CancellationToken cancellationToken)
        {
            if (viewName == null)
                throw new ArgumentNullException(nameof(viewName));

            return Connection.ExecuteScalarAsync<string>(DefinitionQuery, new { SchemaName = viewName.Schema, ViewName = viewName.LocalName });
        }

        protected virtual string DefinitionQuery => DefinitionQuerySql;

        private const string DefinitionQuerySql = @"
select sm.definition
from sys.sql_modules sm
inner join sys.views v on sm.object_id = v.object_id
where schema_name(v.schema_id) = @SchemaName and v.name = @ViewName";

        protected virtual IReadOnlyCollection<IDatabaseIndex> LoadIndexesSync(Identifier viewName, IReadOnlyDictionary<Identifier, IDatabaseColumn> columns)
        {
            if (viewName == null)
                throw new ArgumentNullException(nameof(viewName));
            if (columns == null)
                throw new ArgumentNullException(nameof(columns));

            var queryResult = Connection.Query<IndexColumns>(IndexesQuery, new { SchemaName = viewName.Schema, ViewName = viewName.LocalName });
            if (queryResult.Empty())
                return Array.Empty<IDatabaseIndex>();

            var indexColumns = queryResult.GroupBy(row => new { row.IndexName, row.IsUnique, row.IsDisabled }).ToList();
            if (indexColumns.Count == 0)
                return Array.Empty<IDatabaseIndex>();

            var result = new List<IDatabaseIndex>(indexColumns.Count);
            foreach (var indexInfo in indexColumns)
            {
                var isUnique = indexInfo.Key.IsUnique;
                var indexName = Identifier.CreateQualifiedIdentifier(indexInfo.Key.IndexName);
                var isEnabled = !indexInfo.Key.IsDisabled;

                var indexCols = indexInfo
                    .Where(row => !row.IsIncludedColumn)
                    .OrderBy(row => row.KeyOrdinal)
                    .ThenBy(row => row.IndexColumnId)
                    .Select(row => new { row.IsDescending, Column = columns[row.ColumnName] })
                    .Select(row => new DatabaseIndexColumn(row.Column, row.IsDescending ? IndexColumnOrder.Descending : IndexColumnOrder.Ascending))
                    .ToList();

                var includedCols = indexInfo
                    .Where(row => row.IsIncludedColumn)
                    .OrderBy(row => row.KeyOrdinal)
                    .ThenBy(row => row.IndexColumnId)
                    .Select(row => columns[row.ColumnName])
                    .ToList();

                var index = new DatabaseIndex(indexName, isUnique, indexCols, includedCols, isEnabled);
                result.Add(index);
            }

            return result;
        }

        protected virtual Task<IReadOnlyCollection<IDatabaseIndex>> LoadIndexesAsync(Identifier viewName, IReadOnlyDictionary<Identifier, IDatabaseColumn> columns, CancellationToken cancellationToken)
        {
            if (viewName == null)
                throw new ArgumentNullException(nameof(viewName));
            if (columns == null)
                throw new ArgumentNullException(nameof(columns));

            return LoadIndexesAsyncCore(viewName, columns, cancellationToken);
        }

        private async Task<IReadOnlyCollection<IDatabaseIndex>> LoadIndexesAsyncCore(Identifier viewName, IReadOnlyDictionary<Identifier, IDatabaseColumn> columns, CancellationToken cancellationToken)
        {
            var queryResult = await Connection.QueryAsync<IndexColumns>(IndexesQuery, new { SchemaName = viewName.Schema, ViewName = viewName.LocalName }).ConfigureAwait(false);
            if (queryResult.Empty())
                return Array.Empty<IDatabaseIndex>();

            var indexColumns = queryResult.GroupBy(row => new { row.IndexName, row.IsUnique, row.IsDisabled }).ToList();
            if (indexColumns.Count == 0)
                return Array.Empty<IDatabaseIndex>();

            var result = new List<IDatabaseIndex>(indexColumns.Count);
            foreach (var indexInfo in indexColumns)
            {
                var isUnique = indexInfo.Key.IsUnique;
                var indexName = Identifier.CreateQualifiedIdentifier(indexInfo.Key.IndexName);
                var isEnabled = !indexInfo.Key.IsDisabled;

                var indexCols = indexInfo
                    .Where(row => !row.IsIncludedColumn)
                    .OrderBy(row => row.KeyOrdinal)
                    .ThenBy(row => row.IndexColumnId)
                    .Select(row => new { row.IsDescending, Column = columns[row.ColumnName] })
                    .Select(row => new DatabaseIndexColumn(row.Column, row.IsDescending ? IndexColumnOrder.Descending : IndexColumnOrder.Ascending))
                    .ToList();

                var includedCols = indexInfo
                    .Where(row => row.IsIncludedColumn)
                    .OrderBy(row => row.KeyOrdinal)
                    .ThenBy(row => row.IndexColumnId)
                    .Select(row => columns[row.ColumnName])
                    .ToList();

                var index = new DatabaseIndex(indexName, isUnique, indexCols, includedCols, isEnabled);
                result.Add(index);
            }

            return result;
        }

        protected virtual string IndexesQuery => IndexesQuerySql;

        private const string IndexesQuerySql = @"
select
    i.name as IndexName,
    i.is_unique as IsUnique,
    ic.key_ordinal as KeyOrdinal,
    ic.index_column_id as IndexColumnId,
    ic.is_included_column as IsIncludedColumn,
    ic.is_descending_key as IsDescending,
    c.name as ColumnName,
    i.is_disabled as IsDisabled
from sys.views v
inner join sys.indexes i on v.object_id = i.object_id
inner join sys.index_columns ic on i.object_id = ic.object_id and i.index_id = ic.index_id
inner join sys.columns c on ic.object_id = c.object_id and ic.column_id = c.column_id
where schema_name(v.schema_id) = @SchemaName and v.name = @ViewName
    and i.is_primary_key = 0 and i.is_unique_constraint = 0
order by ic.index_id, ic.key_ordinal, ic.index_column_id";

        protected virtual IReadOnlyList<IDatabaseColumn> LoadColumnsSync(Identifier viewName)
        {
            if (viewName == null)
                throw new ArgumentNullException(nameof(viewName));

            var query = Connection.Query<ColumnData>(ColumnsQuery, new { SchemaName = viewName.Schema, ViewName = viewName.LocalName });
            var result = new List<IDatabaseColumn>();

            foreach (var row in query)
            {
                var typeMetadata = new ColumnTypeMetadata
                {
                    TypeName = Identifier.CreateQualifiedIdentifier(row.ColumnTypeSchema, row.ColumnTypeName),
                    Collation = row.Collation.IsNullOrWhiteSpace() ? null : Identifier.CreateQualifiedIdentifier(row.Collation),
                    MaxLength = row.MaxLength,
                    NumericPrecision = new NumericPrecision(row.Precision, row.Scale)
                };
                var columnType = TypeProvider.CreateColumnType(typeMetadata);

                var columnName = Identifier.CreateQualifiedIdentifier(row.ColumnName);
                var isAutoIncrement = row.IdentitySeed.HasValue && row.IdentityIncrement.HasValue;
                var autoIncrement = isAutoIncrement
                    ? new AutoIncrement(row.IdentitySeed.Value, row.IdentityIncrement.Value)
                    : (IAutoIncrement)null;

                var column = new DatabaseColumn(columnName, columnType, row.IsNullable, row.DefaultValue, autoIncrement);

                result.Add(column);
            }

            return result.AsReadOnly();
        }

        protected virtual Task<IReadOnlyList<IDatabaseColumn>> LoadColumnsAsync(Identifier viewName, CancellationToken cancellationToken)
        {
            if (viewName == null)
                throw new ArgumentNullException(nameof(viewName));

            return LoadColumnsAsyncCore(viewName, cancellationToken);
        }

        private async Task<IReadOnlyList<IDatabaseColumn>> LoadColumnsAsyncCore(Identifier viewName, CancellationToken cancellationToken)
        {
            var query = await Connection.QueryAsync<ColumnData>(ColumnsQuery, new { SchemaName = viewName.Schema, ViewName = viewName.LocalName }).ConfigureAwait(false);
            var result = new List<IDatabaseColumn>();

            foreach (var row in query)
            {
                var typeMetadata = new ColumnTypeMetadata
                {
                    TypeName = Identifier.CreateQualifiedIdentifier(row.ColumnTypeSchema, row.ColumnTypeName),
                    Collation = row.Collation.IsNullOrWhiteSpace() ? null : Identifier.CreateQualifiedIdentifier(row.Collation),
                    MaxLength = row.MaxLength,
                    NumericPrecision = new NumericPrecision(row.Precision, row.Scale)
                };
                var columnType = TypeProvider.CreateColumnType(typeMetadata);

                var columnName = Identifier.CreateQualifiedIdentifier(row.ColumnName);
                var isAutoIncrement = row.IdentitySeed.HasValue && row.IdentityIncrement.HasValue;
                var autoIncrement = isAutoIncrement
                    ? new AutoIncrement(row.IdentitySeed.Value, row.IdentityIncrement.Value)
                    : (IAutoIncrement)null;

                var column = new DatabaseColumn(columnName, columnType, row.IsNullable, row.DefaultValue, autoIncrement);

                result.Add(column);
            }

            return result.AsReadOnly();
        }

        protected virtual string ColumnsQuery => ColumnsQuerySql;

        private const string ColumnsQuerySql = @"
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

        protected Identifier QualifyViewName(Identifier viewName)
        {
            if (viewName == null)
                throw new ArgumentNullException(nameof(viewName));

            var schema = viewName.Schema ?? IdentifierDefaults.Schema;
            return Identifier.CreateQualifiedIdentifier(IdentifierDefaults.Server, IdentifierDefaults.Database, schema, viewName.LocalName);
        }

        private static IReadOnlyDictionary<Identifier, IDatabaseColumn> GetColumnLookup(IReadOnlyCollection<IDatabaseColumn> columns)
        {
            if (columns == null)
                throw new ArgumentNullException(nameof(columns));

            var result = new Dictionary<Identifier, IDatabaseColumn>(columns.Count);

            foreach (var column in columns)
            {
                if (column.Name != null)
                    result[column.Name.LocalName] = column;
            }

            return result;
        }
    }
}
