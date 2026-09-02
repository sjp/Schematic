using System;
using LanguageExt;
using Moq;
using NUnit.Framework;
using SJP.Schematic.Tests.Utilities;

namespace SJP.Schematic.Core.Tests;

[TestFixture]
internal static class DatabaseRoutineParameterTests
{
    private static Option<Identifier> ParameterName => Option<Identifier>.Some("test_parameter");

    [Test]
    public static void Ctor_GivenNullType_ThrowsArgumentNullException()
    {
        Assert.That(
            () => new DatabaseRoutineParameter(ParameterName, null, RoutineParameterDirection.Input, Option<string>.None, 1),
            Throws.ArgumentNullException);
    }

    [Test]
    public static void Ctor_GivenInvalidDirection_ThrowsArgumentException()
    {
        var dbType = Mock.Of<IDbType>();
        const RoutineParameterDirection direction = (RoutineParameterDirection)55;

        Assert.That(
            () => new DatabaseRoutineParameter(ParameterName, dbType, direction, Option<string>.None, 1),
            Throws.ArgumentException);
    }

    [TestCase(0)]
    [TestCase(-1)]
    public static void Ctor_GivenNonPositiveOrdinal_ThrowsArgumentOutOfRangeException(int ordinal)
    {
        var dbType = Mock.Of<IDbType>();

        Assert.That(
            () => new DatabaseRoutineParameter(ParameterName, dbType, RoutineParameterDirection.Input, Option<string>.None, ordinal),
            Throws.InstanceOf<ArgumentOutOfRangeException>());
    }

    [Test]
    public static void Name_GivenQualifiedCtorArg_PropertyGetReturnsLocalNameOnly()
    {
        var parameterName = Option<Identifier>.Some(Identifier.CreateQualifiedIdentifier("test_schema", "test_parameter"));
        var dbType = Mock.Of<IDbType>();

        var parameter = new DatabaseRoutineParameter(parameterName, dbType, RoutineParameterDirection.Input, Option<string>.None, 1);

        Assert.That(parameter.Name.UnwrapSome(), Is.EqualTo(Identifier.CreateQualifiedIdentifier("test_parameter")));
    }

    [Test]
    public static void Name_GivenNoName_PropertyGetReturnsNone()
    {
        var dbType = Mock.Of<IDbType>();

        var parameter = new DatabaseRoutineParameter(Option<Identifier>.None, dbType, RoutineParameterDirection.Input, Option<string>.None, 1);

        Assert.That(parameter.Name, OptionIs.None);
    }

    [Test]
    public static void PropertyGets_WhenGivenValidCtorArgs_MatchCtorArgs()
    {
        var dbType = Mock.Of<IDbType>();
        var defaultValue = Option<string>.Some("test_default");

        var parameter = new DatabaseRoutineParameter(ParameterName, dbType, RoutineParameterDirection.InputOutput, defaultValue, 3);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(parameter.Type, Is.EqualTo(dbType));
            Assert.That(parameter.Direction, Is.EqualTo(RoutineParameterDirection.InputOutput));
            Assert.That(parameter.DefaultValue.UnwrapSome(), Is.EqualTo("test_default"));
            Assert.That(parameter.Ordinal, Is.EqualTo(3));
        }
    }

    [Test]
    public static void ToString_WhenNamed_ContainsParameterName()
    {
        var typeMock = new Mock<IDbType>();
        typeMock.Setup(static t => t.Definition).Returns("varchar(20)");

        var parameter = new DatabaseRoutineParameter(ParameterName, typeMock.Object, RoutineParameterDirection.Input, Option<string>.None, 1);

        Assert.That(parameter.ToString(), Is.EqualTo("Parameter: test_parameter varchar(20)"));
    }

    [Test]
    public static void ToString_WhenPositional_ContainsOrdinal()
    {
        var typeMock = new Mock<IDbType>();
        typeMock.Setup(static t => t.Definition).Returns("integer");

        var parameter = new DatabaseRoutineParameter(Option<Identifier>.None, typeMock.Object, RoutineParameterDirection.Input, Option<string>.None, 2);

        Assert.That(parameter.ToString(), Is.EqualTo("Parameter: $2 integer"));
    }
}
