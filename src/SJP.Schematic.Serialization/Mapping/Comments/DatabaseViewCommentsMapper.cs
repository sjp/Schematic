using System;
using Boxed.Mapping;
using LanguageExt;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Comments;

namespace SJP.Schematic.Serialization.Mapping.Comments;

/// <summary>
/// Maps the comments attached to a view and to its columns between their core and serialized representations.
/// </summary>
public class DatabaseViewCommentsMapper
    : IImmutableMapper<Dto.Comments.DatabaseViewComments, IDatabaseViewComments>
    , IImmutableMapper<IDatabaseViewComments, Dto.Comments.DatabaseViewComments>
{
    /// <summary>
    /// Maps serialized view comments to their core representation.
    /// </summary>
    /// <param name="source">Serialized view comments.</param>
    /// <returns>View comments.</returns>
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

    /// <summary>
    /// Maps view comments to their serialized representation.
    /// </summary>
    /// <param name="source">View comments.</param>
    /// <returns>Serialized view comments.</returns>
    /// <exception cref="ArgumentException">The column comment lookup is keyed by a qualified name rather than a name local to the view.</exception>
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