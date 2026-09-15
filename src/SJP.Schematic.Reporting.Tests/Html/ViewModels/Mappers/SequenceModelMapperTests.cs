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

    private static IDatabaseSequence CreateSequence(IDbType type)
    {
        return new DatabaseSequence(
            CounterName,
            type,
            1,
            1,
            Option<decimal>.None,
            Option<decimal>.None,
            false,
            SequenceCacheMode.EngineDefault,
            Option<int>.None,
            true);
    }
}
