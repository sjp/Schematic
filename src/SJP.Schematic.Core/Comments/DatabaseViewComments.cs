using System;
using System.Collections.Generic;
using LanguageExt;

namespace SJP.Schematic.Core.Comments;

/// <summary>
/// Comments for an <see cref="IDatabaseView"/> instance.
/// </summary>
/// <seealso cref="IDatabaseViewComments" />
public class DatabaseViewComments : IDatabaseViewComments
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DatabaseViewComments"/> class.
    /// </summary>
    /// <param name="viewName">A view name.</param>
    /// <param name="comment">The comment for the view, if available.</param>
    /// <param name="columnComments">The view's column's comments.</param>
    /// <exception cref="ArgumentNullException"><paramref name="viewName"/> or <paramref name="columnComments"/> is <c>null</c></exception>
    public DatabaseViewComments(
        Identifier viewName,
        Option<string> comment,
        IReadOnlyDictionary<Identifier, Option<string>> columnComments
    )
    {
        ViewName = viewName ?? throw new ArgumentNullException(nameof(viewName));
        Comment = comment;
        ColumnComments = columnComments ?? throw new ArgumentNullException(nameof(columnComments));
    }

    /// <summary>
    /// The name of an <see cref="IDatabaseView" /> instance.
    /// </summary>
    /// <value>The synonym name.</value>
    public Identifier ViewName { get; }

    /// <summary>
    /// A comment for the <see cref="IDatabaseView" /> instance.
    /// </summary>
    /// <value>A comment, if available.</value>
    public Option<string> Comment { get; }

    /// <summary>
    /// Comments defined for columns in the view.
    /// </summary>
    /// <value>The column comments. If no comment exists for the column, its value will be the none state.</value>
    public IReadOnlyDictionary<Identifier, Option<string>> ColumnComments { get; }
}
