using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Boxed.Mapping;
using SJP.Schematic.Core;

namespace SJP.Schematic.Serialization.Mapping;

public class RelationalDatabaseProfile
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

        var tablesTask = source.GetAllTables(cancellationToken)
            .Select(t => tableMapper.Map<IRelationalDatabaseTable, Dto.RelationalDatabaseTable>(t))
            .ToListAsync(cancellationToken)
            .AsTask();
        var viewsTask = source.GetAllViews(cancellationToken)
            .Select(v => viewMapper.Map<IDatabaseView, Dto.DatabaseView>(v))
            .ToListAsync(cancellationToken)
            .AsTask();
        var sequencesTask = source.GetAllSequences(cancellationToken)
            .Select(s => sequenceMapper.Map<IDatabaseSequence, Dto.DatabaseSequence>(s))
            .ToListAsync(cancellationToken)
            .AsTask();
        var synonymsTask = source.GetAllSynonyms(cancellationToken)
            .Select(s => synonymMapper.Map<IDatabaseSynonym, Dto.DatabaseSynonym>(s))
            .ToListAsync(cancellationToken)
            .AsTask();
        var routinesTask = source.GetAllRoutines(cancellationToken)
            .Select(r => routineMapper.Map<IDatabaseRoutine, Dto.DatabaseRoutine>(r))
            .ToListAsync(cancellationToken)
            .AsTask();

        await Task.WhenAll(tablesTask, viewsTask, sequencesTask, synonymsTask, routinesTask).ConfigureAwait(false);

        var identifierDefaultsMapper = MapperRegistry.GetMapper<IIdentifierDefaults, Dto.IdentifierDefaults>();
        var dtoIdentifierDefaults = identifierDefaultsMapper.Map<IIdentifierDefaults, Dto.IdentifierDefaults>(source.IdentifierDefaults);
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
