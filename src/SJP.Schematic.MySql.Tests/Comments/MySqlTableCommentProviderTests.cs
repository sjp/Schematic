using Moq;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.MySql.Comments;

namespace SJP.Schematic.MySql.Tests.Comments;

[TestFixture]
internal static class MySqlTableCommentProviderTests
{
    [Test]
    public static void Ctor_GivenNullConnection_ThrowsArgNullException()
    {
        var identifierDefaults = Mock.Of<IIdentifierDefaults>();

        Assert.That(() => new MySqlTableCommentProvider(null, identifierDefaults), Throws.ArgumentNullException);
    }

    [Test]
    public static void Ctor_GivenNullIdentifierDefaults_ThrowsArgNullException()
    {
        var connection = Mock.Of<IDbConnectionFactory>();

        Assert.That(() => new MySqlTableCommentProvider(connection, null), Throws.ArgumentNullException);
    }

    [Test]
    public static void GetTableComments_GivenNullTableName_ThrowsArgNullException()
    {
        var connection = Mock.Of<IDbConnectionFactory>();
        var identifierDefaults = Mock.Of<IIdentifierDefaults>();

        var commentProvider = new MySqlTableCommentProvider(connection, identifierDefaults);

        Assert.That(() => commentProvider.GetTableComments(null), Throws.ArgumentNullException);
    }
}