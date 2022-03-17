using System.Collections.Generic;
using AutoMapper;
using LanguageExt;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Comments;

namespace SJP.Schematic.Serialization.Mapping.Comments;

public class DatabaseViewCommentsProfile : Profile
{
    public DatabaseViewCommentsProfile()
    {
        CreateMap<Dto.Comments.DatabaseViewComments, DatabaseViewComments>()
            .ConstructUsing(static (dto, ctx) => new DatabaseViewComments(
                ctx.Mapper.Map<Dto.Identifier, Identifier>(dto.ViewName!),
                dto.Comment == null
                    ? Option<string>.None
                    : Option<string>.Some(dto.Comment),
                AsCoreCommentLookup(dto.ColumnComments)
            ))
            .ForAllMembers(static cfg => cfg.Ignore());

        CreateMap<IDatabaseViewComments, Dto.Comments.DatabaseViewComments>()
            .ConstructUsing(static (src, ctx) => new Dto.Comments.DatabaseViewComments
            {
                ViewName = ctx.Mapper.Map<Identifier, Dto.Identifier>(src.ViewName!),
                Comment = src.Comment.MatchUnsafe(c => c, (string?)null),
                ColumnComments = AsDtoCommentLookup(src.ColumnComments)
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
