using NUnit.Framework;
using SJP.Schematic.Tool.Commands;

namespace SJP.Schematic.Tool.Tests.Commands;

[TestFixture]
internal static class CommonSettingsTests
{
    [Test]
    public static void ValidateConnectionOptions_GivenConfigFile_ReturnsNull()
    {
        var result = CommonSettings.ValidateConnectionOptions(hasConfigFile: true, hasDialect: false, hasConnectionString: false, defaultConfigExists: false);

        Assert.That(result, Is.Null);
    }

    [Test]
    public static void ValidateConnectionOptions_GivenDefaultConfigExists_ReturnsNull()
    {
        var result = CommonSettings.ValidateConnectionOptions(hasConfigFile: false, hasDialect: false, hasConnectionString: false, defaultConfigExists: true);

        Assert.That(result, Is.Null);
    }

    [Test]
    public static void ValidateConnectionOptions_GivenDialectAndConnectionString_ReturnsNull()
    {
        var result = CommonSettings.ValidateConnectionOptions(hasConfigFile: false, hasDialect: true, hasConnectionString: true, defaultConfigExists: false);

        Assert.That(result, Is.Null);
    }

    [Test]
    public static void ValidateConnectionOptions_GivenDialectWithoutConnectionString_ReturnsError()
    {
        var result = CommonSettings.ValidateConnectionOptions(hasConfigFile: false, hasDialect: true, hasConnectionString: false, defaultConfigExists: false);

        Assert.That(result, Does.Contain("--connection-string"));
    }

    [Test]
    public static void ValidateConnectionOptions_GivenConnectionStringWithoutDialect_ReturnsError()
    {
        var result = CommonSettings.ValidateConnectionOptions(hasConfigFile: false, hasDialect: false, hasConnectionString: true, defaultConfigExists: false);

        Assert.That(result, Does.Contain("--dialect"));
    }

    [Test]
    public static void ValidateConnectionOptions_GivenNothing_ReturnsError()
    {
        var result = CommonSettings.ValidateConnectionOptions(hasConfigFile: false, hasDialect: false, hasConnectionString: false, defaultConfigExists: false);

        Assert.That(result, Does.Contain("No database connection was specified"));
    }
}
