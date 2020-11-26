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
                .ConstructUsing(static (dto, ctx) => dto.IsComputed
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
                .ForAllMembers(static cfg => cfg.Ignore());

            CreateMap<IDatabaseColumn, Dto.DatabaseColumn>()
                .ForMember(static dto => dto.AutoIncrement, static src => src.MapFrom(static c => c.AutoIncrement))
                .ForMember(static dto => dto.ColumnName, static src => src.MapFrom(static c => c.Name))
                .ForMember(static dto => dto.DefaultValue, static src => src.MapFrom(static c => c.DefaultValue))
                .ForMember(static dto => dto.Definition, static src => src.MapFrom(static _ => (string?)null))
                .ForMember(static dto => dto.IsComputed, static src => src.MapFrom(static c => c.IsComputed))
                .ForMember(static dto => dto.IsNullable, static src => src.MapFrom(static c => c.IsNullable));

            CreateMap<IDatabaseComputedColumn, Dto.DatabaseColumn>()
                .ForMember(static dto => dto.AutoIncrement, static src => src.MapFrom(static c => c.AutoIncrement))
                .ForMember(static dto => dto.ColumnName, static src => src.MapFrom(static c => c.Name))
                .ForMember(static dto => dto.DefaultValue, static src => src.MapFrom(static c => c.DefaultValue))
                .ForMember(static dto => dto.Definition, static src => src.MapFrom(static c => c.Definition))
                .ForMember(static dto => dto.IsComputed, static src => src.MapFrom(static _ => true))
                .ForMember(static dto => dto.IsNullable, static src => src.MapFrom(static c => c.IsNullable));
        }
    }
}
