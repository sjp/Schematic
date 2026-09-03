using System;
using System.Collections.Generic;
using System.Linq;
using EnumsNET;
using LanguageExt;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Sqlite.Parsing;

/// <summary>
/// The parsed definition of a foreign key in a SQLite <c>CREATE TABLE</c> definition.
/// </summary>
public class ForeignKey
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ForeignKey"/> class.
    /// </summary>
    /// <param name="constraintName">The constraint name.</param>
    /// <param name="columnName">The column name.</param>
    /// <param name="parentTable">The parent table that the foreign key refers to.</param>
    /// <param name="parentColumnNames">The column names in the parent table that the foreign key refers to. Should be a single column name, or empty when the constraint omitted the parent column list.</param>
    public ForeignKey(Option<string> constraintName, string columnName, Identifier parentTable, IReadOnlyCollection<string> parentColumnNames)
        : this(constraintName, [columnName], parentTable, parentColumnNames)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ForeignKey"/> class.
    /// </summary>
    /// <param name="constraintName">The constraint name.</param>
    /// <param name="columnNames">The column names comprising this foreign key.</param>
    /// <param name="parentTable">The parent table that the foreign key refers to.</param>
    /// <param name="parentColumnNames">The column names in the parent table that the foreign key refers to, or empty when the constraint omitted the parent column list.</param>
    /// <exception cref="ArgumentNullException"><paramref name="parentTable"/>, <paramref name="columnNames"/> or <paramref name="parentColumnNames"/> is <see langword="null" />, or <paramref name="columnNames"/> or <paramref name="parentColumnNames"/> contains a <see langword="null" /> value.</exception>
    /// <exception cref="ArgumentException"><paramref name="columnNames"/> is empty, <paramref name="columnNames"/> or <paramref name="parentColumnNames"/> contains an empty or whitespace value, or a non-empty <paramref name="parentColumnNames"/> has a different number of elements to <paramref name="columnNames"/>.</exception>
    public ForeignKey(Option<string> constraintName, IReadOnlyCollection<string> columnNames, Identifier parentTable, IReadOnlyCollection<string> parentColumnNames)
        : this(constraintName, columnNames, parentTable, parentColumnNames, ConstraintDeferrability.NotDeferrable, ForeignKeyMatchType.Simple)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ForeignKey"/> class.
    /// </summary>
    /// <param name="constraintName">The constraint name.</param>
    /// <param name="columnNames">The column names comprising this foreign key.</param>
    /// <param name="parentTable">The parent table that the foreign key refers to.</param>
    /// <param name="parentColumnNames">The column names in the parent table that the foreign key refers to, or empty when the constraint omitted the parent column list.</param>
    /// <param name="deferrability">The declared <c>DEFERRABLE</c> behaviour.</param>
    /// <param name="matchType">The declared <c>MATCH</c> behaviour.</param>
    /// <exception cref="ArgumentNullException"><paramref name="parentTable"/>, <paramref name="columnNames"/> or <paramref name="parentColumnNames"/> is <see langword="null" />, or <paramref name="columnNames"/> or <paramref name="parentColumnNames"/> contains a <see langword="null" /> value.</exception>
    /// <exception cref="ArgumentException"><paramref name="columnNames"/> is empty, <paramref name="columnNames"/> or <paramref name="parentColumnNames"/> contains an empty or whitespace value, a non-empty <paramref name="parentColumnNames"/> has a different number of elements to <paramref name="columnNames"/>, or <paramref name="deferrability"/> or <paramref name="matchType"/> is not a valid enum.</exception>
    public ForeignKey(
        Option<string> constraintName,
        IReadOnlyCollection<string> columnNames,
        Identifier parentTable,
        IReadOnlyCollection<string> parentColumnNames,
        ConstraintDeferrability deferrability,
        ForeignKeyMatchType matchType
    )
    {
        if (columnNames.NullOrAnyNull())
            throw new ArgumentNullException(nameof(columnNames));
        if (parentColumnNames.NullOrAnyNull())
            throw new ArgumentNullException(nameof(parentColumnNames));
        if (columnNames.Empty() || columnNames.Any(static c => c.IsNullOrWhiteSpace()))
            throw new ArgumentException("A foreign key must have at least one column, and its column names must not be empty or whitespace.", nameof(columnNames));
        if (parentColumnNames.Any(static c => c.IsNullOrWhiteSpace()))
            throw new ArgumentException("The parent column names of a foreign key must not be empty or whitespace.", nameof(parentColumnNames));
        // An omitted parent column list is valid SQLite and refers to the parent table's primary key,
        // so an empty collection is accepted and only a mismatched non-empty list is rejected.
        if (parentColumnNames.Count > 0 && columnNames.Count != parentColumnNames.Count)
            throw new ArgumentException($"The number of source columns ({columnNames.Count}) does not match the number of target columns ({parentColumnNames.Count}).", nameof(parentColumnNames));
        if (!deferrability.IsValid())
            throw new ArgumentException($"The {nameof(ConstraintDeferrability)} provided must be a valid enum.", nameof(deferrability));
        if (!matchType.IsValid())
            throw new ArgumentException($"The {nameof(ForeignKeyMatchType)} provided must be a valid enum.", nameof(matchType));

        ParentTable = parentTable ?? throw new ArgumentNullException(nameof(parentTable));
        Name = constraintName;
        Columns = columnNames;
        ParentColumns = parentColumnNames;
        Deferrability = deferrability;
        MatchType = matchType;
    }

    /// <summary>
    /// The name, if available, of the foreign key constraint.
    /// </summary>
    /// <value>A constraint name, if available.
    /// </value>
    public Option<string> Name { get; }

    /// <summary>
    /// The columns comprising the constraint.
    /// </summary>
    /// <value>The columns.</value>
    public IEnumerable<string> Columns { get; }

    /// <summary>
    /// The parent table name that the foreign key refers to.
    /// </summary>
    /// <value>A parent table name.</value>
    public Identifier ParentTable { get; }

    /// <summary>
    /// The columns in the parent table that the foreign key refers to. Empty when the constraint
    /// omitted the parent column list, which refers to the parent table's primary key.
    /// </summary>
    /// <value>Columns names in the parent table.</value>
    public IEnumerable<string> ParentColumns { get; }

    /// <summary>
    /// The <c>DEFERRABLE</c> behaviour declared in the constraint's definition.
    /// </summary>
    /// <value>A deferrability value.</value>
    public ConstraintDeferrability Deferrability { get; }

    /// <summary>
    /// <para>The <c>MATCH</c> behaviour declared in the constraint's definition.</para>
    /// <para>
    /// SQLite parses a <c>MATCH</c> clause but does not act on it; every foreign key behaves as
    /// <see cref="ForeignKeyMatchType.Simple"/> regardless of what was declared.
    /// </para>
    /// </summary>
    /// <value>A foreign key match type.</value>
    public ForeignKeyMatchType MatchType { get; }
}