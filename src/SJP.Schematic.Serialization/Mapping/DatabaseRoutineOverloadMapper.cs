using System.Linq;
using Boxed.Mapping;
using LanguageExt;
using SJP.Schematic.Core;

namespace SJP.Schematic.Serialization.Mapping;

/// <summary>
/// Maps a routine overload between its core and serialized representations.
/// </summary>
public class DatabaseRoutineOverloadMapper
    : IImmutableMapper<Dto.DatabaseRoutineOverload, IDatabaseRoutineOverload>
    , IImmutableMapper<IDatabaseRoutineOverload, Dto.DatabaseRoutineOverload>
{
    /// <summary>
    /// Maps a serialized routine overload to its core representation.
    /// </summary>
    /// <param name="source">A serialized routine overload.</param>
    /// <returns>A routine overload.</returns>
    public IDatabaseRoutineOverload Map(Dto.DatabaseRoutineOverload source)
    {
        var parameterMapper = MapperRegistry.GetMapper<Dto.DatabaseRoutineParameter, IDatabaseRoutineParameter>();
        var returnTypeMapper = MapperRegistry.GetMapper<Dto.DbType?, Option<IDbType>>();

        return new DatabaseRoutineOverload(
            source.Definition,
            source.Parameters.Select(parameterMapper.Map).ToList(),
            returnTypeMapper.Map(source.ReturnType)
        );
    }

    /// <summary>
    /// Maps a routine overload to its serialized representation.
    /// </summary>
    /// <param name="source">A routine overload.</param>
    /// <returns>A serialized routine overload.</returns>
    public Dto.DatabaseRoutineOverload Map(IDatabaseRoutineOverload source)
    {
        var parameterMapper = MapperRegistry.GetMapper<IDatabaseRoutineParameter, Dto.DatabaseRoutineParameter>();
        var returnTypeMapper = MapperRegistry.GetMapper<Option<IDbType>, Dto.DbType?>();

        return new Dto.DatabaseRoutineOverload
        {
            Definition = source.Definition,
            Parameters = source.Parameters.Select(parameterMapper.Map).ToList(),
            ReturnType = returnTypeMapper.Map(source.ReturnType),
        };
    }
}
