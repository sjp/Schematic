using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Utilities;

namespace SJP.Schematic.Lint.Rules;

/// <summary>
/// A linting rule which reports when foreign key relationships are implied, but not enforced by a foreign key constraint.
/// </summary>
/// <seealso cref="Rule"/>
/// <seealso cref="ITableRule"/>
public class ForeignKeyMissingRule : Rule, ITableRule
{
    /// <summary>
    /// The reporting level this rule uses unless a caller overrides it: information, because
    /// a name-based guess at a missing relationship, so false positives are expected.
    /// </summary>
    public const RuleLevel DefaultLevel = RuleLevel.Information;

    /// <summary>
    /// Initializes a new instance of the <see cref="ForeignKeyMissingRule"/> class.
    /// </summary>
    /// <param name="level">The reporting level, or <see langword="null" /> to use <see cref="DefaultLevel"/>.</param>
    public ForeignKeyMissingRule(RuleLevel? level = null)
        : base(RuleId, RuleTitle, level ?? DefaultLevel)
    {
    }

    /// <summary>
    /// Analyses database tables. Reports messages when a foreign key relationship is implied, but missing a foreign key constraint to enforce it.
    /// </summary>
    /// <param name="tables">A set of database tables.</param>
    /// <param name="cancellationToken">A cancellation token used to interrupt analysis.</param>
    /// <returns>A set of linting messages used for reporting. An empty set indicates no issues discovered.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="tables"/> is <see langword="null" />.</exception>
    public Task<IReadOnlyCollection<IRuleMessage>> AnalyseTables(IReadOnlyCollection<IRelationalDatabaseTable> tables, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(tables);

        var tableNames = BuildTableNameLookup(tables.Select(t => t.Name));
        var messages = tables.SelectMany(t => AnalyseTable(t, tableNames)).ToList();
        return Task.FromResult<IReadOnlyCollection<IRuleMessage>>(messages);
    }

    /// <summary>
    /// Analyses a database table. Reports messages when a foreign key relationship is implied, but missing a foreign key constraint to enforce it.
    /// </summary>
    /// <param name="table">A database table.</param>
    /// <param name="tableNames">Other table names in the database.</param>
    /// <returns>A set of linting messages used for reporting. An empty set indicates no issues discovered.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="table"/> or <paramref name="tableNames"/> is <see langword="null" />.</exception>
    protected IReadOnlyCollection<IRuleMessage> AnalyseTable(IRelationalDatabaseTable table, IEnumerable<Identifier> tableNames)
    {
        ArgumentNullException.ThrowIfNull(table);
        ArgumentNullException.ThrowIfNull(tableNames);

        return AnalyseTable(table, BuildTableNameLookup(tableNames));
    }

    /// <summary>
    /// Analyses a database table. Reports messages when a foreign key relationship is implied, but missing a foreign key constraint to enforce it.
    /// </summary>
    /// <param name="table">A database table.</param>
    /// <param name="tableNames">Other table names in the database, keyed by their local names.</param>
    /// <returns>A set of linting messages used for reporting. An empty set indicates no issues discovered.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="table"/> or <paramref name="tableNames"/> is <see langword="null" />.</exception>
    protected IReadOnlyCollection<IRuleMessage> AnalyseTable(IRelationalDatabaseTable table, IReadOnlyDictionary<string, Identifier> tableNames)
    {
        ArgumentNullException.ThrowIfNull(table);
        ArgumentNullException.ThrowIfNull(tableNames);

        var result = new List<IRuleMessage>();

        var foreignKeyColumnNames = table.ParentKeys
            .Select(fk => fk.ChildKey)
            .SelectMany(fk => fk.Columns)
            .Select(c => c.Name.LocalName)
            .ToHashSet(IdentifierComparer);

        var currentTableName = table.Name.LocalName;
        var columnNames = table.Columns.Select(c => c.Name.LocalName);

        foreach (var columnName in columnNames)
        {
            var impliedTable = GetImpliedTableName(columnName);
            if (IdentifierComparer.Equals(impliedTable, currentTableName))
                continue;

            if (!tableNames.TryGetValue(impliedTable, out var targetTableName))
                continue;

            // now check whether the column name is already part of an FK
            if (foreignKeyColumnNames.Contains(columnName))
                continue;

            var message = BuildMessage(columnName, table.Name, targetTableName);
            result.Add(message);
        }

        return result;
    }

    /// <summary>
    /// Builds a lookup of table names, keyed by their local names. Where local names are duplicated, the first name provided is retained.
    /// </summary>
    /// <param name="tableNames">A set of table names.</param>
    /// <returns>A lookup of table names, keyed case-insensitively by their local names.</returns>
    private static IReadOnlyDictionary<string, Identifier> BuildTableNameLookup(IEnumerable<Identifier> tableNames)
    {
        var result = new Dictionary<string, Identifier>(IdentifierComparer);

        foreach (var tableName in tableNames)
            result.TryAdd(tableName.LocalName, tableName);

        return result;
    }

    /// <summary>
    /// Gets the name of the implied table.
    /// </summary>
    /// <param name="columnName">The name of the column that can imply a table name.</param>
    /// <returns>The implied table name if found, otherwise the value of <paramref name="columnName"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="columnName"/> is <see langword="null" />.</exception>
    /// <exception cref="ArgumentException"><paramref name="columnName"/> is empty or whitespace.</exception>
    protected static string GetImpliedTableName(string columnName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(columnName);

        const string snakeCaseSuffix = "_id";
        if (columnName.EndsWith(snakeCaseSuffix, StringComparison.OrdinalIgnoreCase))
            return columnName[..^snakeCaseSuffix.Length];

        const string camelCaseSuffix = "Id";
        if (columnName.EndsWith(camelCaseSuffix, StringComparison.Ordinal))
            return columnName[..^camelCaseSuffix.Length];

        return columnName;
    }

    /// <summary>
    /// Builds the message used for reporting.
    /// </summary>
    /// <param name="columnName">The name of the column that implies a foreign key relationship.</param>
    /// <param name="tableName">The name of the table.</param>
    /// <param name="targetTableName">The implied target table.</param>
    /// <returns>A formatted linting message.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="tableName"/>, <paramref name="targetTableName"/> or <paramref name="columnName"/> is <see langword="null" />.</exception>
    /// <exception cref="ArgumentException"><paramref name="columnName"/> is empty or whitespace.</exception>
    protected virtual IRuleMessage BuildMessage(string columnName, Identifier tableName, Identifier targetTableName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(columnName);
        ArgumentNullException.ThrowIfNull(tableName);
        ArgumentNullException.ThrowIfNull(targetTableName);

        var builder = StringBuilderCache.Acquire();

        builder.Append("The table ")
            .Append(tableName)
            .Append(" has a column ")
            .Append(columnName)
            .Append(" implying a relationship to ")
            .Append(targetTableName)
            .Append(" which is missing a foreign key constraint.");

        var messageText = builder.GetStringAndRelease();
        return new RuleMessage(RuleId, RuleTitle, Level, messageText, tableName);
    }

    private static StringComparer IdentifierComparer { get; } = StringComparer.OrdinalIgnoreCase;

    /// <summary>
    /// The rule identifier.
    /// </summary>
    /// <value>A rule identifier.</value>
    protected static string RuleId => "SCHEMATIC0008";

    /// <summary>
    /// Gets the rule title.
    /// </summary>
    /// <value>The rule title.</value>
    protected static string RuleTitle => "Column name implies a relationship missing a foreign key constraint.";
}