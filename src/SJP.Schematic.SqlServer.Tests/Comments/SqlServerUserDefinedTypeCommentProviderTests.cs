using Moq;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.SqlServer.Comments;

namespace SJP.Schematic.SqlServer.Tests.Comments;

[TestFixture]
internal static class SqlServerUserDefinedTypeCommentProviderTests
{
    [Test]
    public static void Ctor_GivenNullConnection_ThrowsArgNullException()
    {
        var identifierDefaults = Mock.Of<IIdentifierDefaults>();

        Assert.That(() => new SqlServerUserDefinedTypeCommentProvider(null, identifierDefaults), Throws.ArgumentNullException);
    }

    [Test]
    public static void Ctor_GivenNullIdentifierDefaults_ThrowsArgNullException()
    {
        var connection = Mock.Of<IDbConnectionFactory>();

        Assert.That(() => new SqlServerUserDefinedTypeCommentProvider(connection, null), Throws.ArgumentNullException);
    }

    [Test]
    public static void GetUserDefinedTypeComments_GivenNullTypeName_ThrowsArgNullException()
    {
        var connection = Mock.Of<IDbConnectionFactory>();
        var identifierDefaults = Mock.Of<IIdentifierDefaults>();

        var commentProvider = new SqlServerUserDefinedTypeCommentProvider(connection, identifierDefaults);

        Assert.That(() => commentProvider.GetUserDefinedTypeComments(null), Throws.ArgumentNullException);
    }
}
