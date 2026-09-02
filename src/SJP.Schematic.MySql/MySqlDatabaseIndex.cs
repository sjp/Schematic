using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using EnumsNET;
using LanguageExt;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Core.Utilities;

namespace SJP.Schematic.MySql;

/// <summary>
/// A MySQL index definition.
/// </summary>
/// <seealso cref="IDatabaseIndex" />
[DebuggerDisplay("{" + nameof(DebuggerDisplay) + ",nq}")]
public class MySqlDatabaseIndex : IDatabaseIndex
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MySqlDatabaseIndex"/> class.
    /// </summary>
    /// <param name="name">An index name.</param>
    /// <param name="isUnique">Determines whether the index is unique, if <see langword="true"/>, the index is unique.</param>
    /// <param name="columns">The columns.</param>
    /// <exception cref="ArgumentNullException"><paramref name="name"/> or <paramref name="columns"/> is <see langword="null" />, or <paramref name="columns"/> contains a <see langword="null" /> value.</exception>
    /// <exception cref="ArgumentException"><paramref name="columns"/> is empty.</exception>
    public MySqlDatabaseIndex(Identifier name, bool isUnique, IReadOnlyCollection<IDatabaseIndexColumn> columns)
        : this(name, isUnique, columns, IndexType.Unknown, true)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MySqlDatabaseIndex"/> class.
    /// </summary>
    /// <param name="name">An index name.</param>
    /// <param name="isUnique">Determines whether the index is unique, if <see langword="true"/>, the index is unique.</param>
    /// <param name="columns">The columns.</param>
    /// <param name="indexType">The physical structure used to implement the index.</param>
    /// <param name="isVisible">Whether the optimizer is permitted to use the index.</param>
    /// <exception cref="ArgumentNullException"><paramref name="name"/> or <paramref name="columns"/> is <see langword="null" />, or <paramref name="columns"/> contains a <see langword="null" /> value.</exception>
    /// <exception cref="ArgumentException"><paramref name="columns"/> is empty, or <paramref name="indexType"/> is an invalid enum value.</exception>
    public MySqlDatabaseIndex(Identifier name, bool isUnique, IReadOnlyCollection<IDatabaseIndexColumn> columns, IndexType indexType, bool isVisible)
    {
        ArgumentNullException.ThrowIfNull(name);
        if (columns.NullOrAnyNull())
            throw new ArgumentNullException(nameof(columns));
        if (columns.Empty())
            throw new ArgumentException("An index must have at least one column.", nameof(columns));
        if (!indexType.IsValid())
            throw new ArgumentException($"The {nameof(Core.IndexType)} provided must be a valid enum.", nameof(indexType));

        Name = name.LocalName;
        IsUnique = isUnique;
        Columns = columns;
        IndexType = indexType;
        IsVisible = isVisible;
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
    /// The included or leaf columns that are also available once the key columns have been searched. Always empty.
    /// </summary>
    /// <value>An empty collection of columns.</value>
    public IReadOnlyCollection<IDatabaseColumn> IncludedColumns { get; } = [];

    /// <summary>
    /// Indicates whether this instance is enabled.
    /// </summary>
    /// <value>Always <see langword="true" />.</value>
    public bool IsEnabled { get; } = true;

    /// <summary>
    /// If the index is filtered to a subset of rows, contains the expression for the subset of rows included in the filtered index.
    /// </summary>
    /// <value>Always 'none', i.e. missing.</value>
    public Option<string> FilterDefinition { get; } = Option<string>.None;

    /// <summary>
    /// The physical structure used to implement the index.
    /// </summary>
    /// <value>An index structure.</value>
    public IndexType IndexType { get; }

    /// <summary>
    /// The percentage of each index page left free when the index was built.
    /// </summary>
    /// <value>Always 'none'. MySQL does not expose a per-index fill factor.</value>
    public Option<int> FillFactor { get; } = Option<int>.None;

    /// <summary>
    /// Indicates whether the index is complete and therefore usable by the query planner.
    /// </summary>
    /// <value>Always <see langword="true" />. MySQL does not report incomplete indexes.</value>
    public bool IsValid { get; } = true;

    /// <summary>
    /// Indicates whether the optimizer is permitted to use the index.
    /// </summary>
    /// <value><see langword="true" /> if the index is visible; otherwise, <see langword="false" />.</value>
    public bool IsVisible { get; }

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