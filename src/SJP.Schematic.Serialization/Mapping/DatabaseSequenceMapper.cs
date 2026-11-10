using System;
using Boxed.Mapping;
using LanguageExt;
using SJP.Schematic.Core;

namespace SJP.Schematic.Serialization.Mapping;

/// <summary>
/// Maps a database sequence between its core and serialized representations.
/// </summary>
public class DatabaseSequenceMapper
    : IImmutableMapper<Dto.DatabaseSequence, IDatabaseSequence>
    , IImmutableMapper<IDatabaseSequence, Dto.DatabaseSequence>
{
    /// <summary>
    /// Maps a serialized sequence to its core representation.
    /// </summary>
    /// <param name="source">A serialized sequence.</param>
    /// <returns>A sequence.</returns>
    public IDatabaseSequence Map(Dto.DatabaseSequence source)
    {
        var identifierMapper = MapperRegistry.GetMapper<Dto.Identifier, Identifier>();
        var decimalMapper = MapperRegistry.GetMapper<decimal?, Option<decimal>>();
        var dbTypeMapper = MapperRegistry.GetMapper<Dto.DbType, IDbType>();

        return new DatabaseSequence(
            identifierMapper.Map(source.SequenceName),
            dbTypeMapper.Map(source.Type),
            source.Start,
            source.Increment,
            decimalMapper.Map(source.MinValue),
            decimalMapper.Map(source.MaxValue),
            source.Cycle,
            MapCache(source),
            source.IsOrdered
        );
    }

    /// <summary>
    /// Maps a sequence to its serialized representation.
    /// </summary>
    /// <param name="source">A sequence.</param>
    /// <returns>A serialized sequence.</returns>
    public Dto.DatabaseSequence Map(IDatabaseSequence source)
    {
        var identifierMapper = MapperRegistry.GetMapper<Identifier, Dto.Identifier>();
        var decimalMapper = MapperRegistry.GetMapper<Option<decimal>, decimal?>();
        var dbTypeMapper = MapperRegistry.GetMapper<IDbType, Dto.DbType>();
        var (cacheMode, cacheSize) = MapCache(source.Cache);

        return new Dto.DatabaseSequence
        {
            SequenceName = identifierMapper.Map(source.Name),
            Type = dbTypeMapper.Map(source.Type),
            Start = source.Start,
            Increment = source.Increment,
            MinValue = decimalMapper.Map(source.MinValue),
            MaxValue = decimalMapper.Map(source.MaxValue),
            Cycle = source.Cycle,
            CacheMode = cacheMode,
            CacheSize = cacheSize,
            IsOrdered = source.IsOrdered,
        };
    }

    // a sized cache written without a size, which no dialect produces, reads back as an unknown
    // cache rather than failing, as the size is the one thing a sized cache is meant to report
    private static SequenceCache MapCache(Dto.DatabaseSequence source) => source.CacheMode switch
    {
        Dto.SequenceCacheMode.Unknown => new SequenceCache.Unknown(),
        Dto.SequenceCacheMode.None => new SequenceCache.None(),
        Dto.SequenceCacheMode.Sized when source.CacheSize is int size => new SequenceCache.Sized(size),
        Dto.SequenceCacheMode.Sized => new SequenceCache.Unknown(),
        Dto.SequenceCacheMode.EngineDefault => new SequenceCache.EngineDefault(),
        _ => throw new ArgumentException($"The {nameof(Dto.SequenceCacheMode)} provided must be a valid enum.", nameof(source)),
    };

    private static (Dto.SequenceCacheMode CacheMode, int? CacheSize) MapCache(SequenceCache cache) => cache switch
    {
        SequenceCache.Unknown => (Dto.SequenceCacheMode.Unknown, null),
        SequenceCache.None => (Dto.SequenceCacheMode.None, null),
        SequenceCache.Sized sized => (Dto.SequenceCacheMode.Sized, sized.Size),
        SequenceCache.EngineDefault => (Dto.SequenceCacheMode.EngineDefault, null),
    };
}