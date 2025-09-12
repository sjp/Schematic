using System.Collections.Generic;
using Boxed.Mapping;
using LanguageExt;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Comments;

namespace SJP.Schematic.Serialization.Mapping.Comments;

public class DatabaseViewCommentsMapper
    : IImmutableMapper<Dto.Comments.DatabaseViewComments, IDatabaseViewComments>
    , IImmutableMapper<IDatabaseViewComments, Dto.Comments.DatabaseViewComments>
{
    public IDatabaseViewComments Map(Dto.Comments.DatabaseViewComments source)
    {
        var identifierMapper = MapperRegistry.GetMapper<Dto.Identifier, Identifier>();
        var optionMapper = MapperRegistry.GetMapper<string?, Option<string>>();

        return new DatabaseViewComments(
            identifierMapper.Map(source.ViewName),
            optionMapper.Map(source.Comment),
            AsCoreCommentLookup(source.ColumnComments)
        );
    }

    public Dto.Comments.DatabaseViewComments Map(IDatabaseViewComments source)
    {
        var identifierMapper = MapperRegistry.GetMapper<Identifier, Dto.Identifier>();
        var optionMapper = MapperRegistry.GetMapper<Option<string>, string?>();

        return new Dto.Comments.DatabaseViewComments
        {
            ViewName = identifierMapper.Map(source.ViewName),
            Comment = optionMapper.Map(source.Comment),
            ColumnComments = AsDtoCommentLookup(source.ColumnComments),
        };
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