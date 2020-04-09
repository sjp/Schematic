using System;
using AutoMapper;
using SJP.Schematic.Core;

namespace SJP.Schematic.Serialization.Mapping
{
    public class DbTypeProfile : Profile
    {
        public DbTypeProfile()
        {
            CreateMap<Dto.DbType, ColumnDataType>()
                .ForMember(dest => dest.ClrType, src => src.MapFrom(dto => Type.GetType(dto.ClrTypeName)));

            CreateMap<IDbType, Dto.DbType>()
                .ForMember(dest => dest.ClrTypeName, src => src.MapFrom(dbType => dbType.ClrType.ToString()));
        }
    }
}
