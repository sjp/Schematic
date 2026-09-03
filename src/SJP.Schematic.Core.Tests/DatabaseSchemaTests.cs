using LanguageExt;
using NUnit.Framework;
using SJP.Schematic.Tests.Utilities;

namespace SJP.Schematic.Core.Tests;

[TestFixture]
internal static class DatabaseSchemaTests
{
    [Test]
    public static void Ctor_GivenNullName_ThrowsArgumentNullException()
    {
        Assert.That(() => new DatabaseSchema(null, Option<string>.None, false, false), Throws.ArgumentNullException);
    }

    [Test]
    public static void Name_PropertyGet_EqualsCtorArg()
    {
        Identifier schemaName = "test_schema";
        var schema = new DatabaseSchema(schemaName, Option<string>.None, false, false);

        Assert.That(schema.Name, Is.EqualTo(schemaName));
    }

    [Test]
    public static void Owner_PropertyGetGivenNoneCtorArg_IsNone()
    {
        var schema = new DatabaseSchema("test_schema", Option<string>.None, false, false);

        Assert.That(schema.Owner, OptionIs.None);
    }

    [Test]
    public static void Owner_PropertyGetGivenValidOwnerArg_MatchesOwnerArg()
    {
        const string owner = "test_owner";
        var schema = new DatabaseSchema("test_schema", Option<string>.Some(owner), false, false);

        Assert.That(schema.Owner.UnwrapSome(), Is.EqualTo(owner));
    }

    [Test]
    public static void IsDefault_PropertyGet_EqualsCtorArg()
    {
        var schema = new DatabaseSchema("test_schema", Option<string>.None, true, false);

        Assert.That(schema.IsDefault, Is.True);
    }

    [Test]
    public static void IsSystem_PropertyGet_EqualsCtorArg()
    {
        var schema = new DatabaseSchema("test_schema", Option<string>.None, false, true);

        Assert.That(schema.IsSystem, Is.True);
    }

    [Test]
    public static void ToString_WhenInvoked_ContainsSchemaName()
    {
        var schema = new DatabaseSchema("test_schema", Option<string>.None, false, false);

        Assert.That(schema.ToString(), Is.EqualTo("Schema: test_schema"));
    }
}
