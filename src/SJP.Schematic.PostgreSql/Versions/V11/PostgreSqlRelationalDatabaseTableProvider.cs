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
    /// <exception cref="ArgumentNullException"><paramref name="connection"/> or <paramref name="identifierDefaults"/> or <paramref name="identifierResolver"/> is <see langword="null" />.</exception>
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
    /// <exception cref="ArgumentNullException"><paramref name="tableName"/> or <paramref name="queryCache"/> are <see langword="null" />.</exception>
    protected override Task<IReadOnlyCollection<IDatabaseIndex>> LoadIndexesAsync(Identifier tableName, PostgreSqlTableQueryCache queryCache, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(tableName);
        ArgumentNullException.ThrowIfNull(queryCache);

        return LoadIndexesAsyncCore(tableName, queryCache, cancellationToken);
    }

    private async Task<IReadOnlyCollection<IDatabaseIndex>> LoadIndexesAsyncCore(Identifier tableName, PostgreSqlTableQueryCache queryCache, CancellationToken cancellationToken)
    {
        var queryResult = await DbConnection.QueryAsync(
            GetV11TableIndexes.Sql,
            new GetV11TableIndexes.Query { SchemaName = tableName.Schema!, TableName = tableName.LocalName },
            cancellationToken
        );

        if (queryResult.Empty())
            return [];

        var indexColumns = queryResult
            .GroupAsDictionary(static row => new
            {
                row.IndexName,
                row.IsUnique,
                row.IsPrimary,
                row.FilterDefinition,
                row.KeyColumnCount,
            })
            .ToList();
        if (indexColumns.Empty())
            return [];

        var columns = await queryCache.GetColumnsAsync(tableName, cancellationToken);
        var columnLookup = GetColumnLookup(columns);

        var result = new List<IDatabaseIndex>(indexColumns.Count);
        foreach (var indexInfo in indexColumns)
        {
            var isUnique = indexInfo.Key.IsUnique;
            var indexName = Identifier.CreateQualifiedIdentifier(indexInfo.Key.IndexName);

            var filterDefinition = !indexInfo.Key.FilterDefinition.IsNullOrWhiteSpace()
                ? Option<string>.Some(indexInfo.Key.FilterDefinition)
                : Option<string>.None;

            var indexCols = indexInfo.Value
                .Where(static row => row.IndexColumnExpression != null)
                .OrderBy(static row => row.IndexColumnId)
                .Select(row => new
                {
                    row.IsDescending,
                    Expression = row.IndexColumnExpression,
                    Column = row.IndexColumnExpression != null && columnLookup.ContainsKey(row.IndexColumnExpression)
                        ? columnLookup[row.IndexColumnExpression]
                        : null,
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

            var index = new PostgreSqlDatabaseIndex(indexName, isUnique, indexCols, includedCols, filterDefinition);
            result.Add(index);
        }

        return result;
    }
}