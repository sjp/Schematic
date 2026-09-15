using System;
using Moq;
using NUnit.Framework;
using SJP.Schematic.Core;

namespace SJP.Schematic.Oracle.Tests;

internal static class OracleRelationalDatabaseTableProviderTests
{
    [Test]
    public static void Ctor_GivenNullConnection_ThrowsArgNullException()
    {
        var identifierDefaults = Mock.Of<IIdentifierDefaults>();
        var identifierResolver = Mock.Of<IIdentifierResolutionStrategy>();

        Assert.That(() => new OracleRelationalDatabaseTableProvider(null, identifierDefaults, identifierResolver), Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("connection"));
    }

    [Test]
    public static void Ctor_GivenNullIdentifierDefaults_ThrowsArgNullException()
    {
        var connection = Mock.Of<ISchematicConnection>();
        var identifierResolver = Mock.Of<IIdentifierResolutionStrategy>();

        Assert.That(() => new OracleRelationalDatabaseTableProvider(connection, null, identifierResolver), Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("identifierDefaults"));
    }

    [Test]
    public static void Ctor_GivenNullIdentifierResolver_ThrowsArgNullException()
    {
        var connection = Mock.Of<ISchematicConnection>();
        var identifierDefaults = Mock.Of<IIdentifierDefaults>();

        Assert.That(() => new OracleRelationalDatabaseTableProvider(connection, identifierDefaults, null), Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("identifierResolver"));
    }

    [Test]
    public static void GetTable_GivenNullTableName_ThrowsArgNullException()
    {
        var connection = Mock.Of<ISchematicConnection>();
        var identifierDefaults = Mock.Of<IIdentifierDefaults>();
        var identifierResolver = Mock.Of<IIdentifierResolutionStrategy>();

        var tableProvider = new OracleRelationalDatabaseTableProvider(connection, identifierDefaults, identifierResolver);

        Assert.That(() => tableProvider.GetTable(null), Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("tableName"));
    }
}