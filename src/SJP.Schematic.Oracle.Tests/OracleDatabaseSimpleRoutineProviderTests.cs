using Moq;
using NUnit.Framework;
using SJP.Schematic.Core;

namespace SJP.Schematic.Oracle.Tests;

[TestFixture]
internal static class OracleDatabaseSimpleRoutineProviderTests
{
    [Test]
    public static void Ctor_GivenNullConnection_ThrowsArgNullException()
    {
        var identifierDefaults = Mock.Of<IIdentifierDefaults>();
        var identifierResolver = Mock.Of<IIdentifierResolutionStrategy>();

        Assert.That(() => new OracleDatabaseSimpleRoutineProvider(null, identifierDefaults, identifierResolver), Throws.ArgumentNullException);
    }

    [Test]
    public static void Ctor_GivenNullIdentifierDefaults_ThrowsArgNullException()
    {
        var connection = Mock.Of<IDbConnectionFactory>();
        var identifierResolver = Mock.Of<IIdentifierResolutionStrategy>();

        Assert.That(() => new OracleDatabaseSimpleRoutineProvider(connection, null, identifierResolver), Throws.ArgumentNullException);
    }

    [Test]
    public static void Ctor_GivenNullIdentifierResolver_ThrowsArgNullException()
    {
        var connection = Mock.Of<IDbConnectionFactory>();
        var identifierDefaults = Mock.Of<IIdentifierDefaults>();

        Assert.That(() => new OracleDatabaseSimpleRoutineProvider(connection, identifierDefaults, null), Throws.ArgumentNullException);
    }

    [Test]
    public static void GetRoutine_GivenNullRoutineName_ThrowsArgNullException()
    {
        var connection = Mock.Of<IDbConnectionFactory>();
        var identifierDefaults = Mock.Of<IIdentifierDefaults>();
        var identifierResolver = Mock.Of<IIdentifierResolutionStrategy>();

        var routineProvider = new OracleDatabaseSimpleRoutineProvider(connection, identifierDefaults, identifierResolver);

        Assert.That(() => routineProvider.GetRoutine(null), Throws.ArgumentNullException);
    }
}