using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Core.Utilities;

namespace SJP.Schematic.Core
{
    /// <summary>
    /// A database view implementation, containing information about database views.
    /// </summary>
    /// <seealso cref="IDatabaseView" />
    [DebuggerDisplay("{" + nameof(DebuggerDisplay) + ",nq}")]
    public class DatabaseView : IDatabaseView
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseView"/> class.
        /// </summary>
        /// <param name="viewName">The view name.</param>
        /// <param name="definition">The view definition.</param>
        /// <param name="columns">An ordered collection of columns defined by the view definition.</param>
        /// <exception cref="ArgumentNullException"><paramref name="viewName"/> is <c>null</c>, or <paramref name="definition"/> is <c>null</c>, empty, or whitespace, or <paramref name="columns"/> is <c>null</c> or contains <c>null</c> values.</exception>
        public DatabaseView(
            Identifier viewName,
            string definition,
            IReadOnlyList<IDatabaseColumn> columns
        )
        {
            if (definition.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(definition));
            if (columns == null || columns.AnyNull())
                throw new ArgumentNullException(nameof(columns));

            Name = viewName ?? throw new ArgumentNullException(nameof(viewName));
            Columns = columns;
            Definition = definition;
        }

        /// <summary>
        /// The view name.
        /// </summary>
        /// <value>A view name.</value>
        public Identifier Name { get; }

        /// <summary>
        /// The definition of the view.
        /// </summary>
        /// <value>The view definition.</value>
        public string Definition { get; }

        /// <summary>
        /// An ordered collection of database columns that define the view.
        /// </summary>
        /// <value>The view columns.</value>
        public IReadOnlyList<IDatabaseColumn> Columns { get; }

        /// <summary>
        /// Determines whether this view is materialized or pre-computed.
        /// </summary>
        /// <value><c>true</c> if this view is materialized; otherwise, <c>false</c>.</value>
        /// <remarks>Always <c>false</c> unless overridden.</remarks>
        public virtual bool IsMaterialized { get; }

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

                builder.Append("View: ");

                if (!Name.Schema.IsNullOrWhiteSpace())
                    builder.Append(Name.Schema).Append('.');

                builder.Append(Name.LocalName);

                return builder.GetStringAndRelease();
            }
        }
    }
}
