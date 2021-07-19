using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using SJP.Schematic.Core;

namespace SJP.Schematic.Serialization.Mapping
{
    internal static class RelationalDatabaseExtensions
    {
        public static Task<Dto.RelationalDatabase> ToDtoAsync(this IMapper mapper, IRelationalDatabase database, CancellationToken cancellationToken = default)
        {
            if (mapper == null)
                throw new ArgumentNullException(nameof(mapper));
            if (database == null)
                throw new ArgumentNullException(nameof(database));

            return ToDtoAsyncCore(mapper, database, cancellationToken);
        }

        private static async Task<Dto.RelationalDatabase> ToDtoAsyncCore(IMapper mapper, IRelationalDatabase database, CancellationToken cancellationToken)
        {
            var tablesTask = database.GetAllTables(cancellationToken)
                .Select(t => mapper.Map<IRelationalDatabaseTable, Dto.RelationalDatabaseTable>(t))
                .ToListAsync(cancellationToken)
                .AsTask();
            var viewsTask = database.GetAllViews(cancellationToken)
                .Select(v => mapper.Map<IDatabaseView, Dto.DatabaseView>(v))
                .ToListAsync(cancellationToken)
                .AsTask();
            var sequencesTask = database.GetAllSequences(cancellationToken)
                .Select(s => mapper.Map<IDatabaseSequence, Dto.DatabaseSequence>(s))
                .ToListAsync(cancellationToken)
                .AsTask();
            var synonymsTask = database.GetAllSynonyms(cancellationToken)
                .Select(s => mapper.Map<IDatabaseSynonym, Dto.DatabaseSynonym>(s))
                .ToListAsync(cancellationToken)
                .AsTask();
            var routinesTask = database.GetAllRoutines(cancellationToken)
                .Select(r => mapper.Map<IDatabaseRoutine, Dto.DatabaseRoutine>(r))
                .ToListAsync(cancellationToken)
                .AsTask();

            await Task.WhenAll(tablesTask, viewsTask, sequencesTask, synonymsTask, routinesTask).ConfigureAwait(false);

            var dtoIdentifierDefaults = mapper.Map<IIdentifierDefaults, Dto.IdentifierDefaults>(database.IdentifierDefaults);
            var dtoTables = await tablesTask.ConfigureAwait(false);
            var dtoViews = await viewsTask.ConfigureAwait(false);
            var dtoSequences = await sequencesTask.ConfigureAwait(false);
            var dtoSynonyms = await synonymsTask.ConfigureAwait(false);
            var dtoRoutines = await routinesTask.ConfigureAwait(false);

            return new Dto.RelationalDatabase
            {
                IdentifierDefaults = dtoIdentifierDefaults,
                Tables = dtoTables,
                Views = dtoViews,
                Sequences = dtoSequences,
                Synonyms = dtoSynonyms,
                Routines = dtoRoutines
            };
        }
    }
}
