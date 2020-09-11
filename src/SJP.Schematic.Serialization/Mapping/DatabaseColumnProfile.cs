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
                )
                .ForAllMembers(cfg => cfg.Ignore());

            CreateMap<IDatabaseColumn, Dto.DatabaseColumn>()
                .ForMember(dto => dto.AutoIncrement, src => src.MapFrom(c => c.AutoIncrement))
                .ForMember(dto => dto.ColumnName, src => src.MapFrom(c => c.Name))
                .ForMember(dto => dto.DefaultValue, src => src.MapFrom(c => c.DefaultValue))
                .ForMember(dto => dto.Definition, src => src.MapFrom(_ => (string?)null))
                .ForMember(dto => dto.IsComputed, src => src.MapFrom(c => c.IsComputed))
                .ForMember(dto => dto.IsNullable, src => src.MapFrom(c => c.IsNullable));

            CreateMap<IDatabaseComputedColumn, Dto.DatabaseColumn>()
                .ForMember(dto => dto.AutoIncrement, src => src.MapFrom(c => c.AutoIncrement))
                .ForMember(dto => dto.ColumnName, src => src.MapFrom(c => c.Name))
                .ForMember(dto => dto.DefaultValue, src => src.MapFrom(c => c.DefaultValue))
                .ForMember(dto => dto.Definition, src => src.MapFrom(c => c.Definition))
                .ForMember(dto => dto.IsComputed, src => src.MapFrom(_ => true))
                .ForMember(dto => dto.IsNullable, src => src.MapFrom(c => c.IsNullable));
        }
    }
}
