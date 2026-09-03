using LanguageExt;

namespace SJP.Schematic.Core.Comments;

/// <summary>
/// Defines comment information related to <see cref="IDatabaseUserDefinedType"/> instances.
/// </summary>
public interface IDatabaseUserDefinedTypeComments
{
    /// <summary>
    /// The name of an <see cref="IDatabaseUserDefinedType"/> instance.
    /// </summary>
    /// <value>The type name.</value>
    Identifier TypeName { get; }

    /// <summary>
    /// A comment for the <see cref="IDatabaseUserDefinedType"/> instance.
    /// </summary>
    /// <value>The comment.</value>
    Option<string> Comment { get; }
}
