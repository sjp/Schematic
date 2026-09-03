using Moq;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.SqlServer.Comments;

namespace SJP.Schematic.SqlServer.Tests.Comments;

[TestFixture]
internal static class SqlServerSchemaCommentProviderTests
{
    [Test]
    public static void Ctor_GivenNullConnection_ThrowsArgNullException()
    {
        var identifierDefaults = Mock.Of<IIdentifierDefaults>();

        Assert.That(() => new SqlServerSchemaCommentProvider(null, identifierDefaults), Throws.ArgumentNullException);
    }

    [Test]
    public static void Ctor_GivenNullIdentifierDefaults_ThrowsArgNullException()
    {
        var connection = Mock.Of<IDbConnectionFactory>();

        Assert.That(() => new SqlServerSchemaCommentProvider(connection, null), Throws.ArgumentNullException);
    }

    [Test]
    public static void GetSchemaComments_GivenNullSchemaName_ThrowsArgNullException()
    {
        var connection = Mock.Of<IDbConnectionFactory>();
        var identifierDefaults = Mock.Of<IIdentifierDefaults>();

        var commentProvider = new SqlServerSchemaCommentProvider(connection, identifierDefaults);

        Assert.That(() => commentProvider.GetSchemaComments(null), Throws.ArgumentNullException);
    }
}
