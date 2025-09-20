using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SJP.Schematic.Core;

namespace SJP.Schematic.Lint.Rules;

/// <summary>
/// A linting rule which reports when the primary key on a table is not an integer.
/// </summary>
/// <seealso cref="Rule"/>
/// <seealso cref="ITableRule"/>
public class PrimaryKeyNotIntegerRule : Rule, ITableRule
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PrimaryKeyNotIntegerRule"/> class.
    /// </summary>
    /// <param name="level">The reporting level.</param>
    public PrimaryKeyNotIntegerRule(RuleLevel level)
        : base(RuleId, RuleTitle, level)
    {
    }

    /// <summary>
    /// Analyses database tables. Reports messages when the primary key on a table is not an integer.
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
    /// Analyses a database table. Reports messages when the primary key on the table is not an integer.
    /// </summary>
    /// <param name="table">A database table.</param>
    /// <returns>A set of linting messages used for reporting. An empty set indicates no issues discovered.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="table"/> is <see langword="null" />.</exception>
    protected IReadOnlyCollection<IRuleMessage> AnalyseTable(IRelationalDatabaseTable table)
    {
        ArgumentNullException.ThrowIfNull(table);

        return table.PrimaryKey
            .Where(pk => pk.Columns.Count != 1 || !ColumnIsInteger(pk.Columns.First()))
            .Match(_ => [BuildMessage(table.Name)], Array.Empty<IRuleMessage>);
    }

    /// <summary>
    /// Determines whether a column stores integer data.
    /// </summary>
    /// <param name="column">The column.</param>
    /// <returns><see langword="true" /> if the column stores integer data; otherwise <see langword="false" />.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="column"/> is <see langword="null" />.</exception>
    protected static bool ColumnIsInteger(IDatabaseColumn column)
    {
        ArgumentNullException.ThrowIfNull(column);

        var integerTypes = new[] { DataType.BigInteger, DataType.Integer, DataType.SmallInteger };
        return integerTypes.Contains(column.Type.DataType);
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

        var messageText = $"The table {tableName} has a primary key which is not a single-column whose type is an integer.";
        return new RuleMessage(RuleId, RuleTitle, Level, messageText);
    }

    /// <summary>
    /// The rule identifier.
    /// </summary>
    /// <value>A rule identifier.</value>
    protected static string RuleId { get; } = "SCHEMATIC0018";

    /// <summary>
    /// Gets the rule title.
    /// </summary>
    /// <value>The rule title.</value>
    protected static string RuleTitle { get; } = "Table contains a non-integer primary key.";
}