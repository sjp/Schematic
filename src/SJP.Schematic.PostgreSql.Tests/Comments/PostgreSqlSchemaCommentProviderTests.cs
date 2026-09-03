using Moq;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.PostgreSql.Comments;

namespace SJP.Schematic.PostgreSql.Tests.Comments;

[TestFixture]
internal static class PostgreSqlSchemaCommentProviderTests
{
    [Test]
    public static void Ctor_GivenNullConnection_ThrowsArgNullException()
    {
        var identifierDefaults = Mock.Of<IIdentifierDefaults>();

        Assert.That(() => new PostgreSqlSchemaCommentProvider(null, identifierDefaults), Throws.ArgumentNullException);
    }

    [Test]
    public static void Ctor_GivenNullIdentifierDefaults_ThrowsArgNullException()
    {
        var connection = Mock.Of<IDbConnectionFactory>();

        Assert.That(() => new PostgreSqlSchemaCommentProvider(connection, null), Throws.ArgumentNullException);
    }

    [Test]
    public static void GetSchemaComments_GivenNullSchemaName_ThrowsArgNullException()
    {
        var connection = Mock.Of<IDbConnectionFactory>();
        var identifierDefaults = Mock.Of<IIdentifierDefaults>();

        var commentProvider = new PostgreSqlSchemaCommentProvider(connection, identifierDefaults);

        Assert.That(() => commentProvider.GetSchemaComments(null), Throws.ArgumentNullException);
    }
}
