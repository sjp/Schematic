using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LanguageExt;
using SJP.Schematic.Core;

namespace SJP.Schematic.Serialization.Mapping
{
    internal static class RelationalDatabaseMappingExtensions
    {
        public static Task<Dto.RelationalDatabase> ToDto(this IRelationalDatabase database, CancellationToken cancellationToken = default)
        {
            if (database == null)
                throw new ArgumentNullException(nameof(database));

            return ToDtoCore(database, cancellationToken);
        }

        private static async Task<Dto.RelationalDatabase> ToDtoCore(this IRelationalDatabase database, CancellationToken cancellationToken = default)
        {
            var sequences = await database.GetAllSequences(cancellationToken).ConfigureAwait(false);
            var synonyms = await database.GetAllSynonyms(cancellationToken).ConfigureAwait(false);
            var tables = await database.GetAllTables(cancellationToken).ConfigureAwait(false);
            var views = await database.GetAllViews(cancellationToken).ConfigureAwait(false);
            var routines = await database.GetAllRoutines(cancellationToken).ConfigureAwait(false);

            var sequenceDtos = sequences.Select(s => s.ToDto()).ToList();
            var synonymDtos = synonyms.Select(s => s.ToDto()).ToList();
            var tableDtos = tables.Select(t => t.ToDto()).ToList();
            var viewDtos = views.Select(v => v.ToDto()).ToList();
            var routineDtos = routines.Select(r => r.ToDto()).ToList();

            return new Dto.RelationalDatabase
            {
                Tables = tableDtos,
                Views = viewDtos,
                Sequences = sequenceDtos,
                Synonyms = synonymDtos,
                Routines = routineDtos
            };
        }

        public static IRelationalDatabase FromDto(this Dto.RelationalDatabase dto)
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

            var sequences = (dto.Sequences ?? Array.Empty<Dto.DatabaseSequence>()).Select(s => s.FromDto()).ToList();
            var synonyms = (dto.Synonyms ?? Array.Empty<Dto.DatabaseSynonym>()).Select(s => s.FromDto()).ToList();
            var tables = (dto.Tables ?? Array.Empty<Dto.RelationalDatabaseTable>()).Select(t => t.FromDto()).ToList();
            var views = (dto.Views ?? Array.Empty<Dto.DatabaseView>()).Select(v => v.FromDto()).ToList();
            var routines = (dto.Routines ?? Array.Empty<Dto.DatabaseRoutine>()).Select(r => r.FromDto()).ToList();

            return new RelationalDatabase
            {
                Sequences = sequences,
                Synonyms = synonyms,
                Tables = tables,
                Views = views,
                Routines = routines
            };
        }
    }
}
