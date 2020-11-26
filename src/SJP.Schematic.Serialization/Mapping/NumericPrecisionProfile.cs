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
                .ConstructUsing(static dto =>
                    dto == null
                        ? Option<INumericPrecision>.None
                        : Option<INumericPrecision>.Some(new NumericPrecision(dto.Precision, dto.Scale)))
                .ForAllMembers(static cfg => cfg.Ignore());

            CreateMap<Option<INumericPrecision>, Dto.NumericPrecision?>()
                .ConstructUsing(precision =>
                     precision.MatchUnsafe(
                        static p => new Dto.NumericPrecision { Precision = p.Precision, Scale = p.Scale },
                        static () => (Dto.NumericPrecision?)null
                    ))
                .ForAllMembers(static cfg => cfg.Ignore());
        }
    }
}
