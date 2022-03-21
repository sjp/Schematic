using Boxed.Mapping;
using LanguageExt;
using SJP.Schematic.Core;

namespace SJP.Schematic.Serialization.Mapping;

public class DatabaseCheckMapper
    : IImmutableMapper<Dto.DatabaseCheckConstraint, IDatabaseCheckConstraint>
    , IImmutableMapper<IDatabaseCheckConstraint, Dto.DatabaseCheckConstraint>
{
    public IDatabaseCheckConstraint Map(Dto.DatabaseCheckConstraint source)
    {
        var identifierMapper = MapperRegistry.GetMapper<Dto.Identifier?, Option<Identifier>>();
        return new DatabaseCheckConstraint(
            identifierMapper.Map(source.CheckName),
            source.Definition!,
            source.IsEnabled
        );
    }

    public Dto.DatabaseCheckConstraint Map(IDatabaseCheckConstraint source)
    {
        var identifierMapper = MapperRegistry.GetMapper<Option<Identifier>, Dto.Identifier?>();
        return new Dto.DatabaseCheckConstraint
        {
            CheckName = identifierMapper.Map(source.Name),
            Definition = source.Definition,
            IsEnabled = source.IsEnabled
        };
    }
}