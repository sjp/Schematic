using Boxed.Mapping;
using LanguageExt;
using SJP.Schematic.Core;

namespace SJP.Schematic.Serialization.Mapping;

/// <summary>
/// Maps a database object name between its core and serialized representations.
/// </summary>
public class IdentifierMapper
    : IImmutableMapper<Dto.Identifier?, Option<Identifier>>
    , IImmutableMapper<Option<Identifier>, Dto.Identifier?>
    , IImmutableMapper<Identifier, Dto.Identifier>
    , IImmutableMapper<Dto.Identifier, Identifier>
{
    /// <summary>
    /// Maps an optional serialized name to its core representation.
    /// </summary>
    /// <param name="source">A serialized name, or <see langword="null"/> when no name is available.</param>
    /// <returns>The name, if one is available.</returns>
    public Option<Identifier> Map(Dto.Identifier? source)
    {
        return source == null
            ? Option<Identifier>.None
            : Option<Identifier>.Some(Identifier.CreateQualifiedIdentifier(source.Server, source.Database, source.Schema, source.LocalName));
    }

    /// <summary>
    /// Maps an optional name to its serialized representation.
    /// </summary>
    /// <param name="source">A name, if one is available.</param>
    /// <returns>A serialized name, or <see langword="null"/> when no name is available.</returns>
    public Dto.Identifier? Map(Option<Identifier> source)
    {
        return source.MatchUnsafe(
            static ident => new Dto.Identifier
            {
                Server = ident.Server,
                Database = ident.Database,
                Schema = ident.Schema,
                LocalName = ident.LocalName,
            },
            static () => (Dto.Identifier?)null
        );
    }

    /// <summary>
    /// Maps a name to its serialized representation.
    /// </summary>
    /// <param name="source">A name.</param>
    /// <returns>A serialized name.</returns>
    public Dto.Identifier Map(Identifier source)
    {
        return new Dto.Identifier
        {
            Server = source.Server,
            Database = source.Database,
            Schema = source.Schema,
            LocalName = source.LocalName,
        };
    }

    Identifier IImmutableMapper<Dto.Identifier, Identifier>.Map(Dto.Identifier source)
    {
        return Identifier.CreateQualifiedIdentifier(source.Server, source.Database, source.Schema, source.LocalName);
    }
}