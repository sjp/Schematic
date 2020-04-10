using System.Collections.Generic;
using AutoMapper;
using LanguageExt;
using SJP.Schematic.Core;

namespace SJP.Schematic.Serialization.Mapping
{
    public class DatabaseKeyProfile : Profile
    {
        public DatabaseKeyProfile()
        {
            CreateMap<Dto.DatabaseKey, DatabaseKey>()
                .ConstructUsing((dto, ctx) => new DatabaseKey(
                    ctx.Mapper.Map<Dto.Identifier?, Option<Identifier>>(dto.Name),
                    dto.KeyType,
                    ctx.Mapper.Map<IEnumerable<Dto.DatabaseColumn>, List<DatabaseColumn>>(dto.Columns),
                    dto.IsEnabled
                ));
            CreateMap<IDatabaseKey, Dto.DatabaseKey>();

            CreateMap<Option<IDatabaseKey>, Dto.DatabaseKey?>()
                .ConstructUsing((key, ctx) => key.MatchUnsafe(
                    k => ctx.Mapper.Map<IDatabaseKey, Dto.DatabaseKey>(k),
                    (Dto.DatabaseKey?)null));
            CreateMap<Dto.DatabaseKey?, Option<IDatabaseKey>>()
                .ConstructUsing((dto, ctx) => dto == null
                    ? Option<IDatabaseKey>.None
                    : Option<IDatabaseKey>.Some(ctx.Mapper.Map<Dto.DatabaseKey, DatabaseKey>(dto)));
        }
    }
}
