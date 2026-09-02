using Boxed.Mapping;
using LanguageExt;
using SJP.Schematic.Core;

namespace SJP.Schematic.Serialization.Mapping;

/// <summary>
/// Maps a routine parameter between its core and serialized representations.
/// </summary>
public class DatabaseRoutineParameterMapper
    : IImmutableMapper<Dto.DatabaseRoutineParameter, IDatabaseRoutineParameter>
    , IImmutableMapper<IDatabaseRoutineParameter, Dto.DatabaseRoutineParameter>
{
    /// <summary>
    /// Maps a serialized routine parameter to its core representation.
    /// </summary>
    /// <param name="source">A serialized routine parameter.</param>
    /// <returns>A routine parameter.</returns>
    public IDatabaseRoutineParameter Map(Dto.DatabaseRoutineParameter source)
    {
        var identifierMapper = MapperRegistry.GetMapper<Dto.Identifier?, Option<Identifier>>();
        var dbTypeMapper = MapperRegistry.GetMapper<Dto.DbType, IDbType>();
        var optionalMapper = MapperRegistry.GetMapper<string?, Option<string>>();

        return new DatabaseRoutineParameter(
            identifierMapper.Map(source.ParameterName),
            dbTypeMapper.Map(source.Type),
            source.Direction,
            optionalMapper.Map(source.DefaultValue),
            source.Ordinal
        );
    }

    /// <summary>
    /// Maps a routine parameter to its serialized representation.
    /// </summary>
    /// <param name="source">A routine parameter.</param>
    /// <returns>A serialized routine parameter.</returns>
    public Dto.DatabaseRoutineParameter Map(IDatabaseRoutineParameter source)
    {
        var identifierMapper = MapperRegistry.GetMapper<Option<Identifier>, Dto.Identifier?>();
        var dbTypeMapper = MapperRegistry.GetMapper<IDbType, Dto.DbType>();
        var optionalMapper = MapperRegistry.GetMapper<Option<string>, string?>();

        return new Dto.DatabaseRoutineParameter
        {
            ParameterName = identifierMapper.Map(source.Name),
            Type = dbTypeMapper.Map(source.Type),
            Direction = source.Direction,
            DefaultValue = optionalMapper.Map(source.DefaultValue),
            Ordinal = source.Ordinal,
        };
    }
}
