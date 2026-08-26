using Boxed.Mapping;
using LanguageExt;
using SJP.Schematic.Core;

namespace SJP.Schematic.Serialization.Mapping;

/// <summary>
/// Maps a key constraint between its core and serialized representations.
/// </summary>
public class DatabaseKeyMapper
    : IImmutableMapper<Dto.DatabaseKey, IDatabaseKey>
    , IImmutableMapper<IDatabaseKey, Dto.DatabaseKey>
    , IImmutableMapper<Dto.DatabaseKey?, Option<IDatabaseKey>>
    , IImmutableMapper<Option<IDatabaseKey>, Dto.DatabaseKey?>
{
    /// <summary>
    /// Maps a serialized key constraint to its core representation.
    /// </summary>
    /// <param name="source">A serialized key constraint.</param>
    /// <returns>A key constraint.</returns>
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

    /// <summary>
    /// Maps a key constraint to its serialized representation.
    /// </summary>
    /// <param name="source">A key constraint.</param>
    /// <returns>A serialized key constraint.</returns>
    public Dto.DatabaseKey Map(IDatabaseKey source)
    {
        var identifierMapper = MapperRegistry.GetMapper<Option<Identifier>, Dto.Identifier?>();
        var columnMapper = MapperRegistry.GetMapper<IDatabaseColumn, Dto.DatabaseColumn>();

        return new Dto.DatabaseKey
        {
            Name = identifierMapper.Map(source.Name),
            KeyType = source.KeyType,
            Columns = columnMapper.MapList(source.Columns),
            IsEnabled = source.IsEnabled,
        };
    }

    /// <summary>
    /// Maps an optional key constraint to its serialized representation.
    /// </summary>
    /// <param name="source">A key constraint, if one is defined.</param>
    /// <returns>A serialized key constraint, or <see langword="null"/> when no key is defined.</returns>
    public Dto.DatabaseKey? Map(Option<IDatabaseKey> source)
    {
        return source.MatchUnsafe(
            Map,
            (Dto.DatabaseKey?)null
        );
    }

    Option<IDatabaseKey> IImmutableMapper<Dto.DatabaseKey?, Option<IDatabaseKey>>.Map(Dto.DatabaseKey? source)
    {
        return source == null
            ? Option<IDatabaseKey>.None
            : Option<IDatabaseKey>.Some(Map(source));
    }
}