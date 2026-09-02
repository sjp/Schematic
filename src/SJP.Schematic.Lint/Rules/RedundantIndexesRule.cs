using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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
    /// The reporting level this rule uses unless a caller overrides it: warning, because
    /// a redundant index costs write throughput and storage for nothing.
    /// </summary>
    public const RuleLevel DefaultLevel = RuleLevel.Warning;

    /// <summary>
    /// Initializes a new instance of the <see cref="RedundantIndexesRule"/> class.
    /// </summary>
    /// <param name="level">The reporting level, or <see langword="null" /> to use <see cref="DefaultLevel"/>.</param>
    public RedundantIndexesRule(RuleLevel? level = null)
        : base(RuleId, RuleTitle, level ?? DefaultLevel)
    {
    }

    /// <summary>
    /// Analyses database tables.
    /// Reports messages when tables contain redundant indexes, where the index column set is a prefix of another index.
    /// Additionally, this requires both column sort ordering to be equivalent and the included columns (if present) to be a subset also.
    /// A unique index is only redundant against a unique index covering the same key columns, a filtered index only against an identically filtered index,
    /// and an index only against another index of the same type.
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
    /// Analyses a database table.
    /// Reports messages when the table contains redundant indexes, where the index column set is a prefix of another index.
    /// Additionally, this requires both column sort ordering to be equivalent and the included columns (if present) to be a subset also.
    /// A unique index is only redundant against a unique index covering the same key columns, a filtered index only against an identically filtered index,
    /// and an index only against another index of the same type.
    /// </summary>
    /// <param name="table">A database table.</param>
    /// <returns>A set of linting messages used for reporting. An empty set indicates no issues discovered.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="table"/> is <see langword="null" />.</exception>
    protected IReadOnlyCollection<IRuleMessage> AnalyseTable(IRelationalDatabaseTable table)
    {
        ArgumentNullException.ThrowIfNull(table);

        var result = new List<IRuleMessage>();

        var indexes = table.Indexes.ToList();
        for (var i = 0; i < indexes.Count; i++)
        {
            var index = indexes[i];
            for (var j = 0; j < indexes.Count; j++)
            {
                var otherIndex = indexes[j];
                if (index.Name == otherIndex.Name || !IsIndexRedundant(index, otherIndex))
                    continue;

                // equivalent indexes are redundant against each other, so only report the pair once
                if (j < i && IsIndexRedundant(otherIndex, index))
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

        // indexes are only interchangeable when built the same way, e.g. a b-tree index does not
        // answer the queries a full-text or spatial index does, however its columns line up
        if (index.IndexType != otherIndex.IndexType)
            return false;

        // can't be redundant if we have more columns
        if (index.Columns.Count > otherIndex.Columns.Count)
            return false;

        // a unique index enforces a constraint that a non-unique index does not, and a wider
        // unique index enforces a weaker one, so uniqueness is only preserved by another
        // unique index covering exactly the same key columns
        if (index.IsUnique && (!otherIndex.IsUnique || index.Columns.Count != otherIndex.Columns.Count))
            return false;

        // a filtered index only covers the rows matching its filter, so it neither implies nor is
        // implied by an index with a different filter, or with no filter at all
        var filtersEquivalent = index.FilterDefinition.Match(
            filter => otherIndex.FilterDefinition.Match(otherFilter => string.Equals(filter, otherFilter, StringComparison.Ordinal), static () => false),
            () => otherIndex.FilterDefinition.IsNone);
        if (!filtersEquivalent)
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
        return new RuleMessage(RuleId, RuleTitle, Level, messageText, tableName);
    }

    /// <summary>
    /// The rule identifier.
    /// </summary>
    /// <value>A rule identifier.</value>
    protected static string RuleId => "SCHEMATIC0019";

    /// <summary>
    /// Gets the rule title.
    /// </summary>
    /// <value>The rule title.</value>
    protected static string RuleTitle => "Redundant indexes on a table.";
}