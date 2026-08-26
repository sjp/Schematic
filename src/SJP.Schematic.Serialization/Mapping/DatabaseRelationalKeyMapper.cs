using Boxed.Mapping;
using SJP.Schematic.Core;

namespace SJP.Schematic.Serialization.Mapping;

/// <summary>
/// Maps a foreign key relationship between its core and serialized representations.
/// </summary>
public class DatabaseRelationalKeyMapper
    : IImmutableMapper<Dto.DatabaseRelationalKey, IDatabaseRelationalKey>
    , IImmutableMapper<IDatabaseRelationalKey, Dto.DatabaseRelationalKey>
{
    /// <summary>
    /// Maps a serialized foreign key relationship to its core representation.
    /// </summary>
    /// <param name="source">A serialized foreign key relationship.</param>
    /// <returns>A foreign key relationship.</returns>
    public IDatabaseRelationalKey Map(Dto.DatabaseRelationalKey source)
    {
        var identifierMapper = MapperRegistry.GetMapper<Dto.Identifier, Identifier>();
        var databaseKeyMapper = MapperRegistry.GetMapper<Dto.DatabaseKey, IDatabaseKey>();

        return new DatabaseRelationalKey(
            identifierMapper.Map(source.ChildTable),
            databaseKeyMapper.Map(source.ChildKey),
            identifierMapper.Map(source.ParentTable),
            databaseKeyMapper.Map(source.ParentKey),
            source.DeleteAction,
            source.UpdateAction
        );
    }

    /// <summary>
    /// Maps a foreign key relationship to its serialized representation.
    /// </summary>
    /// <param name="source">A foreign key relationship.</param>
    /// <returns>A serialized foreign key relationship.</returns>
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