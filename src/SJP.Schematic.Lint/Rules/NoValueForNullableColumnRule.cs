using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LanguageExt;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Lint.Rules;

/// <summary>
/// A linting rule which reports when no non-null values exist for a nullable column in a table.
/// </summary>
/// <seealso cref="Rule"/>
/// <seealso cref="ITableRule"/>
public class NoValueForNullableColumnRule : Rule, ITableRule
{
    /// <summary>
    /// The reporting level this rule uses unless a caller overrides it: information, because
    /// an always-null column may simply be unused so far.
    /// </summary>
    public const RuleLevel DefaultLevel = RuleLevel.Information;

    /// <summary>
    /// Initializes a new instance of the <see cref="NoValueForNullableColumnRule"/> class.
    /// </summary>
    /// <param name="connection">A database connection.</param>
    /// <param name="level">The reporting level, or <see langword="null" /> to use <see cref="DefaultLevel"/>.</param>
    /// <param name="tableStatistics">The statistics the database records for its tables, used to skip tables known to be empty. <see langword="null" /> to always query.</param>
    /// <exception cref="ArgumentNullException"><paramref name="connection"/> is <see langword="null" />.</exception>
    public NoValueForNullableColumnRule(ISchematicConnection connection, RuleLevel? level = null, ITableStatisticsProvider? tableStatistics = null)
        : base(RuleId, RuleTitle, level ?? DefaultLevel)
    {
        Connection = connection ?? throw new ArgumentNullException(nameof(connection));
        TableStatistics = tableStatistics ?? new EmptyTableStatisticsProvider();

        _probeLimiter = ProbeConcurrencyLimiter.GetForConnection(connection);
    }

    /// <summary>
    /// A database connection, qualified with a dialect.
    /// </summary>
    /// <value>The connection.</value>
    protected ISchematicConnection Connection { get; }

    /// <summary>
    /// A database connection factory.
    /// </summary>
    /// <value>The database connection factory.</value>
    protected IDbConnectionFactory DbConnection => Connection.ConnectionFactory;

    /// <summary>
    /// A database dialect.
    /// </summary>
    /// <value>The dialect associated with <see cref="DbConnection"/>.</value>
    protected IDatabaseDialect Dialect => Connection.Dialect;

    /// <summary>
    /// The statistics the database records for its tables.
    /// </summary>
    /// <value>A table statistics provider, which records nothing when the rule was given none.</value>
    protected ITableStatisticsProvider TableStatistics { get; }

    /// <summary>
    /// Analyses database tables. Reports messages when no non-null values exist for a nullable column in a table.
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
            .SelectMany(_ => _)
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

    private async Task<IReadOnlyCollection<IRuleMessage>> AnalyseTableAsyncCore(IRelationalDatabaseTable table, Option<ITableStatistics> statistics, CancellationToken cancellationToken)
    {
        var nullableColumns = table.Columns.Where(static c => c.IsNullable).ToArray();
        if (nullableColumns.Empty())
            return [];

        // nothing is reported for a table without rows, so one known to be empty needs no probing
        if (statistics.Exists(IsKnownToBeEmpty))
            return [];

        var alwaysNullColumnNamesByBatch = await nullableColumns
            .Chunk(ProbeBatchSize)
            .Select(batch => FindAlwaysNullColumnNamesAsync(table, batch, cancellationToken))
            .ToArray()
            .WhenAll();

        return alwaysNullColumnNamesByBatch
            .SelectMany(columnNames => columnNames)
            .Select(columnName => BuildMessage(table.Name, columnName))
            .ToArray();
    }

    // Only an exact count of zero proves a table is empty. An estimate of zero is also what an
    // engine reports for a table whose statistics have never been gathered, and an estimate above
    // zero says nothing about individual columns, so neither avoids a probe.
    private static bool IsKnownToBeEmpty(ITableStatistics statistics)
    {
        return statistics.IsExact && statistics.RowCount.Exists(static count => count == 0);
    }

    /// <summary>
    /// Determines which columns in a batch hold no non-null values.
    /// </summary>
    /// <remarks>
    /// The whole batch is answered by a single query holding one <c>exists</c> test per column, each of
    /// which stops reading at the first non-null value it finds, so a populated column usually costs a
    /// handful of rows rather than a scan of the whole table. Only a column that never holds a value
    /// needs its table read in full, and each such column is read separately, so a table holding many of
    /// them costs more than a single aggregate query over the batch would. That is the rarer case, as
    /// most nullable columns hold values. A further <c>exists</c> test in the same query determines
    /// whether the table has any rows at all, removing the need for a separate probe.
    /// </remarks>
    /// <param name="table">A database table.</param>
    /// <param name="columns">A batch of nullable columns belonging to <paramref name="table"/>.</param>
    /// <param name="cancellationToken">A cancellation token used to interrupt analysis.</param>
    /// <returns>The names of any columns in <paramref name="columns"/> whose values are always null. Always empty for a table without rows.</returns>
    private async Task<IReadOnlyCollection<string>> FindAlwaysNullColumnNamesAsync(IRelationalDatabaseTable table, IReadOnlyList<IDatabaseColumn> columns, CancellationToken cancellationToken)
    {
        var query = BuildValueExistsQuery(table, columns);

        // the number of flags returned varies with the size of the batch, so the row is read as an
        // untyped set of values keyed by alias rather than being mapped onto a fixed result type
        var flags = (IDictionary<string, object>)await _probeLimiter.RunAsync(ct => DbConnection.QuerySingleAsync<object>(query, ct), cancellationToken);

        if (!GetFlag(flags, RowsExistAlias))
            return [];

        return columns
            .Where((_, i) => !GetFlag(flags, GetColumnFlagAlias(i)))
            .Select(static c => c.Name.LocalName)
            .ToArray();
    }

    /// <summary>
    /// Builds a query returning whether a table has any rows, alongside whether each column in a batch holds any non-null value.
    /// </summary>
    /// <param name="table">A database table.</param>
    /// <param name="columns">A batch of nullable columns belonging to <paramref name="table"/>.</param>
    /// <returns>A query returning a single row of flags, one column per alias, each <c>1</c> when true and <c>0</c> otherwise.</returns>
    private string BuildValueExistsQuery(IRelationalDatabaseTable table, IReadOnlyList<IDatabaseColumn> columns)
    {
        var quotedTableName = Dialect.QuoteName(Identifier.CreateQualifiedIdentifier(table.Name.Schema, table.Name.LocalName));

        // a case expression rather than a bare exists, because engines such as SQL Server cannot
        // select the result of a predicate directly
        var selectList = columns
            .Select((c, i) => BuildExistsFlag($"select 1 from {quotedTableName} where {Dialect.QuoteIdentifier(c.Name.LocalName)} is not null", GetColumnFlagAlias(i)))
            .Prepend(BuildExistsFlag("select 1 from " + quotedTableName, RowsExistAlias))
            .Join(", ");

        var query = "select " + selectList;

        // engines such as Oracle reject a select without a from clause, and say so through their dialect
        return Dialect.Capabilities.FromLessSelectSuffix
            .Match(suffix => query + " from " + suffix, query);
    }

    private string BuildExistsFlag(string filterSql, string alias) => $"case when exists ({filterSql}) then 1 else 0 end as {Dialect.QuoteIdentifier(alias)}";

    /// <summary>
    /// Retrieves a flag returned by <see cref="BuildValueExistsQuery(IRelationalDatabaseTable, IReadOnlyList{IDatabaseColumn})"/>.
    /// </summary>
    /// <param name="flags">A row of flags, keyed by alias.</param>
    /// <param name="alias">The alias of the flag to retrieve.</param>
    /// <returns><see langword="true" /> if the flag associated with <paramref name="alias"/> is set; otherwise <see langword="false" />.</returns>
    /// <exception cref="InvalidOperationException">No flag was returned for <paramref name="alias"/>.</exception>
    private static bool GetFlag(IDictionary<string, object> flags, string alias)
    {
        if (!flags.TryGetValue(alias, out var flag) || flag == null)
            throw new InvalidOperationException($"Expected a flag aliased as '{alias}' to be returned, but none was present.");

        // the numeric type of the flag varies by engine and driver, e.g. a decimal on Oracle
        return Convert.ToInt64(flag, CultureInfo.InvariantCulture) != 0;
    }

    /// <summary>
    /// Gets the alias used for the flag recording whether a column holds any non-null value.
    /// </summary>
    /// <param name="columnIndex">The index of a column within its batch.</param>
    /// <returns>An alias, generated rather than derived from the column name so that it is always a valid and unambiguous identifier.</returns>
    private static string GetColumnFlagAlias(int columnIndex) => "c" + columnIndex.ToString(CultureInfo.InvariantCulture);

    /// <summary>
    /// The alias used for the flag recording whether the table being analysed has any rows.
    /// </summary>
    private const string RowsExistAlias = "rc";

    /// <summary>
    /// The maximum number of columns tested by a single query. Chosen conservatively to stay well within
    /// every supported dialect's limits on statement length and select list size.
    /// </summary>
    private const int ProbeBatchSize = 64;

    /// <summary>
    /// Builds the message used for reporting.
    /// </summary>
    /// <param name="tableName">The name of the table.</param>
    /// <param name="columnName">A name of the nullable column.</param>
    /// <returns>A formatted linting message.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="tableName"/> or <paramref name="columnName"/> is <see langword="null" />.</exception>
    /// <exception cref="ArgumentException"><paramref name="columnName"/> is empty or whitespace.</exception>
    protected virtual IRuleMessage BuildMessage(Identifier tableName, string columnName)
    {
        ArgumentNullException.ThrowIfNull(tableName);
        ArgumentException.ThrowIfNullOrWhiteSpace(columnName);

        var messageText = $"The table '{tableName}' has a nullable column '{columnName}' whose values are always null. Consider removing the column.";
        return new RuleMessage(RuleId, RuleTitle, Level, messageText, tableName);
    }

    /// <summary>
    /// The rule identifier.
    /// </summary>
    /// <value>A rule identifier.</value>
    protected static string RuleId => "SCHEMATIC0014";

    /// <summary>
    /// Gets the rule title.
    /// </summary>
    /// <value>The rule title.</value>
    protected static string RuleTitle => "No not-null values exist for a nullable column.";

    private readonly ProbeConcurrencyLimiter _probeLimiter;
}