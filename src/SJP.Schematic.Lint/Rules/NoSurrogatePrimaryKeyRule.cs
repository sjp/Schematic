using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SJP.Schematic.Core;

namespace SJP.Schematic.Lint.Rules;

/// <summary>
/// A linting rule which reports when no surrogate primary key is present on a table. This occurs when a multi-column primary key exists on a table.
/// </summary>
/// <seealso cref="Rule"/>
/// <seealso cref="ITableRule"/>
public class NoSurrogatePrimaryKeyRule : Rule, ITableRule
{
    /// <summary>
    /// Initializes a new instance of the <see cref="NoSurrogatePrimaryKeyRule"/> class.
    /// </summary>
    /// <param name="level">The reporting level.</param>
    public NoSurrogatePrimaryKeyRule(RuleLevel level)
        : base(RuleId, RuleTitle, level)
    {
    }

    /// <summary>
    /// Analyses database tables. Reports messages when no surrogate primary keys are present on tables. This occurs when a multi-column primary key exists on a table.
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
    /// Analyses a database table. Reports messages when no surrogate primary keys are present on a table. This occurs when a multi-column primary key exists on a table.
    /// </summary>
    /// <param name="table">A database table.</param>
    /// <returns>A set of linting messages used for reporting. An empty set indicates no issues discovered.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="table"/> is <see langword="null" />.</exception>
    protected IReadOnlyCollection<IRuleMessage> AnalyseTable(IRelationalDatabaseTable table)
    {
        ArgumentNullException.ThrowIfNull(table);

        return table.PrimaryKey
            .Match(
                Some: pk =>
                {
                    if (pk.Columns.Count == 1)
                        return [];

                    var fkColumns = table.ParentKeys
                        .Select(fk => fk.ChildKey)
                        .SelectMany(fk => fk.Columns)
                        .Select(fkc => fkc.Name.LocalName)
                        .Distinct(StringComparer.Ordinal)
                        .ToList();

                    var areAllColumnsFks = pk.Columns.All(c => fkColumns.Contains(c.Name.LocalName, StringComparer.Ordinal));
                    return areAllColumnsFks
                        ? []
                        : [BuildMessage(table.Name)];
                },
                None: Array.Empty<IRuleMessage>
            );
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

        var messageText = $"The table {tableName} has a multi-column primary key. Consider introducing a surrogate primary key.";
        return new RuleMessage(RuleId, RuleTitle, Level, messageText);
    }

    /// <summary>
    /// The rule identifier.
    /// </summary>
    /// <value>A rule identifier.</value>
    protected static string RuleId { get; } = "SCHEMATIC0013";

    /// <summary>
    /// Gets the rule title.
    /// </summary>
    /// <value>The rule title.</value>
    protected static string RuleTitle { get; } = "No surrogate primary key present on table.";
}