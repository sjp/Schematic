using System.IO;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;
using SJP.Schematic.Tool.Commands;
using SJP.Schematic.Tool.Handlers;

namespace SJP.Schematic.Tool.Tests.Handlers;

[TestFixture]
internal static class DatabaseCommandDependencyProviderFactoryTests
{
    private sealed class FakeSettings : CommonSettings
    {
    }

    [Test]
    public static void GetDbDependencies_GivenInlineFlagsWithoutConfigFile_UsesInlineValues()
    {
        var factory = new DatabaseCommandDependencyProviderFactory();
        var settings = new FakeSettings
        {
            Dialect = "sqlite",
            ConnectionString = "Data Source=inline.db",
        };

        var provider = factory.GetDbDependencies(settings);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(provider.Configuration.GetValue<string>("Dialect"), Is.EqualTo("sqlite"));
            Assert.That(provider.GetConnectionString(), Is.EqualTo("Data Source=inline.db"));
        }
    }

    [Test]
    public static void GetDbDependencies_GivenInlineFlagsAndConfigFile_InlineValuesOverrideFile()
    {
        var configPath = Path.GetTempFileName();
        File.WriteAllText(configPath, """
            {
                "Dialect": "postgresql",
                "ConnectionStrings": { "Schematic": "Host=file-host" }
            }
            """);

        try
        {
            var factory = new DatabaseCommandDependencyProviderFactory();
            var settings = new FakeSettings
            {
                ConfigFile = new FileInfo(configPath),
                Dialect = "sqlite",
                ConnectionString = "Data Source=inline.db",
            };

            var provider = factory.GetDbDependencies(settings);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(provider.Configuration.GetValue<string>("Dialect"), Is.EqualTo("sqlite"));
                Assert.That(provider.GetConnectionString(), Is.EqualTo("Data Source=inline.db"));
            }
        }
        finally
        {
            File.Delete(configPath);
        }
    }

    [Test]
    public static void GetDbDependencies_GivenConfigFileOnly_UsesFileValues()
    {
        var configPath = Path.GetTempFileName();
        File.WriteAllText(configPath, """
            {
                "Dialect": "postgresql",
                "ConnectionStrings": { "Schematic": "Host=file-host" }
            }
            """);

        try
        {
            var factory = new DatabaseCommandDependencyProviderFactory();
            var settings = new FakeSettings { ConfigFile = new FileInfo(configPath) };

            var provider = factory.GetDbDependencies(settings);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(provider.Configuration.GetValue<string>("Dialect"), Is.EqualTo("postgresql"));
                Assert.That(provider.GetConnectionString(), Is.EqualTo("Host=file-host"));
            }
        }
        finally
        {
            File.Delete(configPath);
        }
    }
}
