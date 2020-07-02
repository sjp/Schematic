using AutoMapper;
using LanguageExt;
using SJP.Schematic.Core;

namespace SJP.Schematic.Serialization.Mapping
{
    public class NumericPrecisionProfile : Profile
    {
        public NumericPrecisionProfile()
        {
            CreateMap<Dto.NumericPrecision?, Option<INumericPrecision>>()
                .ConstructUsing(dto =>
                    dto == null
                        ? Option<INumericPrecision>.None
                        : Option<INumericPrecision>.Some(new NumericPrecision(dto.Precision, dto.Scale)))
                .ForAllMembers(cfg => cfg.Ignore());

            CreateMap<Option<INumericPrecision>, Dto.NumericPrecision?>()
                .ConstructUsing(precision =>
                     precision.MatchUnsafe(
                        p => new Dto.NumericPrecision { Precision = p.Precision, Scale = p.Scale },
                        () => (Dto.NumericPrecision?)null
                    ))
                .ForAllMembers(cfg => cfg.Ignore());
        }
    }
}
