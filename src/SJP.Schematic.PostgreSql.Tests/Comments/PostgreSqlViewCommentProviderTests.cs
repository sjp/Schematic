﻿using Moq;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.PostgreSql.Comments;

namespace SJP.Schematic.PostgreSql.Tests.Comments;

[TestFixture]
internal static class PostgreSqlViewCommentProviderTests
{
    [Test]
    public static void Ctor_GivenNullConnection_ThrowsArgNullException()
    {
        var identifierDefaults = Mock.Of<IIdentifierDefaults>();
        var identifierResolver = new DefaultPostgreSqlIdentifierResolutionStrategy();

        Assert.That(() => new PostgreSqlViewCommentProvider(null, identifierDefaults, identifierResolver), Throws.ArgumentNullException);
    }

    [Test]
    public static void Ctor_GivenNullIdentifierDefaults_ThrowsArgNullException()
    {
        var connection = Mock.Of<IDbConnectionFactory>();
        var identifierResolver = new DefaultPostgreSqlIdentifierResolutionStrategy();

        Assert.That(() => new PostgreSqlViewCommentProvider(connection, null, identifierResolver), Throws.ArgumentNullException);
    }

    [Test]
    public static void Ctor_GivenNullIdentifierResolver_ThrowsArgNullException()
    {
        var connection = Mock.Of<IDbConnectionFactory>();
        var identifierDefaults = Mock.Of<IIdentifierDefaults>();

        Assert.That(() => new PostgreSqlViewCommentProvider(connection, identifierDefaults, null), Throws.ArgumentNullException);
    }

    [Test]
    public static void GetViewComments_GivenNullViewName_ThrowsArgNullException()
    {
        var connection = Mock.Of<IDbConnectionFactory>();
        var identifierDefaults = Mock.Of<IIdentifierDefaults>();
        var identifierResolver = new DefaultPostgreSqlIdentifierResolutionStrategy();

        var commentProvider = new PostgreSqlViewCommentProvider(connection, identifierDefaults, identifierResolver);

        Assert.That(() => commentProvider.GetViewComments(null), Throws.ArgumentNullException);
    }
}