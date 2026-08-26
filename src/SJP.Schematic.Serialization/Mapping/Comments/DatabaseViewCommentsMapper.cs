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
            CommentLookup.ToCore(source.ColumnComments)
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
            ColumnComments = CommentLookup.ToDto(source.ColumnComments),
        };
    }
}