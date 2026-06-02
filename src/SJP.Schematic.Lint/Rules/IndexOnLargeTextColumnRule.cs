using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SJP.Schematic.Core;

namespace SJP.Schematic.Lint.Rules;

/// <summary>
/// A linting rule which reports when an index is defined over a large text or binary column. Such indexes are often expensive and ineffective.
/// </summary>
/// <seealso cref="Rule"/>
/// <seealso cref="ITableRule"/>
public class IndexOnLargeTextColumnRule : Rule, ITableRule
{
    private static readonly IReadOnlyCollection<DataType> LargeDataTypes =
    [
        DataType.Text,
        DataType.UnicodeText,
        DataType.LargeBinary,
    ];

    /// <summary>
    /// Initializes a new instance of the <see cref="IndexOnLargeTextColumnRule"/> class.
    /// </summary>
    /// <param name="level">The reporting level.</param>
    public IndexOnLargeTextColumnRule(RuleLevel level)
        : base(RuleId, RuleTitle, level)
    {
    }

    /// <summary>
    /// Analyses database tables. Reports messages when an index is defined over a large text or binary column.
    /// </summary>
    /// <param name="tables">A set of database tables.</param>
    /// <param name="cancellationToken">A cancellation token used to interrupt analysis.</param>
    /// <returns>A set of linting messages used for reporting. An empty set indicates no issues discovered.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="tables"/> is <see langword="null" />.</exception>
    public Task<IReadOnlyCollection<IRuleMessage>> AnalyseTables(IReadOnlyCollection<IRelationalDatabaseTable> tables, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(tables);

        var messages = tables.SelectMany(AnalyseTable).ToList();
        return Task.FromResult<IReadOnlyCollection<IRuleMessage>>(messages);
    }

    /// <summary>
    /// Analyses a database table. Reports messages when an index is defined over a large text or binary column.
    /// </summary>
    /// <param name="table">A database table.</param>
    /// <returns>A set of linting messages used for reporting. An empty set indicates no issues discovered.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="table"/> is <see langword="null" />.</exception>
    protected IReadOnlyCollection<IRuleMessage> AnalyseTable(IRelationalDatabaseTable table)
    {
        ArgumentNullException.ThrowIfNull(table);

        var result = new List<IRuleMessage>();
        foreach (var index in table.Indexes)
        {
            var largeColumns = index.Columns
                .SelectMany(static ic => ic.DependentColumns)
                .Where(static c => LargeDataTypes.Contains(c.Type.DataType))
                .Select(static c => c.Name.LocalName)
                .Distinct(StringComparer.Ordinal)
                .ToList();

            foreach (var columnName in largeColumns)
                result.Add(BuildMessage(table.Name, index.Name, columnName));
        }

        return result;
    }

    /// <summary>
    /// Builds the message used for reporting.
    /// </summary>
    /// <param name="tableName">The name of the table.</param>
    /// <param name="indexName">The name of the index.</param>
    /// <param name="columnName">The name of the large text or binary column.</param>
    /// <returns>A formatted linting message.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="tableName"/> or <paramref name="indexName"/> is <see langword="null" />.</exception>
    protected virtual IRuleMessage BuildMessage(Identifier tableName, Identifier indexName, string columnName)
    {
        ArgumentNullException.ThrowIfNull(tableName);
        ArgumentNullException.ThrowIfNull(indexName);
        ArgumentNullException.ThrowIfNull(columnName);

        var messageText = $"The table {tableName} has an index '{indexName.LocalName}' defined over the large text or binary column '{columnName}'. Indexing such columns is often expensive and ineffective; consider indexing a derived or truncated value instead.";
        return new RuleMessage(RuleId, RuleTitle, Level, messageText);
    }

    /// <summary>
    /// The rule identifier.
    /// </summary>
    /// <value>A rule identifier.</value>
    protected static string RuleId { get; } = "SCHEMATIC0034";

    /// <summary>
    /// Gets the rule title.
    /// </summary>
    /// <value>The rule title.</value>
    protected static string RuleTitle { get; } = "Index defined over a large text or binary column.";
}
