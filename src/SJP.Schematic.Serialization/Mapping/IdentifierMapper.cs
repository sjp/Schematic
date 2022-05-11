using Boxed.Mapping;
using LanguageExt;
using SJP.Schematic.Core;

namespace SJP.Schematic.Serialization.Mapping;

public class IdentifierMapper
    : IImmutableMapper<Dto.Identifier?, Option<Identifier>>
    , IImmutableMapper<Option<Identifier>, Dto.Identifier>
    , IImmutableMapper<Identifier, Dto.Identifier>
    , IImmutableMapper<Dto.Identifier, Identifier>
{
    public Option<Identifier> Map(Dto.Identifier? source)
    {
        return source == null
            ? Option<Identifier>.None
            : Option<Identifier>.Some(Identifier.CreateQualifiedIdentifier(source.Server, source.Database, source.Schema, source.LocalName));
    }

    public Dto.Identifier Map(Option<Identifier> source)
    {
        var result = source.MatchUnsafe(
            static ident => new Dto.Identifier
            {
                Server = ident.Server,
                Database = ident.Database,
                Schema = ident.Schema,
                LocalName = ident.LocalName
            },
            static () => default!
        );

        return result!;
    }

    public Dto.Identifier Map(Identifier source)
    {
        return new Dto.Identifier
        {
            Server = source.Server,
            Database = source.Database,
            Schema = source.Schema,
            LocalName = source.LocalName
        };
    }

    Identifier IImmutableMapper<Dto.Identifier, Identifier>.Map(Dto.Identifier source)
    {
        return Identifier.CreateQualifiedIdentifier(source.Server, source.Database, source.Schema, source.LocalName);
    }
}