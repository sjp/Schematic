using AutoMapper;
using LanguageExt;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Comments;

namespace SJP.Schematic.Serialization.Mapping.Comments;

public class DatabaseRoutineCommentsProfile : Profile
{
    public DatabaseRoutineCommentsProfile()
    {
        CreateMap<Dto.Comments.DatabaseRoutineComments, DatabaseRoutineComments>()
            .ConstructUsing(static (dto, ctx) => new DatabaseRoutineComments(
                ctx.Mapper.Map<Dto.Identifier, Identifier>(dto.RoutineName!),
                dto.Comment == null
                    ? Option<string>.None
                    : Option<string>.Some(dto.Comment)
            ))
            .ForAllMembers(static cfg => cfg.Ignore());
        CreateMap<IDatabaseRoutineComments, Dto.Comments.DatabaseRoutineComments>()
            .ForMember(static dest => dest.RoutineName, static src => src.MapFrom(static s => s.RoutineName))
            .ForMember(static dest => dest.Comment, static src => src.MapFrom(static s => s.Comment.MatchUnsafe(c => c, (string?)null)));
    }
}
