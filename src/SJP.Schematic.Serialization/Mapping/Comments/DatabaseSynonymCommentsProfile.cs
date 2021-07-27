using AutoMapper;
using LanguageExt;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Comments;

namespace SJP.Schematic.Serialization.Mapping.Comments
{
    public class DatabaseSynonymCommentsProfile : Profile
    {
        public DatabaseSynonymCommentsProfile()
        {
            CreateMap<Dto.Comments.DatabaseSynonymComments, DatabaseSynonymComments>()
                .ConstructUsing(static (dto, ctx) => new DatabaseSynonymComments(
                    ctx.Mapper.Map<Dto.Identifier, Identifier>(dto.SynonymName!),
                    dto.Comment == null
                        ? Option<string>.None
                        : Option<string>.Some(dto.Comment)
                ))
                .ForAllMembers(static cfg => cfg.Ignore());
            CreateMap<IDatabaseSynonymComments, Dto.Comments.DatabaseSynonymComments>()
                .ForMember(static dest => dest.SynonymName, static src => src.MapFrom(static s => s.SynonymName))
                .ForMember(static dest => dest.Comment, static src => src.MapFrom(static s => s.Comment.MatchUnsafe(c => c, (string?)null)));
        }
    }
}
