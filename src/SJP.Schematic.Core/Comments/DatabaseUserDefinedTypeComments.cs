using System;
using LanguageExt;

namespace SJP.Schematic.Core.Comments;

/// <summary>
/// Comments for an <see cref="IDatabaseUserDefinedType"/> instance.
/// </summary>
/// <seealso cref="IDatabaseUserDefinedTypeComments" />
public class DatabaseUserDefinedTypeComments : IDatabaseUserDefinedTypeComments
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DatabaseUserDefinedTypeComments"/> class.
    /// </summary>
    /// <param name="typeName">The name of the user-defined type.</param>
    /// <param name="comment">The comment, if available.</param>
    /// <exception cref="ArgumentNullException"><paramref name="typeName"/> is <see langword="null" />.</exception>
    public DatabaseUserDefinedTypeComments(Identifier typeName, Option<string> comment)
    {
        TypeName = typeName ?? throw new ArgumentNullException(nameof(typeName));
        Comment = comment;
    }

    /// <summary>
    /// The name of an <see cref="IDatabaseUserDefinedType" /> instance.
    /// </summary>
    /// <value>The type name.</value>
    public Identifier TypeName { get; }

    /// <summary>
    /// A comment for the <see cref="IDatabaseUserDefinedType" /> instance.
    /// </summary>
    /// <value>The comment, if available.</value>
    public Option<string> Comment { get; }
}
