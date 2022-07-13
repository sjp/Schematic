using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
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
    /// <exception cref="ArgumentNullException"><paramref name="name"/> is <c>null</c>. Alternatively if <paramref name="columns"/> is <c>null</c>, empty or has a <c>null</c> value.</exception>
    public PostgreSqlDatabaseIndex(Identifier name, bool isUnique, IReadOnlyCollection<IDatabaseIndexColumn> columns, Option<string> filterDefinition)
        : this(name, isUnique, columns, Array.Empty<IDatabaseColumn>(), filterDefinition)
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
    /// <exception cref="ArgumentNullException"><paramref name="name"/> is <c>null</c>. Alternatively if <paramref name="columns"/> or <paramref name="includedColumns"/> are <c>null</c>, empty or has a <c>null</c> value.</exception>
    public PostgreSqlDatabaseIndex(Identifier name, bool isUnique, IReadOnlyCollection<IDatabaseIndexColumn> columns, IReadOnlyCollection<IDatabaseColumn> includedColumns, Option<string> filterDefinition)
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
    /// Indicates whether this instance is enabled.
    /// </summary>
    /// <value>Always <c>true</c>.</value>
    public bool IsEnabled { get; } = true;

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