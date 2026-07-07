using System;
using NUnit.Framework;
using SJP.Schematic.Tool.Handlers;

namespace SJP.Schematic.Tool.Tests.Handlers;

[TestFixture]
internal static class EnvironmentVariableSubstitutionTests
{
    [Test]
    public static void Resolve_GivenNull_ReturnsNull()
    {
        var result = EnvironmentVariableSubstitution.Resolve(null);

        Assert.That(result, Is.Null);
    }

    [Test]
    public static void Resolve_GivenValueWithoutPlaceholder_ReturnsValueUnchanged()
    {
        var result = EnvironmentVariableSubstitution.Resolve("Data Source=app.db");

        Assert.That(result, Is.EqualTo("Data Source=app.db"));
    }

    [Test]
    public static void Resolve_GivenSinglePlaceholder_ResolvesFromEnvironment()
    {
        const string varName = "SCHEMATIC_TEST_VAR_SINGLE";
        Environment.SetEnvironmentVariable(varName, "secret-value");

        try
        {
            var result = EnvironmentVariableSubstitution.Resolve($"${{{varName}}}");

            Assert.That(result, Is.EqualTo("secret-value"));
        }
        finally
        {
            Environment.SetEnvironmentVariable(varName, null);
        }
    }

    [Test]
    public static void Resolve_GivenPlaceholderEmbeddedInLargerString_ResolvesInPlace()
    {
        const string varName = "SCHEMATIC_TEST_VAR_EMBEDDED";
        Environment.SetEnvironmentVariable(varName, "s3cr3t");

        try
        {
            var result = EnvironmentVariableSubstitution.Resolve($"Server=localhost;Password=${{{varName}}};Trusted=true");

            Assert.That(result, Is.EqualTo("Server=localhost;Password=s3cr3t;Trusted=true"));
        }
        finally
        {
            Environment.SetEnvironmentVariable(varName, null);
        }
    }

    [Test]
    public static void Resolve_GivenMultiplePlaceholders_ResolvesAll()
    {
        const string userVar = "SCHEMATIC_TEST_VAR_USER";
        const string passwordVar = "SCHEMATIC_TEST_VAR_PASSWORD";
        Environment.SetEnvironmentVariable(userVar, "admin");
        Environment.SetEnvironmentVariable(passwordVar, "hunter2");

        try
        {
            var result = EnvironmentVariableSubstitution.Resolve($"User=${{{userVar}}};Password=${{{passwordVar}}}");

            Assert.That(result, Is.EqualTo("User=admin;Password=hunter2"));
        }
        finally
        {
            Environment.SetEnvironmentVariable(userVar, null);
            Environment.SetEnvironmentVariable(passwordVar, null);
        }
    }

    [Test]
    public static void Resolve_GivenEmptyButSetVariable_ResolvesToEmptyString()
    {
        const string varName = "SCHEMATIC_TEST_VAR_EMPTY";
        Environment.SetEnvironmentVariable(varName, string.Empty);

        try
        {
            var result = EnvironmentVariableSubstitution.Resolve($"Password=${{{varName}}}");

            Assert.That(result, Is.EqualTo("Password="));
        }
        finally
        {
            Environment.SetEnvironmentVariable(varName, null);
        }
    }

    [Test]
    public static void Resolve_GivenMissingVariable_ThrowsInvalidOperationException()
    {
        const string varName = "SCHEMATIC_TEST_VAR_MISSING_DOES_NOT_EXIST";
        Environment.SetEnvironmentVariable(varName, null);

        Assert.That(
            () => EnvironmentVariableSubstitution.Resolve($"Password=${{{varName}}}"),
            Throws.InvalidOperationException);
    }
}
