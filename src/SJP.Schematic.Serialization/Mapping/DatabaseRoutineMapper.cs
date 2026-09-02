using System.Linq;
using Boxed.Mapping;
using LanguageExt;
using SJP.Schematic.Core;

namespace SJP.Schematic.Serialization.Mapping;

/// <summary>
/// Maps a database routine between its core and serialized representations.
/// </summary>
public class DatabaseRoutineMapper
    : IImmutableMapper<Dto.DatabaseRoutine, IDatabaseRoutine>
    , IImmutableMapper<IDatabaseRoutine, Dto.DatabaseRoutine>
{
    /// <summary>
    /// Maps a serialized routine to its core representation.
    /// </summary>
    /// <param name="source">A serialized routine.</param>
    /// <returns>A routine.</returns>
    public IDatabaseRoutine Map(Dto.DatabaseRoutine source)
    {
        var identifierMapper = MapperRegistry.GetMapper<Dto.Identifier, Identifier>();
        var optionalMapper = MapperRegistry.GetMapper<string?, Option<string>>();
        var parameterMapper = MapperRegistry.GetMapper<Dto.DatabaseRoutineParameter, IDatabaseRoutineParameter>();
        var overloadMapper = MapperRegistry.GetMapper<Dto.DatabaseRoutineOverload, IDatabaseRoutineOverload>();
        var returnTypeMapper = MapperRegistry.GetMapper<Dto.DbType?, Option<IDbType>>();

        return new DatabaseRoutine(
            identifierMapper.Map(source.RoutineName),
            source.Definition,
            source.RoutineType,
            optionalMapper.Map(source.Language),
            source.Parameters.Select(parameterMapper.Map).ToList(),
            returnTypeMapper.Map(source.ReturnType),
            source.Overloads.Select(overloadMapper.Map).ToList()
        );
    }

    /// <summary>
    /// Maps a routine to its serialized representation.
    /// </summary>
    /// <param name="source">A routine.</param>
    /// <returns>A serialized routine.</returns>
    public Dto.DatabaseRoutine Map(IDatabaseRoutine source)
    {
        var identifierMapper = MapperRegistry.GetMapper<Identifier, Dto.Identifier>();
        var optionalMapper = MapperRegistry.GetMapper<Option<string>, string?>();
        var parameterMapper = MapperRegistry.GetMapper<IDatabaseRoutineParameter, Dto.DatabaseRoutineParameter>();
        var overloadMapper = MapperRegistry.GetMapper<IDatabaseRoutineOverload, Dto.DatabaseRoutineOverload>();
        var returnTypeMapper = MapperRegistry.GetMapper<Option<IDbType>, Dto.DbType?>();

        return new Dto.DatabaseRoutine
        {
            RoutineName = identifierMapper.Map(source.Name),
            Definition = source.Definition,
            RoutineType = source.RoutineType,
            Language = optionalMapper.Map(source.Language),
            Parameters = source.Parameters.Select(parameterMapper.Map).ToList(),
            ReturnType = returnTypeMapper.Map(source.ReturnType),
            Overloads = source.Overloads.Select(overloadMapper.Map).ToList(),
        };
    }
}
