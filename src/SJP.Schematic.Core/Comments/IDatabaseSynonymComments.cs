using LanguageExt;

namespace SJP.Schematic.Core.Comments
{
    /// <summary>
    /// Defines comment information related to <see cref="IDatabaseSynonym"/> instances.
    /// </summary>
    public interface IDatabaseSynonymComments
    {
        /// <summary>
        /// The name of an <see cref="IDatabaseSynonym"/> instance.
        /// </summary>
        /// <value>The synonym name.</value>
        Identifier SynonymName { get; }

        /// <summary>
        /// A comment for the <see cref="IDatabaseSynonym"/> instance.
        /// </summary>
        /// <value>A comment, if available.</value>
        Option<string> Comment { get; }
    }
}
