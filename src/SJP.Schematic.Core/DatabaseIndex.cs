using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Core.Utilities;

namespace SJP.Schematic.Core;

/// <summary>
/// A database index.
/// </summary>
/// <seealso cref="IDatabaseIndex" />
[DebuggerDisplay("{" + nameof(DebuggerDisplay) + ",nq}")]
public class DatabaseIndex : IDatabaseIndex
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DatabaseIndex"/> class.
    /// </summary>
    /// <param name="name">The index name.</param>
    /// <param name="isUnique">Whether the index is unique.</param>
    /// <param name="columns">The index columns.</param>
    /// <param name="includedColumns">Columns included when <paramref name="columns"/> have been searched.</param>
    /// <param name="isEnabled">Whether the index is enabled.</param>
    /// <exception cref="ArgumentNullException"><paramref name="name"/> is <c>null</c>. Alternatively if <paramref name="columns"/> or <paramref name="includedColumns"/> are <c>null</c> or contain <c>null</c> values.</exception>
    public DatabaseIndex(Identifier name, bool isUnique, IReadOnlyCollection<IDatabaseIndexColumn> columns, IReadOnlyCollection<IDatabaseColumn> includedColumns, bool isEnabled)
    {
        if (name == null)
            throw new ArgumentNullException(nameof(name));
        if (columns.NullOrEmpty() || columns.AnyNull())
            throw new ArgumentNullException(nameof(columns));
        if (includedColumns.NullOrAnyNull())
            throw new ArgumentNullException(nameof(includedColumns));

        Name = name.LocalName;
        IsUnique = isUnique;
        Columns = columns;
        IncludedColumns = includedColumns;
        IsEnabled = isEnabled;
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
    /// <value>A collection of database columns.</value>
    public IReadOnlyCollection<IDatabaseColumn> IncludedColumns { get; }

    /// <summary>
    /// Indicates whether the index is enabled.
    /// </summary>
    /// <value><c>true</c> if this index is enabled; otherwise, <c>false</c>.</value>
    public bool IsEnabled { get; }

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