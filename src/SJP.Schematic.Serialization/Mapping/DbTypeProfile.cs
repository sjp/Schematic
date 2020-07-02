using System;
using AutoMapper;
using LanguageExt;
using SJP.Schematic.Core;

namespace SJP.Schematic.Serialization.Mapping
{
    public class DbTypeProfile : Profile
    {
        public DbTypeProfile()
        {
            CreateMap<Dto.DbType, ColumnDataType>()
                .ConstructUsing((dto, ctx) => new ColumnDataType(
                    ctx.Mapper.Map<Dto.Identifier, Identifier>(dto.TypeName!),
                    dto.DataType,
                    dto.Definition!,
                    Type.GetType(dto.ClrTypeName ?? "System.Object"),
                    dto.IsFixedLength,
                    dto.MaxLength,
                    ctx.Mapper.Map<Dto.NumericPrecision?, Option<INumericPrecision>>(dto.NumericPrecision),
                    ctx.Mapper.Map<Dto.Identifier?, Option<Identifier>>(dto.Collation)
                ))
                .ForAllMembers(cfg => cfg.Ignore());

            CreateMap<IDbType, Dto.DbType>()
                .ForMember(dest => dest.ClrTypeName, src => src.MapFrom(dbType => dbType.ClrType.ToString()));
        }
    }
}
