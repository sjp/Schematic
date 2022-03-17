﻿using System.Data;
using NUnit.Framework;

namespace SJP.Schematic.Sqlite.Tests;

[TestFixture]
internal static class SqliteConnectionFactoryTests
{
    [TestCase(null)]
    [TestCase("")]
    [TestCase("    ")]
    public static void Ctor_GivenNullOrWhiteSpaceName_ThrowsArgumentNullException(string connectionString)
    {
        Assert.That(() => new SqliteConnectionFactory(connectionString), Throws.ArgumentNullException);
    }

    [Test]
    public static void CreateConnection_WhenInvoked_ReturnsConnectionInClosedState()
    {
        var factory = new SqliteConnectionFactory("Data Source=:memory:");
        using var connection = factory.CreateConnection();

        Assert.That(connection.State, Is.EqualTo(ConnectionState.Closed));
    }
}
