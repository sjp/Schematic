using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LanguageExt;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Lint.Rules;

/// <summary>
/// A linting rule which reports when a table contains no rows.
/// </summary>
/// <seealso cref="Rule"/>
/// <seealso cref="ITableRule"/>
public class NoRowsPresentOnTableRule : Rule, ITableRule
{
    /// <summary>
    /// The reporting level this rule uses unless a caller overrides it: information, because
    /// an empty table is normal in a fresh or reference schema.
    /// </summary>
    public const RuleLevel DefaultLevel = RuleLevel.Information;

    /// <summary>
    /// Initializes a new instance of the <see cref="NoRowsPresentOnTableRule"/> class.
    /// </summary>
    /// <param name="connection">A database connection.</param>
    /// <param name="level">The reporting level, or <see langword="null" /> to use <see cref="DefaultLevel"/>.</param>
    /// <param name="tableStatistics">The statistics the database records for its tables, used in place of a query where they answer the question. <see langword="null" /> to always query.</param>
    /// <exception cref="ArgumentNullException"><paramref name="connection"/> is <see langword="null" />.</exception>
    public NoRowsPresentOnTableRule(ISchematicConnection connection, RuleLevel? level = null, ITableStatisticsProvider? tableStatistics = null)
        : base(RuleId, RuleTitle, level ?? DefaultLevel)
    {
        Connection = connection ?? throw new ArgumentNullException(nameof(connection));
        TableStatistics = tableStatistics ?? new EmptyTableStatisticsProvider();

        _existsQueryExecutor = ExistsQueryExecutor.GetForConnection(connection);
    }

    /// <summary>
    /// A database connection, qualified with a dialect.
    /// </summary>
    /// <value>The connection.</value>
    protected ISchematicConnection Connection { get; }

    /// <summary>
    /// A database dialect.
    /// </summary>
    /// <value>The dialect associated with <see cref="Connection"/>.</value>
    protected IDatabaseDialect Dialect => Connection.Dialect;

    /// <summary>
    /// The statistics the database records for its tables.
    /// </summary>
    /// <value>A table statistics provider, which records nothing when the rule was given none.</value>
    protected ITableStatisticsProvider TableStatistics { get; }

    /// <summary>
    /// Analyses database tables. Reports messages when a table contains no rows.
    /// </summary>
    /// <param name="tables">A set of database tables.</param>
    /// <param name="cancellationToken">A cancellation token used to interrupt analysis.</param>
    /// <returns>A set of linting messages used for reporting. An empty set indicates no issues discovered.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="tables"/> is <see langword="null" />.</exception>
    public Task<IReadOnlyCollection<IRuleMessage>> AnalyseTables(IReadOnlyCollection<IRelationalDatabaseTable> tables, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(tables);

        return AnalyseTablesCore(tables, cancellationToken);
    }

    private async Task<IReadOnlyCollection<IRuleMessage>> AnalyseTablesCore(IEnumerable<IRelationalDatabaseTable> tables, CancellationToken cancellationToken = default)
    {
        // the statistics for every table are retrieved once, rather than once per table, so that
        // they cost less than the queries they save
        var statistics = await GetStatisticsLookupAsync(cancellationToken);

        var messages = await tables
            .Select(t => AnalyseTableAsyncCore(t, GetStatistics(statistics, t.Name), cancellationToken))
            .ToArray()
            .WhenAll();

        return messages
            .OfType<IRuleMessage>()
            .ToArray();
    }

    private async Task<IReadOnlyDictionary<Identifier, ITableStatistics>> GetStatisticsLookupAsync(CancellationToken cancellationToken)
    {
        var statistics = await TableStatistics.GetAllTableStatistics(cancellationToken);

        return statistics
            .GroupBy(static stat => stat.TableName, IdentifierComparer.OrdinalIgnoreCase)
            .ToDictionary(static group => group.Key, static group => group.First(), IdentifierComparer.OrdinalIgnoreCase);
    }

    private static Option<ITableStatistics> GetStatistics(IReadOnlyDictionary<Identifier, ITableStatistics> statistics, Identifier tableName)
    {
        return statistics.TryGetValue(tableName, out var tableStatistics)
            ? Option<ITableStatistics>.Some(tableStatistics)
            : Option<ITableStatistics>.None;
    }

    /// <summary>
    /// Analyses a database table. Reports a message when the table contains no rows.
    /// </summary>
    /// <param name="table">A database table.</param>
    /// <param name="cancellationToken">A cancellation token used to interrupt analysis.</param>
    /// <returns>A linting message if the table contains no rows; otherwise <see langword="null" />.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="table"/> is <see langword="null" />.</exception>
    protected Task<IRuleMessage?> AnalyseTableAsync(IRelationalDatabaseTable table, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(table);

        return AnalyseTableWithStatisticsAsync(table, cancellationToken);
    }

    // a table analysed on its own has its statistics retrieved on their own, unlike the whole-set
    // analysis, which retrieves them for every table at once
    private async Task<IRuleMessage?> AnalyseTableWithStatisticsAsync(IRelationalDatabaseTable table, CancellationToken cancellationToken)
    {
        var statistics = await TableStatistics.GetTableStatistics(table.Name, cancellationToken).ToOption();
        return await AnalyseTableAsyncCore(table, statistics, cancellationToken);
    }

    private async Task<IRuleMessage?> AnalyseTableAsyncCore(IRelationalDatabaseTable table, Option<ITableStatistics> statistics, CancellationToken cancellationToken)
    {
        if (DatabaseManagedTables.IsManagedByDatabase(table))
            return null;

        var tableHasRows = await TableHasRowsAsync(table, statistics, cancellationToken);
        return tableHasRows
            ? null
            : BuildMessage(table.Name);
    }

    /// <summary>
    /// Determines whether a table has any rows present.
    /// </summary>
    /// <param name="table">A database table.</param>
    /// <param name="statistics">The statistics the database records for the table, if any.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns><see langword="true" /> if the table has any rows; otherwise <see langword="false" />.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="table"/> is <see langword="null" />.</exception>
    protected Task<bool> TableHasRowsAsync(IRelationalDatabaseTable table, Option<ITableStatistics> statistics, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(table);

        return TableHasRowsAsyncCore(table, statistics, cancellationToken);
    }

    private Task<bool> TableHasRowsAsyncCore(IRelationalDatabaseTable table, Option<ITableStatistics> statistics, CancellationToken cancellationToken)
    {
        var knownToHaveRows = statistics.Bind(RowsPresentFromStatistics);
        if (knownToHaveRows.IsSome)
            return Task.FromResult(knownToHaveRows.MatchUnsafe(static hasRows => hasRows, static () => false));

        var quotedTableName = Dialect.QuoteName(Identifier.CreateQualifiedIdentifier(table.Name.Schema, table.Name.LocalName));
        var filterSql = "select 1 as dummy_col from " + quotedTableName;

        return _existsQueryExecutor.ExistsAsync(filterSql, cancellationToken);
    }

    // An estimate above zero is enough to know that rows are present. An estimate of zero is not,
    // because it is also what an engine reports for a table whose statistics have never been
    // gathered, so only an exact count of zero settles the question without a query.
    private static Option<bool> RowsPresentFromStatistics(ITableStatistics statistics)
    {
        return statistics.RowCount.Bind(count => count > 0 || statistics.IsExact
            ? Option<bool>.Some(count > 0)
            : Option<bool>.None);
    }

    /// <summary>
    /// Builds the message used for reporting.
    /// </summary>
    /// <param name="tableName">The name of the table.</param>
    /// <returns>A formatted linting message.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="tableName"/> is <see langword="null" />.</exception>
    protected virtual IRuleMessage BuildMessage(Identifier tableName)
    {
        ArgumentNullException.ThrowIfNull(tableName);

        var messageText = $"The table '{tableName}' contains no rows. Consider removing it if it is unused.";
        return new RuleMessage(RuleId, RuleTitle, Level, messageText, tableName);
    }

    /// <summary>
    /// The rule identifier.
    /// </summary>
    /// <value>A rule identifier.</value>
    protected static string RuleId => "SCHEMATIC0039";

    /// <summary>
    /// Gets the rule title.
    /// </summary>
    /// <value>The rule title.</value>
    protected static string RuleTitle => "No rows present in table.";

    private readonly ExistsQueryExecutor _existsQueryExecutor;
}
