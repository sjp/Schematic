using System;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Reporting.Html.ViewModels.Mappers;
using SJP.Schematic.Tests.Utilities;

namespace SJP.Schematic.Reporting.Tests.Html.ViewModels.Mappers;

internal static class UserDefinedTypeTargetsTests
{
    private static readonly Identifier MoodName = Identifier.CreateQualifiedIdentifier("app", "mood");

    [Test]
    public static void Ctor_GivenNullTypeNames_ThrowsArgumentNullException()
    {
        Assert.That(() => new UserDefinedTypeTargets(null!), Throws.ArgumentNullException);
    }

    [Test]
    public static void Ctor_GivenNullTypeName_ThrowsArgumentNullException()
    {
        Assert.That(() => new UserDefinedTypeTargets([null!]), Throws.ArgumentNullException);
    }

    [Test]
    public static void GetTypeUrl_GivenNullType_ThrowsArgumentNullException()
    {
        var targets = new UserDefinedTypeTargets([]);

        Assert.That(() => targets.GetTypeUrl((IDbType)null!), Throws.ArgumentNullException);
    }

    [Test]
    public static void GetTypeUrl_GivenNullTypeName_ThrowsArgumentNullException()
    {
        var targets = new UserDefinedTypeTargets([]);

        Assert.That(() => targets.GetTypeUrl((Identifier)null!), Throws.ArgumentNullException);
    }

    [Test]
    public static void GetTypeUrl_GivenTypeNotInReport_ReturnsNone()
    {
        var targets = new UserDefinedTypeTargets([]);

        Assert.That(targets.GetTypeUrl(UserDefinedDbTypes.Named(MoodName)), OptionIs.None);
    }

    [Test]
    public static void GetTypeUrl_GivenUserDefinedType_ReturnsTypeUrl()
    {
        var targets = new UserDefinedTypeTargets([MoodName]);

        var url = targets.GetTypeUrl(UserDefinedDbTypes.Named(MoodName));

        Assert.That(UrlOf(url), Is.EqualTo(UrlRouter.GetUserDefinedTypeUrl(MoodName)));
    }

    [Test]
    public static void GetTypeUrl_GivenTypeNameDifferingOnlyInCasing_ReturnsUrlOfTypeName()
    {
        var targets = new UserDefinedTypeTargets([MoodName]);

        var url = targets.GetTypeUrl(UserDefinedDbTypes.Named(Identifier.CreateQualifiedIdentifier("APP", "MOOD")));

        Assert.That(UrlOf(url), Is.EqualTo(UrlRouter.GetUserDefinedTypeUrl(MoodName)));
    }

    [Test]
    public static void GetTypeUrl_GivenTypesDifferingOnlyInCasing_ReturnsUrlOfExactMatch()
    {
        var mixedName = Identifier.CreateQualifiedIdentifier("app", "Mood");
        var targets = new UserDefinedTypeTargets([MoodName, mixedName]);

        var url = targets.GetTypeUrl(UserDefinedDbTypes.Named(mixedName));

        Assert.That(UrlOf(url), Is.EqualTo(UrlRouter.GetUserDefinedTypeUrl(mixedName)));
    }

    [Test]
    public static void GetTypeUrl_GivenBuiltInTypeSharingLocalNameWithUserDefinedType_ReturnsNone()
    {
        var targets = new UserDefinedTypeTargets([Identifier.CreateQualifiedIdentifier("app", "text")]);

        var builtIn = UserDefinedDbTypes.Named(Identifier.CreateQualifiedIdentifier("pg_catalog", "text"));

        Assert.That(targets.GetTypeUrl(builtIn), OptionIs.None);
    }

    [Test]
    public static void GetTypeUrl_GivenTypeNameWithoutSchema_ReturnsNone()
    {
        var targets = new UserDefinedTypeTargets([MoodName]);

        Assert.That(targets.GetTypeUrl(UserDefinedDbTypes.Named("mood")), OptionIs.None);
    }

    [Test]
    public static void GetTypeUrl_GivenArrayOfUserDefinedType_ReturnsElementTypeUrl()
    {
        var targets = new UserDefinedTypeTargets([MoodName]);

        var arrayType = UserDefinedDbTypes.ArrayOf(
            Identifier.CreateQualifiedIdentifier("app", "_mood"),
            UserDefinedDbTypes.Named(MoodName));

        Assert.That(UrlOf(targets.GetTypeUrl(arrayType)), Is.EqualTo(UrlRouter.GetUserDefinedTypeUrl(MoodName)));
    }

    [Test]
    public static void GetTypeUrl_GivenArrayOfBuiltInType_ReturnsNone()
    {
        var targets = new UserDefinedTypeTargets([MoodName]);

        var arrayType = UserDefinedDbTypes.ArrayOf(
            Identifier.CreateQualifiedIdentifier("pg_catalog", "_text"),
            UserDefinedDbTypes.Named(Identifier.CreateQualifiedIdentifier("pg_catalog", "text")));

        Assert.That(targets.GetTypeUrl(arrayType), OptionIs.None);
    }

    private static string UrlOf(LanguageExt.Option<Uri> url)
        => url.MatchUnsafe(static u => u.ToString(), static () => null);
}
