using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Core.Utilities;

namespace SJP.Schematic.Lint.Rules;

/// <summary>
/// A linting rule which reports when tables contain redundant indexes, where the index column set is a prefix of another index.
/// </summary>
/// <seealso cref="Rule"/>
/// <seealso cref="ITableRule"/>
public class RedundantIndexesRule : Rule, ITableRule
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RedundantIndexesRule"/> class.
    /// </summary>
    /// <param name="level">The reporting level.</param>
    public RedundantIndexesRule(RuleLevel level)
        : base(RuleId, RuleTitle, level)
    {
    }

    /// <summary>
    /// Analyses database tables. Reports messages when tables contain redundant indexes, where the index column set is a prefix of another index.
    /// </summary>
    /// <param name="tables">A set of database tables.</param>
    /// <param name="cancellationToken">A cancellation token used to interrupt analysis.</param>
    /// <returns>A set of linting messages used for reporting. An empty set indicates no issues discovered.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="tables"/> is <c>null</c>.</exception>
    public IAsyncEnumerable<IRuleMessage> AnalyseTables(IEnumerable<IRelationalDatabaseTable> tables, CancellationToken cancellationToken = default)
    {
        if (tables == null)
            throw new ArgumentNullException(nameof(tables));

        return tables.SelectMany(AnalyseTable).ToAsyncEnumerable();
    }

    /// <summary>
    /// Analyses a database table. Reports messages when the table contains redundant indexes, where the index column set is a prefix of another index.
    /// </summary>
    /// <param name="table">A database table.</param>
    /// <returns>A set of linting messages used for reporting. An empty set indicates no issues discovered.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="table"/> is <c>null</c>.</exception>
    protected IEnumerable<IRuleMessage> AnalyseTable(IRelationalDatabaseTable table)
    {
        if (table == null)
            throw new ArgumentNullException(nameof(table));

        var result = new List<IRuleMessage>();

        var indexes = table.Indexes;
        foreach (var index in indexes)
        {
            var indexColumnList = index.Columns
                .SelectMany(c => c.DependentColumns)
                .Select(c => c.Name);

            var otherIndexes = indexes.Where(i => i.Name != index.Name);
            foreach (var otherIndex in otherIndexes)
            {
                var otherIndexColumnList = otherIndex.Columns
                    .SelectMany(c => c.DependentColumns)
                    .Select(c => c.Name);

                var isPrefix = IsPrefixOf(indexColumnList, otherIndexColumnList);
                if (isPrefix)
                {
                    var redundantIndexColumns = index.Columns
                        .SelectMany(c => c.DependentColumns)
                        .Select(c => c.Name.LocalName);
                    var otherIndexColumns = otherIndex.Columns
                        .SelectMany(c => c.DependentColumns)
                        .Select(c => c.Name.LocalName);

                    var message = BuildMessage(table.Name, index.Name.LocalName, redundantIndexColumns, otherIndex.Name.LocalName, otherIndexColumns);
                    result.Add(message);
                }
            }
        }

        return result;
    }

    /// <summary>
    /// Determines whether one sequence is a prefix of another.
    /// </summary>
    /// <typeparam name="T">A set of database objects.</typeparam>
    /// <param name="prefixSet">The set to test whether it is a prefix.</param>
    /// <param name="superSet">The alternate set.</param>
    /// <returns><c>true</c> if <paramref name="prefixSet"/> is a prefix of <paramref name="superSet"/>; otherwise, <c>false</c>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="prefixSet"/> or <paramref name="superSet"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentException"><paramref name="prefixSet"/> or <paramref name="superSet"/> is empty.</exception>
    protected static bool IsPrefixOf<T>(IEnumerable<T> prefixSet, IEnumerable<T> superSet)
    {
        if (prefixSet == null)
            throw new ArgumentNullException(nameof(prefixSet));
        if (superSet == null)
            throw new ArgumentNullException(nameof(superSet));

        var prefixSetList = prefixSet.ToList();
        if (prefixSetList.Empty())
            throw new ArgumentException("The given prefix set contained no values.", nameof(prefixSet));

        var superSetList = superSet.ToList();
        if (superSetList.Empty())
            throw new ArgumentException("The given super set contained no values.", nameof(superSet));

        if (prefixSetList.Count > superSetList.Count)
            return false;

        if (superSetList.Count > prefixSetList.Count)
            superSetList = superSetList.Take(prefixSetList.Count).ToList();

        return prefixSetList.SequenceEqual(superSetList);
    }

    /// <summary>
    /// Builds the message used for reporting.
    /// </summary>
    /// <param name="tableName">The name of the table.</param>
    /// <param name="indexName">The name of the index that has redundant columns.</param>
    /// <param name="redundantIndexColumnNames">The names of the columns in <paramref name="indexName"/> that are redundant.</param>
    /// <param name="otherIndexName">The other index that is a superset of <paramref name="indexName"/>.</param>
    /// <param name="otherIndexColumnNames">The column names in the index <paramref name="otherIndexName"/>.</param>
    /// <returns>A formatted linting message.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="tableName"/> is <c>null</c>; or <paramref name="indexName"/> is <c>null</c>, empty or whitespace; or <paramref name="redundantIndexColumnNames"/> is <c>null</c> or empty; or <paramref name="otherIndexName"/> is <c>null</c>, empty or whitespace; or <paramref name="otherIndexColumnNames"/> is <c>null</c> or empty.</exception>
    protected virtual IRuleMessage BuildMessage(Identifier tableName, string indexName, IEnumerable<string> redundantIndexColumnNames, string otherIndexName, IEnumerable<string> otherIndexColumnNames)
    {
        if (tableName == null)
            throw new ArgumentNullException(nameof(tableName));
        if (indexName.IsNullOrWhiteSpace())
            throw new ArgumentNullException(nameof(indexName));
        if (redundantIndexColumnNames == null || redundantIndexColumnNames.Empty())
            throw new ArgumentNullException(nameof(redundantIndexColumnNames));
        if (otherIndexName.IsNullOrWhiteSpace())
            throw new ArgumentNullException(nameof(otherIndexName));
        if (otherIndexColumnNames == null || otherIndexColumnNames.Empty())
            throw new ArgumentNullException(nameof(otherIndexColumnNames));

        var builder = StringBuilderCache.Acquire();
        builder.Append("The table ")
            .Append(tableName)
            .Append(" has an index '")
            .Append(indexName)
            .Append("' which may be redundant, as its column set (")
            .AppendJoin(", ", redundantIndexColumnNames)
            .Append(") is the prefix of another index '")
            .Append(otherIndexName)
            .Append("' (")
            .AppendJoin(", ", otherIndexColumnNames)
            .Append(").");

        var messageText = builder.GetStringAndRelease();
        return new RuleMessage(RuleId, RuleTitle, Level, messageText);
    }

    /// <summary>
    /// The rule identifier.
    /// </summary>
    /// <value>A rule identifier.</value>
    protected static string RuleId { get; } = "SCHEMATIC0019";

    /// <summary>
    /// Gets the rule title.
    /// </summary>
    /// <value>The rule title.</value>
    protected static string RuleTitle { get; } = "Redundant indexes on a table.";
}
