using System;
using System.Collections.Generic;
using LanguageExt;
using Moq;
using NUnit.Framework;
using SJP.Schematic.Tests.Utilities;

namespace SJP.Schematic.Core.Tests;

internal static class DatabaseUserDefinedTypeTests
{
    private static IDatabaseColumn CreateColumn(string columnName) => new DatabaseColumn(columnName, TestDbTypes.BigInteger, true, Option<DatabaseDefaultValue>.None, Option<IAutoIncrement>.None);

    private static IDatabaseCheckConstraint CreateCheck() => new DatabaseCheckConstraint(Option<Identifier>.Some("ck_test"), "([value] > 0)", true);

    private static IEnumerable<TestCaseData> EveryCase()
    {
        yield return new TestCaseData((DatabaseUserDefinedType)new DatabaseUserDefinedType.Unknown("test_type")).SetArgDisplayNames(nameof(DatabaseUserDefinedType.Unknown));
        yield return new TestCaseData((DatabaseUserDefinedType)new DatabaseUserDefinedType.Alias("test_type", TestDbTypes.BigInteger)).SetArgDisplayNames(nameof(DatabaseUserDefinedType.Alias));
        yield return new TestCaseData((DatabaseUserDefinedType)new DatabaseUserDefinedType.Domain("test_type", TestDbTypes.BigInteger)).SetArgDisplayNames(nameof(DatabaseUserDefinedType.Domain));
        yield return new TestCaseData((DatabaseUserDefinedType)new DatabaseUserDefinedType.Enum("test_type", ["a"])).SetArgDisplayNames(nameof(DatabaseUserDefinedType.Enum));
        yield return new TestCaseData((DatabaseUserDefinedType)new DatabaseUserDefinedType.Composite("test_type", [CreateColumn("attr")])).SetArgDisplayNames(nameof(DatabaseUserDefinedType.Composite));
        yield return new TestCaseData((DatabaseUserDefinedType)new DatabaseUserDefinedType.Range("test_type", TestDbTypes.BigInteger)).SetArgDisplayNames(nameof(DatabaseUserDefinedType.Range));
        yield return new TestCaseData((DatabaseUserDefinedType)new DatabaseUserDefinedType.Table("test_type", [CreateColumn("col")])).SetArgDisplayNames(nameof(DatabaseUserDefinedType.Table));
        yield return new TestCaseData((DatabaseUserDefinedType)new DatabaseUserDefinedType.Clr("test_type")).SetArgDisplayNames(nameof(DatabaseUserDefinedType.Clr));
        yield return new TestCaseData((DatabaseUserDefinedType)new DatabaseUserDefinedType.Collection("test_type", TestDbTypes.BigInteger)).SetArgDisplayNames(nameof(DatabaseUserDefinedType.Collection));
    }

    [TestCaseSource(nameof(EveryCase))]
    public static void Name_WhenReadThroughUnion_EqualsCaseName(DatabaseUserDefinedType userDefinedType)
    {
        Assert.That(userDefinedType.Name.LocalName, Is.EqualTo("test_type"));
    }

    [TestCaseSource(nameof(EveryCase))]
    public static void Name_WhenReadThroughDatabaseEntity_EqualsCaseName(DatabaseUserDefinedType userDefinedType)
    {
        IDatabaseEntity entity = userDefinedType;

        Assert.That(entity.Name.LocalName, Is.EqualTo("test_type"));
    }

    [TestCaseSource(nameof(EveryCase))]
    public static void ToString_WhenInvoked_DescribesTypeName(DatabaseUserDefinedType userDefinedType)
    {
        Assert.That(userDefinedType.ToString(), Is.EqualTo("Type: test_type"));
    }

    [TestCase("", "test_type", "Type: test_type")]
    [TestCase("test_schema", "test_type", "Type: test_schema.test_type")]
    public static void ToString_WhenInvoked_ReturnsExpectedString(string schema, string localName, string expectedOutput)
    {
        var typeName = Identifier.CreateQualifiedIdentifier(schema, localName);
        DatabaseUserDefinedType userDefinedType = new DatabaseUserDefinedType.Alias(typeName, TestDbTypes.BigInteger);

        Assert.That(userDefinedType.ToString(), Is.EqualTo(expectedOutput));
    }

    [Test]
    public static void ToString_WhenDefault_ReturnsEmptyString()
    {
        DatabaseUserDefinedType userDefinedType = default;

        Assert.That(userDefinedType.ToString(), Is.Empty);
    }

    [Test]
    public static void Unknown_GivenNullName_ThrowsArgNullException()
    {
        Assert.That(
            () => new DatabaseUserDefinedType.Unknown(null),
            Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("name"));
    }

    [Test]
    public static void Unknown_WhenCreatedWithoutDefinition_HasNoDefinition()
    {
        var userDefinedType = new DatabaseUserDefinedType.Unknown("test_type");

        Assert.That(userDefinedType.Definition, OptionIs.None);
    }

    [Test]
    public static void Unknown_GivenDefinition_PropertyEqualsCtorArg()
    {
        var userDefinedType = new DatabaseUserDefinedType.Unknown("test_type", Option<string>.Some("create type ..."));

        Assert.That(userDefinedType.Definition.UnwrapSome(), Is.EqualTo("create type ..."));
    }

    [Test]
    public static void Alias_GivenNullName_ThrowsArgNullException()
    {
        Assert.That(
            () => new DatabaseUserDefinedType.Alias(null, TestDbTypes.BigInteger),
            Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("name"));
    }

    [Test]
    public static void Alias_GivenNullAliasedType_ThrowsArgNullException()
    {
        Assert.That(
            () => new DatabaseUserDefinedType.Alias("test_type", null),
            Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("aliasedType"));
    }

    [Test]
    public static void Alias_WhenCreatedWithAliasedTypeOnly_IsNullableWithNoDefault()
    {
        var userDefinedType = new DatabaseUserDefinedType.Alias("test_type", TestDbTypes.BigInteger);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(userDefinedType.AliasedType, Is.EqualTo(TestDbTypes.BigInteger));
            Assert.That(userDefinedType.IsNullable, Is.True);
            Assert.That(userDefinedType.DefaultValue, OptionIs.None);
        }
    }

    [Test]
    public static void Alias_GivenFullDefinition_PropertiesEqualCtorArgs()
    {
        var userDefinedType = new DatabaseUserDefinedType.Alias("test_type", TestDbTypes.BigInteger, false, Option<string>.Some("0"));

        using (Assert.EnterMultipleScope())
        {
            Assert.That(userDefinedType.AliasedType, Is.EqualTo(TestDbTypes.BigInteger));
            Assert.That(userDefinedType.IsNullable, Is.False);
            Assert.That(userDefinedType.DefaultValue.UnwrapSome(), Is.EqualTo("0"));
        }
    }

    [Test]
    public static void Domain_GivenNullName_ThrowsArgNullException()
    {
        Assert.That(
            () => new DatabaseUserDefinedType.Domain(null, TestDbTypes.BigInteger),
            Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("name"));
    }

    [Test]
    public static void Domain_GivenNullAliasedType_ThrowsArgNullException()
    {
        Assert.That(
            () => new DatabaseUserDefinedType.Domain("test_type", null),
            Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("aliasedType"));
    }

    [Test]
    public static void Domain_GivenNullChecks_ThrowsArgNullException()
    {
        Assert.That(
            () => new DatabaseUserDefinedType.Domain("test_type", TestDbTypes.BigInteger, true, Option<string>.None, null),
            Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("checks"));
    }

    [Test]
    public static void Domain_GivenChecksWithNullValue_ThrowsArgNullException()
    {
        Assert.That(
            () => new DatabaseUserDefinedType.Domain("test_type", TestDbTypes.BigInteger, true, Option<string>.None, [null]),
            Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("checks"));
    }

    [Test]
    public static void Domain_WhenCreatedWithAliasedTypeOnly_IsNullableWithNoDefaultOrChecks()
    {
        var userDefinedType = new DatabaseUserDefinedType.Domain("test_type", TestDbTypes.BigInteger);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(userDefinedType.AliasedType, Is.EqualTo(TestDbTypes.BigInteger));
            Assert.That(userDefinedType.IsNullable, Is.True);
            Assert.That(userDefinedType.DefaultValue, OptionIs.None);
            Assert.That(userDefinedType.Checks, Is.Empty);
        }
    }

    [Test]
    public static void Domain_GivenFullDefinition_PropertiesEqualCtorArgs()
    {
        var check = CreateCheck();
        var userDefinedType = new DatabaseUserDefinedType.Domain("test_type", TestDbTypes.BigInteger, false, Option<string>.Some("'abc'"), [check]);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(userDefinedType.AliasedType, Is.EqualTo(TestDbTypes.BigInteger));
            Assert.That(userDefinedType.IsNullable, Is.False);
            Assert.That(userDefinedType.DefaultValue.UnwrapSome(), Is.EqualTo("'abc'"));
            Assert.That(userDefinedType.Checks, Is.EqualTo(new[] { check }));
        }
    }

    [Test]
    public static void Enum_GivenNullName_ThrowsArgNullException()
    {
        Assert.That(
            () => new DatabaseUserDefinedType.Enum(null, ["a"]),
            Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("name"));
    }

    [Test]
    public static void Enum_GivenNullValues_ThrowsArgNullException()
    {
        Assert.That(
            () => new DatabaseUserDefinedType.Enum("test_type", null),
            Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("values"));
    }

    [Test]
    public static void Enum_GivenValuesWithNullValue_ThrowsArgNullException()
    {
        Assert.That(
            () => new DatabaseUserDefinedType.Enum("test_type", [null]),
            Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("values"));
    }

    [Test]
    public static void Enum_GivenValues_PropertyEqualsCtorArg()
    {
        var userDefinedType = new DatabaseUserDefinedType.Enum("test_type", ["a", "b"]);

        Assert.That(userDefinedType.Values, Is.EqualTo(new[] { "a", "b" }));
    }

    [Test]
    public static void Composite_GivenNullName_ThrowsArgNullException()
    {
        Assert.That(
            () => new DatabaseUserDefinedType.Composite(null, []),
            Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("name"));
    }

    [Test]
    public static void Composite_GivenNullAttributes_ThrowsArgNullException()
    {
        Assert.That(
            () => new DatabaseUserDefinedType.Composite("test_type", null),
            Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("attributes"));
    }

    [Test]
    public static void Composite_GivenAttributesWithNullValue_ThrowsArgNullException()
    {
        Assert.That(
            () => new DatabaseUserDefinedType.Composite("test_type", [null]),
            Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("attributes"));
    }

    [Test]
    public static void Composite_WhenCreatedWithoutDefinition_HasNoDefinition()
    {
        var userDefinedType = new DatabaseUserDefinedType.Composite("test_type", []);

        Assert.That(userDefinedType.Definition, OptionIs.None);
    }

    [Test]
    public static void Composite_GivenFullDefinition_PropertiesEqualCtorArgs()
    {
        var attribute = CreateColumn("attr");
        var userDefinedType = new DatabaseUserDefinedType.Composite("test_type", [attribute], Option<string>.Some("create type ..."));

        using (Assert.EnterMultipleScope())
        {
            Assert.That(userDefinedType.Attributes, Is.EqualTo(new[] { attribute }));
            Assert.That(userDefinedType.Definition.UnwrapSome(), Is.EqualTo("create type ..."));
        }
    }

    [Test]
    public static void Range_GivenNullName_ThrowsArgNullException()
    {
        Assert.That(
            () => new DatabaseUserDefinedType.Range(null, TestDbTypes.BigInteger),
            Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("name"));
    }

    [Test]
    public static void Range_GivenNullSubtype_ThrowsArgNullException()
    {
        Assert.That(
            () => new DatabaseUserDefinedType.Range("test_type", null),
            Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("subtype"));
    }

    [Test]
    public static void Range_GivenSubtype_PropertyEqualsCtorArg()
    {
        var userDefinedType = new DatabaseUserDefinedType.Range("test_type", TestDbTypes.BigInteger);

        Assert.That(userDefinedType.Subtype, Is.EqualTo(TestDbTypes.BigInteger));
    }

    [Test]
    public static void Table_GivenNullName_ThrowsArgNullException()
    {
        Assert.That(
            () => new DatabaseUserDefinedType.Table(null, []),
            Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("name"));
    }

    [Test]
    public static void Table_GivenNullColumns_ThrowsArgNullException()
    {
        Assert.That(
            () => new DatabaseUserDefinedType.Table("test_type", null),
            Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("columns"));
    }

    [Test]
    public static void Table_GivenColumnsWithNullValue_ThrowsArgNullException()
    {
        Assert.That(
            () => new DatabaseUserDefinedType.Table("test_type", [null]),
            Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("columns"));
    }

    [Test]
    public static void Table_GivenNullChecks_ThrowsArgNullException()
    {
        Assert.That(
            () => new DatabaseUserDefinedType.Table("test_type", [], null),
            Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("checks"));
    }

    [Test]
    public static void Table_GivenChecksWithNullValue_ThrowsArgNullException()
    {
        Assert.That(
            () => new DatabaseUserDefinedType.Table("test_type", [], [null]),
            Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("checks"));
    }

    [Test]
    public static void Table_WhenCreatedWithColumnsOnly_HasNoChecks()
    {
        var userDefinedType = new DatabaseUserDefinedType.Table("test_type", [CreateColumn("col")]);

        Assert.That(userDefinedType.Checks, Is.Empty);
    }

    [Test]
    public static void Table_GivenFullDefinition_PropertiesEqualCtorArgs()
    {
        var column = CreateColumn("col");
        var check = CreateCheck();
        var userDefinedType = new DatabaseUserDefinedType.Table("test_type", [column], [check]);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(userDefinedType.Columns, Is.EqualTo(new[] { column }));
            Assert.That(userDefinedType.Checks, Is.EqualTo(new[] { check }));
        }
    }

    [Test]
    public static void Clr_GivenNullName_ThrowsArgNullException()
    {
        Assert.That(
            () => new DatabaseUserDefinedType.Clr(null),
            Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("name"));
    }

    [Test]
    public static void Clr_WhenCreatedWithoutExternalName_HasNoExternalName()
    {
        var userDefinedType = new DatabaseUserDefinedType.Clr("test_type");

        Assert.That(userDefinedType.ExternalName, OptionIs.None);
    }

    [Test]
    public static void Clr_GivenExternalName_PropertyEqualsCtorArg()
    {
        var userDefinedType = new DatabaseUserDefinedType.Clr("test_type", Option<string>.Some("TestAssembly.TestNamespace.TestType"));

        Assert.That(userDefinedType.ExternalName.UnwrapSome(), Is.EqualTo("TestAssembly.TestNamespace.TestType"));
    }

    [Test]
    public static void Collection_GivenNullName_ThrowsArgNullException()
    {
        Assert.That(
            () => new DatabaseUserDefinedType.Collection(null, TestDbTypes.BigInteger),
            Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("name"));
    }

    [Test]
    public static void Collection_GivenNullElementType_ThrowsArgNullException()
    {
        Assert.That(
            () => new DatabaseUserDefinedType.Collection("test_type", null),
            Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("elementType"));
    }

    [Test]
    public static void Collection_WhenCreatedWithoutDefinition_HasNoDefinition()
    {
        var userDefinedType = new DatabaseUserDefinedType.Collection("test_type", TestDbTypes.BigInteger);

        Assert.That(userDefinedType.Definition, OptionIs.None);
    }

    [Test]
    public static void Collection_GivenFullDefinition_PropertiesEqualCtorArgs()
    {
        var userDefinedType = new DatabaseUserDefinedType.Collection("test_type", TestDbTypes.BigInteger, Option<string>.Some("create type ..."));

        using (Assert.EnterMultipleScope())
        {
            Assert.That(userDefinedType.ElementType, Is.EqualTo(TestDbTypes.BigInteger));
            Assert.That(userDefinedType.Definition.UnwrapSome(), Is.EqualTo("create type ..."));
        }
    }

    [Test]
    public static void Collections_WhenSourceCollectionsMutatedAfterConstruction_RemainUnchanged()
    {
        var values = new List<string> { "small" };
        var attributes = new List<IDatabaseColumn> { Mock.Of<IDatabaseColumn>() };
        var columns = new List<IDatabaseColumn> { Mock.Of<IDatabaseColumn>() };
        var domainChecks = new List<IDatabaseCheckConstraint> { Mock.Of<IDatabaseCheckConstraint>() };
        var tableChecks = new List<IDatabaseCheckConstraint> { Mock.Of<IDatabaseCheckConstraint>() };

        var enumType = new DatabaseUserDefinedType.Enum("test_type", values);
        var compositeType = new DatabaseUserDefinedType.Composite("test_type", attributes);
        var domainType = new DatabaseUserDefinedType.Domain("test_type", TestDbTypes.BigInteger, true, Option<string>.None, domainChecks);
        var tableType = new DatabaseUserDefinedType.Table("test_type", columns, tableChecks);

        values.Add("large");
        attributes.Add(Mock.Of<IDatabaseColumn>());
        columns.Add(Mock.Of<IDatabaseColumn>());
        domainChecks.Add(Mock.Of<IDatabaseCheckConstraint>());
        tableChecks.Add(Mock.Of<IDatabaseCheckConstraint>());

        using (Assert.EnterMultipleScope())
        {
            Assert.That(enumType.Values, Is.EqualTo(new[] { "small" }));
            Assert.That(compositeType.Attributes, Has.Count.EqualTo(1));
            Assert.That(domainType.Checks, Has.Count.EqualTo(1));
            Assert.That(tableType.Columns, Has.Count.EqualTo(1));
            Assert.That(tableType.Checks, Has.Count.EqualTo(1));
        }
    }
}
