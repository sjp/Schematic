using System.Threading.Tasks;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Serialization.Mapping.Comments;

namespace SJP.Schematic.Serialization.Tests.Mapping.Comments;

internal static class DatabaseCommentProviderMapperTests
{
    [Test]
    public static void Map_GivenNullSource_ThrowsArgumentNullException()
    {
        var mapper = new DatabaseCommentProviderMapper();

        Assert.That(() => mapper.Map(null, new VerbatimIdentifierResolutionStrategy()), Throws.ArgumentNullException);
    }

    [Test]
    public static void Map_GivenNullIdentifierResolver_ThrowsArgumentNullException()
    {
        var mapper = new DatabaseCommentProviderMapper();

        Assert.That(() => mapper.Map(EmptyDto, null), Throws.ArgumentNullException);
    }

    [Test]
    public static async Task Map_GivenIdentifierResolver_UsesResolverForObjectLookup()
    {
        var mapper = new DatabaseCommentProviderMapper();
        var resolver = new RecordingIdentifierResolutionStrategy();

        var commentProvider = mapper.Map(EmptyDto, resolver);
        _ = await commentProvider.GetTableComments("test_table").IsSome;

        Assert.That(resolver.ResolvedNames, Does.Contain((Identifier)"test_table"));
    }

    private static Dto.Comments.DatabaseCommentProvider EmptyDto => new()
    {
        IdentifierDefaults = new Dto.IdentifierDefaults { Schema = "main" },
        TableComments = [],
        ViewComments = [],
        SequenceComments = [],
        SynonymComments = [],
        RoutineComments = [],
    };
}