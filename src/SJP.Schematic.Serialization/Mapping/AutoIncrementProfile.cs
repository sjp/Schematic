using AutoMapper;
using LanguageExt;
using SJP.Schematic.Core;

namespace SJP.Schematic.Serialization.Mapping
{
    public class AutoIncrementProfile : Profile
    {
        public AutoIncrementProfile()
        {
            CreateMap<Dto.AutoIncrement?, Option<IAutoIncrement>>()
                .ConstructUsing(dto =>
                    dto == null
                        ? Option<IAutoIncrement>.None
                        : Option<IAutoIncrement>.Some(new AutoIncrement(dto.InitialValue, dto.Increment))
                );

            CreateMap<Option<IAutoIncrement>, Dto.AutoIncrement?>()
                .ConstructUsing(ai =>
                    ai.MatchUnsafe(
                        incr => new Dto.AutoIncrement
                        {
                            Increment = incr.Increment,
                            InitialValue = incr.InitialValue
                        },
                        () => (Dto.AutoIncrement?)null
                    )
                );
        }
    }
}
