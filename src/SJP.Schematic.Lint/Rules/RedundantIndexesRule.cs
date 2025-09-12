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
    /// Analyses database tables.
    /// Reports messages when tables contain redundant indexes, where the index column set is a prefix of another index.
    /// Additionally, this requires both column sort ordering to be equivalent and the included columns (if present) to be a subset also.
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
    /// Analyses a database table.
    /// Reports messages when the table contains redundant indexes, where the index column set is a prefix of another index.
    /// Additionally, this requires both column sort ordering to be equivalent and the included columns (if present) to be a subset also.
    /// </summary>
    /// <param name="table">A database table.</param>
    /// <returns>A set of linting messages used for reporting. An empty set indicates no issues discovered.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="table"/> is <see langword="null" />.</exception>
    protected IEnumerable<IRuleMessage> AnalyseTable(IRelationalDatabaseTable table)
    {
        ArgumentNullException.ThrowIfNull(table);

        var result = new List<IRuleMessage>();

        var indexes = table.Indexes;
        foreach (var index in indexes)
        {
            var otherIndexes = indexes.Where(i => i.Name != index.Name);
            foreach (var otherIndex in otherIndexes)
            {
                if (!IsIndexRedundant(index, otherIndex))
                    continue;

                var message = BuildMessage(
                    table.Name,
                    index,
                    otherIndex);
                result.Add(message);
            }
        }

        return result;
    }

    /// <summary>
    /// Determines whether an index is redundant.
    /// </summary>
    /// <param name="index">The index that is tested for being redundant.</param>
    /// <param name="otherIndex">An index that is being compared against for <paramref name="index"/>. <paramref name="index"/> is redundant if <paramref name="otherIndex"/> has at least the equivalent behaviour (if not more).</param>
    /// <returns><see langword="true" /> if <paramref name="index"/> is redundant; <see langword="false" /> otherwise.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="index"/> is <see langword="null" />; or <paramref name="otherIndex"/> is <see langword="null" />.</exception>
    private static bool IsIndexRedundant(IDatabaseIndex index, IDatabaseIndex otherIndex)
    {
        ArgumentNullException.ThrowIfNull(index);
        ArgumentNullException.ThrowIfNull(otherIndex);

        // can't be redundant if we have more columns
        if (index.Columns.Count > otherIndex.Columns.Count)
            return false;

        var indexColumns = index.Columns;
        var otherIndexColumns = otherIndex.Columns.Count > indexColumns.Count
            ? otherIndex.Columns.Take(indexColumns.Count).ToList()
            : otherIndex.Columns;

        // when we have more than one column, ordering becomes important
        if (indexColumns.Count > 1)
        {
            var sortOrdersEqual = indexColumns.Select(c => c.Order)
                .SequenceEqual(otherIndexColumns.Select(c => c.Order));
            if (!sortOrdersEqual)
                return false;
        }

        // if we have different included column sets then we know that the index
        // is not equivalent, even if it may have the same sorting behaviour
        if (index.IncludedColumns.Count > 0)
        {
            var indexIncludedColumns = index.IncludedColumns.Select(c => c.Name.LocalName).ToHashSet(StringComparer.Ordinal);
            var otherIndexIncludedColumns = otherIndex.IncludedColumns.Select(c => c.Name.LocalName).ToHashSet(StringComparer.Ordinal);
            var includedColumnSubset = indexIncludedColumns.IsSubsetOf(otherIndexIncludedColumns);
            if (!includedColumnSubset)
                return false;
        }

        var indexColumnNames = index.Columns
            .SelectMany(c => c.DependentColumns)
            .Select(c => c.Name);
        var otherIndexColumnNames = otherIndex.Columns
            .SelectMany(c => c.DependentColumns)
            .Select(c => c.Name);
        return IsPrefixOf(indexColumnNames, otherIndexColumnNames);
    }

    /// <summary>
    /// Determines whether one sequence is a prefix of another.
    /// </summary>
    /// <typeparam name="T">A set of database objects.</typeparam>
    /// <param name="prefixSet">The set to test whether it is a prefix.</param>
    /// <param name="superSet">The alternate set.</param>
    /// <returns><see langword="true" /> if <paramref name="prefixSet"/> is a prefix of <paramref name="superSet"/>; otherwise, <see langword="false" />.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="prefixSet"/> or <paramref name="superSet"/> is <see langword="null" />.</exception>
    /// <exception cref="ArgumentException"><paramref name="prefixSet"/> or <paramref name="superSet"/> is empty.</exception>
    private static bool IsPrefixOf<T>(IEnumerable<T> prefixSet, IEnumerable<T> superSet)
    {
        ArgumentNullException.ThrowIfNull(prefixSet);
        ArgumentNullException.ThrowIfNull(superSet);

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
    /// <param name="redundantIndex">The index that is redundant.</param>
    /// <param name="otherIndex">The other index that is either equivalent or a superset of <paramref name="redundantIndex"/>.</param>
    /// <returns>A formatted linting message.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="tableName"/> is <see langword="null" />; or <paramref name="redundantIndex"/> is <see langword="null" />; or <paramref name="otherIndex"/> is <see langword="null" />.</exception>
    protected virtual IRuleMessage BuildMessage(Identifier tableName, IDatabaseIndex redundantIndex, IDatabaseIndex otherIndex)
    {
        ArgumentNullException.ThrowIfNull(tableName);
        ArgumentNullException.ThrowIfNull(redundantIndex);
        ArgumentNullException.ThrowIfNull(otherIndex);

        var redundantIndexColumnNames = redundantIndex.Columns
            .SelectMany(c => c.DependentColumns)
            .Select(c => c.Name)
            .ToList();
        var redundantIncludedColumnNames = redundantIndex.IncludedColumns
            .Select(c => c.Name)
            .ToList();
        var otherIndexColumnNames = otherIndex.Columns
            .SelectMany(c => c.DependentColumns)
            .Select(c => c.Name)
            .ToList();
        var otherIncludedColumnNames = otherIndex.IncludedColumns
            .Select(c => c.Name)
            .ToList();

        var builder = StringBuilderCache.Acquire();
        builder.Append("The table ")
            .Append(tableName)
            .Append(" has an index '")
            .Append(redundantIndex.Name.LocalName)
            .Append("' which is redundant, as its column set (")
            .AppendJoin(", ", redundantIndexColumnNames)
            .Append(')');

        if (redundantIndex.IncludedColumns.Count > 0)
        {
            builder.Append(" INCLUDE (")
                .AppendJoin(", ", redundantIncludedColumnNames)
                .Append(')');
        }

        builder
            .Append(" is the prefix or subset of another index '")
            .Append(otherIndex.Name.LocalName)
            .Append("' (")
            .AppendJoin(", ", otherIndexColumnNames)
            .Append(')');

        if (otherIndex.IncludedColumns.Count > 0)
        {
            builder.Append(" INCLUDE (")
                .AppendJoin(", ", otherIncludedColumnNames)
                .Append(')');
        }

        builder.Append('.');

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