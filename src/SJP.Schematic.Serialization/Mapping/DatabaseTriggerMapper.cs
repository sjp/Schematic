using Boxed.Mapping;
using SJP.Schematic.Core;

namespace SJP.Schematic.Serialization.Mapping;

public class DatabaseTriggerMapper
    : IImmutableMapper<Dto.DatabaseTrigger, IDatabaseTrigger>
    , IImmutableMapper<IDatabaseTrigger, Dto.DatabaseTrigger>
{
    public IDatabaseTrigger Map(Dto.DatabaseTrigger source)
    {
        var identifierMapper = MapperRegistry.GetMapper<Dto.Identifier, Identifier>();

        return new DatabaseTrigger(
            identifierMapper.Map(source.TriggerName!),
            source.Definition!,
            source.QueryTiming,
            source.TriggerEvent,
            source.IsEnabled
        );
    }

    public Dto.DatabaseTrigger Map(IDatabaseTrigger source)
    {
        var identifierMapper = MapperRegistry.GetMapper<Identifier, Dto.Identifier>();

        return new Dto.DatabaseTrigger
        {
            TriggerName = identifierMapper.Map(source.Name),
            Definition = source.Definition,
            QueryTiming = source.QueryTiming,
            TriggerEvent = source.TriggerEvent,
            IsEnabled = source.IsEnabled,
        };
    }
}