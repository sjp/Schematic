#nullable enable
using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using NUnit.Framework;
using SJP.Schematic.Tool.Commands;
using SJP.Schematic.Tool.Handlers;
using Spectre.Console;
using Spectre.Console.Cli;

namespace SJP.Schematic.Tool.Tests.Commands;

[TestFixture]
internal static class LintCommandTests
{
    [Test]
    public static async Task ExecuteAsync_GivenSqliteDatabaseWithLintIssue_ReportsRuleAndSucceeds()
    {
        var dbPath = CommandAppHarness.CreateSampleSqliteDatabase();
        try
        {
            var (console, writer) = CommandAppHarness.CreateCapturingConsole();

            var registrar = new CommandAppHarness.InstanceRegistrar();
            registrar.RegisterInstance(typeof(IAnsiConsole), console);
            registrar.RegisterInstance(typeof(IDatabaseCommandDependencyProviderFactory), new DatabaseCommandDependencyProviderFactory());

            var app = new CommandApp(registrar);
            app.Configure(config => config.AddCommand<LintCommand>("lint"));

            var exitCode = await app.RunAsync(["lint", "--dialect", "sqlite", "--connection-string", $"Data Source={dbPath}"]);

            var output = writer.ToString();
            using (Assert.EnterMultipleScope())
            {
                Assert.That(exitCode, Is.Zero);
                Assert.That(output, Does.Contain("Rule:"));
                Assert.That(output, Does.Contain("foreign key"));
            }
        }
        finally
        {
            CommandAppHarness.DeleteSqliteDatabase(dbPath);
        }
    }

    [Test]
    public static async Task ExecuteAsync_GivenTextFormatExplicitly_MatchesDefaultOutput()
    {
        var dbPath = CommandAppHarness.CreateSampleSqliteDatabase();
        try
        {
            var (console, writer) = CommandAppHarness.CreateCapturingConsole();

            var registrar = new CommandAppHarness.InstanceRegistrar();
            registrar.RegisterInstance(typeof(IAnsiConsole), console);
            registrar.RegisterInstance(typeof(IDatabaseCommandDependencyProviderFactory), new DatabaseCommandDependencyProviderFactory());

            var app = new CommandApp(registrar);
            app.Configure(config => config.AddCommand<LintCommand>("lint"));

            var exitCode = await app.RunAsync(["lint", "--dialect", "sqlite", "--connection-string", $"Data Source={dbPath}", "--format", "text"]);

            var output = writer.ToString();
            using (Assert.EnterMultipleScope())
            {
                Assert.That(exitCode, Is.Zero);
                Assert.That(output, Does.Contain("Rule:"));
                Assert.That(output, Does.Contain("foreign key"));
            }
        }
        finally
        {
            CommandAppHarness.DeleteSqliteDatabase(dbPath);
        }
    }

    [Test]
    public static async Task ExecuteAsync_GivenJsonFormat_ProducesSortedJsonArray()
    {
        var dbPath = CommandAppHarness.CreateSampleSqliteDatabase();
        try
        {
            var (console, writer) = CommandAppHarness.CreateCapturingConsole();

            var registrar = new CommandAppHarness.InstanceRegistrar();
            registrar.RegisterInstance(typeof(IAnsiConsole), console);
            registrar.RegisterInstance(typeof(IDatabaseCommandDependencyProviderFactory), new DatabaseCommandDependencyProviderFactory());

            var app = new CommandApp(registrar);
            app.Configure(config => config.AddCommand<LintCommand>("lint"));

            var exitCode = await app.RunAsync(["lint", "--dialect", "sqlite", "--connection-string", $"Data Source={dbPath}", "--format", "json"]);

            var output = writer.ToString();
            using var document = JsonDocument.Parse(output);
            var root = document.RootElement;

            using (Assert.EnterMultipleScope())
            {
                Assert.That(exitCode, Is.Zero);
                Assert.That(root.ValueKind, Is.EqualTo(JsonValueKind.Array));
                Assert.That(root.GetArrayLength(), Is.GreaterThan(0));

                var first = root[0];
                Assert.That(first.TryGetProperty("ruleId", out _), Is.True);
                Assert.That(first.TryGetProperty("title", out _), Is.True);
                Assert.That(first.TryGetProperty("level", out _), Is.True);
                Assert.That(first.TryGetProperty("message", out _), Is.True);

                var ruleIds = root.EnumerateArray().Select(e => e.GetProperty("ruleId").GetString()).ToList();
                var sortedRuleIds = ruleIds.OrderBy(static id => id, StringComparer.Ordinal).ToList();
                Assert.That(ruleIds, Is.EqualTo(sortedRuleIds));
            }
        }
        finally
        {
            CommandAppHarness.DeleteSqliteDatabase(dbPath);
        }
    }

    [Test]
    public static async Task ExecuteAsync_GivenFailOnNotSet_ExitsZeroRegardlessOfIssues()
    {
        var dbPath = CommandAppHarness.CreateSampleSqliteDatabase();
        try
        {
            var (console, _) = CommandAppHarness.CreateCapturingConsole();

            var registrar = new CommandAppHarness.InstanceRegistrar();
            registrar.RegisterInstance(typeof(IAnsiConsole), console);
            registrar.RegisterInstance(typeof(IDatabaseCommandDependencyProviderFactory), new DatabaseCommandDependencyProviderFactory());

            var app = new CommandApp(registrar);
            app.Configure(config => config.AddCommand<LintCommand>("lint"));

            var exitCode = await app.RunAsync(["lint", "--dialect", "sqlite", "--connection-string", $"Data Source={dbPath}"]);

            Assert.That(exitCode, Is.Zero);
        }
        finally
        {
            CommandAppHarness.DeleteSqliteDatabase(dbPath);
        }
    }

    [Test]
    public static async Task ExecuteAsync_GivenFailOnMatchingLevel_ExitsNonZero()
    {
        var dbPath = CommandAppHarness.CreateSampleSqliteDatabase();
        try
        {
            var (console, _) = CommandAppHarness.CreateCapturingConsole();

            var registrar = new CommandAppHarness.InstanceRegistrar();
            registrar.RegisterInstance(typeof(IAnsiConsole), console);
            registrar.RegisterInstance(typeof(IDatabaseCommandDependencyProviderFactory), new DatabaseCommandDependencyProviderFactory());

            var app = new CommandApp(registrar);
            app.Configure(config => config.AddCommand<LintCommand>("lint"));

            // Every finding is at least Information, whatever level each rule reports at, so
            // --fail-on information trips as soon as anything at all is reported.
            var exitCode = await app.RunAsync(["lint", "--dialect", "sqlite", "--connection-string", $"Data Source={dbPath}", "--fail-on", "information"]);

            Assert.That(exitCode, Is.EqualTo(1));
        }
        finally
        {
            CommandAppHarness.DeleteSqliteDatabase(dbPath);
        }
    }

    [Test]
    public static async Task ExecuteAsync_GivenFailOnHigherThanAnyIssue_ExitsZero()
    {
        var dbPath = CommandAppHarness.CreateSampleSqliteDatabase();
        try
        {
            var (console, _) = CommandAppHarness.CreateCapturingConsole();

            var registrar = new CommandAppHarness.InstanceRegistrar();
            registrar.RegisterInstance(typeof(IAnsiConsole), console);
            registrar.RegisterInstance(typeof(IDatabaseCommandDependencyProviderFactory), new DatabaseCommandDependencyProviderFactory());

            var app = new CommandApp(registrar);
            app.Configure(config => config.AddCommand<LintCommand>("lint"));

            // --level pins every rule to Information, so nothing is reported at Warning or above.
            var exitCode = await app.RunAsync(["lint", "--dialect", "sqlite", "--connection-string", $"Data Source={dbPath}", "--level", "information", "--fail-on", "warning"]);

            Assert.That(exitCode, Is.Zero);
        }
        finally
        {
            CommandAppHarness.DeleteSqliteDatabase(dbPath);
        }
    }

    [Test]
    public static async Task ExecuteAsync_GivenLevelWarning_ReportsIssuesAtWarningLevel()
    {
        var dbPath = CommandAppHarness.CreateSampleSqliteDatabase();
        try
        {
            var (console, _) = CommandAppHarness.CreateCapturingConsole();

            var registrar = new CommandAppHarness.InstanceRegistrar();
            registrar.RegisterInstance(typeof(IAnsiConsole), console);
            registrar.RegisterInstance(typeof(IDatabaseCommandDependencyProviderFactory), new DatabaseCommandDependencyProviderFactory());

            var app = new CommandApp(registrar);
            app.Configure(config => config.AddCommand<LintCommand>("lint"));

            // --level relabels every rule's reporting level (it does not filter rules), so
            // --fail-on warning only trips once issues are actually reported at Warning or above.
            var exitCode = await app.RunAsync(["lint", "--dialect", "sqlite", "--connection-string", $"Data Source={dbPath}", "--level", "warning", "--fail-on", "warning"]);

            Assert.That(exitCode, Is.EqualTo(1));
        }
        finally
        {
            CommandAppHarness.DeleteSqliteDatabase(dbPath);
        }
    }

    [Test]
    public static async Task ExecuteAsync_GivenLevelError_ReportsIssuesAtErrorLevel()
    {
        var dbPath = CommandAppHarness.CreateSampleSqliteDatabase();
        try
        {
            var (console, _) = CommandAppHarness.CreateCapturingConsole();

            var registrar = new CommandAppHarness.InstanceRegistrar();
            registrar.RegisterInstance(typeof(IAnsiConsole), console);
            registrar.RegisterInstance(typeof(IDatabaseCommandDependencyProviderFactory), new DatabaseCommandDependencyProviderFactory());

            var app = new CommandApp(registrar);
            app.Configure(config => config.AddCommand<LintCommand>("lint"));

            var exitCode = await app.RunAsync(["lint", "--dialect", "sqlite", "--connection-string", $"Data Source={dbPath}", "--level", "error", "--fail-on", "error"]);

            Assert.That(exitCode, Is.EqualTo(1));
        }
        finally
        {
            CommandAppHarness.DeleteSqliteDatabase(dbPath);
        }
    }

    [Test]
    public static async Task ExecuteAsync_GivenLevelInformation_ReportsIssues()
    {
        var dbPath = CommandAppHarness.CreateSampleSqliteDatabase();
        try
        {
            var (console, writer) = CommandAppHarness.CreateCapturingConsole();

            var registrar = new CommandAppHarness.InstanceRegistrar();
            registrar.RegisterInstance(typeof(IAnsiConsole), console);
            registrar.RegisterInstance(typeof(IDatabaseCommandDependencyProviderFactory), new DatabaseCommandDependencyProviderFactory());

            var app = new CommandApp(registrar);
            app.Configure(config => config.AddCommand<LintCommand>("lint"));

            var exitCode = await app.RunAsync(["lint", "--dialect", "sqlite", "--connection-string", $"Data Source={dbPath}", "--level", "information"]);

            var output = writer.ToString();
            using (Assert.EnterMultipleScope())
            {
                Assert.That(exitCode, Is.Zero);
                Assert.That(output, Does.Contain("Rule:"));
                Assert.That(output, Does.Contain("foreign key"));
            }
        }
        finally
        {
            CommandAppHarness.DeleteSqliteDatabase(dbPath);
        }
    }

    [Test]
    public static async Task ExecuteAsync_GivenNoLevel_ReportsEachRuleAtItsOwnLevel()
    {
        var dbPath = CommandAppHarness.CreateSampleSqliteDatabase();
        try
        {
            var (console, writer) = CommandAppHarness.CreateCapturingConsole();

            var registrar = new CommandAppHarness.InstanceRegistrar();
            registrar.RegisterInstance(typeof(IAnsiConsole), console);
            registrar.RegisterInstance(typeof(IDatabaseCommandDependencyProviderFactory), new DatabaseCommandDependencyProviderFactory());

            var app = new CommandApp(registrar);
            app.Configure(config => config.AddCommand<LintCommand>("lint"));

            var exitCode = await app.RunAsync(["lint", "--dialect", "sqlite", "--connection-string", $"Data Source={dbPath}", "--format", "json"]);

            var output = writer.ToString();
            using var document = JsonDocument.Parse(output);

            var levels = document.RootElement
                .EnumerateArray()
                .Select(static e => e.GetProperty("level").GetString())
                .Distinct(StringComparer.Ordinal)
                .ToList();

            using (Assert.EnterMultipleScope())
            {
                Assert.That(exitCode, Is.Zero);
                // The point of per-rule levels: without --level, findings are not all one severity.
                Assert.That(levels, Has.Count.GreaterThan(1));
            }
        }
        finally
        {
            CommandAppHarness.DeleteSqliteDatabase(dbPath);
        }
    }

    [Test]
    public static async Task ExecuteAsync_GivenJsonFormat_AttributesIssuesToTheirObject()
    {
        var dbPath = CommandAppHarness.CreateSampleSqliteDatabase();
        try
        {
            var (console, writer) = CommandAppHarness.CreateCapturingConsole();

            var registrar = new CommandAppHarness.InstanceRegistrar();
            registrar.RegisterInstance(typeof(IAnsiConsole), console);
            registrar.RegisterInstance(typeof(IDatabaseCommandDependencyProviderFactory), new DatabaseCommandDependencyProviderFactory());

            var app = new CommandApp(registrar);
            app.Configure(config => config.AddCommand<LintCommand>("lint"));

            var exitCode = await app.RunAsync(["lint", "--dialect", "sqlite", "--connection-string", $"Data Source={dbPath}", "--format", "json"]);

            var output = writer.ToString();
            using var document = JsonDocument.Parse(output);

            var objectNames = document.RootElement
                .EnumerateArray()
                .Where(static e => e.TryGetProperty("objectName", out _))
                .Select(static e => e.GetProperty("objectName").GetString())
                .ToList();

            using (Assert.EnterMultipleScope())
            {
                Assert.That(exitCode, Is.Zero);
                Assert.That(objectNames, Is.Not.Empty);
                Assert.That(objectNames, Has.Some.Contains("book").Or.Some.Contains("author"));
            }
        }
        finally
        {
            CommandAppHarness.DeleteSqliteDatabase(dbPath);
        }
    }

    [Test]
    public static async Task ExecuteAsync_GivenSarifFormat_EmitsLogicalLocationsForIssues()
    {
        var dbPath = CommandAppHarness.CreateSampleSqliteDatabase();
        try
        {
            var (console, writer) = CommandAppHarness.CreateCapturingConsole();

            var registrar = new CommandAppHarness.InstanceRegistrar();
            registrar.RegisterInstance(typeof(IAnsiConsole), console);
            registrar.RegisterInstance(typeof(IDatabaseCommandDependencyProviderFactory), new DatabaseCommandDependencyProviderFactory());

            var app = new CommandApp(registrar);
            app.Configure(config => config.AddCommand<LintCommand>("lint"));

            var exitCode = await app.RunAsync(["lint", "--dialect", "sqlite", "--connection-string", $"Data Source={dbPath}", "--format", "sarif"]);

            var output = writer.ToString();
            using var document = JsonDocument.Parse(output);

            var results = document.RootElement
                .GetProperty("runs")[0]
                .GetProperty("results")
                .EnumerateArray()
                .ToList();

            var located = results
                .Where(static r => r.TryGetProperty("locations", out _))
                .Select(static r => r.GetProperty("locations")[0]
                    .GetProperty("logicalLocations")[0]
                    .GetProperty("fullyQualifiedName")
                    .GetString())
                .ToList();

            using (Assert.EnterMultipleScope())
            {
                Assert.That(exitCode, Is.Zero);
                Assert.That(results, Is.Not.Empty);
                Assert.That(located, Is.Not.Empty);
            }
        }
        finally
        {
            CommandAppHarness.DeleteSqliteDatabase(dbPath);
        }
    }

    [Test]
    public static async Task ExecuteAsync_GivenInvalidLevel_ReturnsParseError()
    {
        var (console, _) = CommandAppHarness.CreateCapturingConsole();

        var registrar = new CommandAppHarness.InstanceRegistrar();
        registrar.RegisterInstance(typeof(IAnsiConsole), console);
        registrar.RegisterInstance(typeof(IDatabaseCommandDependencyProviderFactory), new DatabaseCommandDependencyProviderFactory());

        var app = new CommandApp(registrar);
        app.Configure(config => config.AddCommand<LintCommand>("lint"));

        var exitCode = await app.RunAsync(["lint", "--dialect", "sqlite", "--connection-string", "Data Source=unused.db", "--level", "bogus"]);

        Assert.That(exitCode, Is.Not.Zero);
    }

    [Test]
    public static async Task ExecuteAsync_GivenSarifFormat_ProducesValidSarifLogWithSortedRulesAndResults()
    {
        var dbPath = CommandAppHarness.CreateSampleSqliteDatabase();
        try
        {
            var (console, writer) = CommandAppHarness.CreateCapturingConsole();

            var registrar = new CommandAppHarness.InstanceRegistrar();
            registrar.RegisterInstance(typeof(IAnsiConsole), console);
            registrar.RegisterInstance(typeof(IDatabaseCommandDependencyProviderFactory), new DatabaseCommandDependencyProviderFactory());

            var app = new CommandApp(registrar);
            app.Configure(config => config.AddCommand<LintCommand>("lint"));

            var exitCode = await app.RunAsync(["lint", "--dialect", "sqlite", "--connection-string", $"Data Source={dbPath}", "--format", "sarif"]);

            var output = writer.ToString();
            using var document = JsonDocument.Parse(output);
            var root = document.RootElement;

            using (Assert.EnterMultipleScope())
            {
                Assert.That(exitCode, Is.Zero);
                Assert.That(root.GetProperty("$schema").GetString(), Does.Contain("sarif-schema-2.1.0"));
                Assert.That(root.GetProperty("version").GetString(), Is.EqualTo("2.1.0"));

                var run = root.GetProperty("runs")[0];
                var rules = run.GetProperty("tool").GetProperty("driver").GetProperty("rules");
                var results = run.GetProperty("results");

                Assert.That(rules.GetArrayLength(), Is.GreaterThan(0));
                Assert.That(results.GetArrayLength(), Is.GreaterThan(0));
                Assert.That(results[0].GetProperty("ruleId").ValueKind, Is.EqualTo(JsonValueKind.String));
                Assert.That(results[0].GetProperty("message").GetProperty("text").ValueKind, Is.EqualTo(JsonValueKind.String));

                var ruleIds = rules.EnumerateArray().Select(r => r.GetProperty("id").GetString()).ToList();
                var sortedRuleIds = ruleIds.OrderBy(static id => id, StringComparer.Ordinal).ToList();
                Assert.That(ruleIds, Is.EqualTo(sortedRuleIds));

                var resultRuleIds = results.EnumerateArray().Select(r => r.GetProperty("ruleId").GetString()).ToList();
                var sortedResultRuleIds = resultRuleIds.OrderBy(static id => id, StringComparer.Ordinal).ToList();
                Assert.That(resultRuleIds, Is.EqualTo(sortedResultRuleIds));
            }
        }
        finally
        {
            CommandAppHarness.DeleteSqliteDatabase(dbPath);
        }
    }
}
