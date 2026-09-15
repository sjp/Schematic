using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Reporting.Html.ViewModels.Mappers;
using SJP.Schematic.Tests.Utilities;

namespace SJP.Schematic.Reporting.Tests.Html.ViewModels.Mappers;

internal static class SynonymTargetsTests
{
    private static readonly Identifier TargetName = Identifier.CreateQualifiedIdentifier("app", "mood");

    [Test]
    public static void GetTargetUrl_GivenNullTargetName_ThrowsArgumentNullException()
    {
        var targets = new SynonymTargets([], [], [], [], [], []);

        Assert.That(() => targets.GetTargetUrl(null), Throws.ArgumentNullException);
    }

    [Test]
    public static void GetTargetUrl_GivenUnknownTarget_ReturnsNone()
    {
        var targets = new SynonymTargets([], [], [], [], [], []);

        var url = targets.GetTargetUrl(TargetName);

        Assert.That(url, OptionIs.None);
    }

    [Test]
    public static void GetTargetUrl_GivenUserDefinedTypeTarget_ReturnsTypeUrl()
    {
        var targets = new SynonymTargets([], [], [], [], [], [TargetName]);

        var url = targets.GetTargetUrl(Identifier.CreateQualifiedIdentifier("APP", "MOOD"));

        Assert.That(url.MatchUnsafe(static u => u.ToString(), static () => null), Is.EqualTo(UrlRouter.GetUserDefinedTypeUrl(TargetName)));
    }

    [Test]
    public static void GetTargetUrl_GivenTargetDifferingOnlyInCasing_ReturnsUrlOfObjectName()
    {
        var tableName = Identifier.CreateQualifiedIdentifier("app", "orders");
        var targets = new SynonymTargets([tableName], [], [], [], [], []);

        var url = targets.GetTargetUrl(Identifier.CreateQualifiedIdentifier("App", "Orders"));

        Assert.That(url.MatchUnsafe(static u => u.ToString(), static () => null), Is.EqualTo(UrlRouter.GetTableUrl(tableName)));
    }

    [Test]
    public static void GetTargetUrl_GivenObjectsDifferingOnlyInCasing_ReturnsUrlOfExactMatch()
    {
        var lowerName = Identifier.CreateQualifiedIdentifier("app", "orders");
        var mixedName = Identifier.CreateQualifiedIdentifier("app", "Orders");
        var targets = new SynonymTargets([lowerName, mixedName], [], [], [], [], []);

        var url = targets.GetTargetUrl(mixedName);

        Assert.That(url.MatchUnsafe(static u => u.ToString(), static () => null), Is.EqualTo(UrlRouter.GetTableUrl(mixedName)));
    }

    [Test]
    public static void GetTargetUrl_GivenNameSharedByRoutineAndUserDefinedType_PrefersRoutine()
    {
        var targets = new SynonymTargets([], [], [], [], [TargetName], [TargetName]);

        var url = targets.GetTargetUrl(TargetName);

        Assert.That(url.MatchUnsafe(static u => u.ToString(), static () => null), Is.EqualTo(UrlRouter.GetRoutineUrl(TargetName)));
    }
}
