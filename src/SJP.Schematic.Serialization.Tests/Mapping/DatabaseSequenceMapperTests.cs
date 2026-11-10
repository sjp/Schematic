using System;
using System.Collections.Generic;
using LanguageExt;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Serialization.Mapping;
using SJP.Schematic.Tests.Utilities;

namespace SJP.Schematic.Serialization.Tests.Mapping;

internal static class DatabaseSequenceMapperTests
{
    private static DatabaseSequence CreateSequence(SequenceCache cache) => new(
        "test_sequence",
        TestDbTypes.BigInteger,
        1,
        1,
        Option<decimal>.None,
        Option<decimal>.None,
        false,
        cache,
        true
    );

    private static Dto.DatabaseSequence CreateSequenceDto(Dto.SequenceCacheMode cacheMode, int? cacheSize)
    {
        var identifierMapper = new IdentifierMapper();
        var dbTypeMapper = new DbTypeMapper();

        return new Dto.DatabaseSequence
        {
            SequenceName = identifierMapper.Map((Identifier)"test_sequence"),
            Type = dbTypeMapper.Map(TestDbTypes.BigInteger),
            Start = 1,
            Increment = 1,
            Cycle = false,
            IsOrdered = true,
            CacheMode = cacheMode,
            CacheSize = cacheSize,
        };
    }

    private static IEnumerable<TestCaseData> CachesWithModes()
    {
        yield return new TestCaseData((SequenceCache)new SequenceCache.Unknown(), Dto.SequenceCacheMode.Unknown, null).SetArgDisplayNames("Unknown");
        yield return new TestCaseData((SequenceCache)new SequenceCache.None(), Dto.SequenceCacheMode.None, null).SetArgDisplayNames("None");
        yield return new TestCaseData((SequenceCache)new SequenceCache.Sized(20), Dto.SequenceCacheMode.Sized, 20).SetArgDisplayNames("Sized");
        yield return new TestCaseData((SequenceCache)new SequenceCache.EngineDefault(), Dto.SequenceCacheMode.EngineDefault, null).SetArgDisplayNames("EngineDefault");
    }

    [TestCaseSource(nameof(CachesWithModes))]
    public static void Map_GivenCacheOfEachCase_WritesModeAndSize(SequenceCache cache, Dto.SequenceCacheMode expectedMode, int? expectedSize)
    {
        var mapper = new DatabaseSequenceMapper();

        var result = mapper.Map(CreateSequence(cache));

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.CacheMode, Is.EqualTo(expectedMode));
            Assert.That(result.CacheSize, Is.EqualTo(expectedSize));
        }
    }

    [TestCaseSource(nameof(CachesWithModes))]
    public static void Map_GivenCacheOfEachCase_RoundTrips(SequenceCache cache, Dto.SequenceCacheMode _, int? __)
    {
        var mapper = new DatabaseSequenceMapper();

        var result = mapper.Map(mapper.Map(CreateSequence(cache)));

        Assert.That(result.Cache.Value, Is.EqualTo(cache.Value));
    }

    [TestCaseSource(nameof(CachesWithModes))]
    public static void Map_GivenSerializedCacheOfEachMode_ReadsBackAsMatchingCase(SequenceCache expectedCache, Dto.SequenceCacheMode cacheMode, int? cacheSize)
    {
        var mapper = new DatabaseSequenceMapper();
        var dto = CreateSequenceDto(cacheMode, cacheSize);

        var result = mapper.Map(dto);

        Assert.That(result.Cache.Value, Is.EqualTo(expectedCache.Value));
    }

    [Test]
    public static void Map_GivenSerializedSizedCacheWithoutSize_ReadsBackAsUnknownCache()
    {
        var mapper = new DatabaseSequenceMapper();
        var dto = CreateSequenceDto(Dto.SequenceCacheMode.Sized, null);

        var result = mapper.Map(dto);

        Assert.That(result.Cache.Value, Is.EqualTo(new SequenceCache.Unknown()));
    }

    [TestCase(Dto.SequenceCacheMode.Unknown)]
    [TestCase(Dto.SequenceCacheMode.None)]
    [TestCase(Dto.SequenceCacheMode.EngineDefault)]
    public static void Map_GivenSerializedUnsizedCacheWithSize_DropsSize(Dto.SequenceCacheMode cacheMode)
    {
        var mapper = new DatabaseSequenceMapper();
        var dto = CreateSequenceDto(cacheMode, 20);

        var result = mapper.Map(mapper.Map(dto));

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.CacheMode, Is.EqualTo(cacheMode));
            Assert.That(result.CacheSize, Is.Null);
        }
    }

    [Test]
    public static void Map_GivenSerializedCacheWithInvalidMode_ThrowsArgumentException()
    {
        var mapper = new DatabaseSequenceMapper();
        var dto = CreateSequenceDto((Dto.SequenceCacheMode)55, null);

        Assert.That(
            () => mapper.Map(dto),
            Throws.ArgumentException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("source")
        );
    }
}
