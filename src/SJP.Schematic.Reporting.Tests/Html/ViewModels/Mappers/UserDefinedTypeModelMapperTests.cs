using System.Linq;
using LanguageExt;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Reporting.Html.ViewModels.Mappers;
using SJP.Schematic.Tests.Utilities;

namespace SJP.Schematic.Reporting.Tests.Html.ViewModels.Mappers;

internal static class UserDefinedTypeModelMapperTests
{
    private static readonly Identifier MoodName = Identifier.CreateQualifiedIdentifier("test_schema", "mood");
    private static readonly Identifier FeelingName = Identifier.CreateQualifiedIdentifier("test_schema", "feeling");

    [Test]
    public static void Map_GivenAttributesOfUserDefinedAndBuiltInTypes_LinksOnlyTheUserDefinedOne()
    {
        IDatabaseColumn[] attributes =
        [
            new DatabaseColumn("intensity", TestDbTypes.BigInteger, false, Option<DatabaseDefaultValue>.None, null),
            new DatabaseColumn("mood", UserDefinedDbTypes.Named(MoodName), true, Option<DatabaseDefaultValue>.None, null),
        ];
        var feeling = new DatabaseUserDefinedType.Composite(FeelingName, attributes);

        var model = new UserDefinedTypeModelMapper().Map(feeling, new UserDefinedTypeTargets([MoodName, FeelingName]));
        var mappedAttributes = model.Attributes.ToList();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(mappedAttributes[0].TypeUrl, Is.Null);
            Assert.That(mappedAttributes[1].TypeUrl, Is.EqualTo(UrlRouter.GetUserDefinedTypeUrl(MoodName)));
        }
    }

    [Test]
    public static void Map_GivenDomainOverUserDefinedType_LinksBaseTypeToItsPage()
    {
        var domain = CreateDomain();

        var model = new UserDefinedTypeModelMapper().Map(domain, new UserDefinedTypeTargets([MoodName, FeelingName]));

        Assert.That(model.BaseTypeUrl, Is.EqualTo(UrlRouter.GetUserDefinedTypeUrl(MoodName)));
    }

    [Test]
    public static void MapSummary_GivenDomainOverUserDefinedType_LinksBaseTypeToItsPage()
    {
        var domain = CreateDomain();

        var model = new MainModelMapper().Map(domain, new UserDefinedTypeTargets([MoodName, FeelingName]));

        Assert.That(model.BaseTypeUrl, Is.EqualTo(UrlRouter.GetUserDefinedTypeUrl(MoodName)));
    }

    [Test]
    public static void Map_GivenDomainOverBuiltInType_DoesNotLinkBaseType()
    {
        var domain = new DatabaseUserDefinedType.Domain(FeelingName, TestDbTypes.BigInteger);

        var model = new UserDefinedTypeModelMapper().Map(domain, new UserDefinedTypeTargets([MoodName, FeelingName]));

        Assert.That(model.BaseTypeUrl, Is.Null);
    }

    private static DatabaseUserDefinedType CreateDomain()
    {
        return new DatabaseUserDefinedType.Domain(FeelingName, UserDefinedDbTypes.Named(MoodName));
    }
}
