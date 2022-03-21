using System;
using LanguageExt;

namespace SJP.Schematic.Core.Comments;

/// <summary>
/// Comments for an <see cref="IDatabaseSynonym"/> instance.
/// </summary>
/// <seealso cref="IDatabaseSynonymComments" />
public class DatabaseSynonymComments : IDatabaseSynonymComments
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DatabaseSynonymComments"/> class.
    /// </summary>
    /// <param name="synonymName">A synonym name.</param>
    /// <param name="comment">The comment for the synonym.</param>
    /// <exception cref="ArgumentNullException"><paramref name="synonymName"/> is <c>null</c>.</exception>
    public DatabaseSynonymComments(Identifier synonymName, Option<string> comment)
    {
        SynonymName = synonymName ?? throw new ArgumentNullException(nameof(synonymName));
        Comment = comment;
    }

    /// <summary>
    /// The name of an <see cref="IDatabaseSynonym" /> instance.
    /// </summary>
    /// <value>The synonym name.</value>
    public Identifier SynonymName { get; }

    /// <summary>
    /// A comment for the <see cref="IDatabaseSynonym" /> instance.
    /// </summary>
    /// <value>A comment, if available.</value>
    public Option<string> Comment { get; }
}