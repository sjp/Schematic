using System.Linq;
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
    public IDatabaseKey Map(Dto.DatabaseKey source) => Map(source, ColumnLookup.Empty);

    internal IDatabaseKey Map(Dto.DatabaseKey source, ColumnLookup columns)
    {
        var identifierMapper = MapperRegistry.GetMapper<Dto.Identifier?, Option<Identifier>>();
        var indexMapper = (IndexMapper)MapperRegistry.GetMapper<Dto.DatabaseIndex, IDatabaseIndex>();

        var backingIndex = source.BackingIndex != null
            ? Option<IDatabaseIndex>.Some(indexMapper.Map(source.BackingIndex, columns))
            : Option<IDatabaseIndex>.None;

        return new DatabaseKey(
            identifierMapper.Map(source.Name),
            source.KeyType,
            columns.ResolveList(source.Columns),
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
    public Dto.DatabaseKey Map(IDatabaseKey source) => Map(source, new SerializedObjectCache());

    internal Dto.DatabaseKey Map(IDatabaseKey source, SerializedObjectCache cache)
    {
        if (cache.Keys.TryGetValue(source, out var cached))
            return cached;

        var identifierMapper = MapperRegistry.GetMapper<Option<Identifier>, Dto.Identifier?>();
        var columnMapper = (DatabaseColumnMapper)MapperRegistry.GetMapper<IDatabaseColumn, Dto.DatabaseColumn>();
        var indexMapper = (IndexMapper)MapperRegistry.GetMapper<IDatabaseIndex, Dto.DatabaseIndex>();

        var result = new Dto.DatabaseKey
        {
            Name = identifierMapper.Map(source.Name),
            KeyType = source.KeyType,
            Columns = source.Columns.Select(column => columnMapper.Map(column, cache)).ToList(),
            IsEnabled = source.IsEnabled,
            BackingIndex = source.BackingIndex.MatchUnsafe(index => indexMapper.Map(index, cache), (Dto.DatabaseIndex?)null),
            IsValidated = source.IsValidated,
            Deferrability = source.Deferrability,
        };

        cache.Keys.Add(source, result);
        return result;
    }

    internal Dto.DatabaseKey? Map(Option<IDatabaseKey> source, SerializedObjectCache cache)
    {
        return source.MatchUnsafe(
            key => Map(key, cache),
            (Dto.DatabaseKey?)null
        );
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