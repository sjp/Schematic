using System.Collections.Generic;
using LanguageExt;

namespace SJP.Schematic.Core.Comments
{
    /// <summary>
    /// Defines comment information related to <see cref="IDatabaseView"/> instances.
    /// </summary>
    public interface IDatabaseViewComments
    {
        /// <summary>
        /// The name of an <see cref="IDatabaseView"/> instance.
        /// </summary>
        /// <value>The synonym name.</value>
        Identifier ViewName { get; }

        /// <summary>
        /// A comment for the <see cref="IDatabaseView"/> instance.
        /// </summary>
        /// <value>A comment, if available.</value>
        Option<string> Comment { get; }

        /// <summary>
        /// Comments defined for columns in the view.
        /// </summary>
        /// <value>The column comments. If no comment exists for the column, its value will be the none state.</value>
        IReadOnlyDictionary<Identifier, Option<string>> ColumnComments { get; }
    }
}
