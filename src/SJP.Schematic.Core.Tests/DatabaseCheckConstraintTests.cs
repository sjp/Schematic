using LanguageExt;
using NUnit.Framework;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Tests.Utilities;

namespace SJP.Schematic.Core.Tests;

[TestFixture]
internal static class DatabaseCheckConstraintTests
{
    [TestCase((string)null)]
    [TestCase("")]
    [TestCase("    ")]
    public static void Ctor_GivenNullOrWhiteSpaceDefinition_ThrowsArgumentNullException(string definition)
    {
        Assert.That(() => new DatabaseCheckConstraint(Option<Identifier>.Some("test_check"), definition, true), Throws.ArgumentNullException);
    }

    [Test]
    public static void Name_PropertyGet_EqualsCtorArg()
    {
        Identifier checkName = "test_check";
        var check = new DatabaseCheckConstraint(checkName, "test_check", true);

        Assert.That(check.Name.UnwrapSome(), Is.EqualTo(checkName));
    }

    [Test]
    public static void Definition_PropertyGet_EqualsCtorArg()
    {
        const string checkDefinition = "test_check_definition";
        var check = new DatabaseCheckConstraint(Option<Identifier>.Some("test_check"), checkDefinition, true);

        Assert.That(check.Definition, Is.EqualTo(checkDefinition));
    }

    [Test]
    public static void IsEnabled_WhenTrueProvidedInCtor_ReturnsTrue()
    {
        var check = new DatabaseCheckConstraint(Option<Identifier>.Some("test_check"), "test_check_definition", true);

        Assert.That(check.IsEnabled, Is.True);
    }

    [Test]
    public static void IsEnabled_WhenFalseProvidedInCtor_ReturnsFalse()
    {
        var check = new DatabaseCheckConstraint(Option<Identifier>.Some("test_check"), "test_check_definition", false);

        Assert.That(check.IsEnabled, Is.False);
    }

    [TestCase(null, "Check")]
    [TestCase("test_check", "Check: test_check")]
    public static void ToString_WhenInvoked_ReturnsExpectedValues(string name, string expectedResult)
    {
        var checkName = !name.IsNullOrWhiteSpace()
            ? Option<Identifier>.Some(name)
            : Option<Identifier>.None;

        var check = new DatabaseCheckConstraint(checkName, "test_check_definition", true);
        var result = check.ToString();

        Assert.That(result, Is.EqualTo(expectedResult));
    }
}
