using AutoMapper;
using LanguageExt;
using SJP.Schematic.Core;

namespace SJP.Schematic.Serialization.Mapping
{
    public class DatabaseColumnProfile : Profile
    {
        public DatabaseColumnProfile()
        {
            CreateMap<Dto.DatabaseColumn, DatabaseColumn>()
                .ConstructUsing((dto, ctx) => dto.IsComputed
                    ? new DatabaseComputedColumn(
                        ctx.Mapper.Map<Dto.Identifier, Identifier>(dto.ColumnName!),
                        ctx.Mapper.Map<Dto.DbType, ColumnDataType>(dto.Type!),
                        dto.IsNullable,
                        ctx.Mapper.Map<string?, Option<string>>(dto.DefaultValue),
                        ctx.Mapper.Map<string?, Option<string>>(dto.Definition)
                      )
                    : new DatabaseColumn(
                        ctx.Mapper.Map<Dto.Identifier, Identifier>(dto.ColumnName!),
                        ctx.Mapper.Map<Dto.DbType, ColumnDataType>(dto.Type!),
                        dto.IsNullable,
                        ctx.Mapper.Map<string?, Option<string>>(dto.DefaultValue),
                        ctx.Mapper.Map<Dto.AutoIncrement?, Option<IAutoIncrement>>(dto.AutoIncrement)
                      )
                );
            CreateMap<IDatabaseColumn, Dto.DatabaseColumn>()
                .ForMember(dto => dto.ColumnName, src => src.MapFrom(c => c.Name));
            CreateMap<IDatabaseComputedColumn, Dto.DatabaseColumn>()
                .ForMember(dto => dto.ColumnName, src => src.MapFrom(c => c.Name));
        }
    }
}
