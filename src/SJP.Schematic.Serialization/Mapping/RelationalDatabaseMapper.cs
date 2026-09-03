using System;
using System.Threading;
using System.Threading.Tasks;
using Boxed.Mapping;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Serialization.Mapping;

/// <summary>
/// Maps a database definition between its core and serialized representations.
/// </summary>
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
        var userDefinedTypeMapper = MapperRegistry.GetMapper<Dto.DatabaseUserDefinedType, IDatabaseUserDefinedType>();

        return new RelationalDatabase(
            identifierDefaultsMapper.Map(source.IdentifierDefaults),
            identifierResolver,
            tableMapper.MapList(source.Tables),
            viewMapper.MapList(source.Views),
            sequenceMapper.MapList(source.Sequences),
            synonymMapper.MapList(source.Synonyms),
            routineMapper.MapList(source.Routines),
            userDefinedTypeMapper.MapList(source.UserDefinedTypes)
        );
    }

    /// <summary>
    /// Maps a database to a serialized database definition.
    /// </summary>
    /// <param name="source">A database.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A serialized database definition.</returns>
    public async Task<Dto.RelationalDatabase> MapAsync(IRelationalDatabase source, CancellationToken cancellationToken)
    {
        var tableMapper = MapperRegistry.GetMapper<IRelationalDatabaseTable, Dto.RelationalDatabaseTable>();
        var viewMapper = MapperRegistry.GetMapper<IDatabaseView, Dto.DatabaseView>();
        var sequenceMapper = MapperRegistry.GetMapper<IDatabaseSequence, Dto.DatabaseSequence>();
        var synonymMapper = MapperRegistry.GetMapper<IDatabaseSynonym, Dto.DatabaseSynonym>();
        var routineMapper = MapperRegistry.GetMapper<IDatabaseRoutine, Dto.DatabaseRoutine>();
        var userDefinedTypeMapper = MapperRegistry.GetMapper<IDatabaseUserDefinedType, Dto.DatabaseUserDefinedType>();

        var (
            tables,
            views,
            sequences,
            synonyms,
            routines,
            userDefinedTypes
        ) = await (
            source.GetAllTables(cancellationToken),
            source.GetAllViews(cancellationToken),
            source.GetAllSequences(cancellationToken),
            source.GetAllSynonyms(cancellationToken),
            source.GetAllRoutines(cancellationToken),
            source.GetAllUserDefinedTypes(cancellationToken)
        ).WhenAll();

        var identifierDefaultsMapper = MapperRegistry.GetMapper<IIdentifierDefaults, Dto.IdentifierDefaults>();

        return new Dto.RelationalDatabase
        {
            IdentifierDefaults = identifierDefaultsMapper.Map(source.IdentifierDefaults),
            Tables = tableMapper.MapList(tables),
            Views = viewMapper.MapList(views),
            Sequences = sequenceMapper.MapList(sequences),
            Synonyms = synonymMapper.MapList(synonyms),
            Routines = routineMapper.MapList(routines),
            UserDefinedTypes = userDefinedTypeMapper.MapList(userDefinedTypes),
        };
    }
}