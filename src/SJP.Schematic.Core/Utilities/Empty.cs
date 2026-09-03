using System.Collections.Frozen;
using System.Collections.Generic;
using System.Threading.Tasks;
using LanguageExt;
using SJP.Schematic.Core.Comments;

namespace SJP.Schematic.Core.Utilities;

/// <summary>
/// For internal use only. Stores pre-allocated 'empty' objects. Frozen collections are used
/// throughout so that the shared instances cannot be mutated by casting to a mutable interface.
/// </summary>
public static class Empty
{
    /// <summary>
    /// Gets an empty comment lookup.
    /// </summary>
    /// <value>An empty comment lookup.</value>
    public static FrozenDictionary<Identifier, Option<string>> CommentLookup { get; } = FrozenDictionary<Identifier, Option<string>>.Empty;

    /// <summary>
    /// Contains pre-allocated tasks of empty data.
    /// </summary>
    public static class Tasks
    {
        /// <summary>
        /// An empty collection of schemas.
        /// </summary>
        public static Task<IReadOnlyCollection<IDatabaseSchema>> Schemas { get; } = Task.FromResult<IReadOnlyCollection<IDatabaseSchema>>(FrozenSet<IDatabaseSchema>.Empty);

        /// <summary>
        /// An empty collection of tables.
        /// </summary>
        public static Task<IReadOnlyCollection<IRelationalDatabaseTable>> Tables { get; } = Task.FromResult<IReadOnlyCollection<IRelationalDatabaseTable>>(FrozenSet<IRelationalDatabaseTable>.Empty);

        /// <summary>
        /// An empty collection of views.
        /// </summary>
        public static Task<IReadOnlyCollection<IDatabaseView>> Views { get; } = Task.FromResult<IReadOnlyCollection<IDatabaseView>>(FrozenSet<IDatabaseView>.Empty);

        /// <summary>
        /// An empty collection of sequences.
        /// </summary>
        public static Task<IReadOnlyCollection<IDatabaseSequence>> Sequences { get; } = Task.FromResult<IReadOnlyCollection<IDatabaseSequence>>(FrozenSet<IDatabaseSequence>.Empty);

        /// <summary>
        /// An empty collection of synonyms.
        /// </summary>
        public static Task<IReadOnlyCollection<IDatabaseSynonym>> Synonyms { get; } = Task.FromResult<IReadOnlyCollection<IDatabaseSynonym>>(FrozenSet<IDatabaseSynonym>.Empty);

        /// <summary>
        /// An empty collection of routines.
        /// </summary>
        public static Task<IReadOnlyCollection<IDatabaseRoutine>> Routines { get; } = Task.FromResult<IReadOnlyCollection<IDatabaseRoutine>>(FrozenSet<IDatabaseRoutine>.Empty);

        /// <summary>
        /// An empty collection of user-defined types.
        /// </summary>
        public static Task<IReadOnlyCollection<IDatabaseUserDefinedType>> UserDefinedTypes { get; } = Task.FromResult<IReadOnlyCollection<IDatabaseUserDefinedType>>(FrozenSet<IDatabaseUserDefinedType>.Empty);

        /// <summary>
        /// An empty collection of table statistics.
        /// </summary>
        public static Task<IReadOnlyCollection<ITableStatistics>> TableStatistics { get; } = Task.FromResult<IReadOnlyCollection<ITableStatistics>>(FrozenSet<ITableStatistics>.Empty);

        /// <summary>
        /// An empty collection of schema comments.
        /// </summary>
        public static Task<IReadOnlyCollection<IDatabaseSchemaComments>> SchemaComments { get; } = Task.FromResult<IReadOnlyCollection<IDatabaseSchemaComments>>(FrozenSet<IDatabaseSchemaComments>.Empty);

        /// <summary>
        /// An empty collection of table comments.
        /// </summary>
        public static Task<IReadOnlyCollection<IRelationalDatabaseTableComments>> TableComments { get; } = Task.FromResult<IReadOnlyCollection<IRelationalDatabaseTableComments>>(FrozenSet<IRelationalDatabaseTableComments>.Empty);

        /// <summary>
        /// An empty collection of view comments.
        /// </summary>
        public static Task<IReadOnlyCollection<IDatabaseViewComments>> ViewComments { get; } = Task.FromResult<IReadOnlyCollection<IDatabaseViewComments>>(FrozenSet<IDatabaseViewComments>.Empty);

        /// <summary>
        /// An empty collection of sequence comments.
        /// </summary>
        public static Task<IReadOnlyCollection<IDatabaseSequenceComments>> SequenceComments { get; } = Task.FromResult<IReadOnlyCollection<IDatabaseSequenceComments>>(FrozenSet<IDatabaseSequenceComments>.Empty);

        /// <summary>
        /// An empty collection of synonym comments.
        /// </summary>
        public static Task<IReadOnlyCollection<IDatabaseSynonymComments>> SynonymComments { get; } = Task.FromResult<IReadOnlyCollection<IDatabaseSynonymComments>>(FrozenSet<IDatabaseSynonymComments>.Empty);

        /// <summary>
        /// An empty collection of routine comments.
        /// </summary>
        public static Task<IReadOnlyCollection<IDatabaseRoutineComments>> RoutineComments { get; } = Task.FromResult<IReadOnlyCollection<IDatabaseRoutineComments>>(FrozenSet<IDatabaseRoutineComments>.Empty);

        /// <summary>
        /// An empty collection of user-defined type comments.
        /// </summary>
        public static Task<IReadOnlyCollection<IDatabaseUserDefinedTypeComments>> UserDefinedTypeComments { get; } = Task.FromResult<IReadOnlyCollection<IDatabaseUserDefinedTypeComments>>(FrozenSet<IDatabaseUserDefinedTypeComments>.Empty);
    }
}
