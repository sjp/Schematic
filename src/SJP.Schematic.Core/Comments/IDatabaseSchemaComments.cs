using LanguageExt;

namespace SJP.Schematic.Core.Comments;

/// <summary>
/// Defines comment information related to <see cref="IDatabaseSchema"/> instances.
/// </summary>
public interface IDatabaseSchemaComments
{
    /// <summary>
    /// The name of an <see cref="IDatabaseSchema"/> instance.
    /// </summary>
    /// <value>The schema name.</value>
    Identifier SchemaName { get; }

    /// <summary>
    /// A comment for the <see cref="IDatabaseSchema"/> instance.
    /// </summary>
    /// <value>The comment.</value>
    Option<string> Comment { get; }
}
