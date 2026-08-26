using Boxed.Mapping;
using LanguageExt;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Comments;

namespace SJP.Schematic.Serialization.Mapping.Comments;

/// <summary>
/// Maps the comments attached to a routine between their core and serialized representations.
/// </summary>
public class DatabaseRoutineCommentsMapper
    : IImmutableMapper<Dto.Comments.DatabaseRoutineComments, IDatabaseRoutineComments>
    , IImmutableMapper<IDatabaseRoutineComments, Dto.Comments.DatabaseRoutineComments>
{
    /// <summary>
    /// Maps serialized routine comments to their core representation.
    /// </summary>
    /// <param name="source">Serialized routine comments.</param>
    /// <returns>Routine comments.</returns>
    public IDatabaseRoutineComments Map(Dto.Comments.DatabaseRoutineComments source)
    {
        var identifierMapper = MapperRegistry.GetMapper<Dto.Identifier, Identifier>();
        var optionMapper = MapperRegistry.GetMapper<string?, Option<string>>();

        return new DatabaseRoutineComments(
            identifierMapper.Map(source.RoutineName),
            optionMapper.Map(source.Comment)
        );
    }

    /// <summary>
    /// Maps routine comments to their serialized representation.
    /// </summary>
    /// <param name="source">Routine comments.</param>
    /// <returns>Serialized routine comments.</returns>
    public Dto.Comments.DatabaseRoutineComments Map(IDatabaseRoutineComments source)
    {
        var identifierMapper = MapperRegistry.GetMapper<Identifier, Dto.Identifier>();
        var optionMapper = MapperRegistry.GetMapper<Option<string>, string?>();

        return new Dto.Comments.DatabaseRoutineComments
        {
            RoutineName = identifierMapper.Map(source.RoutineName),
            Comment = optionMapper.Map(source.Comment),
        };
    }
}