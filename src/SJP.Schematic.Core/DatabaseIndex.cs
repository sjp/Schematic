using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using EnumsNET;
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
    /// <param name="name">The index name. Only the local name is kept.</param>
    /// <param name="isUnique">Whether the index is unique.</param>
    /// <param name="columns">The index columns.</param>
    /// <param name="includedColumns">Columns included when <paramref name="columns"/> have been searched.</param>
    /// <param name="isEnabled">Whether the index is enabled.</param>
    /// <param name="filterDefinition">The definition, if present, for the subset of rows the index applies to</param>
    /// <exception cref="ArgumentNullException"><paramref name="name"/> is <see langword="null" />. Alternatively if <paramref name="columns"/> or <paramref name="includedColumns"/> are <see langword="null" /> or contain <see langword="null" /> values.</exception>
    /// <exception cref="ArgumentException"><paramref name="columns"/> is empty.</exception>
    public DatabaseIndex(Identifier name, bool isUnique, IReadOnlyCollection<IDatabaseIndexColumn> columns, IReadOnlyCollection<IDatabaseColumn> includedColumns, bool isEnabled, Option<string> filterDefinition)
        : this(name, isUnique, columns, includedColumns, isEnabled, filterDefinition, IndexType.Unknown, Option<int>.None, true, true)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DatabaseIndex"/> class.
    /// </summary>
    /// <param name="name">The index name. Only the local name is kept.</param>
    /// <param name="isUnique">Whether the index is unique.</param>
    /// <param name="columns">The index columns.</param>
    /// <param name="includedColumns">Columns included when <paramref name="columns"/> have been searched.</param>
    /// <param name="isEnabled">Whether the index is enabled.</param>
    /// <param name="filterDefinition">The definition, if present, for the subset of rows the index applies to</param>
    /// <param name="indexType">The physical structure used to implement the index.</param>
    /// <param name="fillFactor">The percentage of each index page left free, if the database reports one.</param>
    /// <param name="isValid">Whether the index is complete and usable by the query planner.</param>
    /// <param name="isVisible">Whether the query planner is permitted to use the index.</param>
    /// <exception cref="ArgumentNullException"><paramref name="name"/> is <see langword="null" />. Alternatively if <paramref name="columns"/> or <paramref name="includedColumns"/> are <see langword="null" /> or contain <see langword="null" /> values.</exception>
    /// <exception cref="ArgumentException"><paramref name="columns"/> is empty, or <paramref name="indexType"/> is an invalid enum value.</exception>
    public DatabaseIndex(
        Identifier name,
        bool isUnique,
        IReadOnlyCollection<IDatabaseIndexColumn> columns,
        IReadOnlyCollection<IDatabaseColumn> includedColumns,
        bool isEnabled,
        Option<string> filterDefinition,
        IndexType indexType,
        Option<int> fillFactor,
        bool isValid,
        bool isVisible
    )
    {
        ArgumentNullException.ThrowIfNull(name);

        var indexColumns = columns.ToDefensiveCopy(nameof(columns));
        if (indexColumns.Empty())
            throw new ArgumentException("An index must have at least one column.", nameof(columns));
        var indexIncludedColumns = includedColumns.ToDefensiveCopy(nameof(includedColumns));
        if (!indexType.IsValid())
            throw new ArgumentException($"The {nameof(Core.IndexType)} provided must be a valid enum.", nameof(indexType));

        Name = name.LocalName;
        IsUnique = isUnique;
        Columns = indexColumns;
        IncludedColumns = indexIncludedColumns;
        IsEnabled = isEnabled;
        FilterDefinition = filterDefinition;
        IndexType = indexType;
        FillFactor = fillFactor;
        IsValid = isValid;
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
    /// The physical structure used to implement the index.
    /// </summary>
    /// <value>An index structure, or <see cref="Core.IndexType.Unknown"/> when the database does not report one.</value>
    public IndexType IndexType { get; }

    /// <summary>
    /// The percentage of each index page left free when the index was built or last rebuilt.
    /// </summary>
    /// <value>A fill factor between <c>1</c> and <c>100</c>, if the database reports one.</value>
    public Option<int> FillFactor { get; }

    /// <summary>
    /// Indicates whether the index is complete and therefore usable by the query planner.
    /// </summary>
    /// <value><see langword="true" /> if the index is valid; otherwise, <see langword="false" />.</value>
    public bool IsValid { get; }

    /// <summary>
    /// Indicates whether the query planner is permitted to use the index.
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

            builder.Append("Index: ")
                .Append(Name.LocalName);

            return builder.GetStringAndRelease();
        }
    }
}