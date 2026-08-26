using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Boxed.Mapping;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Serialization.Mapping;

public class RelationalDatabaseMapper
    : IAsyncImmutableMapper<IRelationalDatabase, Dto.RelationalDatabase>
{
    /// <summary>
    /// Maps a serialized database definition to a database.
    /// </summary>
    /// <param name="source">A serialized database definition.</param>
    /// <param name="identifierResolver">An identifier resolver used by the resulting database to look up objects.</param>
    /// <returns>A database.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="identifierResolver"/> is <c>null</c>.</exception>
    public IRelationalDatabase Map(Dto.RelationalDatabase source, IIdentifierResolutionStrategy identifierResolver)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(identifierResolver);

        var identifierDefaultsMapper = MapperRegistry.GetMapper<Dto.IdentifierDefaults, IIdentifierDefaults>();
        var tableMapper = MapperRegistry.GetMapper<Dto.RelationalDatabaseTable, IRelationalDatabaseTable>();
        var viewMapper = MapperRegistry.GetMapper<Dto.DatabaseView, IDatabaseView>();
        var sequenceMapper = MapperRegistry.GetMapper<Dto.DatabaseSequence, IDatabaseSequence>();
        var synonymMapper = MapperRegistry.GetMapper<Dto.DatabaseSynonym, IDatabaseSynonym>();
        var routineMapper = MapperRegistry.GetMapper<Dto.DatabaseRoutine, IDatabaseRoutine>();

        return new RelationalDatabase(
            identifierDefaultsMapper.Map(source.IdentifierDefaults),
            identifierResolver,
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