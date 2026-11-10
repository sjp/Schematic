using System;
using System.Linq;
using Boxed.Mapping;
using LanguageExt;
using SJP.Schematic.Core;

namespace SJP.Schematic.Serialization.Mapping;

/// <summary>
/// Maps a key constraint between its core and serialized representations.
/// </summary>
/// <remarks>
/// The serialized form records the kind of key in <see cref="Dto.DatabaseKey.KeyType"/>, so it is
/// unchanged by the kinds of key being separate cases of <see cref="DatabaseKey"/>.
/// </remarks>
public class DatabaseKeyMapper
    : IImmutableMapper<Dto.DatabaseKey, DatabaseKey>
    , IImmutableMapper<DatabaseKey, Dto.DatabaseKey>
    , IImmutableMapper<Dto.DatabaseKey?, Option<DatabaseKey>>
    , IImmutableMapper<Option<DatabaseKey>, Dto.DatabaseKey?>
{
    /// <summary>
    /// Maps a serialized key constraint to its core representation.
    /// </summary>
    /// <param name="source">A serialized key constraint.</param>
    /// <returns>A key constraint of the kind <see cref="Dto.DatabaseKey.KeyType"/> records.</returns>
    /// <exception cref="ArgumentException"><see cref="Dto.DatabaseKey.KeyType"/> of <paramref name="source"/> is an invalid enum value.</exception>
    /// <remarks>A backing index recorded for a foreign key is not mapped, as a foreign key has no backing index.</remarks>
    public DatabaseKey Map(Dto.DatabaseKey source) => Map(source, ColumnLookup.Empty);

    internal DatabaseKey Map(Dto.DatabaseKey source, ColumnLookup columns)
    {
        var identifierMapper = MapperRegistry.GetMapper<Dto.Identifier?, Option<Identifier>>();

        var name = identifierMapper.Map(source.Name);
        var keyColumns = columns.ResolveList(source.Columns);

        return source.KeyType switch
        {
            DatabaseKeyType.Primary => new DatabaseKey.Primary(name, keyColumns, source.IsEnabled, MapBackingIndex(source, columns), source.IsValidated, source.Deferrability),
            DatabaseKeyType.Unique => new DatabaseKey.Unique(name, keyColumns, source.IsEnabled, MapBackingIndex(source, columns), source.IsValidated, source.Deferrability),
            DatabaseKeyType.Foreign => new DatabaseKey.Foreign(name, keyColumns, source.IsEnabled, source.IsValidated, source.Deferrability),
            _ => throw new ArgumentException($"The {nameof(DatabaseKeyType)} provided must be a valid enum.", nameof(source)),
        };
    }

    internal DatabaseKey.Primary MapPrimaryKey(Dto.DatabaseKey source, ColumnLookup columns)
    {
        return Map(source, columns) is DatabaseKey.Primary primaryKey
            ? primaryKey
            : throw UnexpectedKeyType(source, DatabaseKeyType.Primary);
    }

    internal DatabaseKey.Unique MapUniqueKey(Dto.DatabaseKey source, ColumnLookup columns)
    {
        return Map(source, columns) is DatabaseKey.Unique uniqueKey
            ? uniqueKey
            : throw UnexpectedKeyType(source, DatabaseKeyType.Unique);
    }

    internal DatabaseKey.Foreign MapForeignKey(Dto.DatabaseKey source, ColumnLookup columns)
    {
        return Map(source, columns) is DatabaseKey.Foreign foreignKey
            ? foreignKey
            : throw UnexpectedKeyType(source, DatabaseKeyType.Foreign);
    }

    internal CandidateKey MapCandidateKey(Dto.DatabaseKey source, ColumnLookup columns)
    {
        return Map(source, columns) switch
        {
            DatabaseKey.Primary primaryKey => primaryKey,
            DatabaseKey.Unique uniqueKey => uniqueKey,
            DatabaseKey.Foreign => throw new ArgumentException($"A primary or unique key was expected, but the serialized key has a key type of '{source.KeyType}'.", nameof(source)),
        };
    }

    private static Option<IDatabaseIndex> MapBackingIndex(Dto.DatabaseKey source, ColumnLookup columns)
    {
        if (source.BackingIndex == null)
            return Option<IDatabaseIndex>.None;

        var indexMapper = (IndexMapper)MapperRegistry.GetMapper<Dto.DatabaseIndex, IDatabaseIndex>();
        return Option<IDatabaseIndex>.Some(indexMapper.Map(source.BackingIndex, columns));
    }

    private static ArgumentException UnexpectedKeyType(Dto.DatabaseKey source, DatabaseKeyType expected)
    {
        return new ArgumentException($"A key type of '{expected}' was expected, but the serialized key has a key type of '{source.KeyType}'.", nameof(source));
    }

    /// <summary>
    /// Maps a key constraint to its serialized representation.
    /// </summary>
    /// <param name="source">A key constraint.</param>
    /// <returns>A serialized key constraint.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> holds no key.</exception>
    public Dto.DatabaseKey Map(DatabaseKey source) => Map(source, new SerializedObjectCache());

    internal Dto.DatabaseKey Map(DatabaseKey source, SerializedObjectCache cache)
    {
        var key = source.Value ?? throw new ArgumentNullException(nameof(source));
        if (cache.Keys.TryGetValue(key, out var cached))
            return cached;

        var (keyType, backingIndex) = source switch
        {
            DatabaseKey.Primary primaryKey => (DatabaseKeyType.Primary, primaryKey.BackingIndex),
            DatabaseKey.Unique uniqueKey => (DatabaseKeyType.Unique, uniqueKey.BackingIndex),
            DatabaseKey.Foreign => (DatabaseKeyType.Foreign, Option<IDatabaseIndex>.None),
        };

        var identifierMapper = MapperRegistry.GetMapper<Option<Identifier>, Dto.Identifier?>();
        var columnMapper = (DatabaseColumnMapper)MapperRegistry.GetMapper<IDatabaseColumn, Dto.DatabaseColumn>();
        var indexMapper = (IndexMapper)MapperRegistry.GetMapper<IDatabaseIndex, Dto.DatabaseIndex>();

        var result = new Dto.DatabaseKey
        {
            Name = identifierMapper.Map(source.Name),
            KeyType = keyType,
            Columns = source.Columns.Select(column => columnMapper.Map(column, cache)).ToList(),
            IsEnabled = source.IsEnabled,
            BackingIndex = backingIndex.MatchUnsafe(index => indexMapper.Map(index, cache), (Dto.DatabaseIndex?)null),
            IsValidated = source.IsValidated,
            Deferrability = source.Deferrability,
        };

        cache.Keys.Add(key, result);
        return result;
    }

    internal Dto.DatabaseKey? Map(Option<DatabaseKey.Primary> source, SerializedObjectCache cache)
    {
        return source.MatchUnsafe(
            key => Map((DatabaseKey)key, cache),
            (Dto.DatabaseKey?)null
        );
    }

    /// <summary>
    /// Maps an optional key constraint to its serialized representation.
    /// </summary>
    /// <param name="source">A key constraint, if one is defined.</param>
    /// <returns>A serialized key constraint, or <see langword="null"/> when no key is defined.</returns>
    public Dto.DatabaseKey? Map(Option<DatabaseKey> source)
    {
        return source.MatchUnsafe(
            Map,
            (Dto.DatabaseKey?)null
        );
    }

    Option<DatabaseKey> IImmutableMapper<Dto.DatabaseKey?, Option<DatabaseKey>>.Map(Dto.DatabaseKey? source)
    {
        return source == null
            ? Option<DatabaseKey>.None
            : Option<DatabaseKey>.Some(Map(source));
    }
}
