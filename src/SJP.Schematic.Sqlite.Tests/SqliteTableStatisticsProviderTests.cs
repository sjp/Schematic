using System;
using Moq;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Sqlite.Pragma;

namespace SJP.Schematic.Sqlite.Tests;

internal static class SqliteTableStatisticsProviderTests
{
    [Test]
    public static void Ctor_GivenNullConnection_ThrowsArgNullException()
    {
        var pragma = Mock.Of<ISqliteConnectionPragma>();
        var identifierDefaults = Mock.Of<IIdentifierDefaults>();

        Assert.That(() => new SqliteTableStatisticsProvider(null, pragma, identifierDefaults), Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("connection"));
    }

    [Test]
    public static void Ctor_GivenNullConnectionPragma_ThrowsArgNullException()
    {
        var connection = Mock.Of<ISchematicConnection>();
        var identifierDefaults = Mock.Of<IIdentifierDefaults>();

        Assert.That(() => new SqliteTableStatisticsProvider(connection, null, identifierDefaults), Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("pragma"));
    }

    [Test]
    public static void Ctor_GivenNullIdentifierDefaults_ThrowsArgNullException()
    {
        var connection = Mock.Of<ISchematicConnection>();
        var pragma = Mock.Of<ISqliteConnectionPragma>();

        Assert.That(() => new SqliteTableStatisticsProvider(connection, pragma, null), Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("identifierDefaults"));
    }

    [Test]
    public static void GetTableStatistics_GivenNullTableName_ThrowsArgNullException()
    {
        var provider = new SqliteTableStatisticsProvider(
            Mock.Of<ISchematicConnection>(),
            Mock.Of<ISqliteConnectionPragma>(),
            Mock.Of<IIdentifierDefaults>()
        );

        Assert.That(() => provider.GetTableStatistics(null), Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("tableName"));
    }
}
