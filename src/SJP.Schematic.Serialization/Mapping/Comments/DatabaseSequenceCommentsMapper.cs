using Boxed.Mapping;
using LanguageExt;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Comments;

namespace SJP.Schematic.Serialization.Mapping.Comments;

/// <summary>
/// Maps the comments attached to a sequence between their core and serialized representations.
/// </summary>
public class DatabaseSequenceCommentsMapper
    : IImmutableMapper<Dto.Comments.DatabaseSequenceComments, IDatabaseSequenceComments>
    , IImmutableMapper<IDatabaseSequenceComments, Dto.Comments.DatabaseSequenceComments>
{
    /// <summary>
    /// Maps serialized sequence comments to their core representation.
    /// </summary>
    /// <param name="source">Serialized sequence comments.</param>
    /// <returns>Sequence comments.</returns>
    public IDatabaseSequenceComments Map(Dto.Comments.DatabaseSequenceComments source)
    {
        var identifierMapper = MapperRegistry.GetMapper<Dto.Identifier, Identifier>();
        var optionMapper = MapperRegistry.GetMapper<string?, Option<string>>();

        return new DatabaseSequenceComments(
            identifierMapper.Map(source.SequenceName),
            optionMapper.Map(source.Comment)
        );
    }

    /// <summary>
    /// Maps sequence comments to their serialized representation.
    /// </summary>
    /// <param name="source">Sequence comments.</param>
    /// <returns>Serialized sequence comments.</returns>
    public Dto.Comments.DatabaseSequenceComments Map(IDatabaseSequenceComments source)
    {
        var identifierMapper = MapperRegistry.GetMapper<Identifier, Dto.Identifier>();
        var optionMapper = MapperRegistry.GetMapper<Option<string>, string?>();

        return new Dto.Comments.DatabaseSequenceComments
        {
            SequenceName = identifierMapper.Map(source.SequenceName),
            Comment = optionMapper.Map(source.Comment),
        };
    }
}