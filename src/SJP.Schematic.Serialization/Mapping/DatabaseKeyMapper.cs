using Boxed.Mapping;
using LanguageExt;
using SJP.Schematic.Core;

namespace SJP.Schematic.Serialization.Mapping;

public class DatabaseKeyMapper
    : IImmutableMapper<Dto.DatabaseKey, IDatabaseKey>
    , IImmutableMapper<IDatabaseKey, Dto.DatabaseKey>
    , IImmutableMapper<Dto.DatabaseKey?, Option<IDatabaseKey>>
    , IImmutableMapper<Option<IDatabaseKey>, Dto.DatabaseKey?>
{
    public IDatabaseKey Map(Dto.DatabaseKey source)
    {
        var identifierMapper = MapperRegistry.GetMapper<Dto.Identifier?, Option<Identifier>>();
        var columnMapper = MapperRegistry.GetMapper<Dto.DatabaseColumn, IDatabaseColumn>();

        return new DatabaseKey(
            identifierMapper.Map(source.Name),
            source.KeyType,
            columnMapper.MapList(source.Columns),
            source.IsEnabled
        );
    }

    public Dto.DatabaseKey Map(IDatabaseKey source)
    {
        var identifierMapper = MapperRegistry.GetMapper<Option<Identifier>, Dto.Identifier?>();
        var columnMapper = MapperRegistry.GetMapper<IDatabaseColumn, Dto.DatabaseColumn>();

        return new Dto.DatabaseKey
        {
            Name = identifierMapper.Map(source.Name),
            KeyType = source.KeyType,
            Columns = columnMapper.MapList(source.Columns),
            IsEnabled = source.IsEnabled
        };
    }

    public Dto.DatabaseKey? Map(Option<IDatabaseKey> source)
    {
        return source.MatchUnsafe(
            key => Map(key),
            (Dto.DatabaseKey ?)null
        );
    }

    Option<IDatabaseKey> IImmutableMapper<Dto.DatabaseKey?, Option<IDatabaseKey>>.Map(Dto.DatabaseKey? source)
    {
        return source == null
            ? Option<IDatabaseKey>.None
            : Option<IDatabaseKey>.Some(Map(source!));
    }
}
