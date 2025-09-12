using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Core.Utilities;

namespace SJP.Schematic.Core;

/// <summary>
/// A materialized database view implementation, containing information about materialized database views.
/// </summary>
/// <seealso cref="DatabaseView" />
[DebuggerDisplay("{" + nameof(DebuggerDisplay) + ",nq}")]
public class DatabaseMaterializedView : DatabaseView
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DatabaseMaterializedView"/> class.
    /// </summary>
    /// <param name="viewName">The view name.</param>
    /// <param name="definition">The view definition.</param>
    /// <param name="columns">An ordered collection of columns defined by the view definition.</param>
    public DatabaseMaterializedView(
        Identifier viewName,
        string definition,
        IReadOnlyList<IDatabaseColumn> columns
    ) : base(viewName, definition, columns)
    {
    }

    /// <summary>
    /// Determines whether this view is materialized or pre-computed.
    /// </summary>
    /// <value><see langword="true" /> if this view is materialized; otherwise, <see langword="false" />.</value>
    /// <remarks>Always <see langword="true" /> unless overridden.</remarks>
    public override bool IsMaterialized { get; } = true;

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

            builder.Append("Materialized View: ");

            if (!Name.Schema.IsNullOrWhiteSpace())
                builder.Append(Name.Schema).Append('.');

            builder.Append(Name.LocalName);

            return builder.GetStringAndRelease();
        }
    }
}