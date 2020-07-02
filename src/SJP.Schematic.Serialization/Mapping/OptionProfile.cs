using AutoMapper;
using LanguageExt;

namespace SJP.Schematic.Serialization.Mapping
{
    public class OptionProfile : Profile
    {
        public OptionProfile()
        {
            CreateMap<Option<string>, string?>()
                .ConstructUsing(opt => opt.MatchUnsafe(v => v, (string?)null))
                .ForAllMembers(cfg => cfg.Ignore());
            CreateMap<string?, Option<string>>()
                .ConstructUsing(val => val == null ? Option<string>.None : Option<string>.Some(val))
                .ForAllMembers(cfg => cfg.Ignore());

            CreateMap<Option<decimal>, decimal?>()
                .ConstructUsing(opt => opt.MatchUnsafe(v => v, (decimal?)null))
                .ForAllMembers(cfg => cfg.Ignore());
            CreateMap<decimal?, Option<decimal>>()
                .ConstructUsing(val => val.ToOption())
                .ForAllMembers(cfg => cfg.Ignore());
        }
    }
}
