using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Utilities;

namespace SJP.Schematic.Lint.Rules;

/// <summary>
/// A linting rule which reports when columns that share the same name across different tables are declared with differing types.
/// </summary>
/// <seealso cref="Rule"/>
/// <seealso cref="ITableRule"/>
public class ColumnTypeMismatchAcrossTablesRule : Rule, ITableRule
{
    /// <summary>
    /// The reporting level this rule uses unless a caller overrides it: information, because
    /// a naming coincidence across unrelated tables is common and harmless.
    /// </summary>
    public const RuleLevel DefaultLevel = RuleLevel.Information;

    /// <summary>
    /// Initializes a new instance of the <see cref="ColumnTypeMismatchAcrossTablesRule"/> class.
    /// </summary>
    /// <param name="level">The reporting level, or <see langword="null" /> to use <see cref="DefaultLevel"/>.</param>
    public ColumnTypeMismatchAcrossTablesRule(RuleLevel? level = null)
        : base(RuleId, RuleTitle, level ?? DefaultLevel)
    {
    }

    /// <summary>
    /// Analyses database tables. Reports messages when identically named columns across tables have differing types.
    /// </summary>
    /// <param name="tables">A set of database tables.</param>
    /// <param name="cancellationToken">A cancellation token used to interrupt analysis.</param>
    /// <returns>A set of linting messages used for reporting. An empty set indicates no issues discovered.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="tables"/> is <see langword="null" />.</exception>
    public Task<IReadOnlyCollection<IRuleMessage>> AnalyseTables(IReadOnlyCollection<IRelationalDatabaseTable> tables, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(tables);

        var columnsByName = tables
            .SelectMany(static t => t.Columns.Select(c => (Table: t.Name, Column: c)))
            .GroupBy(static tc => tc.Column.Name.LocalName, StringComparer.Ordinal);

        var messages = new List<IRuleMessage>();
        foreach (var columnGroup in columnsByName)
        {
            // Grouped by what each type describes rather than by its definition text, so that columns
            // whose types differ only in how they were spelled do not read as a mismatch. Collation
            // plays no part, as the definition naming a group does not carry one.
            // Ordered so that the largest group of agreeing tables reads first and any outlier reads
            // last. Ties and table names are ordered so that repeated runs produce identical messages.
            var typeGroups = columnGroup
                .GroupBy(static tc => tc.Column.Type, DbTypeComparer.StructuralIgnoringCollation)
                .Select(static g => (
                    // a group's members may spell their type differently, so the spelling that names
                    // the group is chosen the same way however the tables were ordered
                    TypeDefinition: g.Select(static tc => tc.Column.Type.Definition).Order(StringComparer.Ordinal).First(),
                    TableNames: (IReadOnlyCollection<Identifier>)g.Select(static tc => tc.Table).Distinct().Order().ToList()
                ))
                .OrderByDescending(static g => g.TableNames.Count)
                .ThenBy(static g => g.TypeDefinition, StringComparer.Ordinal)
                .ToList();

            if (typeGroups.Count <= 1)
                continue;

            messages.Add(BuildMessage(columnGroup.Key, typeGroups));
        }

        return Task.FromResult<IReadOnlyCollection<IRuleMessage>>(messages);
    }

    /// <summary>
    /// Builds the message used for reporting.
    /// </summary>
    /// <param name="columnName">The name of the column shared across tables.</param>
    /// <param name="typeGroups">Each distinct type declared for the column, paired with the tables that declare it.</param>
    /// <returns>A formatted linting message.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="columnName"/> or <paramref name="typeGroups"/> is <see langword="null" />.</exception>
    protected virtual IRuleMessage BuildMessage(string columnName, IReadOnlyCollection<(string TypeDefinition, IReadOnlyCollection<Identifier> TableNames)> typeGroups)
    {
        ArgumentNullException.ThrowIfNull(columnName);
        ArgumentNullException.ThrowIfNull(typeGroups);

        var builder = StringBuilderCache.Acquire();
        builder.Append("The column '")
            .Append(columnName)
            .Append("' is declared with differing types across tables: ");

        var firstGroup = true;
        foreach (var (typeDefinition, tableNames) in typeGroups)
        {
            if (!firstGroup)
                builder.Append("; ");
            firstGroup = false;

            builder.Append(typeDefinition)
                .Append(" in ")
                .AppendJoin(", ", tableNames.Select(static t => t.ToString()));
        }

        builder.Append(". Consider using a consistent type to avoid implicit conversions and join errors.");

        var messageText = builder.GetStringAndRelease();

        // Deliberately reported without an owning object: the finding is about a column name
        // shared by several unrelated tables, so attributing it to any one of them would send
        // the reader to a table that is no more at fault than the others.
        return new RuleMessage(RuleId, RuleTitle, Level, messageText);
    }

    /// <summary>
    /// The rule identifier.
    /// </summary>
    /// <value>A rule identifier.</value>
    protected static string RuleId => "SCHEMATIC0028";

    /// <summary>
    /// Gets the rule title.
    /// </summary>
    /// <value>The rule title.</value>
    protected static string RuleTitle => "Identically named columns have differing types across tables.";
}
