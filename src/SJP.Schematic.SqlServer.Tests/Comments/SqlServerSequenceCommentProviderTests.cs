﻿using Moq;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.SqlServer.Comments;

namespace SJP.Schematic.SqlServer.Tests.Comments;

[TestFixture]
internal static class SqlServerSequenceCommentProviderTests
{
    [Test]
    public static void Ctor_GivenNullConnection_ThrowsArgNullException()
    {
        var identifierDefaults = Mock.Of<IIdentifierDefaults>();

        Assert.That(() => new SqlServerSequenceCommentProvider(null, identifierDefaults), Throws.ArgumentNullException);
    }

    [Test]
    public static void Ctor_GivenNullIdentifierDefaults_ThrowsArgNullException()
    {
        var connection = Mock.Of<IDbConnectionFactory>();

        Assert.That(() => new SqlServerSequenceCommentProvider(connection, null), Throws.ArgumentNullException);
    }

    [Test]
    public static void GetSequenceComments_GivenNullSequenceName_ThrowsArgNullException()
    {
        var connection = Mock.Of<IDbConnectionFactory>();
        var identifierDefaults = Mock.Of<IIdentifierDefaults>();

        var commentProvider = new SqlServerSequenceCommentProvider(connection, identifierDefaults);

        Assert.That(() => commentProvider.GetSequenceComments(null), Throws.ArgumentNullException);
    }
}
