using System.Collections.Generic;

namespace SJP.Schematic.Core;

/// <summary>
/// Defines a database view.
/// </summary>
/// <seealso cref="IDatabaseQueryable" />
public interface IDatabaseView : IDatabaseQueryable
{
    /// <summary>
    /// The definition of the view.
    /// </summary>
    /// <value>The view definition.</value>
    string Definition { get; }

    /// <summary>
    /// An ordered collection of database columns that define the view.
    /// </summary>
    /// <value>The view columns.</value>
    IReadOnlyList<IDatabaseColumn> Columns { get; }

    /// <summary>
    /// Determines whether this view is materialized or pre-computed.
    /// </summary>
    /// <value><c>true</c> if this view is materialized; otherwise, <c>false</c>.</value>
    bool IsMaterialized { get; }
}