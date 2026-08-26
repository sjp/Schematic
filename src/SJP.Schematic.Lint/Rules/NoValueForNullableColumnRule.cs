using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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
    /// <exception cref="ArgumentNullException"><paramref name="connection"/> is <see langword="null" />.</exception>
    public NoValueForNullableColumnRule(ISchematicConnection connection, RuleLevel? level = null)
        : base(RuleId, RuleTitle, level ?? DefaultLevel)
    {
        Connection = connection ?? throw new ArgumentNullException(nameof(connection));

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
        var messages = await tables
            .Select(t => AnalyseTableAsync(t, cancellationToken))
            .ToArray()
            .WhenAll();

        return messages
            .SelectMany(_ => _)
            .ToArray();
    }

    /// <summary>
    /// Analyses a database table. Reports messages when no non-null values exist for a nullable column in a table.
    /// </summary>
    /// <param name="table">A database table.</param>
    /// <param name="cancellationToken">A cancellation token used to interrupt analysis.</param>
    /// <returns>A set of linting messages used for reporting. An empty set indicates no issues discovered.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="table"/> is <see langword="null" />.</exception>
    protected Task<IReadOnlyCollection<IRuleMessage>> AnalyseTableAsync(IRelationalDatabaseTable table, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(table);

        return AnalyseTableAsyncCore(table, cancellationToken);
    }

    private async Task<IReadOnlyCollection<IRuleMessage>> AnalyseTableAsyncCore(IRelationalDatabaseTable table, CancellationToken cancellationToken)
    {
        var nullableColumns = table.Columns.Where(static c => c.IsNullable).ToArray();
        if (nullableColumns.Empty())
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

    /// <summary>
    /// Determines which columns in a batch hold no non-null values.
    /// </summary>
    /// <remarks>
    /// The whole batch is answered by a single aggregate query, because <c>count(column)</c> ignores nulls,
    /// so a column whose count is zero never holds a value. <c>count(*)</c> is part of the same query,
    /// removing the need for a separate probe to determine whether the table has any rows at all.
    /// </remarks>
    /// <param name="table">A database table.</param>
    /// <param name="columns">A batch of nullable columns belonging to <paramref name="table"/>.</param>
    /// <param name="cancellationToken">A cancellation token used to interrupt analysis.</param>
    /// <returns>The names of any columns in <paramref name="columns"/> whose values are always null. Always empty for a table without rows.</returns>
    private async Task<IReadOnlyCollection<string>> FindAlwaysNullColumnNamesAsync(IRelationalDatabaseTable table, IReadOnlyList<IDatabaseColumn> columns, CancellationToken cancellationToken)
    {
        var query = BuildValueCountQuery(table, columns);

        // the number of counts returned varies with the size of the batch, so the row is read as an
        // untyped set of values keyed by alias rather than being mapped onto a fixed result type
        var counts = (IDictionary<string, object>)await _probeLimiter.RunAsync(ct => DbConnection.QuerySingleAsync<object>(query, ct), cancellationToken);

        var tableRowCount = GetCount(counts, RowCountAlias);
        if (tableRowCount == 0)
            return [];

        return columns
            .Where((_, i) => GetCount(counts, GetColumnCountAlias(i)) == 0)
            .Select(static c => c.Name.LocalName)
            .ToArray();
    }

    /// <summary>
    /// Builds a query returning the number of rows in a table, alongside the number of non-null values held by each column in a batch.
    /// </summary>
    /// <param name="table">A database table.</param>
    /// <param name="columns">A batch of nullable columns belonging to <paramref name="table"/>.</param>
    /// <returns>A query returning a single row of counts, one column per alias.</returns>
    private string BuildValueCountQuery(IRelationalDatabaseTable table, IReadOnlyList<IDatabaseColumn> columns)
    {
        var quotedTableName = Dialect.QuoteName(Identifier.CreateQualifiedIdentifier(table.Name.Schema, table.Name.LocalName));

        var selectList = columns
            .Select((c, i) => $"count({Dialect.QuoteIdentifier(c.Name.LocalName)}) as {Dialect.QuoteIdentifier(GetColumnCountAlias(i))}")
            .Prepend($"count(*) as {Dialect.QuoteIdentifier(RowCountAlias)}")
            .Join(", ");

        return $"select {selectList} from {quotedTableName}";
    }

    /// <summary>
    /// Retrieves a count returned by <see cref="BuildValueCountQuery(IRelationalDatabaseTable, IReadOnlyList{IDatabaseColumn})"/>.
    /// </summary>
    /// <param name="counts">A row of counts, keyed by alias.</param>
    /// <param name="alias">The alias of the count to retrieve.</param>
    /// <returns>The count associated with <paramref name="alias"/>.</returns>
    /// <exception cref="InvalidOperationException">No count was returned for <paramref name="alias"/>.</exception>
    private static long GetCount(IDictionary<string, object> counts, string alias)
    {
        if (!counts.TryGetValue(alias, out var count) || count == null)
            throw new InvalidOperationException($"Expected a count aliased as '{alias}' to be returned, but none was present.");

        return Convert.ToInt64(count, CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Gets the alias used for the count of non-null values held by a column.
    /// </summary>
    /// <param name="columnIndex">The index of a column within its batch.</param>
    /// <returns>An alias, generated rather than derived from the column name so that it is always a valid and unambiguous identifier.</returns>
    private static string GetColumnCountAlias(int columnIndex) => "c" + columnIndex.ToString(CultureInfo.InvariantCulture);

    /// <summary>
    /// The alias used for the number of rows present in the table being analysed.
    /// </summary>
    private const string RowCountAlias = "rc";

    /// <summary>
    /// The maximum number of columns counted by a single query. Chosen conservatively to stay well within
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