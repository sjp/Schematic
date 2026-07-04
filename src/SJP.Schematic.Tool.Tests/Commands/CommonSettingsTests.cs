using System.IO;
using NUnit.Framework;
using SJP.Schematic.Tool.Commands;

namespace SJP.Schematic.Tool.Tests.Commands;

[TestFixture]
internal static class CommonSettingsTests
{
    private sealed class FakeSettings : CommonSettings
    {
    }

    [Test]
    public static void ResolveConfigFilePath_GivenExplicitConfigFile_ReturnsItsFullPath()
    {
        var file = new FileInfo("some-config.json");
        var settings = new FakeSettings { ConfigFile = file };

        Assert.That(settings.ResolveConfigFilePath(), Is.EqualTo(file.FullName));
    }

    [Test]
    public static void ResolveConfigFilePath_GivenNoConfigAndNoDefaultFile_ReturnsNull()
    {
        RunInTemporaryDirectory(dir =>
        {
            var settings = new FakeSettings();

            Assert.That(settings.ResolveConfigFilePath(), Is.Null);
        });
    }

    [Test]
    public static void ResolveConfigFilePath_GivenNoConfigButDefaultFileExists_ReturnsDefaultFileFullPath()
    {
        RunInTemporaryDirectory(dir =>
        {
            var defaultPath = Path.Combine(dir, CommonSettings.DefaultConfigFileName);
            File.WriteAllText(defaultPath, "{}");
            var settings = new FakeSettings();

            var resolved = settings.ResolveConfigFilePath();

            using (Assert.EnterMultipleScope())
            {
                Assert.That(resolved, Is.Not.Null);
                Assert.That(Path.GetFileName(resolved!), Is.EqualTo(CommonSettings.DefaultConfigFileName));
                Assert.That(Path.IsPathRooted(resolved), Is.True);
            }
        });
    }

    // Runs an action with the working directory set to a fresh temporary directory,
    // restoring the original directory afterwards.
    private static void RunInTemporaryDirectory(System.Action<string> action)
    {
        var original = Directory.GetCurrentDirectory();
        var tempDir = Directory.CreateTempSubdirectory("schematic-test").FullName;
        try
        {
            Directory.SetCurrentDirectory(tempDir);
            action(tempDir);
        }
        finally
        {
            Directory.SetCurrentDirectory(original);
            Directory.Delete(tempDir, recursive: true);
        }
    }

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
