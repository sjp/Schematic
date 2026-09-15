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
            new DatabaseColumn("intensity", TestDbTypes.BigInteger, false, null, null),
            new DatabaseColumn("mood", UserDefinedDbTypes.Named(MoodName), true, null, null),
        ];
        var feeling = new DatabaseUserDefinedType(
            FeelingName,
            UserDefinedTypeKind.Composite,
            Option<IDbType>.None,
            [],
            attributes,
            [],
            true,
            Option<string>.None,
            Option<string>.None);

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
        var domain = new DatabaseUserDefinedType(
            FeelingName,
            UserDefinedTypeKind.Domain,
            Option<IDbType>.Some(TestDbTypes.BigInteger));

        var model = new UserDefinedTypeModelMapper().Map(domain, new UserDefinedTypeTargets([MoodName, FeelingName]));

        Assert.That(model.BaseTypeUrl, Is.Null);
    }

    private static IDatabaseUserDefinedType CreateDomain()
    {
        return new DatabaseUserDefinedType(
            FeelingName,
            UserDefinedTypeKind.Domain,
            Option<IDbType>.Some(UserDefinedDbTypes.Named(MoodName)));
    }
}
