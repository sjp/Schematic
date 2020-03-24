using System;
using LanguageExt;

namespace SJP.Schematic.Core.Comments
{
    /// <summary>
    /// Stores comment information for database sequences.
    /// </summary>
    /// <seealso cref="IDatabaseSequenceComments" />
    public class DatabaseSequenceComments : IDatabaseSequenceComments
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseSequenceComments"/> class.
        /// </summary>
        /// <param name="sequenceName">The name of the sequence.</param>
        /// <param name="comment">The comment, if available.</param>
        /// <exception cref="ArgumentNullException"><paramref name="sequenceName"/> is <c>null</c>.</exception>
        public DatabaseSequenceComments(Identifier sequenceName, Option<string> comment)
        {
            SequenceName = sequenceName ?? throw new ArgumentNullException(nameof(sequenceName));
            Comment = comment;
        }

        /// <inheritdoc />
        public Identifier SequenceName { get; }

        /// <inheritdoc />
        public Option<string> Comment { get; }
    }
}
