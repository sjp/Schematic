using System.Collections.Generic;
using Boxed.Mapping;
using LanguageExt;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Comments;
using SJP.Schematic.Serialization.Dto.Comments;

namespace SJP.Schematic.Serialization.Mapping.Comments;

public class DatabaseTableCommentsMapper
    : IImmutableMapper<DatabaseTableComments, IRelationalDatabaseTableComments>
    , IImmutableMapper<IRelationalDatabaseTableComments, DatabaseTableComments>
{
    public IRelationalDatabaseTableComments Map(DatabaseTableComments source)
    {
        var identifierMapper = MapperRegistry.GetMapper<Dto.Identifier, Identifier>();
        var optionMapper = MapperRegistry.GetMapper<string?, Option<string>>();

        return new RelationalDatabaseTableComments(
            identifierMapper.Map(source.TableName),
            optionMapper.Map(source.Comment),
            optionMapper.Map(source.PrimaryKeyComment),
            AsCoreCommentLookup(source.ColumnComments),
            AsCoreCommentLookup(source.CheckComments),
            AsCoreCommentLookup(source.UniqueKeyComments),
            AsCoreCommentLookup(source.ForeignKeyComments),
            AsCoreCommentLookup(source.IndexComments),
            AsCoreCommentLookup(source.TriggerComments)
        );
    }

    public DatabaseTableComments Map(IRelationalDatabaseTableComments source)
    {
        var identifierMapper = MapperRegistry.GetMapper<Identifier, Dto.Identifier>();
        var optionMapper = MapperRegistry.GetMapper<Option<string>, string?>();

        return new DatabaseTableComments
        {
            TableName = identifierMapper.Map(source.TableName),
            Comment = optionMapper.Map(source.Comment),
            PrimaryKeyComment = optionMapper.Map(source.PrimaryKeyComment),
            ColumnComments = AsDtoCommentLookup(source.ColumnComments),
            CheckComments = AsDtoCommentLookup(source.CheckComments),
            UniqueKeyComments = AsDtoCommentLookup(source.UniqueKeyComments),
            ForeignKeyComments = AsDtoCommentLookup(source.ForeignKeyComments),
            IndexComments = AsDtoCommentLookup(source.IndexComments),
            TriggerComments = AsDtoCommentLookup(source.TriggerComments)
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
