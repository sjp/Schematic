using Boxed.Mapping;
using LanguageExt;
using SJP.Schematic.Core;

namespace SJP.Schematic.Serialization.Mapping;

/// <summary>
/// Maps a database schema between its core and serialized representations.
/// </summary>
public class DatabaseSchemaMapper
    : IImmutableMapper<Dto.DatabaseSchema, IDatabaseSchema>
    , IImmutableMapper<IDatabaseSchema, Dto.DatabaseSchema>
{
    /// <summary>
    /// Maps a schema to its serialized representation.
    /// </summary>
    /// <param name="source">A schema.</param>
    /// <returns>A serialized schema.</returns>
    public Dto.DatabaseSchema Map(IDatabaseSchema source)
    {
        var identifierMapper = MapperRegistry.GetMapper<Identifier, Dto.Identifier>();
        var optionMapper = MapperRegistry.GetMapper<Option<string>, string?>();

        return new Dto.DatabaseSchema
        {
            SchemaName = identifierMapper.Map(source.Name),
            Owner = optionMapper.Map(source.Owner),
            IsDefault = source.IsDefault,
            IsSystem = source.IsSystem,
        };
    }

    /// <summary>
    /// Maps a serialized schema to its core representation.
    /// </summary>
    /// <param name="source">A serialized schema.</param>
    /// <returns>A schema.</returns>
    public IDatabaseSchema Map(Dto.DatabaseSchema source)
    {
        var identifierMapper = MapperRegistry.GetMapper<Dto.Identifier, Identifier>();
        var optionMapper = MapperRegistry.GetMapper<string?, Option<string>>();

        return new DatabaseSchema(
            identifierMapper.Map(source.SchemaName),
            optionMapper.Map(source.Owner),
            source.IsDefault,
            source.IsSystem
        );
    }
}
