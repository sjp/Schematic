using System.Collections.Generic;

namespace SJP.Schematic.Serialization.Dto.Comments;

/// <summary>
/// The serialized comments attached to a database view and to its columns.
/// </summary>
public class DatabaseViewComments
{
    /// <summary>
    /// The name of the view the comments are attached to.
    /// </summary>
    public required Identifier ViewName { get; init; }

    /// <summary>
    /// The comment attached to the view, if any.
    /// </summary>
    public string? Comment { get; init; }

    /// <summary>
    /// The comments attached to the view's columns, keyed by column name. A <see langword="null"/>
    /// value means the column exists but carries no comment.
    /// </summary>
    public required IReadOnlyDictionary<string, string?> ColumnComments { get; init; }
}
