using LanguageExt;
using Moq;
using NUnit.Framework;
using SJP.Schematic.Tests.Utilities;

namespace SJP.Schematic.Core.Tests;

[TestFixture]
internal static class DatabaseRoutineTests
{
    [Test]
    public static void Ctor_GivenNullName_ThrowsArgumentNullException()
    {
        const string definition = "create function test_function...";

        Assert.That(() => new DatabaseRoutine(null, definition), Throws.ArgumentNullException);
    }

    [Test]
    public static void Ctor_GivenNullDefinition_ThrowsArgumentNullException()
    {
        Identifier routineName = "test_routine";

        Assert.That(() => new DatabaseRoutine(routineName, null!), Throws.ArgumentNullException);
    }

    [TestCase("")]
    [TestCase("    ")]
    public static void Ctor_GivenEmptyOrWhiteSpaceDefinition_ThrowsArgumentException(string definition)
    {
        Identifier routineName = "test_routine";

        Assert.That(() => new DatabaseRoutine(routineName, definition), Throws.ArgumentException);
    }

    [Test]
    public static void Name_PropertyGet_EqualsCtorArg()
    {
        Identifier routineName = "test_routine";
        const string definition = "create function test_function...";

        var routine = new DatabaseRoutine(routineName, definition);

        Assert.That(routine.Name, Is.EqualTo(routineName));
    }

    [Test]
    public static void Definition_PropertyGet_EqualsCtorArg()
    {
        Identifier routineName = "test_routine";
        const string definition = "create function test_function...";

        var routine = new DatabaseRoutine(routineName, definition);

        Assert.That(routine.Definition, Is.EqualTo(definition));
    }

    [TestCase("", "test_routine", "Routine: test_routine")]
    [TestCase("test_schema", "test_routine", "Routine: test_schema.test_routine")]
    public static void ToString_WhenInvoked_ReturnsExpectedString(string schema, string localName, string expectedOutput)
    {
        var routineName = Identifier.CreateQualifiedIdentifier(schema, localName);
        const string definition = "create function test_function...";
        var routine = new DatabaseRoutine(routineName, definition);

        var result = routine.ToString();

        Assert.That(result, Is.EqualTo(expectedOutput));
    }

    [Test]
    public static void Ctor_GivenNameAndDefinitionOnly_DescribesAnUnknownRoutine()
    {
        var routine = new DatabaseRoutine("test_routine", "create function test_function...");

        using (Assert.EnterMultipleScope())
        {
            Assert.That(routine.RoutineType, Is.EqualTo(RoutineType.Unknown));
            Assert.That(routine.Language, OptionIs.None);
            Assert.That(routine.Parameters, Is.Empty);
            Assert.That(routine.ReturnType, OptionIs.None);
            Assert.That(routine.Overloads, Is.Empty);
        }
    }

    [Test]
    public static void Ctor_GivenInvalidRoutineType_ThrowsArgumentException()
    {
        const RoutineType routineType = (RoutineType)55;

        Assert.That(
            () => new DatabaseRoutine("test_routine", "create function test_function...", routineType, Option<string>.None, [], Option<IDbType>.None),
            Throws.ArgumentException);
    }

    [Test]
    public static void Ctor_GivenNullParameters_ThrowsArgumentNullException()
    {
        Assert.That(
            () => new DatabaseRoutine("test_routine", "create function test_function...", RoutineType.Function, Option<string>.None, null!, Option<IDbType>.None),
            Throws.ArgumentNullException);
    }

    [Test]
    public static void Ctor_GivenParametersWithNullElement_ThrowsArgumentNullException()
    {
        var parameters = new IDatabaseRoutineParameter[] { null! };

        Assert.That(
            () => new DatabaseRoutine("test_routine", "create function test_function...", RoutineType.Function, Option<string>.None, parameters, Option<IDbType>.None),
            Throws.ArgumentNullException);
    }

    [Test]
    public static void Ctor_GivenOverloadsWithNullElement_ThrowsArgumentNullException()
    {
        var overloads = new IDatabaseRoutineOverload[] { null! };

        Assert.That(
            () => new DatabaseRoutine("test_routine", "create function test_function...", RoutineType.Function, Option<string>.None, [], Option<IDbType>.None, overloads),
            Throws.ArgumentNullException);
    }

    [Test]
    public static void PropertyGets_WhenGivenFullCtorArgs_MatchCtorArgs()
    {
        var returnDbType = Mock.Of<IDbType>();
        var parameter = new DatabaseRoutineParameter(
            Option<Identifier>.Some("test_parameter"),
            Mock.Of<IDbType>(),
            RoutineParameterDirection.Input,
            Option<string>.None,
            1
        );
        var overload = new DatabaseRoutineOverload("create function test_function(integer) ...", [parameter], Option<IDbType>.Some(returnDbType));

        var routine = new DatabaseRoutine(
            "test_routine",
            "create function test_function...",
            RoutineType.Function,
            Option<string>.Some("plpgsql"),
            [parameter],
            Option<IDbType>.Some(returnDbType),
            [overload]
        );

        using (Assert.EnterMultipleScope())
        {
            Assert.That(routine.RoutineType, Is.EqualTo(RoutineType.Function));
            Assert.That(routine.Language.UnwrapSome(), Is.EqualTo("plpgsql"));
            Assert.That(routine.Parameters, Is.EqualTo(new[] { parameter }));
            Assert.That(routine.ReturnType.UnwrapSome(), Is.EqualTo(returnDbType));
            Assert.That(routine.Overloads, Is.EqualTo(new[] { overload }));
        }
    }
}