using System.Collections.Generic;
using LanguageExt;

namespace SJP.Schematic.Core;

/// <summary>
/// Defines a column covered by a database index.
/// </summary>
public interface IDatabaseIndexColumn
{
    /// <summary>
    /// The ordering applied to the column.
    /// </summary>
    /// <value>The ordering.</value>
    IndexColumnOrder Order { get; }

    /// <summary>
    /// An expression that represents the given index column e.g. <c>UPPER(name)</c>.
    /// </summary>
    /// <value>A textual expression.</value>
    string Expression { get; }

    /// <summary>
    /// The set of columns that the index column is dependent upon.
    /// </summary>
    /// <value>The dependent columns.</value>
    IReadOnlyList<IDatabaseColumn> DependentColumns { get; }

    /// <summary>
    /// Where null values sort relative to non-null values within the column.
    /// </summary>
    /// <value>A null ordering, or <see cref="IndexColumnNullOrder.Default"/> when the database
    /// does not report one.</value>
    IndexColumnNullOrder NullOrder { get; }

    /// <summary>
    /// The collation the index column is sorted by, when the database reports one that is not
    /// simply the database's default collation.
    /// </summary>
    /// <value>A collation name, if the database reports one for the index column.</value>
    Option<Identifier> Collation { get; }

    /// <summary>
    /// The number of leading characters or bytes of the column that are indexed, for a prefix index.
    /// </summary>
    /// <value>A prefix length, if the index covers only a prefix of the column's values.</value>
    Option<int> PrefixLength { get; }
}