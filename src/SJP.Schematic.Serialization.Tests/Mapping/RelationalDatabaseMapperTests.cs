using System.Threading.Tasks;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Serialization.Mapping;

namespace SJP.Schematic.Serialization.Tests.Mapping;

internal static class RelationalDatabaseMapperTests
{
    [Test]
    public static void Map_GivenNullSource_ThrowsArgumentNullException()
    {
        var mapper = new RelationalDatabaseMapper();

        Assert.That(() => mapper.Map(null, new VerbatimIdentifierResolutionStrategy()), Throws.ArgumentNullException);
    }

    [Test]
    public static void Map_GivenNullIdentifierResolver_ThrowsArgumentNullException()
    {
        var mapper = new RelationalDatabaseMapper();

        Assert.That(() => mapper.Map(EmptyDto, null), Throws.ArgumentNullException);
    }

    [Test]
    public static async Task Map_GivenIdentifierResolver_UsesResolverForObjectLookup()
    {
        var mapper = new RelationalDatabaseMapper();
        var resolver = new RecordingIdentifierResolutionStrategy();

        var database = mapper.Map(EmptyDto, resolver);
        _ = await database.GetTable("test_table").IsSome;

        Assert.That(resolver.ResolvedNames, Does.Contain((Identifier)"test_table"));
    }

    private static Dto.RelationalDatabase EmptyDto => new()
    {
        IdentifierDefaults = new Dto.IdentifierDefaults { Schema = "main" },
        Tables = [],
        Views = [],
        Sequences = [],
        Synonyms = [],
        Routines = [],
    };
}