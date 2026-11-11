using System;
using System.Collections.Generic;
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
            new DatabaseColumn("intensity", TestDbTypes.BigInteger, false, Option<DatabaseDefaultValue>.None, new ColumnValueSource.Stored()),
            new DatabaseColumn("mood", UserDefinedDbTypes.Named(MoodName), true, Option<DatabaseDefaultValue>.None, new ColumnValueSource.Stored()),
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

    private static IDatabaseColumn CreateColumn(string columnName) => new DatabaseColumn(columnName, TestDbTypes.BigInteger, true, Option<DatabaseDefaultValue>.None, new ColumnValueSource.Stored());

    private static IDatabaseCheckConstraint CreateCheck() => new DatabaseCheckConstraint(Option<Identifier>.Some("ck_test"), "value > 0", true);

    // each case with the report fields it fills: kind, base type, nullability, default, definition,
    // and the number of enum values, attributes and checks
    private static IEnumerable<TestCaseData> EveryCase()
    {
        yield return new TestCaseData((DatabaseUserDefinedType)new DatabaseUserDefinedType.Unknown(FeelingName, "create type feeling"), "", "", true, "", "create type feeling", 0, 0, 0).SetArgDisplayNames(nameof(DatabaseUserDefinedType.Unknown));
        yield return new TestCaseData((DatabaseUserDefinedType)new DatabaseUserDefinedType.Alias(FeelingName, TestDbTypes.BigInteger, false, "0"), "Alias", "bigint", false, "0", "", 0, 0, 0).SetArgDisplayNames(nameof(DatabaseUserDefinedType.Alias));
        yield return new TestCaseData((DatabaseUserDefinedType)new DatabaseUserDefinedType.Domain(FeelingName, TestDbTypes.BigInteger, false, "1", [CreateCheck()]), "Domain", "bigint", false, "1", "", 0, 0, 1).SetArgDisplayNames(nameof(DatabaseUserDefinedType.Domain));
        yield return new TestCaseData((DatabaseUserDefinedType)new DatabaseUserDefinedType.Enum(FeelingName, ["happy", "sad"]), "Enum", "", true, "", "", 2, 0, 0).SetArgDisplayNames(nameof(DatabaseUserDefinedType.Enum));
        yield return new TestCaseData((DatabaseUserDefinedType)new DatabaseUserDefinedType.Composite(FeelingName, [CreateColumn("a"), CreateColumn("b")], "create type feeling"), "Composite", "", true, "", "create type feeling", 0, 2, 0).SetArgDisplayNames(nameof(DatabaseUserDefinedType.Composite));
        yield return new TestCaseData((DatabaseUserDefinedType)new DatabaseUserDefinedType.Range(FeelingName, TestDbTypes.BigInteger), "Range", "bigint", true, "", "", 0, 0, 0).SetArgDisplayNames(nameof(DatabaseUserDefinedType.Range));
        yield return new TestCaseData((DatabaseUserDefinedType)new DatabaseUserDefinedType.Table(FeelingName, [CreateColumn("a")], [CreateCheck()]), "Table", "", true, "", "", 0, 1, 1).SetArgDisplayNames(nameof(DatabaseUserDefinedType.Table));
        yield return new TestCaseData((DatabaseUserDefinedType)new DatabaseUserDefinedType.Clr(FeelingName, "TestAssembly.TestType"), "CLR", "", true, "", "TestAssembly.TestType", 0, 0, 0).SetArgDisplayNames(nameof(DatabaseUserDefinedType.Clr));
        yield return new TestCaseData((DatabaseUserDefinedType)new DatabaseUserDefinedType.Collection(FeelingName, TestDbTypes.BigInteger, "create type feeling"), "Collection", "bigint", true, "", "create type feeling", 0, 0, 0).SetArgDisplayNames(nameof(DatabaseUserDefinedType.Collection));
    }

    [TestCaseSource(nameof(EveryCase))]
    public static void Map_GivenTypeOfEachCase_FillsFieldsTheCaseHas(
        DatabaseUserDefinedType userDefinedType,
        string expectedKind,
        string expectedBaseType,
        bool expectedIsNullable,
        string expectedDefaultValue,
        string expectedDefinition,
        int expectedEnumValuesCount,
        int expectedAttributesCount,
        int expectedChecksCount)
    {
        var model = new UserDefinedTypeModelMapper().Map(userDefinedType, new UserDefinedTypeTargets([MoodName, FeelingName]));

        using (Assert.EnterMultipleScope())
        {
            Assert.That(model.Kind, Is.EqualTo(expectedKind));
            Assert.That(model.BaseType, Is.EqualTo(expectedBaseType));
            Assert.That(model.IsNullable, Is.EqualTo(expectedIsNullable));
            Assert.That(model.DefaultValue, Is.EqualTo(expectedDefaultValue));
            Assert.That(model.Definition, Is.EqualTo(expectedDefinition));
            Assert.That(model.EnumValuesCount, Is.EqualTo(expectedEnumValuesCount));
            Assert.That(model.AttributesCount, Is.EqualTo(expectedAttributesCount));
            Assert.That(model.ChecksCount, Is.EqualTo(expectedChecksCount));
        }
    }

    [TestCaseSource(nameof(EveryCase))]
    public static void MapSummary_GivenTypeOfEachCase_FillsFieldsTheCaseHas(
        DatabaseUserDefinedType userDefinedType,
        string expectedKind,
        string expectedBaseType,
        bool expectedIsNullable,
        string _,
        string __,
        int expectedEnumValuesCount,
        int expectedAttributesCount,
        int ___)
    {
        var model = new MainModelMapper().Map(userDefinedType, new UserDefinedTypeTargets([MoodName, FeelingName]));

        using (Assert.EnterMultipleScope())
        {
            Assert.That(model.Kind, Is.EqualTo(expectedKind));
            Assert.That(model.BaseType, Is.EqualTo(expectedBaseType));
            Assert.That(model.IsNullable, Is.EqualTo(expectedIsNullable));
            Assert.That(model.EnumValuesCount, Is.EqualTo(expectedEnumValuesCount));
            Assert.That(model.AttributesCount, Is.EqualTo(expectedAttributesCount));
        }
    }

    [Test]
    public static void Map_GivenTableTypeWithColumnOfUserDefinedType_LinksTheColumnAsAnAttribute()
    {
        var table = new DatabaseUserDefinedType.Table(FeelingName, [new DatabaseColumn("mood", UserDefinedDbTypes.Named(MoodName), true, Option<DatabaseDefaultValue>.None, new ColumnValueSource.Stored())]);

        var model = new UserDefinedTypeModelMapper().Map(table, new UserDefinedTypeTargets([MoodName, FeelingName]));

        Assert.That(model.Attributes.Single().TypeUrl, Is.EqualTo(UrlRouter.GetUserDefinedTypeUrl(MoodName)));
    }

    [Test]
    public static void Map_GivenCollectionOfUserDefinedType_LinksElementTypeAsBaseType()
    {
        var collection = new DatabaseUserDefinedType.Collection(FeelingName, UserDefinedDbTypes.Named(MoodName));

        var model = new UserDefinedTypeModelMapper().Map(collection, new UserDefinedTypeTargets([MoodName, FeelingName]));

        Assert.That(model.BaseTypeUrl, Is.EqualTo(UrlRouter.GetUserDefinedTypeUrl(MoodName)));
    }

    [Test]
    public static void Map_GivenDefaultType_ThrowsArgumentNullException()
    {
        Assert.That(
            () => new UserDefinedTypeModelMapper().Map(default, new UserDefinedTypeTargets([])),
            Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("userDefinedType")
        );
    }

    [Test]
    public static void MapSummary_GivenDefaultType_ThrowsArgumentNullException()
    {
        Assert.That(
            () => new MainModelMapper().Map(default(DatabaseUserDefinedType), new UserDefinedTypeTargets([])),
            Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("userDefinedType")
        );
    }

    private static DatabaseUserDefinedType CreateDomain()
    {
        return new DatabaseUserDefinedType.Domain(FeelingName, UserDefinedDbTypes.Named(MoodName));
    }
}
