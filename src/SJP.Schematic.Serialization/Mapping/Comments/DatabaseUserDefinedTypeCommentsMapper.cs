using Boxed.Mapping;
using LanguageExt;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Comments;

namespace SJP.Schematic.Serialization.Mapping.Comments;

/// <summary>
/// Maps the comments attached to a user-defined type between their core and serialized representations.
/// </summary>
public class DatabaseUserDefinedTypeCommentsMapper
    : IImmutableMapper<Dto.Comments.DatabaseUserDefinedTypeComments, IDatabaseUserDefinedTypeComments>
    , IImmutableMapper<IDatabaseUserDefinedTypeComments, Dto.Comments.DatabaseUserDefinedTypeComments>
{
    /// <summary>
    /// Maps serialized user-defined type comments to their core representation.
    /// </summary>
    /// <param name="source">Serialized user-defined type comments.</param>
    /// <returns>User-defined type comments.</returns>
    public IDatabaseUserDefinedTypeComments Map(Dto.Comments.DatabaseUserDefinedTypeComments source)
    {
        var identifierMapper = MapperRegistry.GetMapper<Dto.Identifier, Identifier>();
        var optionMapper = MapperRegistry.GetMapper<string?, Option<string>>();

        return new DatabaseUserDefinedTypeComments(
            identifierMapper.Map(source.TypeName),
            optionMapper.Map(source.Comment)
        );
    }

    /// <summary>
    /// Maps user-defined type comments to their serialized representation.
    /// </summary>
    /// <param name="source">User-defined type comments.</param>
    /// <returns>Serialized user-defined type comments.</returns>
    public Dto.Comments.DatabaseUserDefinedTypeComments Map(IDatabaseUserDefinedTypeComments source)
    {
        var identifierMapper = MapperRegistry.GetMapper<Identifier, Dto.Identifier>();
        var optionMapper = MapperRegistry.GetMapper<Option<string>, string?>();

        return new Dto.Comments.DatabaseUserDefinedTypeComments
        {
            TypeName = identifierMapper.Map(source.TypeName),
            Comment = optionMapper.Map(source.Comment),
        };
    }
}
