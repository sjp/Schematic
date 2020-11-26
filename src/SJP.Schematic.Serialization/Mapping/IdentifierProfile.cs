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
                .ConstructUsing(static dto =>
                    dto == null
                        ? Option<Identifier>.None
                        : Option<Identifier>.Some(Identifier.CreateQualifiedIdentifier(dto.Server, dto.Database, dto.Schema, dto.LocalName)))
                .ForAllMembers(static cfg => cfg.Ignore());

            CreateMap<Option<Identifier>, Dto.Identifier>()
                .ConstructUsing(static identifier =>
                    identifier.MatchUnsafe(
                        static ident => new Dto.Identifier
                        {
                            Server = ident.Server,
                            Database = ident.Database,
                            Schema = ident.Schema,
                            LocalName = ident.LocalName
                        },
                        static () => default!
                    ))
                .ForAllMembers(static cfg => cfg.Ignore());

            CreateMap<Identifier, Dto.Identifier>();
            CreateMap<Dto.Identifier, Identifier>()
                .ConstructUsing(static dto => Identifier.CreateQualifiedIdentifier(dto.Server, dto.Database, dto.Schema, dto.LocalName));
        }
    }
}
