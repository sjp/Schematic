using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LanguageExt;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.PostgreSql.Queries;

namespace SJP.Schematic.PostgreSql.Versions.V11;

/// <summary>
/// A database table provider for PostgreSQL v11 and higher.
/// </summary>
public class PostgreSqlRelationalDatabaseTableProvider : PostgreSqlRelationalDatabaseTableProviderBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PostgreSqlRelationalDatabaseTableProvider"/> class.
    /// </summary>
    /// <param name="connection">A schematic connection.</param>
    /// <param name="identifierDefaults">Database identifier defaults.</param>
    /// <param name="identifierResolver">A database identifier resolver.</param>
    /// <exception cref="ArgumentNullException"><paramref name="connection"/> or <paramref name="identifierDefaults"/> or <paramref name="identifierResolver"/> is <c>null</c>.</exception>
    public PostgreSqlRelationalDatabaseTableProvider(ISchematicConnection connection, IIdentifierDefaults identifierDefaults, IIdentifierResolutionStrategy identifierResolver)
        : base(connection, identifierDefaults, identifierResolver)
    {
    }

    /// <summary>
    /// Retrieves indexes that relate to the given table.
    /// </summary>
    /// <param name="tableName">A table name.</param>
    /// <param name="queryCache">A query cache for the given context.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of indexes.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="tableName"/> or <paramref name="queryCache"/> are <c>null</c>.</exception>
    protected override Task<IReadOnlyCollection<IDatabaseIndex>> LoadIndexesAsync(Identifier tableName, PostgreSqlTableQueryCache queryCache, CancellationToken cancellationToken)
    {
        if (tableName == null)
            throw new ArgumentNullException(nameof(tableName));
        if (queryCache == null)
            throw new ArgumentNullException(nameof(queryCache));

        return LoadIndexesAsyncCore(tableName, queryCache, cancellationToken);
    }

    private async Task<IReadOnlyCollection<IDatabaseIndex>> LoadIndexesAsyncCore(Identifier tableName, PostgreSqlTableQueryCache queryCache, CancellationToken cancellationToken)
    {
        var queryResult = await DbConnection.QueryAsync<GetV11TableIndexes.Result>(
            GetV11TableIndexes.Sql,
            new GetV11TableIndexes.Query { SchemaName = tableName.Schema!, TableName = tableName.LocalName },
            cancellationToken
        ).ConfigureAwait(false);

        if (queryResult.Empty())
            return Array.Empty<IDatabaseIndex>();

        var indexColumns = queryResult.GroupAsDictionary(static row => new { row.IndexName, row.IsUnique, row.IsPrimary, row.KeyColumnCount }).ToList();
        if (indexColumns.Empty())
            return Array.Empty<IDatabaseIndex>();

        var columns = await queryCache.GetColumnsAsync(tableName, cancellationToken).ConfigureAwait(false);
        var columnLookup = GetColumnLookup(columns);

        var result = new List<IDatabaseIndex>(indexColumns.Count);
        foreach (var indexInfo in indexColumns)
        {
            var isUnique = indexInfo.Key.IsUnique;
            var indexName = Identifier.CreateQualifiedIdentifier(indexInfo.Key.IndexName);

            var indexCols = indexInfo.Value
                .Where(static row => row.IndexColumnExpression != null)
                .OrderBy(static row => row.IndexColumnId)
                .Select(row => new
                {
                    row.IsDescending,
                    Expression = row.IndexColumnExpression,
                    Column = row.IndexColumnExpression != null && columnLookup.ContainsKey(row.IndexColumnExpression)
                        ? columnLookup[row.IndexColumnExpression]
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
            var includedCols = indexInfo.Value
                .OrderBy(static row => row.IndexColumnId)
                .Skip(indexInfo.Key.KeyColumnCount)
                .Where(row => row.IndexColumnExpression != null && columnLookup.ContainsKey(row.IndexColumnExpression))
                .Select(row => columnLookup[row.IndexColumnExpression!])
                .ToList();

            var index = new PostgreSqlDatabaseIndex(indexName, isUnique, indexCols, includedCols, Option<string>.None);
            result.Add(index);
        }

        return result;
    }
}