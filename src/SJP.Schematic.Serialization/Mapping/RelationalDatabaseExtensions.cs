using System;
using System.Collections.Generic;
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
            var tablesTask = database.GetAllTables(cancellationToken).ToListAsync(cancellationToken).AsTask();
            var viewsTask = database.GetAllViews(cancellationToken).ToListAsync(cancellationToken).AsTask();
            var sequencesTask = database.GetAllSequences(cancellationToken).ToListAsync(cancellationToken).AsTask();
            var synonymsTask = database.GetAllSynonyms(cancellationToken).ToListAsync(cancellationToken).AsTask();
            var routinesTask = database.GetAllRoutines(cancellationToken).ToListAsync(cancellationToken).AsTask();

            await Task.WhenAll(tablesTask, viewsTask, sequencesTask, synonymsTask, routinesTask).ConfigureAwait(false);

            var tables = await tablesTask.ConfigureAwait(false);
            var views = await viewsTask.ConfigureAwait(false);
            var sequences = await sequencesTask.ConfigureAwait(false);
            var synonyms = await synonymsTask.ConfigureAwait(false);
            var routines = await routinesTask.ConfigureAwait(false);

            var dtoIdentifierDefaults = mapper.Map<IIdentifierDefaults, Dto.IdentifierDefaults>(database.IdentifierDefaults);
            var dtoTables = mapper.Map<IEnumerable<IRelationalDatabaseTable>, IEnumerable<Dto.RelationalDatabaseTable>>(tables);
            var dtoViews = mapper.Map<IEnumerable<IDatabaseView>, IEnumerable<Dto.DatabaseView>>(views);
            var dtoSequences = mapper.Map<IEnumerable<IDatabaseSequence>, IEnumerable<Dto.DatabaseSequence>>(sequences);
            var dtoSynonyms = mapper.Map<IEnumerable<IDatabaseSynonym>, IEnumerable<Dto.DatabaseSynonym>>(synonyms);
            var dtoRoutines = mapper.Map<IEnumerable<IDatabaseRoutine>, IEnumerable<Dto.DatabaseRoutine>>(routines);

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
