using System.Collections.Generic;
using AutoMapper;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Comments;

namespace SJP.Schematic.Serialization.Mapping.Comments
{
    public class DatabaseCommentProviderProfile : Profile
    {
        public DatabaseCommentProviderProfile()
        {
            CreateMap<Dto.Comments.DatabaseCommentProvider, RelationalDatabaseCommentProvider>()
                .ConstructUsing(static (dto, ctx) => new RelationalDatabaseCommentProvider(
                    ctx.Mapper.Map<Dto.IdentifierDefaults, IdentifierDefaults>(dto.IdentifierDefaults),
                    dto.IdentifierResolver ?? new VerbatimIdentifierResolutionStrategy(),
                    ctx.Mapper.Map<IEnumerable<Dto.Comments.DatabaseTableComments>, IEnumerable<RelationalDatabaseTableComments>>(dto.TableComments),
                    ctx.Mapper.Map<IEnumerable<Dto.Comments.DatabaseViewComments>, IEnumerable<DatabaseViewComments>>(dto.ViewComments),
                    ctx.Mapper.Map<IEnumerable<Dto.Comments.DatabaseSequenceComments>, IEnumerable<DatabaseSequenceComments>>(dto.SequenceComments),
                    ctx.Mapper.Map<IEnumerable<Dto.Comments.DatabaseSynonymComments>, IEnumerable<DatabaseSynonymComments>>(dto.SynonymComments),
                    ctx.Mapper.Map<IEnumerable<Dto.Comments.DatabaseRoutineComments>, IEnumerable<DatabaseRoutineComments>>(dto.RoutineComments)
                ))
                .ForAllMembers(static cfg => cfg.Ignore());
        }
    }
}
