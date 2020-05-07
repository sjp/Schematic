using System;
using LanguageExt;

namespace SJP.Schematic.Core.Comments
{
    /// <summary>
    /// Comments for an <see cref="IDatabaseSequence"/> instance.
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

        /// <summary>
        /// The name of an <see cref="IDatabaseSequence" /> instance.
        /// </summary>
        /// <value>The sequence name.</value>
        public Identifier SequenceName { get; }

        /// <summary>
        /// A comment for the <see cref="IDatabaseSequence" /> instance.
        /// </summary>
        /// <value>The comment, if available.</value>
        public Option<string> Comment { get; }
    }
}
