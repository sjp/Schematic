using System;
using System.Collections.Generic;
using LanguageExt;
using SJP.Schematic.Core;

namespace SJP.Schematic.Serialization.Mapping
{
    internal static class IdentifierMappingExtensions
    {
        public static Dto.Identifier ToDto(this Identifier identifier)
        {
            if (identifier == null)
                throw new ArgumentNullException(nameof(identifier));

            return new Dto.Identifier
            {
                Server = identifier.Server,
                Database = identifier.Database,
                Schema = identifier.Schema,
                LocalName = identifier.LocalName
            };
        }

        public static Dto.Identifier? ToDto(this Option<Identifier> identifier)
        {
            return identifier.MatchUnsafe(
                ident => new Dto.Identifier
                {
                    Server = ident.Server,
                    Database = ident.Database,
                    Schema = ident.Schema,
                    LocalName = ident.LocalName
                },
                () => (Dto.Identifier?)null
            );
        }

        public static Option<Identifier> FromDto(this Dto.Identifier? dto)
        {
            return dto == null
                ? Option<Identifier>.None
                : Option<Identifier>.Some(Identifier.CreateQualifiedIdentifier(dto.Server, dto.Database, dto.Schema, dto.LocalName));
        }
    }
}
