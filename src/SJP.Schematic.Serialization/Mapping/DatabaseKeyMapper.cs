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
        var indexMapper = MapperRegistry.GetMapper<Dto.DatabaseIndex, IDatabaseIndex>();

        var backingIndex = source.BackingIndex != null
            ? Option<IDatabaseIndex>.Some(indexMapper.Map(source.BackingIndex))
            : Option<IDatabaseIndex>.None;

        return new DatabaseKey(
            identifierMapper.Map(source.Name),
            source.KeyType,
            columnMapper.MapList(source.Columns),
            source.IsEnabled,
            backingIndex,
            source.IsValidated,
            source.Deferrability
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
        var indexMapper = MapperRegistry.GetMapper<IDatabaseIndex, Dto.DatabaseIndex>();

        return new Dto.DatabaseKey
        {
            Name = identifierMapper.Map(source.Name),
            KeyType = source.KeyType,
            Columns = columnMapper.MapList(source.Columns),
            IsEnabled = source.IsEnabled,
            BackingIndex = source.BackingIndex.MatchUnsafe(indexMapper.Map, (Dto.DatabaseIndex?)null),
            IsValidated = source.IsValidated,
            Deferrability = source.Deferrability,
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