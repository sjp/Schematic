using Boxed.Mapping;
using LanguageExt;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Comments;

namespace SJP.Schematic.Serialization.Mapping.Comments;

public class DatabaseSynonymCommentsMapper
    : IImmutableMapper<Dto.Comments.DatabaseSynonymComments, IDatabaseSynonymComments>
    , IImmutableMapper<IDatabaseSynonymComments, Dto.Comments.DatabaseSynonymComments>
{
    public IDatabaseSynonymComments Map(Dto.Comments.DatabaseSynonymComments source)
    {
        var identifierMapper = MapperRegistry.GetMapper<Dto.Identifier, Identifier>();
        var optionMapper = MapperRegistry.GetMapper<string?, Option<string>>();

        return new DatabaseSynonymComments(
            identifierMapper.Map(source.SynonymName),
            optionMapper.Map(source.Comment)
        );
    }

    public Dto.Comments.DatabaseSynonymComments Map(IDatabaseSynonymComments source)
    {
        var identifierMapper = MapperRegistry.GetMapper<Identifier, Dto.Identifier>();
        var optionMapper = MapperRegistry.GetMapper<Option<string>, string?>();

        return new Dto.Comments.DatabaseSynonymComments
        {
            SynonymName = identifierMapper.Map(source.SynonymName),
            Comment = optionMapper.Map(source.Comment),
        };
    }
}