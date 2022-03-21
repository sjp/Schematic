using System;
using Boxed.Mapping;
using LanguageExt;
using SJP.Schematic.Core;

namespace SJP.Schematic.Serialization.Mapping;

public class AutoIncrementMapper
    : IImmutableMapper<Dto.AutoIncrement?, Option<IAutoIncrement>>
    , IImmutableMapper<Option<IAutoIncrement>, Dto.AutoIncrement?>
{
    public Option<IAutoIncrement> Map(Dto.AutoIncrement? source)
    {
        return source == null
            ? Option<IAutoIncrement>.None
            : Option<IAutoIncrement>.Some(new AutoIncrement(source.InitialValue, source.Increment));
    }

    public Dto.AutoIncrement? Map(Option<IAutoIncrement> source)
    {
        ArgumentNullException.ThrowIfNull(source, nameof(source));

        return source.MatchUnsafe(
            static incr => new Dto.AutoIncrement
            {
                Increment = incr.Increment,
                InitialValue = incr.InitialValue
            },
            static () => (Dto.AutoIncrement?)null
        );
    }
}