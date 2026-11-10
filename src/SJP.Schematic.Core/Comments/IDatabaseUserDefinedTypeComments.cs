using LanguageExt;

namespace SJP.Schematic.Core.Comments;

/// <summary>
/// Defines comment information related to <see cref="DatabaseUserDefinedType"/> instances.
/// </summary>
public interface IDatabaseUserDefinedTypeComments
{
    /// <summary>
    /// The name of an <see cref="DatabaseUserDefinedType"/> instance.
    /// </summary>
    /// <value>The type name.</value>
    Identifier TypeName { get; }

    /// <summary>
    /// A comment for the <see cref="DatabaseUserDefinedType"/> instance.
    /// </summary>
    /// <value>The comment.</value>
    Option<string> Comment { get; }
}
