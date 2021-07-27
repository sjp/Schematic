using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Comments;

namespace SJP.Schematic.Serialization.Mapping.Comments
{
    internal static class DatabaseCommentExtensions
    {
        public static Task<Dto.Comments.DatabaseCommentProvider> ToCommentsDtoAsync(this IMapper mapper, IRelationalDatabaseCommentProvider commentProvider, CancellationToken cancellationToken = default)
        {
            if (mapper == null)
                throw new ArgumentNullException(nameof(mapper));
            if (commentProvider == null)
                throw new ArgumentNullException(nameof(commentProvider));

            return ToCommentsDtoAsyncCore(mapper, commentProvider, cancellationToken);
        }

        private static async Task<Dto.Comments.DatabaseCommentProvider> ToCommentsDtoAsyncCore(IMapper mapper, IRelationalDatabaseCommentProvider commentProvider, CancellationToken cancellationToken)
        {
            var tableCommentsTask = commentProvider.GetAllTableComments(cancellationToken)
                .Select(t => mapper.Map<IRelationalDatabaseTableComments, Dto.Comments.DatabaseTableComments>(t))
                .ToListAsync(cancellationToken)
                .AsTask();
            var viewCommentsTask = commentProvider.GetAllViewComments(cancellationToken)
                .Select(v => mapper.Map<IDatabaseViewComments, Dto.Comments.DatabaseViewComments>(v))
                .ToListAsync(cancellationToken)
                .AsTask();
            var sequenceCommentsTask = commentProvider.GetAllSequenceComments(cancellationToken)
                .Select(s => mapper.Map<IDatabaseSequenceComments, Dto.Comments.DatabaseSequenceComments>(s))
                .ToListAsync(cancellationToken)
                .AsTask();
            var synonymCommentsTask = commentProvider.GetAllSynonymComments(cancellationToken)
                .Select(s => mapper.Map<IDatabaseSynonymComments, Dto.Comments.DatabaseSynonymComments>(s))
                .ToListAsync(cancellationToken)
                .AsTask();
            var routineCommentsTask = commentProvider.GetAllRoutineComments(cancellationToken)
                .Select(r => mapper.Map<IDatabaseRoutineComments, Dto.Comments.DatabaseRoutineComments>(r))
                .ToListAsync(cancellationToken)
                .AsTask();

            await Task.WhenAll(tableCommentsTask, viewCommentsTask, sequenceCommentsTask, synonymCommentsTask, routineCommentsTask).ConfigureAwait(false);

            var dtoIdentifierDefaults = mapper.Map<IIdentifierDefaults, Dto.IdentifierDefaults>(commentProvider.IdentifierDefaults);
            var dtoTableComments = await tableCommentsTask.ConfigureAwait(false);
            var dtoViewComments = await viewCommentsTask.ConfigureAwait(false);
            var dtoSequenceComments = await sequenceCommentsTask.ConfigureAwait(false);
            var dtoSynonymComments = await synonymCommentsTask.ConfigureAwait(false);
            var dtoRoutineComments = await routineCommentsTask.ConfigureAwait(false);

            return new Dto.Comments.DatabaseCommentProvider
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
}
