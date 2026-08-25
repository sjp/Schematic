using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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
    /// <exception cref="ArgumentNullException"><paramref name="connection"/> is <see langword="null" />.</exception>
    public NoRowsPresentOnTableRule(ISchematicConnection connection, RuleLevel? level = null)
        : base(RuleId, RuleTitle, level ?? DefaultLevel)
    {
        Connection = connection ?? throw new ArgumentNullException(nameof(connection));

        _existsQueryExecutor = new ExistsQueryExecutor(connection);
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
        var messages = await tables
            .Select(t => AnalyseTableAsync(t, cancellationToken))
            .ToArray()
            .WhenAll();

        return messages
            .OfType<IRuleMessage>()
            .ToArray();
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

        return AnalyseTableAsyncCore(table, cancellationToken);
    }

    private async Task<IRuleMessage?> AnalyseTableAsyncCore(IRelationalDatabaseTable table, CancellationToken cancellationToken)
    {
        var tableHasRows = await TableHasRowsAsync(table, cancellationToken);
        return tableHasRows
            ? null
            : BuildMessage(table.Name);
    }

    /// <summary>
    /// Determines whether a table has any rows present.
    /// </summary>
    /// <param name="table">A database table.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns><see langword="true" /> if the table has any rows; otherwise <see langword="false" />.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="table"/> is <see langword="null" />.</exception>
    protected Task<bool> TableHasRowsAsync(IRelationalDatabaseTable table, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(table);

        return TableHasRowsAsyncCore(table, cancellationToken);
    }

    private Task<bool> TableHasRowsAsyncCore(IRelationalDatabaseTable table, CancellationToken cancellationToken)
    {
        var quotedTableName = Dialect.QuoteName(Identifier.CreateQualifiedIdentifier(table.Name.Schema, table.Name.LocalName));
        var filterSql = "select 1 as dummy_col from " + quotedTableName;

        return _existsQueryExecutor.ExistsAsync(filterSql, cancellationToken);
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
