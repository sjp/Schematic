using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Core.Utilities;

namespace SJP.Schematic.Lint.Rules;

/// <summary>
/// A linting rule which reports when tables contain unique indexes with nullable columns.
/// </summary>
/// <seealso cref="Rule" />
/// <seealso cref="ITableRule" />
public class UniqueIndexWithNullableColumnsRule : Rule, ITableRule
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UniqueIndexWithNullableColumnsRule"/> class.
    /// </summary>
    /// <param name="level">The reporting level.</param>
    public UniqueIndexWithNullableColumnsRule(RuleLevel level)
        : base(RuleId, RuleTitle, level)
    {
    }

    /// <summary>
    /// Analyses database tables. Reports messages when tables contain unique indexes with nullable columns.
    /// </summary>
    /// <param name="tables">A set of database tables.</param>
    /// <param name="cancellationToken">A cancellation token used to interrupt analysis.</param>
    /// <returns>A set of linting messages used for reporting. An empty set indicates no issues discovered.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="tables"/> is <see langword="null" />.</exception>
    public IAsyncEnumerable<IRuleMessage> AnalyseTables(IEnumerable<IRelationalDatabaseTable> tables, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(tables);

        return tables.SelectMany(AnalyseTable).ToAsyncEnumerable();
    }

    /// <summary>
    /// Analyses a database table. Reports messages when tables contain unique indexes with nullable columns.
    /// </summary>
    /// <param name="table">A database table.</param>
    /// <returns>A set of linting messages used for reporting. An empty set indicates no issues discovered.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="table"/> is <see langword="null" />.</exception>
    protected IEnumerable<IRuleMessage> AnalyseTable(IRelationalDatabaseTable table)
    {
        ArgumentNullException.ThrowIfNull(table);

        var uniqueIndexes = table.Indexes.Where(i => i.IsUnique).ToList();
        if (uniqueIndexes.Empty())
            return [];

        var result = new List<IRuleMessage>();

        foreach (var index in uniqueIndexes)
        {
            var nullableColumns = index.Columns
                .SelectMany(c => c.DependentColumns)
                .Where(c => c.IsNullable)
                .ToList();

            if (nullableColumns.Empty())
                continue;

            var columnNames = nullableColumns.ConvertAll(c => c.Name.LocalName);
            var message = BuildMessage(table.Name, index.Name?.LocalName, columnNames);
            result.Add(message);
        }

        return result;
    }

    /// <summary>
    /// Builds the message used for reporting.
    /// </summary>
    /// <param name="tableName">The name of the table.</param>
    /// <param name="indexName">The name of the index.</param>
    /// <param name="columnNames">The column names present on the index.</param>
    /// <returns>A formatted linting message.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="tableName"/> is <see langword="null" />. Also thrown when <paramref name="columnNames"/> is <see langword="null" /> or empty.</exception>
    protected virtual IRuleMessage BuildMessage(Identifier tableName, string? indexName, IEnumerable<string> columnNames)
    {
        ArgumentNullException.ThrowIfNull(tableName);
        if (columnNames.NullOrEmpty())
            throw new ArgumentNullException(nameof(columnNames));

        var builder = StringBuilderCache.Acquire();
        builder.Append("The table ")
            .Append(tableName)
            .Append(" has a unique index ");

        if (!indexName.IsNullOrWhiteSpace())
        {
            builder.Append('\'')
                .Append(indexName)
                .Append("' ");
        }

        var pluralText = columnNames.Skip(1).Any()
            ? "which contains nullable columns: "
            : "which contains a nullable column: ";
        builder.Append(pluralText)
            .AppendJoin(", ", columnNames);

        var messageText = builder.GetStringAndRelease();
        return new RuleMessage(RuleId, RuleTitle, Level, messageText);
    }

    /// <summary>
    /// The rule identifier.
    /// </summary>
    /// <value>A rule identifier.</value>
    protected static string RuleId { get; } = "SCHEMATIC0022";

    /// <summary>
    /// Gets the rule title.
    /// </summary>
    /// <value>The rule title.</value>
    protected static string RuleTitle { get; } = "Unique index contains nullable columns.";
}