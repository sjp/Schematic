using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LanguageExt;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Oracle.Queries;

namespace SJP.Schematic.Oracle;

/// <summary>
/// A table statistics provider for Oracle databases, backed by the optimizer statistics in
/// <c>ALL_TABLES</c> and <c>ALL_INDEXES</c>.
/// </summary>
/// <remarks>
/// Oracle gathers these statistics on a schedule or on demand through <c>DBMS_STATS</c>, rather
/// than maintaining them as rows are written, so they are never exact and are absent entirely
/// until a table has been analysed. The sizes are derived from the blocks the statistics record
/// and so describe allocated space, not the space the rows occupy.
/// </remarks>
/// <seealso cref="ITableStatisticsProvider" />
public class OracleTableStatisticsProvider : ITableStatisticsProvider
{
    /// <summary>
    /// Initializes a new instance of the <see cref="OracleTableStatisticsProvider"/> class.
    /// </summary>
    /// <param name="connection">A database connection factory.</param>
    /// <param name="identifierDefaults">Identifier defaults for the associated database.</param>
    /// <exception cref="ArgumentNullException"><paramref name="connection"/> or <paramref name="identifierDefaults"/> is <see langword="null" />.</exception>
    public OracleTableStatisticsProvider(IDbConnectionFactory connection, IIdentifierDefaults identifierDefaults)
    {
        Connection = connection ?? throw new ArgumentNullException(nameof(connection));
        IdentifierDefaults = identifierDefaults ?? throw new ArgumentNullException(nameof(identifierDefaults));
    }

    /// <summary>
    /// A database connection factory.
    /// </summary>
    /// <value>A database connection factory.</value>
    protected IDbConnectionFactory Connection { get; }

    /// <summary>
    /// Identifier defaults for the associated database.
    /// </summary>
    /// <value>Identifier defaults.</value>
    protected IIdentifierDefaults IdentifierDefaults { get; }

    /// <summary>
    /// Gets the statistics recorded for a database table.
    /// </summary>
    /// <param name="tableName">A database table name.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>Statistics for the table in the 'some' state if the table is known; otherwise 'none'.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="tableName"/> is <see langword="null" />.</exception>
    public OptionAsync<ITableStatistics> GetTableStatistics(Identifier tableName, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(tableName);

        var candidateTableName = QualifyTableName(tableName);
        return Connection.QueryFirstOrNone(
            Queries.GetTableStatistics.Sql,
            new Queries.GetTableStatistics.Query { SchemaName = candidateTableName.Schema!, TableName = candidateTableName.LocalName },
            cancellationToken
        ).Map<ITableStatistics>(row => MapStatistics(candidateTableName, row));
    }

    /// <summary>
    /// Gets the statistics recorded for all database tables.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A collection of table statistics.</returns>
    public async Task<IReadOnlyCollection<ITableStatistics>> GetAllTableStatistics(CancellationToken cancellationToken = default)
    {
        var rows = await Connection.QueryAsync<Queries.GetAllTableStatistics.Result>(Queries.GetAllTableStatistics.Sql, cancellationToken);

        return rows
            .Select(row => MapStatistics(QualifyTableName(Identifier.CreateQualifiedIdentifier(row.SchemaName, row.TableName)), row))
            .ToList();
    }

    private static ITableStatistics MapStatistics(Identifier tableName, ITableStatisticsRow row)
    {
        return new TableStatistics(
            tableName,
            ToCount(row.RowCount),
            isExact: false,
            ToCount(row.DataSizeBytes),
            ToCount(row.IndexSizeBytes)
        );
    }

    // Oracle reports these as NUMBER, which is wider than the counts the model holds; a value that
    // cannot be represented is treated as no value at all rather than being silently wrapped.
    private static Option<long> ToCount(decimal? value)
    {
        return value is >= 0 and <= long.MaxValue
            ? Option<long>.Some((long)value.Value)
            : Option<long>.None;
    }

    /// <summary>
    /// Qualifies the name of a table, using known identifier defaults.
    /// </summary>
    /// <param name="tableName">A table name to qualify.</param>
    /// <returns>A table name that is at least as qualified as its input.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="tableName"/> is <see langword="null" />.</exception>
    protected Identifier QualifyTableName(Identifier tableName)
    {
        ArgumentNullException.ThrowIfNull(tableName);

        var schema = tableName.Schema ?? IdentifierDefaults.Schema;
        return Identifier.CreateQualifiedIdentifier(IdentifierDefaults.Server, IdentifierDefaults.Database, schema, tableName.LocalName);
    }
}
