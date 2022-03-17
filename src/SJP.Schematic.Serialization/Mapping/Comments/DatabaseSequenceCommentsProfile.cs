using AutoMapper;
using LanguageExt;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Comments;

namespace SJP.Schematic.Serialization.Mapping.Comments;

public class DatabaseSequenceCommentsProfile : Profile
{
    public DatabaseSequenceCommentsProfile()
    {
        CreateMap<Dto.Comments.DatabaseSequenceComments, DatabaseSequenceComments>()
            .ConstructUsing(static (dto, ctx) => new DatabaseSequenceComments(
                ctx.Mapper.Map<Dto.Identifier, Identifier>(dto.SequenceName!),
                dto.Comment == null
                    ? Option<string>.None
                    : Option<string>.Some(dto.Comment)
            ))
            .ForAllMembers(static cfg => cfg.Ignore());
        CreateMap<IDatabaseSequenceComments, Dto.Comments.DatabaseSequenceComments>()
            .ForMember(static dest => dest.SequenceName, static src => src.MapFrom(static s => s.SequenceName))
            .ForMember(static dest => dest.Comment, static src => src.MapFrom(static s => s.Comment.MatchUnsafe(c => c, (string?)null)));
    }
}
