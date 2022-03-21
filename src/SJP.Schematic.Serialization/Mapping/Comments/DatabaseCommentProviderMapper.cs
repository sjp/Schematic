using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Boxed.Mapping;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Comments;
using SJP.Schematic.Serialization.Dto.Comments;

namespace SJP.Schematic.Serialization.Mapping.Comments;

public class DatabaseCommentProviderMapper
    : IImmutableMapper<DatabaseCommentProvider, IRelationalDatabaseCommentProvider>
    , IAsyncImmutableMapper<IRelationalDatabaseCommentProvider, DatabaseCommentProvider>
{
    public IRelationalDatabaseCommentProvider Map(DatabaseCommentProvider source)
    {
        var identifierDefaultsMapper = MapperRegistry.GetMapper<Dto.IdentifierDefaults, IIdentifierDefaults>();
        var tableCommentsMapper = MapperRegistry.GetMapper<DatabaseTableComments, IRelationalDatabaseTableComments>();
        var viewCommentsMapper = MapperRegistry.GetMapper<Dto.Comments.DatabaseViewComments, IDatabaseViewComments>();
        var sequenceCommentsMapper = MapperRegistry.GetMapper<Dto.Comments.DatabaseSequenceComments, IDatabaseSequenceComments>();
        var synonymCommentsMapper = MapperRegistry.GetMapper<Dto.Comments.DatabaseSynonymComments, IDatabaseSynonymComments>();
        var routineCommentsMapper = MapperRegistry.GetMapper<Dto.Comments.DatabaseRoutineComments, IDatabaseRoutineComments>();

        return new RelationalDatabaseCommentProvider(
            identifierDefaultsMapper.Map(source.IdentifierDefaults),
            source.IdentifierResolver ?? new VerbatimIdentifierResolutionStrategy(),
            tableCommentsMapper.MapList(source.TableComments),
            viewCommentsMapper.MapList(source.ViewComments),
            sequenceCommentsMapper.MapList(source.SequenceComments),
            synonymCommentsMapper.MapList(source.SynonymComments),
            routineCommentsMapper.MapList(source.RoutineComments)
        );
    }

    public async Task<DatabaseCommentProvider> MapAsync(IRelationalDatabaseCommentProvider source, CancellationToken cancellationToken)
    {
        var tableCommentMapper = MapperRegistry.GetMapper<IRelationalDatabaseTableComments, DatabaseTableComments>();
        var viewCommentMapper = MapperRegistry.GetMapper<IDatabaseViewComments, Dto.Comments.DatabaseViewComments>();
        var sequenceCommentMapper = MapperRegistry.GetMapper<IDatabaseSequenceComments, Dto.Comments.DatabaseSequenceComments>();
        var synonymCommentMapper = MapperRegistry.GetMapper<IDatabaseSynonymComments, Dto.Comments.DatabaseSynonymComments>();
        var routineCommentMapper = MapperRegistry.GetMapper<IDatabaseRoutineComments, Dto.Comments.DatabaseRoutineComments>();

        var tableCommentsTask = source.GetAllTableComments(cancellationToken)
            .Select(t => tableCommentMapper.Map<IRelationalDatabaseTableComments, DatabaseTableComments>(t))
            .ToListAsync(cancellationToken)
            .AsTask();
        var viewCommentsTask = source.GetAllViewComments(cancellationToken)
            .Select(v => viewCommentMapper.Map<IDatabaseViewComments, Dto.Comments.DatabaseViewComments>(v))
            .ToListAsync(cancellationToken)
            .AsTask();
        var sequenceCommentsTask = source.GetAllSequenceComments(cancellationToken)
            .Select(s => sequenceCommentMapper.Map<IDatabaseSequenceComments, Dto.Comments.DatabaseSequenceComments>(s))
            .ToListAsync(cancellationToken)
            .AsTask();
        var synonymCommentsTask = source.GetAllSynonymComments(cancellationToken)
            .Select(s => synonymCommentMapper.Map<IDatabaseSynonymComments, Dto.Comments.DatabaseSynonymComments>(s))
            .ToListAsync(cancellationToken)
            .AsTask();
        var routineCommentsTask = source.GetAllRoutineComments(cancellationToken)
            .Select(r => routineCommentMapper.Map<IDatabaseRoutineComments, Dto.Comments.DatabaseRoutineComments>(r))
            .ToListAsync(cancellationToken)
            .AsTask();

        await Task.WhenAll(tableCommentsTask, viewCommentsTask, sequenceCommentsTask, synonymCommentsTask, routineCommentsTask).ConfigureAwait(false);

        var identifierDefaultsMapper = MapperRegistry.GetMapper<IIdentifierDefaults, Dto.IdentifierDefaults>();
        var dtoIdentifierDefaults = identifierDefaultsMapper.Map<IIdentifierDefaults, Dto.IdentifierDefaults>(source.IdentifierDefaults);
        var dtoTableComments = await tableCommentsTask.ConfigureAwait(false);
        var dtoViewComments = await viewCommentsTask.ConfigureAwait(false);
        var dtoSequenceComments = await sequenceCommentsTask.ConfigureAwait(false);
        var dtoSynonymComments = await synonymCommentsTask.ConfigureAwait(false);
        var dtoRoutineComments = await routineCommentsTask.ConfigureAwait(false);

        return new DatabaseCommentProvider
        {
            IdentifierDefaults = dtoIdentifierDefaults,
            TableComments = dtoTableComments,
            ViewComments = dtoViewComments,
            SequenceComments = dtoSequenceComments,
            SynonymComments = dtoSynonymComments,
            RoutineComments = dtoRoutineComments
        };
    }
}