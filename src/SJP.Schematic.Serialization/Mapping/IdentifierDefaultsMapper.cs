using Boxed.Mapping;
using SJP.Schematic.Core;

namespace SJP.Schematic.Serialization.Mapping;

public class IdentifierDefaultsMapper
    : IImmutableMapper<Dto.IdentifierDefaults, IIdentifierDefaults>
    , IImmutableMapper<IIdentifierDefaults, Dto.IdentifierDefaults>
{
    public IIdentifierDefaults Map(Dto.IdentifierDefaults source)
    {
        return new IdentifierDefaults(
            source.Server,
            source.Database,
            source.Schema
        );
    }

    public Dto.IdentifierDefaults Map(IIdentifierDefaults source)
    {
        return new Dto.IdentifierDefaults
        {
            Server = source.Server,
            Database = source.Database,
            Schema = source.Schema
        };
    }
}