using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.PostgreSql.Query;

namespace SJP.Schematic.PostgreSql.Versions.V11
{
    public class PostgreSqlRelationalDatabaseTableProvider : V10.PostgreSqlRelationalDatabaseTableProvider
    {
        public PostgreSqlRelationalDatabaseTableProvider(ISchematicConnection connection, IIdentifierDefaults identifierDefaults, IIdentifierResolutionStrategy identifierResolver)
            : base(connection, identifierDefaults, identifierResolver)
        {
        }

        /// <summary>
        /// Retrieves indexes that relate to the given table.
        /// </summary>
        /// <param name="tableName">A table name.</param>
        /// <param name="columns">Columns for the given table.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A collection of indexes.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="tableName"/> or <paramref name="columns"/> are <c>null</c>.</exception>
        protected override Task<IReadOnlyCollection<IDatabaseIndex>> LoadIndexesAsync(Identifier tableName, IReadOnlyDictionary<Identifier, IDatabaseColumn> columns, CancellationToken cancellationToken)
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));
            if (columns == null)
                throw new ArgumentNullException(nameof(columns));

            return LoadIndexesAsyncCore(tableName, columns, cancellationToken);
        }

        private async Task<IReadOnlyCollection<IDatabaseIndex>> LoadIndexesAsyncCore(Identifier tableName, IReadOnlyDictionary<Identifier, IDatabaseColumn> columns, CancellationToken cancellationToken)
        {
            var queryResult = await DbConnection.QueryAsync<IndexColumns>(
                IndexesQuery,
                new { SchemaName = tableName.Schema, TableName = tableName.LocalName },
                cancellationToken
            ).ConfigureAwait(false);

            if (queryResult.Empty())
                return Array.Empty<IDatabaseIndex>();

            var indexColumns = queryResult.GroupBy(row => new { row.IndexName, row.IsUnique, row.IsPrimary, row.KeyColumnCount }).ToList();
            if (indexColumns.Empty())
                return Array.Empty<IDatabaseIndex>();

            var result = new List<IDatabaseIndex>(indexColumns.Count);
            foreach (var indexInfo in indexColumns)
            {
                var isUnique = indexInfo.Key.IsUnique;
                var indexName = Identifier.CreateQualifiedIdentifier(indexInfo.Key.IndexName);

                var indexCols = indexInfo
                    .OrderBy(row => row.IndexColumnId)
                    .Where(row => row.IndexColumnExpression != null)
                    .Select(row => new
                    {
                        row.IsDescending,
                        Expression = row.IndexColumnExpression,
                        Column = row.IndexColumnExpression != null && columns.ContainsKey(row.IndexColumnExpression)
                            ? columns[row.IndexColumnExpression]
                            : null
                    })
                    .Select(row =>
                    {
                        var order = row.IsDescending ? IndexColumnOrder.Descending : IndexColumnOrder.Ascending;
                        var expression = row.Column != null
                            ? Dialect.QuoteName(row.Column.Name)
                            : row.Expression!;
                        return row.Column != null
                            ? new PostgreSqlDatabaseIndexColumn(expression, row.Column, order)
                            : new PostgreSqlDatabaseIndexColumn(expression, order);
                    })
                    .Take(indexInfo.Key.KeyColumnCount)
                    .ToList();
                var includedCols = indexInfo
                    .OrderBy(row => row.IndexColumnId)
                    .Skip(indexInfo.Key.KeyColumnCount)
                    .Where(row => row.IndexColumnExpression != null && columns.ContainsKey(row.IndexColumnExpression))
                    .Select(row => columns[row.IndexColumnExpression!])
                    .ToList();

                var index = new PostgreSqlDatabaseIndex(indexName, isUnique, indexCols, includedCols);
                result.Add(index);
            }

            return result;
        }

        protected override string IndexesQuery => IndexesQuerySql;

        private const string IndexesQuerySql = @"
select
    i.relname as IndexName,
    idx.indisunique as IsUnique,
    idx.indisprimary as IsPrimary,
    idx.indnkeyatts as KeyColumnCount,
    pg_catalog.generate_subscripts(idx.indkey, 1) as IndexColumnId,
    pg_catalog.unnest(array(
        select pg_catalog.pg_get_indexdef(idx.indexrelid, k + 1, true)
        from pg_catalog.generate_subscripts(idx.indkey, 1) k
        order by k
    )) as IndexColumnExpression,
    pg_catalog.unnest(array(
        select pg_catalog.pg_index_column_has_property(idx.indexrelid, k + 1, 'desc')
        from pg_catalog.generate_subscripts(idx.indkey, 1) k
        order by k
    )) as IsDescending,
    (idx.indexprs is not null) or (idx.indkey::int[] @> array[0]) as IsFunctional
from pg_catalog.pg_index idx
    inner join pg_catalog.pg_class t on idx.indrelid = t.oid
    inner join pg_catalog.pg_namespace ns on ns.oid = t.relnamespace
    inner join pg_catalog.pg_class i on i.oid = idx.indexrelid
where
    t.relkind = 'r'
    and t.relname = @TableName
    and ns.nspname = @SchemaName";
    }
}
