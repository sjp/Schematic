using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using EnumsNET;
using LanguageExt;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Core.Utilities;
using SJP.Schematic.Oracle.Query;

namespace SJP.Schematic.Oracle
{
    public class OracleRelationalDatabaseViewProvider : IRelationalDatabaseViewProvider
    {
        public OracleRelationalDatabaseViewProvider(IDbConnection connection, IIdentifierDefaults identifierDefaults, IIdentifierResolutionStrategy identifierResolver, IDbTypeProvider typeProvider)
        {
            Connection = connection ?? throw new ArgumentNullException(nameof(connection));
            IdentifierDefaults = identifierDefaults ?? throw new ArgumentNullException(nameof(identifierDefaults));
            IdentifierResolver = identifierResolver ?? throw new ArgumentNullException(nameof(identifierResolver));
            TypeProvider = typeProvider ?? throw new ArgumentNullException(nameof(typeProvider));
        }

        protected IDbConnection Connection { get; }

        protected IIdentifierDefaults IdentifierDefaults { get; }

        protected IIdentifierResolutionStrategy IdentifierResolver { get; }

        protected IDbTypeProvider TypeProvider { get; }

        protected IDatabaseDialect Dialect { get; }

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
            var queryResult = await Connection.QueryAsync<QualifiedName>(ViewsQuery, cancellationToken).ConfigureAwait(false);
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

        private const string ViewsQuerySql = @"
select
    v.OWNER as SchemaName,
    v.VIEW_NAME as ObjectName
from ALL_VIEWS v
inner join ALL_OBJECTS o on v.OWNER = o.OWNER and v.VIEW_NAME = o.OBJECT_NAME
where o.ORACLE_MAINTAINED <> 'Y'
order by v.OWNER, v.VIEW_NAME";

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

            var resolvedNames = IdentifierResolver
                .GetResolutionOrder(viewName)
                .Select(QualifyViewName);

            return resolvedNames
                .Select(GetResolvedViewNameStrict)
                .FirstSome();
        }

        protected OptionAsync<Identifier> GetResolvedViewNameAsync(Identifier viewName, CancellationToken cancellationToken)
        {
            if (viewName == null)
                throw new ArgumentNullException(nameof(viewName));

            var resolvedNames = IdentifierResolver
                .GetResolutionOrder(viewName)
                .Select(QualifyViewName);

            return resolvedNames
                .Select(name => GetResolvedViewNameStrictAsync(name, cancellationToken))
                .FirstSomeAsync(cancellationToken);
        }

        protected Option<Identifier> GetResolvedViewNameStrict(Identifier viewName)
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

        protected OptionAsync<Identifier> GetResolvedViewNameStrictAsync(Identifier viewName, CancellationToken cancellationToken)
        {
            if (viewName == null)
                throw new ArgumentNullException(nameof(viewName));

            var candidateViewName = QualifyViewName(viewName);
            var qualifiedViewName = Connection.QueryFirstOrNoneAsync<QualifiedName>(
                ViewNameQuery,
                new { SchemaName = candidateViewName.Schema, ViewName = candidateViewName.LocalName },
                cancellationToken
            );

            return qualifiedViewName.Map(name => Identifier.CreateQualifiedIdentifier(candidateViewName.Server, candidateViewName.Database, name.SchemaName, name.ObjectName));
        }

        protected virtual string ViewNameQuery => ViewNameQuerySql;

        private const string ViewNameQuerySql = @"
select v.OWNER as SchemaName, v.VIEW_NAME as ObjectName
from ALL_VIEWS v
inner join ALL_OBJECTS o on v.OWNER = o.OWNER and v.VIEW_NAME = o.OBJECT_NAME
where v.OWNER = :SchemaName and v.VIEW_NAME = :ViewName and o.ORACLE_MAINTAINED <> 'Y'";

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
            var indexes = Array.Empty<IDatabaseIndex>();

            var view = new RelationalDatabaseView(resolvedViewName, definition, columns, indexes);
            return Option<IRelationalDatabaseView>.Some(view);
        }

        protected virtual OptionAsync<IRelationalDatabaseView> LoadViewAsync(Identifier viewName, CancellationToken cancellationToken)
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
            var definition = definitionTask.Result;
            var indexes = Array.Empty<IDatabaseIndex>();

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

            return Connection.ExecuteScalarAsync<string>(
                DefinitionQuery,
                new { SchemaName = viewName.Schema, ViewName = viewName.LocalName },
                cancellationToken
            );
        }

        protected virtual string DefinitionQuery => DefinitionQuerySql;

        private const string DefinitionQuerySql = @"
select TEXT
from ALL_VIEWS
where OWNER = :SchemaName and VIEW_NAME = :ViewName";

        protected virtual IReadOnlyList<IDatabaseColumn> LoadColumnsSync(Identifier viewName)
        {
            if (viewName == null)
                throw new ArgumentNullException(nameof(viewName));

            var query = Connection.Query<ColumnData>(ColumnsQuery, new { SchemaName = viewName.Schema, ViewName = viewName.LocalName });

            var columnNames = query.Select(row => row.ColumnName).ToList();
            var notNullableColumnNames = GetNotNullConstrainedColumns(viewName, columnNames);
            var result = new List<IDatabaseColumn>();

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

                var column = new OracleDatabaseColumn(columnName, columnType, isNullable, row.DefaultValue);

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
            var query = await Connection.QueryAsync<ColumnData>(
                ColumnsQuery,
                new { SchemaName = viewName.Schema, ViewName = viewName.LocalName },
                cancellationToken
            ).ConfigureAwait(false);

            var columnNames = query.Select(row => row.ColumnName).ToList();
            var notNullableColumnNames = await GetNotNullConstrainedColumnsAsync(viewName, columnNames, cancellationToken).ConfigureAwait(false);
            var result = new List<IDatabaseColumn>();

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

                var column = new OracleDatabaseColumn(columnName, columnType, isNullable, row.DefaultValue);

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

        protected virtual IReadOnlyCollection<IDatabaseIndex> LoadIndexesSync(Identifier viewName, IReadOnlyDictionary<Identifier, IDatabaseColumn> columns)
        {
            if (viewName == null)
                throw new ArgumentNullException(nameof(viewName));
            if (columns == null)
                throw new ArgumentNullException(nameof(columns));

            var queryResult = Connection.Query<IndexColumns>(IndexesQuery, new { SchemaName = viewName.Schema, ViewName = viewName.LocalName });
            if (queryResult.Empty())
                return Array.Empty<IDatabaseIndex>();

            var indexColumns = queryResult.GroupBy(row => new { row.IndexName, row.IndexProperty, row.Uniqueness }).ToList();
            if (indexColumns.Empty())
                return Array.Empty<IDatabaseIndex>();

            var result = new List<IDatabaseIndex>(indexColumns.Count);
            foreach (var indexInfo in indexColumns)
            {
                if (!Enums.TryToObject<OracleIndexProperties>(indexInfo.Key.IndexProperty, out var indexProperties))
                    indexProperties = OracleIndexProperties.None;

                var isUnique = indexInfo.Key.Uniqueness == "UNIQUE";
                var indexName = Identifier.CreateQualifiedIdentifier(indexInfo.Key.IndexName);

                var indexCols = indexInfo
                    .OrderBy(row => row.ColumnPosition)
                    .Select(row => new { row.IsDescending, Column = columns[row.ColumnName] })
                    .Select(row =>
                    {
                        var order = row.IsDescending == "Y" ? IndexColumnOrder.Descending : IndexColumnOrder.Ascending;
                        var expression = Dialect.QuoteName(row.Column.Name);
                        return new DatabaseIndexColumn(expression, row.Column, order);
                    })
                    .ToList();

                var index = new OracleDatabaseIndex(indexName, isUnique, indexCols, indexProperties);
                result.Add(index);
            }

            return result;
        }

        protected virtual Task<IReadOnlyCollection<IDatabaseIndex>> IndexesAsync(Identifier viewName, IReadOnlyDictionary<Identifier, IDatabaseColumn> columns, CancellationToken cancellationToken)
        {
            if (viewName == null)
                throw new ArgumentNullException(nameof(viewName));
            if (columns == null)
                throw new ArgumentNullException(nameof(columns));

            return IndexesAsyncCore(viewName, columns, cancellationToken);
        }

        private async Task<IReadOnlyCollection<IDatabaseIndex>> IndexesAsyncCore(Identifier viewName, IReadOnlyDictionary<Identifier, IDatabaseColumn> columns, CancellationToken cancellationToken)
        {
            var queryResult = await Connection.QueryAsync<IndexColumns>(
                IndexesQuery,
                new { SchemaName = viewName.Schema, ViewName = viewName.LocalName },
                cancellationToken
            ).ConfigureAwait(false);

            if (queryResult.Empty())
                return Array.Empty<IDatabaseIndex>();

            var indexColumns = queryResult.GroupBy(row => new { row.IndexName, row.IndexProperty, row.Uniqueness }).ToList();
            if (indexColumns.Empty())
                return Array.Empty<IDatabaseIndex>();

            var result = new List<IDatabaseIndex>(indexColumns.Count);
            foreach (var indexInfo in indexColumns)
            {
                if (!Enums.TryToObject<OracleIndexProperties>(indexInfo.Key.IndexProperty, out var indexProperties))
                    indexProperties = OracleIndexProperties.None;

                var isUnique = indexInfo.Key.Uniqueness == "UNIQUE";
                var indexName = Identifier.CreateQualifiedIdentifier(indexInfo.Key.IndexName);

                var indexCols = indexInfo
                    .OrderBy(row => row.ColumnPosition)
                    .Select(row => new { row.IsDescending, Column = columns[row.ColumnName] })
                    .Select(row =>
                    {
                        var order = row.IsDescending == "Y" ? IndexColumnOrder.Descending : IndexColumnOrder.Ascending;
                        var expression = Dialect.QuoteName(row.Column.Name);
                        return new DatabaseIndexColumn(expression, row.Column, order);
                    })
                    .ToList();

                var index = new OracleDatabaseIndex(indexName, isUnique, indexCols, indexProperties);
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

        protected IEnumerable<string> GetNotNullConstrainedColumns(Identifier viewName, IEnumerable<string> columnNames)
        {
            if (viewName == null)
                throw new ArgumentNullException(nameof(viewName));
            if (columnNames == null)
                throw new ArgumentNullException(nameof(columnNames));

            var checks = Connection.Query<CheckConstraintData>(ChecksQuery, new { SchemaName = viewName.Schema, TableName = viewName.LocalName });
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

        protected Task<IEnumerable<string>> GetNotNullConstrainedColumnsAsync(Identifier viewName, IEnumerable<string> columnNames, CancellationToken cancellationToken)
        {
            if (viewName == null)
                throw new ArgumentNullException(nameof(viewName));
            if (columnNames == null)
                throw new ArgumentNullException(nameof(columnNames));

            return GetNotNullConstrainedColumnsAsyncCore(viewName, columnNames, cancellationToken);
        }

        private async Task<IEnumerable<string>> GetNotNullConstrainedColumnsAsyncCore(Identifier viewName, IEnumerable<string> columnNames, CancellationToken cancellationToken)
        {
            var checks = await Connection.QueryAsync<CheckConstraintData>(
                ChecksQuery,
                new { SchemaName = viewName.Schema, TableName = viewName.LocalName },
                cancellationToken
            ).ConfigureAwait(false);

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

        protected Identifier QualifyViewName(Identifier viewName)
        {
            if (viewName == null)
                throw new ArgumentNullException(nameof(viewName));

            var schema = viewName.Schema ?? IdentifierDefaults.Schema;
            return Identifier.CreateQualifiedIdentifier(IdentifierDefaults.Server, IdentifierDefaults.Database, schema, viewName.LocalName);
        }
    }
}
