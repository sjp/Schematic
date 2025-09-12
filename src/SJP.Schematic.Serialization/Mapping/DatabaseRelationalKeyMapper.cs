using Boxed.Mapping;
using SJP.Schematic.Core;

namespace SJP.Schematic.Serialization.Mapping;

public class DatabaseRelationalKeyMapper
    : IImmutableMapper<Dto.DatabaseRelationalKey, IDatabaseRelationalKey>
    , IImmutableMapper<IDatabaseRelationalKey, Dto.DatabaseRelationalKey>
{
    public IDatabaseRelationalKey Map(Dto.DatabaseRelationalKey source)
    {
        var identifierMapper = MapperRegistry.GetMapper<Dto.Identifier, Identifier>();
        var databaseKeyMapper = MapperRegistry.GetMapper<Dto.DatabaseKey, IDatabaseKey>();

        return new DatabaseRelationalKey(
            identifierMapper.Map(source.ChildTable!),
            databaseKeyMapper.Map(source.ChildKey!),
            identifierMapper.Map(source.ParentTable!),
            databaseKeyMapper.Map(source.ParentKey!),
            source.DeleteAction,
            source.UpdateAction
        );
    }

    public Dto.DatabaseRelationalKey Map(IDatabaseRelationalKey source)
    {
        var identifierMapper = MapperRegistry.GetMapper<Identifier, Dto.Identifier>();
        var databaseKeyMapper = MapperRegistry.GetMapper<IDatabaseKey, Dto.DatabaseKey>();

        return new Dto.DatabaseRelationalKey
        {
            ChildTable = identifierMapper.Map(source.ChildTable),
            ChildKey = databaseKeyMapper.Map(source.ChildKey),
            ParentTable = identifierMapper.Map(source.ParentTable),
            ParentKey = databaseKeyMapper.Map(source.ParentKey),
            DeleteAction = source.DeleteAction,
            UpdateAction = source.UpdateAction,
        };
    }
}