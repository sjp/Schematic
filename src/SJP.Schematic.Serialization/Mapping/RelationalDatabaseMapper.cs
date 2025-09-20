using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Boxed.Mapping;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Serialization.Mapping;

public class RelationalDatabaseMapper
    : IImmutableMapper<Dto.RelationalDatabase, IRelationalDatabase>
    , IAsyncImmutableMapper<IRelationalDatabase, Dto.RelationalDatabase>
{
    public IRelationalDatabase Map(Dto.RelationalDatabase source)
    {
        var identifierDefaultsMapper = MapperRegistry.GetMapper<Dto.IdentifierDefaults, IIdentifierDefaults>();
        var tableMapper = MapperRegistry.GetMapper<Dto.RelationalDatabaseTable, IRelationalDatabaseTable>();
        var viewMapper = MapperRegistry.GetMapper<Dto.DatabaseView, IDatabaseView>();
        var sequenceMapper = MapperRegistry.GetMapper<Dto.DatabaseSequence, IDatabaseSequence>();
        var synonymMapper = MapperRegistry.GetMapper<Dto.DatabaseSynonym, IDatabaseSynonym>();
        var routineMapper = MapperRegistry.GetMapper<Dto.DatabaseRoutine, IDatabaseRoutine>();

        return new RelationalDatabase(
            identifierDefaultsMapper.Map(source.IdentifierDefaults),
            source.IdentifierResolver ?? new VerbatimIdentifierResolutionStrategy(),
            tableMapper.MapList(source.Tables),
            viewMapper.MapList(source.Views),
            sequenceMapper.MapList(source.Sequences),
            synonymMapper.MapList(source.Synonyms),
            routineMapper.MapList(source.Routines)
        );
    }

    public async Task<Dto.RelationalDatabase> MapAsync(IRelationalDatabase source, CancellationToken cancellationToken)
    {
        var tableMapper = MapperRegistry.GetMapper<IRelationalDatabaseTable, Dto.RelationalDatabaseTable>();
        var viewMapper = MapperRegistry.GetMapper<IDatabaseView, Dto.DatabaseView>();
        var sequenceMapper = MapperRegistry.GetMapper<IDatabaseSequence, Dto.DatabaseSequence>();
        var synonymMapper = MapperRegistry.GetMapper<IDatabaseSynonym, Dto.DatabaseSynonym>();
        var routineMapper = MapperRegistry.GetMapper<IDatabaseRoutine, Dto.DatabaseRoutine>();

        var (
            dtoTables,
            dtoViews,
            dtoSequences,
            dtoSynonyms,
            dtoRoutines
        ) = await (
            source.GetAllTables(cancellationToken)
                .Select(t => tableMapper.Map<IRelationalDatabaseTable, Dto.RelationalDatabaseTable>(t))
                .ToListAsync(cancellationToken)
                .AsTask(),
            source.EnumerateAllViews(cancellationToken)
                .Select(v => viewMapper.Map<IDatabaseView, Dto.DatabaseView>(v))
                .ToListAsync(cancellationToken)
                .AsTask(),
            source.EnumerateAllSequences(cancellationToken)
                .Select(s => sequenceMapper.Map<IDatabaseSequence, Dto.DatabaseSequence>(s))
                .ToListAsync(cancellationToken)
                .AsTask(),
            source.EnumerateAllSynonyms(cancellationToken)
                .Select(s => synonymMapper.Map<IDatabaseSynonym, Dto.DatabaseSynonym>(s))
                .ToListAsync(cancellationToken)
                .AsTask(),
            source.EnumerateAllRoutines(cancellationToken)
                .Select(r => routineMapper.Map<IDatabaseRoutine, Dto.DatabaseRoutine>(r))
                .ToListAsync(cancellationToken)
                .AsTask()
        ).WhenAll().ConfigureAwait(false);

        var identifierDefaultsMapper = MapperRegistry.GetMapper<IIdentifierDefaults, Dto.IdentifierDefaults>();
        var dtoIdentifierDefaults = identifierDefaultsMapper.Map<IIdentifierDefaults, Dto.IdentifierDefaults>(source.IdentifierDefaults);

        return new Dto.RelationalDatabase
        {
            IdentifierDefaults = dtoIdentifierDefaults,
            Tables = dtoTables,
            Views = dtoViews,
            Sequences = dtoSequences,
            Synonyms = dtoSynonyms,
            Routines = dtoRoutines,
        };
    }
}