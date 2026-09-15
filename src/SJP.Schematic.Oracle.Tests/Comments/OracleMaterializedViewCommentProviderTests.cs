using System;
using Moq;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Oracle.Comments;

namespace SJP.Schematic.Oracle.Tests.Comments;

internal static class OracleMaterializedViewCommentProviderTests
{
    [Test]
    public static void Ctor_GivenNullConnection_ThrowsArgNullException()
    {
        var identifierDefaults = Mock.Of<IIdentifierDefaults>();
        var identifierResolver = new DefaultOracleIdentifierResolutionStrategy();

        Assert.That(
            () => new OracleMaterializedViewCommentProvider(null, identifierDefaults, identifierResolver),
            Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("connection")
        );
    }

    [Test]
    public static void Ctor_GivenNullIdentifierDefaults_ThrowsArgNullException()
    {
        var connection = Mock.Of<IDbConnectionFactory>();
        var identifierResolver = new DefaultOracleIdentifierResolutionStrategy();

        Assert.That(
            () => new OracleMaterializedViewCommentProvider(connection, null, identifierResolver),
            Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("identifierDefaults")
        );
    }

    [Test]
    public static void Ctor_GivenNullIdentifierResolver_ThrowsArgNullException()
    {
        var connection = Mock.Of<IDbConnectionFactory>();
        var identifierDefaults = Mock.Of<IIdentifierDefaults>();

        Assert.That(
            () => new OracleMaterializedViewCommentProvider(connection, identifierDefaults, null),
            Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("identifierResolver")
        );
    }

    [Test]
    public static void GetViewComments_GivenNullViewName_ThrowsArgNullException()
    {
        var connection = Mock.Of<IDbConnectionFactory>();
        var identifierDefaults = Mock.Of<IIdentifierDefaults>();
        var identifierResolver = new DefaultOracleIdentifierResolutionStrategy();

        var commentProvider = new OracleMaterializedViewCommentProvider(connection, identifierDefaults, identifierResolver);

        Assert.That(
            () => commentProvider.GetViewComments(null),
            Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("viewName")
        );
    }
}