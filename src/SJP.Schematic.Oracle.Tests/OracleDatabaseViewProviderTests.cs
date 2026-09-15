using System;
using Moq;
using NUnit.Framework;
using SJP.Schematic.Core;

namespace SJP.Schematic.Oracle.Tests;

internal static class OracleDatabaseViewProviderTests
{
    [Test]
    public static void Ctor_GivenNullConnection_ThrowsArgNullException()
    {
        var identifierDefaults = Mock.Of<IIdentifierDefaults>();
        var identifierResolver = Mock.Of<IIdentifierResolutionStrategy>();

        Assert.That(
            () => new OracleDatabaseViewProvider(null, identifierDefaults, identifierResolver),
            Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("connection")
        );
    }

    [Test]
    public static void Ctor_GivenNullIdentifierDefaults_ThrowsArgNullException()
    {
        var connection = Mock.Of<ISchematicConnection>();
        var identifierResolver = Mock.Of<IIdentifierResolutionStrategy>();

        Assert.That(
            () => new OracleDatabaseViewProvider(connection, null, identifierResolver),
            Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("identifierDefaults")
        );
    }

    [Test]
    public static void Ctor_GivenNullIdentifierResolver_ThrowsArgNullException()
    {
        var connection = Mock.Of<ISchematicConnection>();
        var identifierDefaults = Mock.Of<IIdentifierDefaults>();

        Assert.That(
            () => new OracleDatabaseViewProvider(connection, identifierDefaults, null),
            Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("identifierResolver")
        );
    }

    [Test]
    public static void GetView_GivenNullViewName_ThrowsArgNullException()
    {
        var connection = Mock.Of<ISchematicConnection>();
        var identifierDefaults = Mock.Of<IIdentifierDefaults>();
        var identifierResolver = Mock.Of<IIdentifierResolutionStrategy>();

        var viewProvider = new OracleDatabaseViewProvider(connection, identifierDefaults, identifierResolver);

        Assert.That(
            () => viewProvider.GetView(null),
            Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("viewName")
        );
    }
}