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
    /// <value><see langword="true" /> if this view is materialized; otherwise, <see langword="false" />.</value>
    bool IsMaterialized { get; }

    /// <summary>
    /// The triggers defined on the view, i.e. <c>INSTEAD OF</c> triggers. Empty when the view has
    /// no triggers, or when the database does not support triggers on views.
    /// </summary>
    /// <value>A collection of triggers.</value>
    IReadOnlyCollection<IDatabaseTrigger> Triggers { get; }

    /// <summary>
    /// The indexes defined on the view. Empty when the view has no indexes, or when the database
    /// does not support indexing a view.
    /// </summary>
    /// <value>A collection of indexes.</value>
    IReadOnlyCollection<IDatabaseIndex> Indexes { get; }

    /// <summary>
    /// The check option constraining rows written through the view.
    /// </summary>
    /// <value>A view check option.</value>
    ViewCheckOption CheckOption { get; }

    /// <summary>
    /// Determines whether rows can be written through this view. Always <see langword="false" />
    /// for a materialized view, and <see langword="false" /> when the database does not report
    /// whether a view is updatable.
    /// </summary>
    /// <value><see langword="true" /> if this view is updatable; otherwise, <see langword="false" />.</value>
    bool IsUpdatable { get; }
}
