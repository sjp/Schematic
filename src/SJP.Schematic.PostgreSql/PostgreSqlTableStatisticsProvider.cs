using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LanguageExt;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.PostgreSql.Queries;

namespace SJP.Schematic.PostgreSql;

/// <summary>
/// A table statistics provider for PostgreSQL databases, backed by <c>pg_class</c> and the object
/// size functions.
/// </summary>
/// <remarks>
/// The row count is the planner's estimate held in <c>pg_class.reltuples</c>, refreshed by
/// <c>ANALYZE</c> and <c>VACUUM</c>, so it is never exact and is absent for a table that has never
/// been analysed. Sizes are read as they stand: the data size covers the table and its TOAST
/// relation, and the index size every index defined on the table.
/// </remarks>
/// <seealso cref="ITableStatisticsProvider" />
public class PostgreSqlTableStatisticsProvider : ITableStatisticsProvider
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PostgreSqlTableStatisticsProvider"/> class.
    /// </summary>
    /// <param name="connection">A database connection factory.</param>
    /// <param name="identifierDefaults">Identifier defaults for the associated database.</param>
    /// <exception cref="ArgumentNullException"><paramref name="connection"/> or <paramref name="identifierDefaults"/> is <see langword="null" />.</exception>
    public PostgreSqlTableStatisticsProvider(IDbConnectionFactory connection, IIdentifierDefaults identifierDefaults)
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
            // reltuples is -1 for a table that has never been analysed, i.e. no estimate at all
            row.RowCount >= 0 ? Option<long>.Some(row.RowCount) : Option<long>.None,
            isExact: false,
            row.DataSizeBytes.ToOption(),
            row.IndexSizeBytes.ToOption()
        );
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
