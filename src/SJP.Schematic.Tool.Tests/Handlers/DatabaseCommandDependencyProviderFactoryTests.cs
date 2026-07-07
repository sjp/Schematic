using System;
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

    [Test]
    public static void GetDbDependencies_GivenConfigFileWithPlaceholderAndVariableSet_ResolvesPlaceholder()
    {
        const string varName = "SCHEMATIC_TEST_FACTORY_VAR_PASSWORD";
        var configPath = Path.GetTempFileName();
        File.WriteAllText(configPath, BuildConfigJsonWithPlaceholder(varName));
        Environment.SetEnvironmentVariable(varName, "s3cr3t");

        try
        {
            var factory = new DatabaseCommandDependencyProviderFactory();
            var settings = new FakeSettings { ConfigFile = new FileInfo(configPath) };

            var provider = factory.GetDbDependencies(settings);

            Assert.That(provider.GetConnectionString(), Is.EqualTo("Host=file-host;Password=s3cr3t"));
        }
        finally
        {
            Environment.SetEnvironmentVariable(varName, null);
            File.Delete(configPath);
        }
    }

    [Test]
    public static void GetDbDependencies_GivenConfigFileWithPlaceholderAndVariableNotSet_ThrowsInvalidOperationException()
    {
        const string varName = "SCHEMATIC_TEST_FACTORY_VAR_MISSING_DOES_NOT_EXIST";
        var configPath = Path.GetTempFileName();
        File.WriteAllText(configPath, BuildConfigJsonWithPlaceholder(varName));
        Environment.SetEnvironmentVariable(varName, null);

        try
        {
            var factory = new DatabaseCommandDependencyProviderFactory();
            var settings = new FakeSettings { ConfigFile = new FileInfo(configPath) };

            Assert.That(() => factory.GetDbDependencies(settings), Throws.InvalidOperationException);
        }
        finally
        {
            File.Delete(configPath);
        }
    }

    // Builds a schematic.json whose connection string embeds a ${varName} placeholder,
    // without fighting raw-string-literal brace-escaping rules for the ${...} syntax.
    private static string BuildConfigJsonWithPlaceholder(string varName)
    {
        var connectionString = "Host=file-host;Password=${" + varName + "}";
        var config = new System.Text.Json.Nodes.JsonObject
        {
            ["Dialect"] = "postgresql",
            ["ConnectionStrings"] = new System.Text.Json.Nodes.JsonObject
            {
                ["Schematic"] = connectionString,
            },
        };
        return config.ToJsonString();
    }
}
