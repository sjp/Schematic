using System.Collections.Generic;
using AutoMapper;
using LanguageExt;
using SJP.Schematic.Core;

namespace SJP.Schematic.Serialization.Mapping
{
    public class RelationalDatabaseTableProfile : Profile
    {
        public RelationalDatabaseTableProfile()
        {
            CreateMap<Dto.RelationalDatabaseTable, RelationalDatabaseTable>()
                .ConstructUsing((dto, ctx) => new RelationalDatabaseTable(
                    ctx.Mapper.Map<Dto.Identifier, Identifier>(dto.TableName!),
                    ctx.Mapper.Map<IEnumerable<Dto.DatabaseColumn>, List<DatabaseColumn>>(dto.Columns),
                    ctx.Mapper.Map<Dto.DatabaseKey?, Option<IDatabaseKey>>(dto.PrimaryKey),
                    ctx.Mapper.Map<IEnumerable<Dto.DatabaseKey>, List<DatabaseKey>>(dto.UniqueKeys),
                    ctx.Mapper.Map<IEnumerable<Dto.DatabaseRelationalKey>, List<DatabaseRelationalKey>>(dto.ParentKeys),
                    ctx.Mapper.Map<IEnumerable<Dto.DatabaseRelationalKey>, List<DatabaseRelationalKey>>(dto.ChildKeys),
                    ctx.Mapper.Map<IEnumerable<Dto.DatabaseIndex>, List<DatabaseIndex>>(dto.Indexes),
                    ctx.Mapper.Map<IEnumerable<Dto.DatabaseCheckConstraint>, List<DatabaseCheckConstraint>>(dto.Checks),
                    ctx.Mapper.Map<IEnumerable<Dto.DatabaseTrigger>, List<DatabaseTrigger>>(dto.Triggers)
                ));
            CreateMap<IRelationalDatabaseTable, Dto.RelationalDatabaseTable>()
                .ForMember(dest => dest.TableName, src => src.MapFrom(t => t.Name));
        }
    }
}
