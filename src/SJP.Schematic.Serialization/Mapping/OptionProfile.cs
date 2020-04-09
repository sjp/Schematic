using AutoMapper;
using LanguageExt;

namespace SJP.Schematic.Serialization.Mapping
{
    public class OptionProfile : Profile
    {
        public OptionProfile()
        {
            CreateMap<Option<string>, string?>()
                .ConstructUsing(opt => opt.MatchUnsafe(v => v, (string?)null));
            CreateMap<string?, Option<string>>()
                .ConstructUsing(val => val == null ? Option<string>.None : Option<string>.Some(val));

            CreateMap<Option<decimal>, decimal?>()
                .ConstructUsing(opt => opt.MatchUnsafe(v => v, (decimal?)null));
            CreateMap<decimal?, Option<decimal>>()
                .ConstructUsing(val => val.ToOption());
        }
    }
}
