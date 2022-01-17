using System.Collections.Generic;
using AutoMapper;
using LanguageExt;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Comments;

namespace SJP.Schematic.Serialization.Mapping.Comments
{
    public class DatabaseTableCommentsProfile : Profile
    {
        public DatabaseTableCommentsProfile()
        {
            CreateMap<Dto.Comments.DatabaseTableComments, RelationalDatabaseTableComments>()
                .ConstructUsing(static (dto, ctx) => new RelationalDatabaseTableComments(
                    ctx.Mapper.Map<Dto.Identifier, Identifier>(dto.TableName!),
                    dto.Comment == null
                        ? Option<string>.None
                        : Option<string>.Some(dto.Comment),
                    dto.PrimaryKeyComment == null
                        ? Option<string>.None
                        : Option<string>.Some(dto.PrimaryKeyComment),
                    AsCoreCommentLookup(dto.ColumnComments),
                    AsCoreCommentLookup(dto.CheckComments),
                    AsCoreCommentLookup(dto.UniqueKeyComments),
                    AsCoreCommentLookup(dto.ForeignKeyComments),
                    AsCoreCommentLookup(dto.IndexComments),
                    AsCoreCommentLookup(dto.TriggerComments)
                ))
                .ForAllMembers(static cfg => cfg.Ignore());

            CreateMap<IRelationalDatabaseTableComments, Dto.Comments.DatabaseTableComments>()
                .ConstructUsing(static (src, ctx) => new Dto.Comments.DatabaseTableComments
                {
                    TableName = ctx.Mapper.Map<Identifier, Dto.Identifier>(src.TableName!),
                    Comment = src.Comment.MatchUnsafe(c => c, (string?)null),
                    PrimaryKeyComment = src.PrimaryKeyComment.MatchUnsafe(c => c, (string?)null),
                    ColumnComments = AsDtoCommentLookup(src.ColumnComments),
                    CheckComments = AsDtoCommentLookup(src.CheckComments),
                    UniqueKeyComments = AsDtoCommentLookup(src.UniqueKeyComments),
                    ForeignKeyComments = AsDtoCommentLookup(src.ForeignKeyComments),
                    IndexComments = AsDtoCommentLookup(src.IndexComments),
                    TriggerComments = AsDtoCommentLookup(src.TriggerComments)
                })
                .ForAllMembers(static cfg => cfg.Ignore());
        }

        private static IReadOnlyDictionary<string, string?> AsDtoCommentLookup(IReadOnlyDictionary<Identifier, Option<string>> coreCommentLookup)
        {
            var result = new Dictionary<string, string?>();

            foreach (var kv in coreCommentLookup)
            {
                var key = kv.Key.LocalName;
                var val = kv.Value.MatchUnsafe(c => c, (string?)null);

                result[key] = val;
            }

            return result;
        }

        private static IReadOnlyDictionary<Identifier, Option<string>> AsCoreCommentLookup(IReadOnlyDictionary<string, string?> dtoCommentLookup)
        {
            var result = new Dictionary<Identifier, Option<string>>();

            foreach (var kv in dtoCommentLookup)
            {
                var key = Identifier.CreateQualifiedIdentifier(kv.Key);
                var val = kv.Value == null
                    ? Option<string>.None
                    : Option<string>.Some(kv.Value);

                result[key] = val;
            }

            return result;
        }
    }
}
