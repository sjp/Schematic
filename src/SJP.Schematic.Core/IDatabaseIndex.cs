using System.Collections.Generic;
using LanguageExt;

namespace SJP.Schematic.Core;

/// <summary>
/// Defines a database index.
/// </summary>
/// <seealso cref="IDatabaseOptional" />
public interface IDatabaseIndex : IDatabaseOptional
{
    /// <summary>
    /// The index name.
    /// </summary>
    /// <value>The name of the index.</value>
    Identifier Name { get; }

    /// <summary>
    /// The index columns that form the primary basis of the index.
    /// </summary>
    /// <value>A collection of index columns.</value>
    IReadOnlyCollection<IDatabaseIndexColumn> Columns { get; }

    /// <summary>
    /// The included or leaf columns that are also available once the key columns have been searched.
    /// </summary>
    /// <value>A collection of database columns.</value>
    IReadOnlyCollection<IDatabaseColumn> IncludedColumns { get; }

    /// <summary>
    /// Indicates whether covered index columns must be unique across the index column set.
    /// </summary>
    /// <value><see langword="true" /> if the index column set must have unique values; otherwise, <see langword="false" />.</value>
    bool IsUnique { get; }

    /// <summary>
    /// If the index is filtered to a subset of rows, contains the expression for the subset of rows included in the filtered index.
    /// </summary>
    Option<string> FilterDefinition { get; }

    /// <summary>
    /// The physical structure used to implement the index.
    /// </summary>
    /// <value>An index structure, or <see cref="Core.IndexType.Unknown"/> when the database does not report one.</value>
    IndexType IndexType { get; }

    /// <summary>
    /// The percentage of each index page left free when the index was built or last rebuilt.
    /// </summary>
    /// <value>A fill factor between <c>1</c> and <c>100</c>, if the database reports one.</value>
    Option<int> FillFactor { get; }

    /// <summary>
    /// Indicates whether the index is complete and therefore usable by the query planner.
    /// </summary>
    /// <remarks>
    /// This is distinct from <see cref="IDatabaseOptional.IsEnabled"/>. An index is invalid when the
    /// database built it incompletely, e.g. a failed concurrent build in PostgreSQL or an unusable
    /// index in Oracle, rather than because a user disabled it.
    /// </remarks>
    /// <value><see langword="true" /> if the index is valid; otherwise, <see langword="false" />.</value>
    bool IsValid { get; }

    /// <summary>
    /// Indicates whether the query planner is permitted to use the index.
    /// </summary>
    /// <remarks>
    /// Invisible (or hidden) indexes are maintained by the database but ignored when planning queries.
    /// Databases without the concept report every index as visible.
    /// </remarks>
    /// <value><see langword="true" /> if the index is visible; otherwise, <see langword="false" />.</value>
    bool IsVisible { get; }
}