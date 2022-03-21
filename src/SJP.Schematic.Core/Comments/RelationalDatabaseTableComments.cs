using System;
using System.Collections.Generic;
using LanguageExt;

namespace SJP.Schematic.Core.Comments;

/// <summary>
/// Contains comment information for <see cref="IRelationalDatabaseTable"/> instances.
/// </summary>
/// <seealso cref="IRelationalDatabaseTableComments" />
public class RelationalDatabaseTableComments : IRelationalDatabaseTableComments
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RelationalDatabaseTableComments"/> class.
    /// </summary>
    /// <param name="tableName">Name of the table.</param>
    /// <param name="comment">The comment.</param>
    /// <param name="primaryKeyComment">The primary key comment.</param>
    /// <param name="columnComments">The column comments.</param>
    /// <param name="checkComments">The check comments.</param>
    /// <param name="uniqueKeyComments">The unique key comments.</param>
    /// <param name="foreignKeyComments">The foreign key comments.</param>
    /// <param name="indexComments">The index comments.</param>
    /// <param name="triggerComments">The trigger comments.</param>
    /// <exception cref="ArgumentNullException">Any of <paramref name="tableName"/>, <paramref name="columnComments"/>, <paramref name="checkComments"/>, <paramref name="uniqueKeyComments"/>, <paramref name="foreignKeyComments"/>, <paramref name="indexComments"/> or <paramref name="triggerComments"/> are <c>null</c></exception>
    public RelationalDatabaseTableComments(
        Identifier tableName,
        Option<string> comment,
        Option<string> primaryKeyComment,
        IReadOnlyDictionary<Identifier, Option<string>> columnComments,
        IReadOnlyDictionary<Identifier, Option<string>> checkComments,
        IReadOnlyDictionary<Identifier, Option<string>> uniqueKeyComments,
        IReadOnlyDictionary<Identifier, Option<string>> foreignKeyComments,
        IReadOnlyDictionary<Identifier, Option<string>> indexComments,
        IReadOnlyDictionary<Identifier, Option<string>> triggerComments
    )
    {
        TableName = tableName ?? throw new ArgumentNullException(nameof(tableName));
        Comment = comment;
        PrimaryKeyComment = primaryKeyComment;
        ColumnComments = columnComments ?? throw new ArgumentNullException(nameof(columnComments));
        CheckComments = checkComments ?? throw new ArgumentNullException(nameof(checkComments));
        UniqueKeyComments = uniqueKeyComments ?? throw new ArgumentNullException(nameof(uniqueKeyComments));
        ForeignKeyComments = foreignKeyComments ?? throw new ArgumentNullException(nameof(foreignKeyComments));
        IndexComments = indexComments ?? throw new ArgumentNullException(nameof(indexComments));
        TriggerComments = triggerComments ?? throw new ArgumentNullException(nameof(triggerComments));
    }

    /// <summary>
    /// The table this object refers to.
    /// </summary>
    /// <value>The name of the table.</value>
    public Identifier TableName { get; }

    /// <summary>
    /// A comment for the table, if available.
    /// </summary>
    /// <value>A comment, if available.</value>
    public Option<string> Comment { get; }

    /// <summary>
    /// A comment for the primary key, if available.
    /// </summary>
    /// <value>The primary key comment, if available.</value>
    public Option<string> PrimaryKeyComment { get; }

    /// <summary>
    /// Comments defined for columns in the table.
    /// </summary>
    /// <value>The column comments. If no comment exists for the column, its value will be the none state.</value>
    public IReadOnlyDictionary<Identifier, Option<string>> ColumnComments { get; }

    /// <summary>
    /// Comments defined for check constraints in the table.
    /// </summary>
    /// <value>The check constraint comments. If no comment exists for the check constraint, its value will be the none state.</value>
    public IReadOnlyDictionary<Identifier, Option<string>> CheckComments { get; }

    /// <summary>
    /// Comments defined for unique constraints in the table.
    /// </summary>
    /// <value>The unique constraint comments. If no comment exists for the unique constraint, its value will be the none state.</value>
    public IReadOnlyDictionary<Identifier, Option<string>> UniqueKeyComments { get; }

    /// <summary>
    /// Comments defined for foreign keys in the table.
    /// </summary>
    /// <value>The foreign key comments. If no comment exists for the foreign key constraint, its value will be the none state.</value>
    public IReadOnlyDictionary<Identifier, Option<string>> ForeignKeyComments { get; }

    /// <summary>
    /// Comments defined for indexes on the table.
    /// </summary>
    /// <value>The index comments. If no comment exists for the index, its value will be the none state.</value>
    public IReadOnlyDictionary<Identifier, Option<string>> IndexComments { get; }

    /// <summary>
    /// Comments defined for triggers on the table.
    /// </summary>
    /// <value>The trigger comments. If no comment exists for the trigger, its value will be the none state.</value>
    public IReadOnlyDictionary<Identifier, Option<string>> TriggerComments { get; }
}