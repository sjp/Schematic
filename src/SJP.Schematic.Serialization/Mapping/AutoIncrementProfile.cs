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
                .ConstructUsing(static dto =>
                    dto == null
                        ? Option<IAutoIncrement>.None
                        : Option<IAutoIncrement>.Some(new AutoIncrement(dto.InitialValue, dto.Increment))
                )
                .ForAllMembers(static cfg => cfg.Ignore());

            CreateMap<Option<IAutoIncrement>, Dto.AutoIncrement?>()
                .ConstructUsing(static ai =>
                    ai.MatchUnsafe(
                        static incr => new Dto.AutoIncrement
                        {
                            Increment = incr.Increment,
                            InitialValue = incr.InitialValue
                        },
                        static () => (Dto.AutoIncrement?)null
                    )
                )
                .ForAllMembers(static cfg => cfg.Ignore());
        }
    }
}
