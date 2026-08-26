using System;
using System.Threading;
using System.Threading.Tasks;
using Boxed.Mapping;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Comments;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Serialization.Dto.Comments;

namespace SJP.Schematic.Serialization.Mapping.Comments;

public class DatabaseCommentProviderMapper
    : IAsyncImmutableMapper<IRelationalDatabaseCommentProvider, DatabaseCommentProvider>
{
    /// <summary>
    /// Maps a serialized comment definition to a database comment provider.
    /// </summary>
    /// <param name="source">A serialized comment definition.</param>
    /// <param name="identifierResolver">An identifier resolver used by the resulting provider to look up objects.</param>
    /// <returns>A database comment provider.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="identifierResolver"/> is <c>null</c>.</exception>
    public IRelationalDatabaseCommentProvider Map(DatabaseCommentProvider source, IIdentifierResolutionStrategy identifierResolver)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(identifierResolver);

        var identifierDefaultsMapper = MapperRegistry.GetMapper<Dto.IdentifierDefaults, IIdentifierDefaults>();
        var tableCommentsMapper = MapperRegistry.GetMapper<DatabaseTableComments, IRelationalDatabaseTableComments>();
        var viewCommentsMapper = MapperRegistry.GetMapper<Dto.Comments.DatabaseViewComments, IDatabaseViewComments>();
        var sequenceCommentsMapper = MapperRegistry.GetMapper<Dto.Comments.DatabaseSequenceComments, IDatabaseSequenceComments>();
        var synonymCommentsMapper = MapperRegistry.GetMapper<Dto.Comments.DatabaseSynonymComments, IDatabaseSynonymComments>();
        var routineCommentsMapper = MapperRegistry.GetMapper<Dto.Comments.DatabaseRoutineComments, IDatabaseRoutineComments>();

        return new RelationalDatabaseCommentProvider(
            identifierDefaultsMapper.Map(source.IdentifierDefaults),
            identifierResolver,
            tableCommentsMapper.MapList(source.TableComments),
            viewCommentsMapper.MapList(source.ViewComments),
            sequenceCommentsMapper.MapList(source.SequenceComments),
            synonymCommentsMapper.MapList(source.SynonymComments),
            routineCommentsMapper.MapList(source.RoutineComments)
        );
    }

    public async Task<DatabaseCommentProvider> MapAsync(IRelationalDatabaseCommentProvider source, CancellationToken cancellationToken)
    {
        var (
            tableComments,
            viewComments,
            sequenceComments,
            synonymComments,
            routineComments
        ) = await (
            source.GetAllTableComments(cancellationToken),
            source.GetAllViewComments(cancellationToken),
            source.GetAllSequenceComments(cancellationToken),
            source.GetAllSynonymComments(cancellationToken),
            source.GetAllRoutineComments(cancellationToken)
        ).WhenAll();

        var tableCommentMapper = MapperRegistry.GetMapper<IRelationalDatabaseTableComments, DatabaseTableComments>();
        var viewCommentMapper = MapperRegistry.GetMapper<IDatabaseViewComments, Dto.Comments.DatabaseViewComments>();
        var sequenceCommentMapper = MapperRegistry.GetMapper<IDatabaseSequenceComments, Dto.Comments.DatabaseSequenceComments>();
        var synonymCommentMapper = MapperRegistry.GetMapper<IDatabaseSynonymComments, Dto.Comments.DatabaseSynonymComments>();
        var routineCommentMapper = MapperRegistry.GetMapper<IDatabaseRoutineComments, Dto.Comments.DatabaseRoutineComments>();

        var identifierDefaultsMapper = MapperRegistry.GetMapper<IIdentifierDefaults, Dto.IdentifierDefaults>();

        return new DatabaseCommentProvider
        {
            IdentifierDefaults = identifierDefaultsMapper.Map(source.IdentifierDefaults),
            TableComments = tableCommentMapper.MapList(tableComments),
            ViewComments = viewCommentMapper.MapList(viewComments),
            SequenceComments = sequenceCommentMapper.MapList(sequenceComments),
            SynonymComments = synonymCommentMapper.MapList(synonymComments),
            RoutineComments = routineCommentMapper.MapList(routineComments),
        };
    }
}