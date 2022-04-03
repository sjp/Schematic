using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using LanguageExt;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Core.Utilities;

namespace SJP.Schematic.Lint.Rules;

/// <summary>
/// A linting rule which reports when foreign keys are missing indexes.
/// </summary>
/// <seealso cref="Rule"/>
/// <seealso cref="ITableRule"/>
public class ForeignKeyIndexRule : Rule, ITableRule
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ForeignKeyIndexRule"/> class.
    /// </summary>
    /// <param name="level">The reporting level.</param>
    public ForeignKeyIndexRule(RuleLevel level)
        : base(RuleId, RuleTitle, level)
    {
    }

    /// <summary>
    /// Analyses database tables. Reports messages when foreign keys are missing indexes.
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
    /// Analyses a database table. Reports messages when foreign keys are missing indexes.
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
        var foreignKeys = table.ParentKeys.Select(fk => fk.ChildKey).ToList();

        foreach (var foreignKey in foreignKeys)
        {
            var columns = foreignKey.Columns;

            var isIndexedKey = indexes.Any(i =>
                ColumnsHaveIndex(columns, i.Columns) || ColumnsHaveIndexWithIncludedColumns(columns, i.Columns, i.IncludedColumns));
            if (!isIndexedKey)
            {
                var columnNames = columns.Select(c => c.Name.LocalName).ToList();
                var message = BuildMessage(foreignKey.Name, table.Name, columnNames);
                result.Add(message);
            }
        }

        return result;
    }

    /// <summary>
    /// Determines whether a column set is covered by an index.
    /// </summary>
    /// <param name="columns">A set of columns.</param>
    /// <param name="indexColumns">The index columns.</param>
    /// <returns><c>true</c> if <paramref name="columns"/> is covered by <paramref name="indexColumns"/>; otherwise <c>false</c>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="columns"/> or <paramref name="indexColumns"/> is <c>null</c>.</exception>
    protected static bool ColumnsHaveIndex(IEnumerable<IDatabaseColumn> columns, IEnumerable<IDatabaseIndexColumn> indexColumns)
    {
        if (columns == null)
            throw new ArgumentNullException(nameof(columns));
        if (indexColumns == null)
            throw new ArgumentNullException(nameof(indexColumns));

        var columnList = columns.ToList();
        var indexColumnList = indexColumns.ToList();
        var dependentColumns = indexColumnList.SelectMany(ic => ic.DependentColumns).ToList();

        // can only check for regular indexes, not functional ones (functions may be composed of multiple columns)
        if (indexColumnList.Count != dependentColumns.Count)
            return false;

        var columnNames = columnList.ConvertAll(c => c.Name);
        var indexColumnNames = dependentColumns.ConvertAll(ic => ic.Name);

        return IsPrefixOf(columnNames, indexColumnNames);
    }

    /// <summary>
    /// Determines whether a column set is covered by an index.
    /// </summary>
    /// <param name="columns">A set of columns.</param>
    /// <param name="indexColumns">The index columns.</param>
    /// <param name="indexIncludedColumns">The index columns.</param>
    /// <returns><c>true</c> if <paramref name="columns"/> is covered by <paramref name="indexColumns"/>; otherwise <c>false</c>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="columns"/> or <paramref name="indexColumns"/> or <paramref name="indexIncludedColumns"/> is <c>null</c>.</exception>
    protected static bool ColumnsHaveIndexWithIncludedColumns(IEnumerable<IDatabaseColumn> columns, IEnumerable<IDatabaseIndexColumn> indexColumns, IEnumerable<IDatabaseColumn> indexIncludedColumns)
    {
        if (columns == null)
            throw new ArgumentNullException(nameof(columns));
        if (indexColumns == null)
            throw new ArgumentNullException(nameof(indexColumns));
        if (indexIncludedColumns == null)
            throw new ArgumentNullException(nameof(indexIncludedColumns));

        if (indexIncludedColumns.Empty())
            return false;

        var columnList = columns.ToList();
        var indexColumnList = indexColumns.ToList();
        var dependentColumns = indexColumnList.SelectMany(ic => ic.DependentColumns).ToList();
        var includedColumnList = indexIncludedColumns.ToList();

        // can only check for regular indexes, not functional ones (functions may be composed of multiple columns)
        if (indexColumnList.Count != dependentColumns.Count)
            return false;

        var columnNames = new System.Collections.Generic.HashSet<Identifier>(columnList.Select(c => c.Name));
        var indexColumnNames = new System.Collections.Generic.HashSet<Identifier>(dependentColumns.Select(ic => ic.Name));

        // index won't be completely used or there are extra columns in it
        if (indexColumnNames.Count > columnNames.Count || indexColumnNames.Any(ic => !columnNames.Contains(ic)))
            return false;

        indexColumnNames.UnionWith(includedColumnList.Select(ic => ic.Name));
        return columnNames.IsSubsetOf(indexColumnNames);
    }

    /// <summary>
    /// Determines whether one sequence is a prefix of another.
    /// </summary>
    /// <typeparam name="T">A set of database objects.</typeparam>
    /// <param name="prefixSet">The set to test whether it is a prefix.</param>
    /// <param name="otherSet">The alternate set.</param>
    /// <returns><c>true</c> if <paramref name="prefixSet"/> is a prefix of <paramref name="otherSet"/>; otherwise, <c>false</c>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="prefixSet"/> or <paramref name="otherSet"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentException"><paramref name="prefixSet"/> or <paramref name="otherSet"/> is empty.</exception>
    protected static bool IsPrefixOf<T>(IEnumerable<T> prefixSet, IEnumerable<T> otherSet)
    {
        if (prefixSet == null)
            throw new ArgumentNullException(nameof(prefixSet));
        if (otherSet == null)
            throw new ArgumentNullException(nameof(otherSet));

        var prefixSetList = prefixSet.ToList();
        if (prefixSetList.Empty())
            throw new ArgumentException("The given prefix set contained no values.", nameof(prefixSet));

        var superSetList = otherSet.ToList();
        if (superSetList.Empty())
            throw new ArgumentException("The given super set contained no values.", nameof(otherSet));

        if (prefixSetList.Count > superSetList.Count)
            return false;

        if (superSetList.Count > prefixSetList.Count)
            superSetList = superSetList.Take(prefixSetList.Count).ToList();

        return prefixSetList.OrderBy(c => c)
            .SequenceEqual(superSetList.OrderBy(c => c));
    }

    /// <summary>
    /// Builds the message used for reporting.
    /// </summary>
    /// <param name="foreignKeyName">The name of the foreign key constraint, if available.</param>
    /// <param name="tableName">The name of the table.</param>
    /// <param name="columnNames">The names of the columns in the foreign key.</param>
    /// <returns>A formatted linting message.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="tableName"/> or <paramref name="columnNames"/> is <c>null</c>. Also when <paramref name="columnNames"/> is empty.</exception>
    protected virtual IRuleMessage BuildMessage(Option<Identifier> foreignKeyName, Identifier tableName, IEnumerable<string> columnNames)
    {
        if (tableName == null)
            throw new ArgumentNullException(nameof(tableName));
        if (columnNames.NullOrEmpty())
            throw new ArgumentNullException(nameof(columnNames));

        var builder = StringBuilderCache.Acquire();
        builder.Append("The table ")
            .Append(tableName)
            .Append(" has a foreign key ");

        foreignKeyName.IfSome(name =>
        {
            builder.Append('\'')
                .Append(name.LocalName)
                .Append("' ");
        });

        builder.Append("which is missing an index on the column");

        // plural check
        if (columnNames.Skip(1).Any())
            builder.Append('s');

        builder.Append(' ')
            .AppendJoin(", ", columnNames);

        var messageText = builder.GetStringAndRelease();
        return new RuleMessage(RuleId, RuleTitle, Level, messageText);
    }

    /// <summary>
    /// The rule identifier.
    /// </summary>
    /// <value>A rule identifier.</value>
    protected static string RuleId { get; } = "SCHEMATIC0006";

    /// <summary>
    /// Gets the rule title.
    /// </summary>
    /// <value>The rule title.</value>
    protected static string RuleTitle { get; } = "Indexes missing on foreign key.";
}