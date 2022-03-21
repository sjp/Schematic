using Boxed.Mapping;
using LanguageExt;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Comments;

namespace SJP.Schematic.Serialization.Mapping.Comments;

public class DatabaseRoutineCommentsMapper
    : IImmutableMapper<Dto.Comments.DatabaseRoutineComments, IDatabaseRoutineComments>
    , IImmutableMapper<IDatabaseRoutineComments, Dto.Comments.DatabaseRoutineComments>
{
    public IDatabaseRoutineComments Map(Dto.Comments.DatabaseRoutineComments source)
    {
        var identifierMapper = MapperRegistry.GetMapper<Dto.Identifier, Identifier>();
        var optionMapper = MapperRegistry.GetMapper<string?, Option<string>>();

        return new DatabaseRoutineComments(
            identifierMapper.Map(source.RoutineName),
            optionMapper.Map(source.Comment)
        );
    }

    public Dto.Comments.DatabaseRoutineComments Map(IDatabaseRoutineComments source)
    {
        var identifierMapper = MapperRegistry.GetMapper<Identifier, Dto.Identifier>();
        var optionMapper = MapperRegistry.GetMapper<Option<string>, string?>();

        return new Dto.Comments.DatabaseRoutineComments
        {
            RoutineName = identifierMapper.Map(source.RoutineName),
            Comment = optionMapper.Map(source.Comment)
        };
    }
}