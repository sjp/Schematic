using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Core.Utilities;

namespace SJP.Schematic.Sqlite;

/// <summary>
/// A SQLite index definition.
/// </summary>
/// <seealso cref="IDatabaseIndex" />
[DebuggerDisplay("{" + nameof(DebuggerDisplay) + ",nq}")]
public class SqliteDatabaseIndex : IDatabaseIndex
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SqliteDatabaseIndex"/> class.
    /// </summary>
    /// <param name="name">The index name.</param>
    /// <param name="isUnique">If set to <c>true</c>, the index is unique.</param>
    /// <param name="columns">The index key columns.</param>
    /// <param name="includedColumns">The index's included columns, available once the key columns are searched.</param>
    /// <exception cref="ArgumentNullException"><paramref name="columns"/>, <paramref name="includedColumns"/> or <paramref name="name"/> is <c>null</c>.</exception>
    public SqliteDatabaseIndex(Identifier name, bool isUnique, IEnumerable<IDatabaseIndexColumn> columns, IEnumerable<IDatabaseColumn> includedColumns)
    {
        if (name == null)
            throw new ArgumentNullException(nameof(name));
        if (columns == null || columns.Empty() || columns.AnyNull())
            throw new ArgumentNullException(nameof(columns));
        if (includedColumns != null && includedColumns.AnyNull())
            throw new ArgumentNullException(nameof(includedColumns));

        if (includedColumns == null)
            includedColumns = Array.Empty<IDatabaseColumn>();

        Name = name.LocalName;
        IsUnique = isUnique;
        Columns = columns.ToList();
        IncludedColumns = includedColumns.ToList();
    }

    /// <summary>
    /// The index name.
    /// </summary>
    /// <value>The name of the index.</value>
    public Identifier Name { get; }

    /// <summary>
    /// Indicates whether covered index columns must be unique across the index column set.
    /// </summary>
    /// <value><c>true</c> if the index column set must have unique values; otherwise, <c>false</c>.</value>
    public bool IsUnique { get; }

    /// <summary>
    /// The index columns that form the primary basis of the index.
    /// </summary>
    /// <value>A collection of index columns.</value>
    public IReadOnlyCollection<IDatabaseIndexColumn> Columns { get; }

    /// <summary>
    /// The included or leaf columns that are also available once the key columns have been searched.
    /// </summary>
    /// <value>A collection of columns that are included in indexes.</value>
    public IReadOnlyCollection<IDatabaseColumn> IncludedColumns { get; }

    /// <summary>
    /// Indicates whether this instance is enabled.
    /// </summary>
    /// <value>Always <c>true</c>.</value>
    public bool IsEnabled { get; } = true;

    /// <summary>
    /// Returns a string that provides a basic string representation of this object.
    /// </summary>
    /// <returns>A <see cref="string"/> that represents this instance.</returns>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public override string ToString() => DebuggerDisplay;

    private string DebuggerDisplay
    {
        get
        {
            var builder = StringBuilderCache.Acquire();

            builder.Append("Index: ");

            if (!Name.Schema.IsNullOrWhiteSpace())
                builder.Append(Name.Schema).Append('.');

            builder.Append(Name.LocalName);

            return builder.GetStringAndRelease();
        }
    }
}
