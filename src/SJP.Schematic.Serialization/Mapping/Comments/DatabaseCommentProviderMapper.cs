using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Boxed.Mapping;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Comments;
using SJP.Schematic.Core.Extensions;
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
        var (
            tableComments,
            viewComments,
            sequenceComments,
            synonymComments,
            routineComments
        ) = await (
            source.GetAllTableComments2(cancellationToken),
            source.GetAllViewComments(cancellationToken),
            source.GetAllSequenceComments(cancellationToken),
            source.GetAllSynonymComments(cancellationToken),
            source.GetAllRoutineComments(cancellationToken)
        ).WhenAll().ConfigureAwait(false);

        var tableCommentMapper = MapperRegistry.GetMapper<IRelationalDatabaseTableComments, DatabaseTableComments>();
        var viewCommentMapper = MapperRegistry.GetMapper<IDatabaseViewComments, Dto.Comments.DatabaseViewComments>();
        var sequenceCommentMapper = MapperRegistry.GetMapper<IDatabaseSequenceComments, Dto.Comments.DatabaseSequenceComments>();
        var synonymCommentMapper = MapperRegistry.GetMapper<IDatabaseSynonymComments, Dto.Comments.DatabaseSynonymComments>();
        var routineCommentMapper = MapperRegistry.GetMapper<IDatabaseRoutineComments, Dto.Comments.DatabaseRoutineComments>();

        var dtoTableComments = tableComments
            .Select(t => tableCommentMapper.Map<IRelationalDatabaseTableComments, DatabaseTableComments>(t))
            .ToList();
        var dtoViewComments = viewComments
            .Select(v => viewCommentMapper.Map<IDatabaseViewComments, Dto.Comments.DatabaseViewComments>(v))
            .ToList();
        var dtoSequenceComments = sequenceComments
            .Select(s => sequenceCommentMapper.Map<IDatabaseSequenceComments, Dto.Comments.DatabaseSequenceComments>(s))
            .ToList();
        var dtoSynonymComments = synonymComments
            .Select(s => synonymCommentMapper.Map<IDatabaseSynonymComments, Dto.Comments.DatabaseSynonymComments>(s))
            .ToList();
        var dtoRoutineComments = routineComments
            .Select(r => routineCommentMapper.Map<IDatabaseRoutineComments, Dto.Comments.DatabaseRoutineComments>(r))
            .ToList();

        var identifierDefaultsMapper = MapperRegistry.GetMapper<IIdentifierDefaults, Dto.IdentifierDefaults>();
        var dtoIdentifierDefaults = identifierDefaultsMapper.Map<IIdentifierDefaults, Dto.IdentifierDefaults>(source.IdentifierDefaults);

        return new DatabaseCommentProvider
        {
            IdentifierDefaults = dtoIdentifierDefaults,
            TableComments = dtoTableComments,
            ViewComments = dtoViewComments,
            SequenceComments = dtoSequenceComments,
            SynonymComments = dtoSynonymComments,
            RoutineComments = dtoRoutineComments,
        };
    }
}