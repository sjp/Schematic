using System;
using LanguageExt;

namespace SJP.Schematic.Core.Comments;

/// <summary>
/// Comments for an <see cref="IDatabaseSchema"/> instance.
/// </summary>
/// <seealso cref="IDatabaseSchemaComments" />
public class DatabaseSchemaComments : IDatabaseSchemaComments
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DatabaseSchemaComments"/> class.
    /// </summary>
    /// <param name="schemaName">The name of the schema.</param>
    /// <param name="comment">The comment, if available.</param>
    /// <exception cref="ArgumentNullException"><paramref name="schemaName"/> is <see langword="null" />.</exception>
    public DatabaseSchemaComments(Identifier schemaName, Option<string> comment)
    {
        SchemaName = schemaName ?? throw new ArgumentNullException(nameof(schemaName));
        Comment = comment;
    }

    /// <summary>
    /// The name of an <see cref="IDatabaseSchema" /> instance.
    /// </summary>
    /// <value>The schema name.</value>
    public Identifier SchemaName { get; }

    /// <summary>
    /// A comment for the <see cref="IDatabaseSchema" /> instance.
    /// </summary>
    /// <value>The comment, if available.</value>
    public Option<string> Comment { get; }
}
