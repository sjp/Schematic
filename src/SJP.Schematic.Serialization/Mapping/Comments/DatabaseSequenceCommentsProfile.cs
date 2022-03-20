using Boxed.Mapping;
using LanguageExt;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Comments;

namespace SJP.Schematic.Serialization.Mapping.Comments;

public class DatabaseSequenceCommentsProfile
    : IImmutableMapper<Dto.Comments.DatabaseSequenceComments, IDatabaseSequenceComments>
    , IImmutableMapper<IDatabaseSequenceComments, Dto.Comments.DatabaseSequenceComments>
{
    public IDatabaseSequenceComments Map(Dto.Comments.DatabaseSequenceComments source)
    {
        var identifierMapper = MapperRegistry.GetMapper<Dto.Identifier, Identifier>();
        var optionMapper = MapperRegistry.GetMapper<string?, Option<string>>();

        return new DatabaseSequenceComments(
            identifierMapper.Map(source.SequenceName),
            optionMapper.Map(source.Comment)
        );
    }

    public Dto.Comments.DatabaseSequenceComments Map(IDatabaseSequenceComments source)
    {
        var identifierMapper = MapperRegistry.GetMapper<Identifier, Dto.Identifier>();
        var optionMapper = MapperRegistry.GetMapper<Option<string>, string?>();

        return new Dto.Comments.DatabaseSequenceComments
        {
            SequenceName = identifierMapper.Map(source.SequenceName),
            Comment = optionMapper.Map(source.Comment)
        };
    }
}
