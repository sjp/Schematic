using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LanguageExt;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Sqlite.Pragma;
using SJP.Schematic.Sqlite.Queries;

namespace SJP.Schematic.Sqlite;

/// <summary>
/// A table statistics provider for SQLite databases, backed by <c>sqlite_stat1</c>.
/// </summary>
/// <remarks>
/// SQLite keeps no statistics of its own. <c>sqlite_stat1</c> exists only once <c>ANALYZE</c> has
/// been run, and holds what it recorded then, so a database that has never been analysed has no
/// statistics at all. SQLite records nothing about the space a table occupies, so only row counts
/// are reported.
/// </remarks>
/// <seealso cref="ITableStatisticsProvider" />
public class SqliteTableStatisticsProvider : ITableStatisticsProvider
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SqliteTableStatisticsProvider"/> class.
    /// </summary>
    /// <param name="connection">A schematic connection.</param>
    /// <param name="pragma">A pragma for the given database connection.</param>
    /// <param name="identifierDefaults">Identifier defaults for the associated database.</param>
    /// <exception cref="ArgumentNullException"><paramref name="connection"/>, <paramref name="pragma"/> or <paramref name="identifierDefaults"/> is <see langword="null" />.</exception>
    public SqliteTableStatisticsProvider(ISchematicConnection connection, ISqliteConnectionPragma pragma, IIdentifierDefaults identifierDefaults)
    {
        Connection = connection ?? throw new ArgumentNullException(nameof(connection));
        ConnectionPragma = pragma ?? throw new ArgumentNullException(nameof(pragma));
        IdentifierDefaults = identifierDefaults ?? throw new ArgumentNullException(nameof(identifierDefaults));
    }

    /// <summary>
    /// A database connection that is specific to a given SQLite database.
    /// </summary>
    /// <value>A database connection.</value>
    protected ISchematicConnection Connection { get; }

    /// <summary>
    /// Accesses pragma that applies to the entire SQLite connection.
    /// </summary>
    /// <value>A connection pragma.</value>
    protected ISqliteConnectionPragma ConnectionPragma { get; }

    /// <summary>
    /// Identifier defaults for the associated database.
    /// </summary>
    /// <value>Identifier defaults.</value>
    protected IIdentifierDefaults IdentifierDefaults { get; }

    /// <summary>
    /// A database connection factory.
    /// </summary>
    /// <value>A database connection factory.</value>
    protected IDbConnectionFactory DbConnection => Connection.ConnectionFactory;

    /// <summary>
    /// The dialect for the associated database.
    /// </summary>
    /// <value>A database dialect.</value>
    protected IDatabaseDialect Dialect => Connection.Dialect;

    /// <summary>
    /// Gets the statistics recorded for a database table.
    /// </summary>
    /// <param name="tableName">A database table name.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>Statistics for the table in the 'some' state if any were recorded; otherwise 'none'.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="tableName"/> is <see langword="null" />.</exception>
    public OptionAsync<ITableStatistics> GetTableStatistics(Identifier tableName, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(tableName);

        return GetTableStatisticsAsyncCore(QualifyTableName(tableName), cancellationToken).ToAsync();
    }

    private async Task<Option<ITableStatistics>> GetTableStatisticsAsyncCore(Identifier tableName, CancellationToken cancellationToken)
    {
        var schemaName = tableName.Schema!;
        if (!await HasStatisticsTableAsync(schemaName, cancellationToken))
            return Option<ITableStatistics>.None;

        var rows = await DbConnection.QueryAsync(
            Queries.GetTableStatistics.Sql(Dialect, schemaName),
            new Queries.GetTableStatistics.Query { TableName = tableName.LocalName },
            cancellationToken
        );

        if (rows.Empty())
            return Option<ITableStatistics>.None;

        return Option<ITableStatistics>.Some(MapStatistics(tableName, rows.Select(static row => row.Stat)));
    }

    /// <summary>
    /// Gets the statistics recorded for all database tables.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A collection of table statistics.</returns>
    public async Task<IReadOnlyCollection<ITableStatistics>> GetAllTableStatistics(CancellationToken cancellationToken = default)
    {
        var dbNames = await ConnectionPragma.DatabaseListAsync(cancellationToken);
        var orderedDbNames = dbNames
            .OrderBy(static d => d.seq)
            .Select(static d => d.name)
            .ToList();

        var result = new List<ITableStatistics>();

        foreach (var dbName in orderedDbNames)
        {
            if (!await HasStatisticsTableAsync(dbName, cancellationToken))
                continue;

            var rows = await DbConnection.QueryAsync<Queries.GetAllTableStatistics.Result>(
                Queries.GetAllTableStatistics.Sql(Dialect, dbName),
                cancellationToken
            );

            var statistics = rows
                .GroupBy(static row => row.TableName, StringComparer.OrdinalIgnoreCase)
                .Select(group => (TableName: Identifier.CreateQualifiedIdentifier(dbName, group.Key), Stats: group.Select(static row => row.Stat).ToList()))
                .Where(static t => !IsReservedTableName(t.TableName))
                .OrderBy(static t => t.TableName.LocalName, StringComparer.Ordinal)
                .Select(t => MapStatistics(t.TableName, t.Stats));

            result.AddRange(statistics);
        }

        return result;
    }

    private async Task<bool> HasStatisticsTableAsync(string schemaName, CancellationToken cancellationToken)
    {
        var statTableName = await DbConnection.ExecuteScalarAsync<string>(
            GetStatisticsTableName.Sql(Dialect, schemaName),
            cancellationToken
        );

        return statTableName != null;
    }

    private static ITableStatistics MapStatistics(Identifier tableName, IEnumerable<string?> stats)
    {
        return new TableStatistics(
            tableName,
            ParseRowCount(stats),
            isExact: false,
            Option<long>.None,
            Option<long>.None
        );
    }

    // ANALYZE writes one row per index, plus one for the table itself when it has no index, each
    // holding a space-separated list of estimates that begins with a row count. A partial index
    // counts only the rows it covers, so the largest of the estimates is the one closest to the
    // number of rows in the table.
    private static Option<long> ParseRowCount(IEnumerable<string?> stats)
    {
        var rowCount = Option<long>.None;

        foreach (var stat in stats)
        {
            var estimate = stat?.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
            if (estimate == null || !long.TryParse(estimate, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed))
                continue;

            rowCount = rowCount.Match(
                current => Option<long>.Some(Math.Max(current, parsed)),
                () => Option<long>.Some(parsed)
            );
        }

        return rowCount;
    }

    /// <summary>
    /// Determines whether a table's name is a SQLite reserved table name.
    /// </summary>
    /// <param name="tableName">A table name.</param>
    /// <returns><see langword="true" /> if the table name is a reserved table name; otherwise, <see langword="false" />.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="tableName"/> is <see langword="null" />.</exception>
    protected static bool IsReservedTableName(Identifier tableName)
    {
        ArgumentNullException.ThrowIfNull(tableName);

        return tableName.LocalName.StartsWith("sqlite_", StringComparison.OrdinalIgnoreCase);
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
        return Identifier.CreateQualifiedIdentifier(schema, tableName.LocalName);
    }
}
