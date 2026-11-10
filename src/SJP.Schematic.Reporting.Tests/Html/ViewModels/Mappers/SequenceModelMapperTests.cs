using System.Collections.Generic;
using LanguageExt;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Reporting.Html.ViewModels.Mappers;
using SJP.Schematic.Tests.Utilities;

namespace SJP.Schematic.Reporting.Tests.Html.ViewModels.Mappers;

internal static class SequenceModelMapperTests
{
    private static readonly Identifier CounterName = Identifier.CreateQualifiedIdentifier("test_schema", "counter");
    private static readonly Identifier TicketNumberName = Identifier.CreateQualifiedIdentifier("test_schema", "ticket_number");

    [Test]
    public static void Map_GivenSequenceOfUserDefinedType_LinksTypeToItsPage()
    {
        var sequence = CreateSequence(UserDefinedDbTypes.Named(TicketNumberName));

        var model = new SequenceModelMapper().Map(sequence, new UserDefinedTypeTargets([TicketNumberName]));

        Assert.That(model.TypeUrl, Is.EqualTo(UrlRouter.GetUserDefinedTypeUrl(TicketNumberName)));
    }

    [Test]
    public static void Map_GivenSequenceOfBuiltInType_DoesNotLinkType()
    {
        var sequence = CreateSequence(TestDbTypes.BigInteger);

        var model = new SequenceModelMapper().Map(sequence, new UserDefinedTypeTargets([TicketNumberName]));

        Assert.That(model.TypeUrl, Is.Null);
    }

    [Test]
    public static void MapSummary_GivenSequenceOfUserDefinedType_LinksTypeToItsPage()
    {
        var sequence = CreateSequence(UserDefinedDbTypes.Named(TicketNumberName));

        var model = new MainModelMapper().Map(sequence, new UserDefinedTypeTargets([TicketNumberName]));

        Assert.That(model.TypeUrl, Is.EqualTo(UrlRouter.GetUserDefinedTypeUrl(TicketNumberName)));
    }

    private static IEnumerable<TestCaseData> CachesWithNames()
    {
        yield return new TestCaseData((SequenceCache)new SequenceCache.Unknown(), string.Empty).SetArgDisplayNames("Unknown");
        yield return new TestCaseData((SequenceCache)new SequenceCache.None(), "None").SetArgDisplayNames("None");
        yield return new TestCaseData((SequenceCache)new SequenceCache.Sized(20), "20").SetArgDisplayNames("Sized");
        yield return new TestCaseData((SequenceCache)new SequenceCache.EngineDefault(), "Database default").SetArgDisplayNames("EngineDefault");
    }

    [TestCaseSource(nameof(CachesWithNames))]
    public static void Map_GivenSequenceWithCache_DescribesCache(SequenceCache cache, string expectedName)
    {
        var sequence = CreateSequence(TestDbTypes.BigInteger, cache);

        var model = new SequenceModelMapper().Map(sequence, new UserDefinedTypeTargets([]));

        Assert.That(model.Cache, Is.EqualTo(expectedName));
    }

    [TestCaseSource(nameof(CachesWithNames))]
    public static void MapSummary_GivenSequenceWithCache_DescribesCache(SequenceCache cache, string expectedName)
    {
        var sequence = CreateSequence(TestDbTypes.BigInteger, cache);

        var model = new MainModelMapper().Map(sequence, new UserDefinedTypeTargets([]));

        Assert.That(model.Cache, Is.EqualTo(expectedName));
    }

    private static IDatabaseSequence CreateSequence(IDbType type) => CreateSequence(type, new SequenceCache.EngineDefault());

    private static IDatabaseSequence CreateSequence(IDbType type, SequenceCache cache)
    {
        return new DatabaseSequence(
            CounterName,
            type,
            1,
            1,
            Option<decimal>.None,
            Option<decimal>.None,
            false,
            cache,
            true);
    }
}
