using AutoMapper;
using LanguageExt;
using SJP.Schematic.Core;

namespace SJP.Schematic.Serialization.Mapping
{
    public class IdentifierProfile : Profile
    {
        public IdentifierProfile()
        {
            CreateMap<Dto.Identifier?, Option<Identifier>>()
                .ConstructUsing(dto =>
                    dto == null
                        ? Option<Identifier>.None
                        : Option<Identifier>.Some(Identifier.CreateQualifiedIdentifier(dto.Server, dto.Database, dto.Schema, dto.LocalName)));

            CreateMap<Option<Identifier>, Dto.Identifier>()
                .ConstructUsing(identifier =>
                    identifier.MatchUnsafe(
                        ident => new Dto.Identifier
                        {
                            Server = ident.Server,
                            Database = ident.Database,
                            Schema = ident.Schema,
                            LocalName = ident.LocalName
                        },
                        () => default!
                    ));

            CreateMap<Identifier, Dto.Identifier>();
            CreateMap<Dto.Identifier?, Identifier>();
        }
    }
}
