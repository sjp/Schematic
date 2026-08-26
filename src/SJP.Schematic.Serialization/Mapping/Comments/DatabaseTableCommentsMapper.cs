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
            CommentLookup.ToCore(source.ColumnComments),
            CommentLookup.ToCore(source.CheckComments),
            CommentLookup.ToCore(source.UniqueKeyComments),
            CommentLookup.ToCore(source.ForeignKeyComments),
            CommentLookup.ToCore(source.IndexComments),
            CommentLookup.ToCore(source.TriggerComments)
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
            ColumnComments = CommentLookup.ToDto(source.ColumnComments),
            CheckComments = CommentLookup.ToDto(source.CheckComments),
            UniqueKeyComments = CommentLookup.ToDto(source.UniqueKeyComments),
            ForeignKeyComments = CommentLookup.ToDto(source.ForeignKeyComments),
            IndexComments = CommentLookup.ToDto(source.IndexComments),
            TriggerComments = CommentLookup.ToDto(source.TriggerComments),
        };
    }
}