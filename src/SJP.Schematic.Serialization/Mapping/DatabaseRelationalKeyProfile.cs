using AutoMapper;
using SJP.Schematic.Core;

namespace SJP.Schematic.Serialization.Mapping
{
    public class DatabaseRelationalKeyProfile : Profile
    {
        public DatabaseRelationalKeyProfile()
        {
            CreateMap<Dto.DatabaseRelationalKey, DatabaseRelationalKey>()
                .ConstructUsing((dto, ctx) => new DatabaseRelationalKey(
                    ctx.Mapper.Map<Dto.Identifier, Identifier>(dto.ChildTable!),
                    ctx.Mapper.Map<Dto.DatabaseKey, DatabaseKey>(dto.ChildKey!),
                    ctx.Mapper.Map<Dto.Identifier, Identifier>(dto.ParentTable!),
                    ctx.Mapper.Map<Dto.DatabaseKey, DatabaseKey>(dto.ParentKey!),
                    dto.DeleteAction,
                    dto.UpdateAction
                ))
                .ForAllMembers(cfg => cfg.Ignore());
            CreateMap<IDatabaseRelationalKey, Dto.DatabaseRelationalKey>();
        }
    }
}
