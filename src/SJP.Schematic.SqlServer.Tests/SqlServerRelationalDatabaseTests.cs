using System;
using Moq;
using NUnit.Framework;
using SJP.Schematic.Core;

namespace SJP.Schematic.SqlServer.Tests;

internal static class SqlServerRelationalDatabaseTests
{
    [Test]
    public static void Ctor_GivenNullConnection_ThrowsArgumentNullException()
    {
        var identifierDefaults = Mock.Of<IIdentifierDefaults>();

        Assert.That(
            () => new SqlServerRelationalDatabase(null, identifierDefaults),
            Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("connection")
        );
    }

    [Test]
    public static void Ctor_GivenNullIdentifierDefaults_ThrowsArgumentNullException()
    {
        var connection = Mock.Of<ISchematicConnection>();

        Assert.That(
            () => new SqlServerRelationalDatabase(connection, null),
            Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("identifierDefaults")
        );
    }

    [Test]
    public static void GetTable_GivenNullIdentifier_ThrowsArgumentNullException()
    {
        var connection = new SchematicConnection(Mock.Of<IDbConnectionFactory>(), Mock.Of<IDatabaseDialect>());
        var identifierDefaults = Mock.Of<IIdentifierDefaults>();

        var database = new SqlServerRelationalDatabase(connection, identifierDefaults);

        Assert.That(
            () => database.GetTable(null),
            Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("tableName")
        );
    }

    [Test]
    public static void GetView_GivenNullIdentifier_ThrowsArgumentNullException()
    {
        var connection = new SchematicConnection(Mock.Of<IDbConnectionFactory>(), Mock.Of<IDatabaseDialect>());
        var identifierDefaults = Mock.Of<IIdentifierDefaults>();

        var database = new SqlServerRelationalDatabase(connection, identifierDefaults);

        Assert.That(
            () => database.GetView(null),
            Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("viewName")
        );
    }

    [Test]
    public static void GetSequence_GivenNullIdentifier_ThrowsArgumentNullException()
    {
        var connection = new SchematicConnection(Mock.Of<IDbConnectionFactory>(), Mock.Of<IDatabaseDialect>());
        var identifierDefaults = Mock.Of<IIdentifierDefaults>();

        var database = new SqlServerRelationalDatabase(connection, identifierDefaults);

        Assert.That(
            () => database.GetSequence(null),
            Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("sequenceName")
        );
    }

    [Test]
    public static void GetSynonym_GivenNullIdentifier_ThrowsArgumentNullException()
    {
        var connection = new SchematicConnection(Mock.Of<IDbConnectionFactory>(), Mock.Of<IDatabaseDialect>());
        var identifierDefaults = Mock.Of<IIdentifierDefaults>();

        var database = new SqlServerRelationalDatabase(connection, identifierDefaults);

        Assert.That(
            () => database.GetSynonym(null),
            Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("synonymName")
        );
    }

    [Test]
    public static void GetRoutine_GivenNullIdentifier_ThrowsArgumentNullException()
    {
        var connection = new SchematicConnection(Mock.Of<IDbConnectionFactory>(), Mock.Of<IDatabaseDialect>());
        var identifierDefaults = Mock.Of<IIdentifierDefaults>();

        var database = new SqlServerRelationalDatabase(connection, identifierDefaults);

        Assert.That(
            () => database.GetRoutine(null),
            Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("routineName")
        );
    }
}