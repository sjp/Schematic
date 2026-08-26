using Boxed.Mapping;
using LanguageExt;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Comments;

namespace SJP.Schematic.Serialization.Mapping.Comments;

/// <summary>
/// Maps the comments attached to a synonym between their core and serialized representations.
/// </summary>
public class DatabaseSynonymCommentsMapper
    : IImmutableMapper<Dto.Comments.DatabaseSynonymComments, IDatabaseSynonymComments>
    , IImmutableMapper<IDatabaseSynonymComments, Dto.Comments.DatabaseSynonymComments>
{
    /// <summary>
    /// Maps serialized synonym comments to their core representation.
    /// </summary>
    /// <param name="source">Serialized synonym comments.</param>
    /// <returns>Synonym comments.</returns>
    public IDatabaseSynonymComments Map(Dto.Comments.DatabaseSynonymComments source)
    {
        var identifierMapper = MapperRegistry.GetMapper<Dto.Identifier, Identifier>();
        var optionMapper = MapperRegistry.GetMapper<string?, Option<string>>();

        return new DatabaseSynonymComments(
            identifierMapper.Map(source.SynonymName),
            optionMapper.Map(source.Comment)
        );
    }

    /// <summary>
    /// Maps synonym comments to their serialized representation.
    /// </summary>
    /// <param name="source">Synonym comments.</param>
    /// <returns>Serialized synonym comments.</returns>
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