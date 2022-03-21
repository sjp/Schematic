using Moq;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.MySql.Comments;

namespace SJP.Schematic.MySql.Tests.Comments;

[TestFixture]
internal static class MySqlRoutineCommentProviderTests
{
    [Test]
    public static void Ctor_GivenNullConnection_ThrowsArgNullException()
    {
        var identifierDefaults = Mock.Of<IIdentifierDefaults>();

        Assert.That(() => new MySqlRoutineCommentProvider(null, identifierDefaults), Throws.ArgumentNullException);
    }

    [Test]
    public static void Ctor_GivenNullIdentifierDefaults_ThrowsArgNullException()
    {
        var connection = Mock.Of<IDbConnectionFactory>();

        Assert.That(() => new MySqlRoutineCommentProvider(connection, null), Throws.ArgumentNullException);
    }

    [Test]
    public static void GetRoutineComments_GivenNullRoutineName_ThrowsArgNullException()
    {
        var connection = Mock.Of<IDbConnectionFactory>();
        var identifierDefaults = Mock.Of<IIdentifierDefaults>();

        var commentProvider = new MySqlRoutineCommentProvider(connection, identifierDefaults);

        Assert.That(() => commentProvider.GetRoutineComments(null), Throws.ArgumentNullException);
    }
}