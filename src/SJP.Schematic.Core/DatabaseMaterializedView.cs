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
/// A materialized database view implementation, containing information about materialized database views.
/// </summary>
/// <seealso cref="DatabaseView" />
/// <seealso cref="IDatabaseMaterializedView" />
[DebuggerDisplay("{" + nameof(DebuggerDisplay) + ",nq}")]
public class DatabaseMaterializedView : DatabaseView, IDatabaseMaterializedView
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DatabaseMaterializedView"/> class, without any
    /// triggers, indexes or refresh metadata.
    /// </summary>
    /// <param name="viewName">The view name.</param>
    /// <param name="definition">The view definition.</param>
    /// <param name="columns">An ordered collection of columns defined by the view definition.</param>
    public DatabaseMaterializedView(
        Identifier viewName,
        string definition,
        IReadOnlyList<IDatabaseColumn> columns
    ) : this(viewName, definition, columns, [], [], MaterializedViewRefreshMode.Unknown, Option<string>.None, false)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DatabaseMaterializedView"/> class.
    /// </summary>
    /// <param name="viewName">The view name.</param>
    /// <param name="definition">The view definition.</param>
    /// <param name="columns">An ordered collection of columns defined by the view definition.</param>
    /// <param name="triggers">The triggers defined on the view. Empty when the database does not support triggers on materialized views.</param>
    /// <param name="indexes">The indexes defined on the view.</param>
    /// <param name="refreshMode">Describes when the stored results of the view are refreshed.</param>
    /// <param name="refreshMethod">How the stored results are recomputed on a refresh, where the database has more than one method.</param>
    /// <param name="isPopulated">Whether the view currently holds data.</param>
    /// <exception cref="ArgumentNullException"><paramref name="viewName"/> or <paramref name="definition"/> is <see langword="null" />, or <paramref name="columns"/>, <paramref name="triggers"/> or <paramref name="indexes"/> is <see langword="null" /> or contains <see langword="null" /> values.</exception>
    /// <exception cref="ArgumentException"><paramref name="definition"/> is empty or whitespace, or <paramref name="refreshMode"/> is an invalid enum value.</exception>
    public DatabaseMaterializedView(
        Identifier viewName,
        string definition,
        IReadOnlyList<IDatabaseColumn> columns,
        IReadOnlyCollection<IDatabaseTrigger> triggers,
        IReadOnlyCollection<IDatabaseIndex> indexes,
        MaterializedViewRefreshMode refreshMode,
        Option<string> refreshMethod,
        bool isPopulated
    ) : base(viewName, definition, columns, triggers, indexes, ViewCheckOption.None, false)
    {
        if (!refreshMode.IsValid())
            throw new ArgumentException($"The {nameof(MaterializedViewRefreshMode)} provided must be a valid enum.", nameof(refreshMode));

        RefreshMode = refreshMode;
        RefreshMethod = refreshMethod;
        IsPopulated = isPopulated;
    }

    /// <summary>
    /// Determines whether this view is materialized or pre-computed.
    /// </summary>
    /// <value><see langword="true" /> if this view is materialized; otherwise, <see langword="false" />.</value>
    /// <remarks>Always <see langword="true" /> unless overridden.</remarks>
    public override bool IsMaterialized { get; } = true;

    /// <summary>
    /// Describes when the stored results of the view are refreshed.
    /// </summary>
    /// <value>A refresh mode.</value>
    public MaterializedViewRefreshMode RefreshMode { get; }

    /// <summary>
    /// How the stored results of the view are recomputed when it is refreshed.
    /// </summary>
    /// <value>A refresh method, if available.</value>
    public Option<string> RefreshMethod { get; }

    /// <summary>
    /// Determines whether the view currently holds data.
    /// </summary>
    /// <value><see langword="true" /> if this view holds data; otherwise, <see langword="false" />.</value>
    public bool IsPopulated { get; }

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
