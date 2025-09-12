using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using LanguageExt;
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
    /// <param name="filterDefinition">The definition, if present, for the subset of rows the index applies to</param>
    /// <exception cref="ArgumentNullException"><paramref name="name"/> is <see langword="null" />. Alternatively if <paramref name="columns"/> or <paramref name="includedColumns"/> are <see langword="null" /> or contain <see langword="null" /> values.</exception>
    public DatabaseIndex(Identifier name, bool isUnique, IReadOnlyCollection<IDatabaseIndexColumn> columns, IReadOnlyCollection<IDatabaseColumn> includedColumns, bool isEnabled, Option<string> filterDefinition)
    {
        ArgumentNullException.ThrowIfNull(name);
        if (columns.NullOrEmpty() || columns.AnyNull())
            throw new ArgumentNullException(nameof(columns));
        if (includedColumns.NullOrAnyNull())
            throw new ArgumentNullException(nameof(includedColumns));

        Name = name.LocalName;
        IsUnique = isUnique;
        Columns = columns;
        IncludedColumns = includedColumns;
        IsEnabled = isEnabled;
        FilterDefinition = filterDefinition;
    }

    /// <summary>
    /// The index name.
    /// </summary>
    /// <value>The name of the index.</value>
    public Identifier Name { get; }

    /// <summary>
    /// Indicates whether covered index columns must be unique across the index column set.
    /// </summary>
    /// <value><see langword="true" /> if the index column set must have unique values; otherwise, <see langword="false" />.</value>
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
    /// <value><see langword="true" /> if this index is enabled; otherwise, <see langword="false" />.</value>
    public bool IsEnabled { get; }

    /// <summary>
    /// If the index is filtered to a subset of rows, contains the expression for the subset of rows included in the filtered index.
    /// </summary>
    public Option<string> FilterDefinition { get; }

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