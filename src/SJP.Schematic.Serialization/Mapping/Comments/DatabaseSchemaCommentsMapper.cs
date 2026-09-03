using Boxed.Mapping;
using LanguageExt;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Comments;

namespace SJP.Schematic.Serialization.Mapping.Comments;

/// <summary>
/// Maps the comments attached to a schema between their core and serialized representations.
/// </summary>
public class DatabaseSchemaCommentsMapper
    : IImmutableMapper<Dto.Comments.DatabaseSchemaComments, IDatabaseSchemaComments>
    , IImmutableMapper<IDatabaseSchemaComments, Dto.Comments.DatabaseSchemaComments>
{
    /// <summary>
    /// Maps serialized schema comments to their core representation.
    /// </summary>
    /// <param name="source">Serialized schema comments.</param>
    /// <returns>Schema comments.</returns>
    public IDatabaseSchemaComments Map(Dto.Comments.DatabaseSchemaComments source)
    {
        var identifierMapper = MapperRegistry.GetMapper<Dto.Identifier, Identifier>();
        var optionMapper = MapperRegistry.GetMapper<string?, Option<string>>();

        return new DatabaseSchemaComments(
            identifierMapper.Map(source.SchemaName),
            optionMapper.Map(source.Comment)
        );
    }

    /// <summary>
    /// Maps schema comments to their serialized representation.
    /// </summary>
    /// <param name="source">Schema comments.</param>
    /// <returns>Serialized schema comments.</returns>
    public Dto.Comments.DatabaseSchemaComments Map(IDatabaseSchemaComments source)
    {
        var identifierMapper = MapperRegistry.GetMapper<Identifier, Dto.Identifier>();
        var optionMapper = MapperRegistry.GetMapper<Option<string>, string?>();

        return new Dto.Comments.DatabaseSchemaComments
        {
            SchemaName = identifierMapper.Map(source.SchemaName),
            Comment = optionMapper.Map(source.Comment),
        };
    }
}
