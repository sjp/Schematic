using Boxed.Mapping;
using SJP.Schematic.Core;

namespace SJP.Schematic.Serialization.Mapping;

/// <summary>
/// Maps a database's identifier defaults between their core and serialized representations.
/// </summary>
public class IdentifierDefaultsMapper
    : IImmutableMapper<Dto.IdentifierDefaults, IIdentifierDefaults>
    , IImmutableMapper<IIdentifierDefaults, Dto.IdentifierDefaults>
{
    /// <summary>
    /// Maps serialized identifier defaults to their core representation.
    /// </summary>
    /// <param name="source">Serialized identifier defaults.</param>
    /// <returns>Identifier defaults.</returns>
    public IIdentifierDefaults Map(Dto.IdentifierDefaults source)
    {
        return new IdentifierDefaults(
            source.Server,
            source.Database,
            source.Schema
        );
    }

    /// <summary>
    /// Maps identifier defaults to their serialized representation.
    /// </summary>
    /// <param name="source">Identifier defaults.</param>
    /// <returns>Serialized identifier defaults.</returns>
    public Dto.IdentifierDefaults Map(IIdentifierDefaults source)
    {
        return new Dto.IdentifierDefaults
        {
            Server = source.Server,
            Database = source.Database,
            Schema = source.Schema,
        };
    }
}