using Moq;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.SqlServer.Comments;

namespace SJP.Schematic.SqlServer.Tests.Comments;

[TestFixture]
internal static class SqlServerSynonymCommentProviderTests
{
    [Test]
    public static void Ctor_GivenNullConnection_ThrowsArgNullException()
    {
        var identifierDefaults = Mock.Of<IIdentifierDefaults>();

        Assert.That(() => new SqlServerSynonymCommentProvider(null, identifierDefaults), Throws.ArgumentNullException);
    }

    [Test]
    public static void Ctor_GivenNullIdentifierDefaults_ThrowsArgNullException()
    {
        var connection = Mock.Of<IDbConnectionFactory>();

        Assert.That(() => new SqlServerSynonymCommentProvider(connection, null), Throws.ArgumentNullException);
    }

    [Test]
    public static void GetSynonymComments_GivenNullSynonymName_ThrowsArgNullException()
    {
        var connection = Mock.Of<IDbConnectionFactory>();
        var identifierDefaults = Mock.Of<IIdentifierDefaults>();

        var commentProvider = new SqlServerSynonymCommentProvider(connection, identifierDefaults);

        Assert.That(() => commentProvider.GetSynonymComments(null), Throws.ArgumentNullException);
    }
}