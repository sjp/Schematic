using Boxed.Mapping;
using LanguageExt;
using SJP.Schematic.Core;

namespace SJP.Schematic.Serialization.Mapping;

/// <summary>
/// Maps a check constraint between its core and serialized representations.
/// </summary>
public class DatabaseCheckMapper
    : IImmutableMapper<Dto.DatabaseCheckConstraint, IDatabaseCheckConstraint>
    , IImmutableMapper<IDatabaseCheckConstraint, Dto.DatabaseCheckConstraint>
{
    /// <summary>
    /// Maps a serialized check constraint to its core representation.
    /// </summary>
    /// <param name="source">A serialized check constraint.</param>
    /// <returns>A check constraint.</returns>
    public IDatabaseCheckConstraint Map(Dto.DatabaseCheckConstraint source)
    {
        var identifierMapper = MapperRegistry.GetMapper<Dto.Identifier?, Option<Identifier>>();
        return new DatabaseCheckConstraint(
            identifierMapper.Map(source.CheckName),
            source.Definition,
            source.IsEnabled
        );
    }

    /// <summary>
    /// Maps a check constraint to its serialized representation.
    /// </summary>
    /// <param name="source">A check constraint.</param>
    /// <returns>A serialized check constraint.</returns>
    public Dto.DatabaseCheckConstraint Map(IDatabaseCheckConstraint source)
    {
        var identifierMapper = MapperRegistry.GetMapper<Option<Identifier>, Dto.Identifier?>();
        return new Dto.DatabaseCheckConstraint
        {
            CheckName = identifierMapper.Map(source.Name),
            Definition = source.Definition,
            IsEnabled = source.IsEnabled,
        };
    }
}