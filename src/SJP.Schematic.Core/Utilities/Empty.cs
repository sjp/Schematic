using System.Collections.Generic;
using System.Threading.Tasks;
using LanguageExt;
using SJP.Schematic.Core.Comments;

namespace SJP.Schematic.Core.Utilities;

/// <summary>
/// For internal use only. Stores pre-allocated 'empty' objects.
/// </summary>
public static class Empty
{
    /// <summary>
    /// Gets an empty comment lookup.
    /// </summary>
    /// <value>An empty comment lookup.</value>
    public static IReadOnlyDictionary<Identifier, Option<string>> CommentLookup { get; } = new Dictionary<Identifier, Option<string>>();

    /// <summary>
    /// Contains pre-allocated tasks of empty data.
    /// </summary>
    public static class Tasks
    {
        /// <summary>
        /// An empty collection of tables.
        /// </summary>
        public static Task<IReadOnlyCollection<IRelationalDatabaseTable>> Tables { get; } = Task.FromResult<IReadOnlyCollection<IRelationalDatabaseTable>>([]);

        /// <summary>
        /// An empty collection of views.
        /// </summary>
        public static Task<IReadOnlyCollection<IDatabaseView>> Views { get; } = Task.FromResult<IReadOnlyCollection<IDatabaseView>>([]);

        /// <summary>
        /// An empty collection of sequences.
        /// </summary>
        public static Task<IReadOnlyCollection<IDatabaseSequence>> Sequences { get; } = Task.FromResult<IReadOnlyCollection<IDatabaseSequence>>([]);

        /// <summary>
        /// An empty collection of synonyms.
        /// </summary>
        public static Task<IReadOnlyCollection<IDatabaseSynonym>> Synonyms { get; } = Task.FromResult<IReadOnlyCollection<IDatabaseSynonym>>([]);

        /// <summary>
        /// An empty collection of routines.
        /// </summary>
        public static Task<IReadOnlyCollection<IDatabaseRoutine>> Routines { get; } = Task.FromResult<IReadOnlyCollection<IDatabaseRoutine>>([]);

        /// <summary>
        /// An empty collection of table comments.
        /// </summary>
        public static Task<IReadOnlyCollection<IRelationalDatabaseTableComments>> TableComments { get; } = Task.FromResult<IReadOnlyCollection<IRelationalDatabaseTableComments>>([]);

        /// <summary>
        /// An empty collection of view comments.
        /// </summary>
        public static Task<IReadOnlyCollection<IDatabaseViewComments>> ViewComments { get; } = Task.FromResult<IReadOnlyCollection<IDatabaseViewComments>>([]);

        /// <summary>
        /// An empty collection of sequence comments.
        /// </summary>
        public static Task<IReadOnlyCollection<IDatabaseSequenceComments>> SequenceComments { get; } = Task.FromResult<IReadOnlyCollection<IDatabaseSequenceComments>>([]);

        /// <summary>
        /// An empty collection of synonym comments.
        /// </summary>
        public static Task<IReadOnlyCollection<IDatabaseSynonymComments>> SynonymComments { get; } = Task.FromResult<IReadOnlyCollection<IDatabaseSynonymComments>>([]);

        /// <summary>
        /// An empty collection of routine comments.
        /// </summary>
        public static Task<IReadOnlyCollection<IDatabaseRoutineComments>> RoutineComments { get; } = Task.FromResult<IReadOnlyCollection<IDatabaseRoutineComments>>([]);
    }
}