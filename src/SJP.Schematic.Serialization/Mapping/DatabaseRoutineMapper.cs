using Boxed.Mapping;
using SJP.Schematic.Core;

namespace SJP.Schematic.Serialization.Mapping;

public class DatabaseRoutineMapper
    : IImmutableMapper<Dto.DatabaseRoutine, IDatabaseRoutine>
    , IImmutableMapper<IDatabaseRoutine, Dto.DatabaseRoutine>
{
    public IDatabaseRoutine Map(Dto.DatabaseRoutine source)
    {
        var identifierMapper = MapperRegistry.GetMapper<Dto.Identifier, Identifier>();

        return new DatabaseRoutine(
            identifierMapper.Map(source.RoutineName!),
            source.Definition!
        );
    }

    public Dto.DatabaseRoutine Map(IDatabaseRoutine source)
    {
        var identifierMapper = MapperRegistry.GetMapper<Identifier, Dto.Identifier>();

        return new Dto.DatabaseRoutine
        {
            RoutineName = identifierMapper.Map(source.Name),
            Definition = source.Definition
        };
    }
}