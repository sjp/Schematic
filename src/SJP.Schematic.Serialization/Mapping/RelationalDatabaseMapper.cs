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
            tables,
            views,
            sequences,
            synonyms,
            routines
        ) = await (
            source.GetAllTables(cancellationToken),
            source.GetAllViews(cancellationToken),
            source.GetAllSequences(cancellationToken),
            source.GetAllSynonyms(cancellationToken),
            source.GetAllRoutines(cancellationToken)
        ).WhenAll();

        var dtoTables = tables.Select(t => tableMapper.Map<IRelationalDatabaseTable, Dto.RelationalDatabaseTable>(t)).ToList();
        var dtoViews = views.Select(v => viewMapper.Map<IDatabaseView, Dto.DatabaseView>(v)).ToList();
        var dtoSequences = sequences.Select(s => sequenceMapper.Map<IDatabaseSequence, Dto.DatabaseSequence>(s)).ToList();
        var dtoSynonyms = synonyms.Select(s => synonymMapper.Map<IDatabaseSynonym, Dto.DatabaseSynonym>(s)).ToList();
        var dtoRoutines = routines.Select(r => routineMapper.Map<IDatabaseRoutine, Dto.DatabaseRoutine>(r)).ToList();

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