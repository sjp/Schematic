using System;
using Moq;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Sqlite.Pragma;

namespace SJP.Schematic.Sqlite.Tests;

internal static class SqliteRelationalDatabaseTableProviderTests
{
    private static IRelationalDatabaseTableProvider TableProvider
    {
        get
        {
            var connection = Mock.Of<ISchematicConnection>();
            var pragma = Mock.Of<ISqliteConnectionPragma>();
            var identifierDefaults = Mock.Of<IIdentifierDefaults>();

            return new SqliteRelationalDatabaseTableProvider(connection, pragma, identifierDefaults);
        }
    }

    [Test]
    public static void Ctor_GivenNullConnection_ThrowsArgNullException()
    {
        var pragma = Mock.Of<ISqliteConnectionPragma>();
        var identifierDefaults = Mock.Of<IIdentifierDefaults>();

        Assert.That(() => new SqliteRelationalDatabaseTableProvider(null, pragma, identifierDefaults), Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("connection"));
    }

    [Test]
    public static void Ctor_GivenNullPragma_ThrowsArgNullException()
    {
        var connection = Mock.Of<ISchematicConnection>();
        var identifierDefaults = Mock.Of<IIdentifierDefaults>();

        Assert.That(() => new SqliteRelationalDatabaseTableProvider(connection, null, identifierDefaults), Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("pragma"));
    }

    [Test]
    public static void Ctor_GivenNullIdentifierDefaults_ThrowsArgNullException()
    {
        var connection = Mock.Of<ISchematicConnection>();
        var pragma = Mock.Of<ISqliteConnectionPragma>();

        Assert.That(() => new SqliteRelationalDatabaseTableProvider(connection, pragma, null), Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("identifierDefaults"));
    }

    [Test]
    public static void GetTable_GivenNullTableName_ThrowsArgNullException()
    {
        Assert.That(() => TableProvider.GetTable(null), Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("tableName"));
    }
}