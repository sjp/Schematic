using System;
using LanguageExt;
using Moq;
using NUnit.Framework;
using SJP.Schematic.Tests.Utilities;

namespace SJP.Schematic.Core.Tests;

[TestFixture]
internal static class DatabaseRoutineOverloadTests
{
    private const string Definition = "create function test_function(integer) ...";

    [Test]
    public static void Ctor_GivenNullDefinition_ThrowsArgumentNullException()
    {
        Assert.That(() => new DatabaseRoutineOverload(null!, [], Option<IDbType>.None), Throws.ArgumentNullException);
    }

    [TestCase("")]
    [TestCase("    ")]
    public static void Ctor_GivenEmptyOrWhiteSpaceDefinition_ThrowsArgumentException(string definition)
    {
        Assert.That(() => new DatabaseRoutineOverload(definition, [], Option<IDbType>.None), Throws.ArgumentException);
    }

    [Test]
    public static void Ctor_GivenNullParameters_ThrowsArgumentNullException()
    {
        Assert.That(() => new DatabaseRoutineOverload(Definition, null!, Option<IDbType>.None), Throws.ArgumentNullException);
    }

    [Test]
    public static void Ctor_GivenParametersWithNullElement_ThrowsArgumentNullException()
    {
        var parameters = new IDatabaseRoutineParameter[] { null! };

        Assert.That(() => new DatabaseRoutineOverload(Definition, parameters, Option<IDbType>.None), Throws.ArgumentNullException);
    }

    [Test]
    public static void PropertyGets_WhenGivenValidCtorArgs_MatchCtorArgs()
    {
        var returnDbType = Mock.Of<IDbType>();
        var parameter = new DatabaseRoutineParameter(
            Option<Identifier>.Some("test_parameter"),
            Mock.Of<IDbType>(),
            RoutineParameterDirection.Input,
            Option<string>.None,
            1
        );

        var overload = new DatabaseRoutineOverload(Definition, [parameter], Option<IDbType>.Some(returnDbType));

        using (Assert.EnterMultipleScope())
        {
            Assert.That(overload.Definition, Is.EqualTo(Definition));
            Assert.That(overload.Parameters, Is.EqualTo(new[] { parameter }));
            Assert.That(overload.ReturnType.UnwrapSome(), Is.EqualTo(returnDbType));
        }
    }

    [Test]
    public static void ToString_WhenInvoked_ListsParameterTypes()
    {
        var typeMock = new Mock<IDbType>();
        typeMock.Setup(static t => t.Definition).Returns("integer");
        var parameter = new DatabaseRoutineParameter(
            Option<Identifier>.None,
            typeMock.Object,
            RoutineParameterDirection.Input,
            Option<string>.None,
            1
        );

        var overload = new DatabaseRoutineOverload(Definition, [parameter], Option<IDbType>.None);

        Assert.That(overload.ToString(), Is.EqualTo("Overload: (integer)"));
    }
}
