using LanguageExt;

namespace SJP.Schematic.Core.Comments;

/// <summary>
/// Defines comment information related to <see cref="IDatabaseSequence"/> instances.
/// </summary>
public interface IDatabaseSequenceComments
{
    /// <summary>
    /// The name of an <see cref="IDatabaseSequence"/> instance.
    /// </summary>
    /// <value>The sequence name.</value>
    Identifier SequenceName { get; }

    /// <summary>
    /// A comment for the <see cref="IDatabaseSequence"/> instance.
    /// </summary>
    /// <value>The comment.</value>
    Option<string> Comment { get; }
}
