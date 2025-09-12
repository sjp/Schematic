using System;
using System.Collections.Generic;
using System.Linq;
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
    /// <param name="parentColumnNames">The column names in the parent table that the foreign key refers to. Should be a single column name.</param>
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
    /// <param name="parentColumnNames">The column names in the parent table that the foreign key refers to. Should be a single column name.</param>
    /// <exception cref="ArgumentNullException"><paramref name="parentTable"/> is <see langword="null" />. Alternatively if either <paramref name="columnNames"/> or <paramref name="parentColumnNames"/> are <see langword="null" />, empty, or contains a <see langword="null" />.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="columnNames"/> and <paramref name="parentColumnNames"/> have a different number of elements.</exception>
    public ForeignKey(Option<string> constraintName, IReadOnlyCollection<string> columnNames, Identifier parentTable, IReadOnlyCollection<string> parentColumnNames)
    {
        if (columnNames.NullOrEmpty() || columnNames.Any(static c => c.IsNullOrWhiteSpace()))
            throw new ArgumentNullException(nameof(columnNames));
        if (parentColumnNames.NullOrEmpty() || parentColumnNames.Any(static c => c.IsNullOrWhiteSpace()))
            throw new ArgumentNullException(nameof(parentColumnNames));
        if (columnNames.Count != parentColumnNames.Count)
            throw new ArgumentException($"The number of source columns ({columnNames.Count}) does not match the number of target columns ({parentColumnNames.Count}).", nameof(parentColumnNames));

        ParentTable = parentTable ?? throw new ArgumentNullException(nameof(parentTable));
        Name = constraintName;
        Columns = columnNames;
        ParentColumns = parentColumnNames;
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
    /// The columns in the parent table that the foreign key refers to.
    /// </summary>
    /// <value>Columns names in the parent table.</value>
    public IEnumerable<string> ParentColumns { get; }
}