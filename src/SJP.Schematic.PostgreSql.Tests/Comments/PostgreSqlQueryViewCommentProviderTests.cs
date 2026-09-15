using System;
using Moq;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.PostgreSql.Comments;

namespace SJP.Schematic.PostgreSql.Tests.Comments;

internal static class PostgreSqlQueryViewCommentProviderTests
{
    [Test]
    public static void Ctor_GivenNullConnection_ThrowsArgNullException()
    {
        var identifierDefaults = Mock.Of<IIdentifierDefaults>();
        var identifierResolver = new DefaultPostgreSqlIdentifierResolutionStrategy();

        Assert.That(() => new PostgreSqlQueryViewCommentProvider(null, identifierDefaults, identifierResolver), Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("connection"));
    }

    [Test]
    public static void Ctor_GivenNullIdentifierDefaults_ThrowsArgNullException()
    {
        var connection = Mock.Of<IDbConnectionFactory>();
        var identifierResolver = new DefaultPostgreSqlIdentifierResolutionStrategy();

        Assert.That(() => new PostgreSqlQueryViewCommentProvider(connection, null, identifierResolver), Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("identifierDefaults"));
    }

    [Test]
    public static void Ctor_GivenNullIdentifierResolver_ThrowsArgNullException()
    {
        var connection = Mock.Of<IDbConnectionFactory>();
        var identifierDefaults = Mock.Of<IIdentifierDefaults>();

        Assert.That(() => new PostgreSqlQueryViewCommentProvider(connection, identifierDefaults, null), Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("identifierResolver"));
    }

    [Test]
    public static void GetViewComments_GivenNullViewName_ThrowsArgNullException()
    {
        var connection = Mock.Of<IDbConnectionFactory>();
        var identifierDefaults = Mock.Of<IIdentifierDefaults>();
        var identifierResolver = new DefaultPostgreSqlIdentifierResolutionStrategy();

        var commentProvider = new PostgreSqlQueryViewCommentProvider(connection, identifierDefaults, identifierResolver);

        Assert.That(() => commentProvider.GetViewComments(null), Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("viewName"));
    }
}