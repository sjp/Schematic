using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LanguageExt;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.MySql.Queries;

namespace SJP.Schematic.MySql;

/// <summary>
/// A table statistics provider for MySQL databases, backed by <c>information_schema.tables</c>.
/// </summary>
/// <remarks>
/// Every value here is whatever the storage engine last reported. For InnoDB the row count is
/// extrapolated from a sample of index pages, so it is never exact and can be substantially wrong;
/// the sizes are the space allocated to the table's data and index files rather than the space its
/// rows occupy.
/// </remarks>
/// <seealso cref="ITableStatisticsProvider" />
public class MySqlTableStatisticsProvider : ITableStatisticsProvider
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MySqlTableStatisticsProvider"/> class.
    /// </summary>
    /// <param name="connection">A database connection factory.</param>
    /// <param name="identifierDefaults">Identifier defaults for the associated database.</param>
    /// <exception cref="ArgumentNullException"><paramref name="connection"/> or <paramref name="identifierDefaults"/> is <see langword="null" />.</exception>
    public MySqlTableStatisticsProvider(IDbConnectionFactory connection, IIdentifierDefaults identifierDefaults)
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
        var rows = await Connection.QueryAsync(
            Queries.GetAllTableStatistics.Sql,
            new Queries.GetAllTableStatistics.Query { SchemaName = IdentifierDefaults.Schema! },
            cancellationToken
        );

        return rows
            .Select(row => MapStatistics(QualifyTableName(Identifier.CreateQualifiedIdentifier(row.SchemaName, row.TableName)), row))
            .ToList();
    }

    private static ITableStatistics MapStatistics(Identifier tableName, ITableStatisticsRow row)
    {
        return new TableStatistics(
            tableName,
            ToSignedCount(row.RowCount),
            isExact: false,
            ToSignedCount(row.DataSizeBytes),
            ToSignedCount(row.IndexSizeBytes)
        );
    }

    // The information schema reports these as unsigned, so a value beyond the signed range is
    // treated as no value at all rather than being wrapped into a negative one.
    private static Option<long> ToSignedCount(ulong? value)
    {
        return value <= long.MaxValue
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
