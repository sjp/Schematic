using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using EnumsNET;
using LanguageExt;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Core.Utilities;

namespace SJP.Schematic.PostgreSql;

/// <summary>
/// A PostgreSQL index definition.
/// </summary>
/// <seealso cref="IDatabaseIndex" />
[DebuggerDisplay("{" + nameof(DebuggerDisplay) + ",nq}")]
public class PostgreSqlDatabaseIndex : IDatabaseIndex
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PostgreSqlDatabaseIndex"/> class.
    /// </summary>
    /// <param name="name">An index name.</param>
    /// <param name="isUnique">Determines whether the index is unique, if <see langword="true"/>, the index is unique.</param>
    /// <param name="columns">The columns.</param>
    /// <param name="filterDefinition">The definition, if present, for the subset of rows the index applies to</param>
    /// <exception cref="ArgumentNullException"><paramref name="name"/> or <paramref name="columns"/> is <see langword="null" />, or <paramref name="columns"/> contains a <see langword="null" /> value.</exception>
    /// <exception cref="ArgumentException"><paramref name="columns"/> is empty.</exception>
    public PostgreSqlDatabaseIndex(Identifier name, bool isUnique, IReadOnlyCollection<IDatabaseIndexColumn> columns, Option<string> filterDefinition)
        : this(name, isUnique, columns, [], filterDefinition)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PostgreSqlDatabaseIndex"/> class.
    /// </summary>
    /// <param name="name">An index name.</param>
    /// <param name="isUnique">Determines whether the index is unique, if <see langword="true"/>, the index is unique.</param>
    /// <param name="columns">The columns.</param>
    /// <param name="includedColumns">Columns included when the index is searched.</param>
    /// <param name="filterDefinition">The definition, if present, for the subset of rows the index applies to</param>
    /// <exception cref="ArgumentNullException"><paramref name="name"/>, <paramref name="columns"/> or <paramref name="includedColumns"/> is <see langword="null" />, or <paramref name="columns"/> or <paramref name="includedColumns"/> contains a <see langword="null" /> value.</exception>
    /// <exception cref="ArgumentException"><paramref name="columns"/> is empty.</exception>
    public PostgreSqlDatabaseIndex(Identifier name, bool isUnique, IReadOnlyCollection<IDatabaseIndexColumn> columns, IReadOnlyCollection<IDatabaseColumn> includedColumns, Option<string> filterDefinition)
        : this(name, isUnique, columns, includedColumns, filterDefinition, IndexType.Unknown, true)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PostgreSqlDatabaseIndex"/> class.
    /// </summary>
    /// <param name="name">An index name.</param>
    /// <param name="isUnique">Determines whether the index is unique, if <see langword="true"/>, the index is unique.</param>
    /// <param name="columns">The columns.</param>
    /// <param name="includedColumns">Columns included when the index is searched.</param>
    /// <param name="filterDefinition">The definition, if present, for the subset of rows the index applies to</param>
    /// <param name="indexType">The access method used to implement the index.</param>
    /// <param name="isValid">Whether the index was built completely, i.e. <c>pg_index.indisvalid</c>.</param>
    /// <exception cref="ArgumentNullException"><paramref name="name"/>, <paramref name="columns"/> or <paramref name="includedColumns"/> is <see langword="null" />, or <paramref name="columns"/> or <paramref name="includedColumns"/> contains a <see langword="null" /> value.</exception>
    /// <exception cref="ArgumentException"><paramref name="columns"/> is empty, or <paramref name="indexType"/> is an invalid enum value.</exception>
    public PostgreSqlDatabaseIndex(Identifier name, bool isUnique, IReadOnlyCollection<IDatabaseIndexColumn> columns, IReadOnlyCollection<IDatabaseColumn> includedColumns, Option<string> filterDefinition, IndexType indexType, bool isValid)
    {
        ArgumentNullException.ThrowIfNull(name);
        if (columns.NullOrAnyNull())
            throw new ArgumentNullException(nameof(columns));
        if (columns.Empty())
            throw new ArgumentException("An index must have at least one column.", nameof(columns));
        if (includedColumns.NullOrAnyNull())
            throw new ArgumentNullException(nameof(includedColumns));
        if (!indexType.IsValid())
            throw new ArgumentException($"The {nameof(Core.IndexType)} provided must be a valid enum.", nameof(indexType));

        Name = name.LocalName;
        IsUnique = isUnique;
        Columns = columns;
        IncludedColumns = includedColumns;
        FilterDefinition = filterDefinition;
        IndexType = indexType;
        IsValid = isValid;
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
    /// Indicates whether this instance is enabled.
    /// </summary>
    /// <value>Always <see langword="true" />. PostgreSQL indexes cannot be disabled; an index that the
    /// planner will not use is reported by <see cref="IsValid"/> instead.</value>
    public bool IsEnabled { get; } = true;

    /// <summary>
    /// If the index is filtered to a subset of rows, contains the expression for the subset of rows included in the filtered index.
    /// </summary>
    public Option<string> FilterDefinition { get; }

    /// <summary>
    /// The access method used to implement the index, e.g. btree or gin.
    /// </summary>
    /// <value>An index structure.</value>
    public IndexType IndexType { get; }

    /// <summary>
    /// The percentage of each index page left free when the index was built.
    /// </summary>
    /// <value>Always 'none'. A PostgreSQL fill factor is a storage parameter rather than index metadata.</value>
    public Option<int> FillFactor { get; } = Option<int>.None;

    /// <summary>
    /// Indicates whether the index is complete and therefore usable by the query planner.
    /// </summary>
    /// <value><see langword="false" /> for an index left incomplete by a failed build; otherwise, <see langword="true" />.</value>
    public bool IsValid { get; }

    /// <summary>
    /// Indicates whether the query planner is permitted to use the index.
    /// </summary>
    /// <value>Always <see langword="true" />. PostgreSQL has no invisible indexes.</value>
    public bool IsVisible { get; } = true;

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