using Boxed.Mapping;
using LanguageExt;
using SJP.Schematic.Core;

namespace SJP.Schematic.Serialization.Mapping;

public class DatabaseSequenceMapper
    : IImmutableMapper<Dto.DatabaseSequence, IDatabaseSequence>
    , IImmutableMapper<IDatabaseSequence, Dto.DatabaseSequence>
{
    public IDatabaseSequence Map(Dto.DatabaseSequence source)
    {
        var identifierMapper = MapperRegistry.GetMapper<Dto.Identifier, Identifier>();
        var decimalMapper = MapperRegistry.GetMapper<decimal?, Option<decimal>>();

        return new DatabaseSequence(
            identifierMapper.Map(source.SequenceName!),
            source.Start,
            source.Increment,
            decimalMapper.Map(source.MinValue),
            decimalMapper.Map(source.MaxValue),
            source.Cycle,
            source.Cache
        );
    }

    public Dto.DatabaseSequence Map(IDatabaseSequence source)
    {
        var identifierMapper = MapperRegistry.GetMapper<Identifier, Dto.Identifier>();
        var decimalMapper = MapperRegistry.GetMapper<Option<decimal>, decimal?>();

        return new Dto.DatabaseSequence
        {
            SequenceName = identifierMapper.Map(source.Name),
            Start = source.Start,
            Increment = source.Increment,
            MinValue = decimalMapper.Map(source.MinValue),
            MaxValue = decimalMapper.Map(source.MaxValue),
            Cycle = source.Cycle,
            Cache = source.Cache,
        };
    }
}