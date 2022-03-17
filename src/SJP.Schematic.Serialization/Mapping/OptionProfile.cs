using AutoMapper;
using LanguageExt;

namespace SJP.Schematic.Serialization.Mapping;

public class OptionProfile : Profile
{
    public OptionProfile()
    {
        CreateMap<Option<string>, string?>()
            .ConstructUsing(static opt => opt.MatchUnsafe(static v => v, (string?)null))
            .ForAllMembers(static cfg => cfg.Ignore());
        CreateMap<string?, Option<string>>()
            .ConstructUsing(static val => val == null ? Option<string>.None : Option<string>.Some(val))
            .ForAllMembers(static cfg => cfg.Ignore());

        CreateMap<Option<decimal>, decimal?>()
            .ConstructUsing(static opt => opt.MatchUnsafe(static v => v, (decimal?)null))
            .ForAllMembers(static cfg => cfg.Ignore());
        CreateMap<decimal?, Option<decimal>>()
            .ConstructUsing(static val => val.ToOption())
            .ForAllMembers(static cfg => cfg.Ignore());
    }
}
