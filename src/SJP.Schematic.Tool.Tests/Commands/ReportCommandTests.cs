using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using NUnit.Framework;
using SJP.Schematic.Tool.Commands;
using SJP.Schematic.Tool.Handlers;
using Spectre.Console;
using Spectre.Console.Cli;

namespace SJP.Schematic.Tool.Tests.Commands;

internal static class ReportCommandTests
{
    [Test]
    public static void Validate_GivenConnectionButNoOutputDirectory_ReturnsError()
    {
        var settings = new ReportCommand.Settings
        {
            Dialect = "sqlite",
            ConnectionString = "Data Source=:memory:",
            OutputDirectory = null,
        };

        var result = settings.Validate();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Successful, Is.False);
            Assert.That(result.Message, Does.Contain("--output"));
        }
    }

    [Test]
    public static void Validate_GivenConnectionAndOutputDirectory_ReturnsSuccess()
    {
        var settings = new ReportCommand.Settings
        {
            Dialect = "sqlite",
            ConnectionString = "Data Source=:memory:",
            OutputDirectory = new DirectoryInfo("./report"),
        };

        var result = settings.Validate();

        Assert.That(result.Successful, Is.True);
    }

    [Test]
    public static async Task ExecuteAsync_GivenSqliteDatabase_GeneratesReport()
    {
        var dbPath = CommandAppHarness.CreateSampleSqliteDatabase();
        var outputDir = Path.Combine(Path.GetTempPath(), $"schematic-report-{Guid.NewGuid():N}");
        try
        {
            var (console, _) = CommandAppHarness.CreateCapturingConsole();
            var fileLauncher = new FakeFileLauncher(open: true);

            var registrar = new CommandAppHarness.InstanceRegistrar();
            registrar.RegisterInstance(typeof(IAnsiConsole), console);
            registrar.RegisterInstance(typeof(IDatabaseCommandDependencyProviderFactory), new DatabaseCommandDependencyProviderFactory());
            registrar.RegisterInstance(typeof(IFileLauncher), fileLauncher);

            var app = new CommandApp(registrar);
            app.Configure(config => config.AddCommand<ReportCommand>("report"));

            var exitCode = await app.RunAsync(
                ["report", "--dialect", "sqlite", "--connection-string", $"Data Source={dbPath}", "--output", outputDir]);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(exitCode, Is.Zero);
                Assert.That(File.Exists(Path.Combine(outputDir, "index.html")), Is.True);
                Assert.That(fileLauncher.OpenedPaths, Is.Empty);
            }
        }
        finally
        {
            CommandAppHarness.DeleteSqliteDatabase(dbPath);
            if (Directory.Exists(outputDir))
                Directory.Delete(outputDir, recursive: true);
        }
    }

    [Test]
    public static async Task ExecuteAsync_GivenOpenFlag_LaunchesGeneratedIndexFile()
    {
        var dbPath = CommandAppHarness.CreateSampleSqliteDatabase();
        var outputDir = Path.Combine(Path.GetTempPath(), $"schematic-report-{Guid.NewGuid():N}");
        try
        {
            var (console, _) = CommandAppHarness.CreateCapturingConsole();
            var fileLauncher = new FakeFileLauncher(open: true);

            var registrar = new CommandAppHarness.InstanceRegistrar();
            registrar.RegisterInstance(typeof(IAnsiConsole), console);
            registrar.RegisterInstance(typeof(IDatabaseCommandDependencyProviderFactory), new DatabaseCommandDependencyProviderFactory());
            registrar.RegisterInstance(typeof(IFileLauncher), fileLauncher);

            var app = new CommandApp(registrar);
            app.Configure(config => config.AddCommand<ReportCommand>("report"));

            var exitCode = await app.RunAsync(
                ["report", "--dialect", "sqlite", "--connection-string", $"Data Source={dbPath}", "--output", outputDir, "--open"]);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(exitCode, Is.Zero);
                Assert.That(fileLauncher.OpenedPaths, Is.EqualTo(new[] { Path.Combine(outputDir, "index.html") }));
            }
        }
        finally
        {
            CommandAppHarness.DeleteSqliteDatabase(dbPath);
            if (Directory.Exists(outputDir))
                Directory.Delete(outputDir, recursive: true);
        }
    }

    [Test]
    public static async Task ExecuteAsync_GivenOpenFlagAndLaunchFailure_StillReturnsSuccess()
    {
        var dbPath = CommandAppHarness.CreateSampleSqliteDatabase();
        var outputDir = Path.Combine(Path.GetTempPath(), $"schematic-report-{Guid.NewGuid():N}");
        try
        {
            var (console, writer) = CommandAppHarness.CreateCapturingConsole();
            var fileLauncher = new FakeFileLauncher(open: false);

            var registrar = new CommandAppHarness.InstanceRegistrar();
            registrar.RegisterInstance(typeof(IAnsiConsole), console);
            registrar.RegisterInstance(typeof(IDatabaseCommandDependencyProviderFactory), new DatabaseCommandDependencyProviderFactory());
            registrar.RegisterInstance(typeof(IFileLauncher), fileLauncher);

            var app = new CommandApp(registrar);
            app.Configure(config => config.AddCommand<ReportCommand>("report"));

            var exitCode = await app.RunAsync(
                ["report", "--dialect", "sqlite", "--connection-string", $"Data Source={dbPath}", "--output", outputDir, "--open"]);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(exitCode, Is.Zero);
                Assert.That(File.Exists(Path.Combine(outputDir, "index.html")), Is.True);
                Assert.That(writer.ToString(), Does.Contain("could not be opened automatically"));
            }
        }
        finally
        {
            CommandAppHarness.DeleteSqliteDatabase(dbPath);
            if (Directory.Exists(outputDir))
                Directory.Delete(outputDir, recursive: true);
        }
    }

    [Test]
    public static async Task ExecuteAsync_GivenSchemaOnlyLintFlag_LeavesOutRulesThatQueryTheDatabase()
    {
        var dbPath = CommandAppHarness.CreateSampleSqliteDatabase();
        var defaultOutputDir = Path.Combine(Path.GetTempPath(), $"schematic-report-{Guid.NewGuid():N}");
        var schemaOnlyOutputDir = Path.Combine(Path.GetTempPath(), $"schematic-report-{Guid.NewGuid():N}");
        try
        {
            var defaultExitCode = await RunReportAsync(dbPath, defaultOutputDir);
            var schemaOnlyExitCode = await RunReportAsync(dbPath, schemaOnlyOutputDir, "--schema-only-lint");

            var defaultRuleIds = await ReadLintRuleIdsAsync(defaultOutputDir);
            var schemaOnlyRuleIds = await ReadLintRuleIdsAsync(schemaOnlyOutputDir);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(defaultExitCode, Is.Zero);
                Assert.That(schemaOnlyExitCode, Is.Zero);
                // Every table in the sample database is empty, which only a rule that reads table
                // data can tell.
                Assert.That(defaultRuleIds, Does.Contain(NoRowsPresentOnTableRuleId));
                Assert.That(schemaOnlyRuleIds, Does.Not.Contain(NoRowsPresentOnTableRuleId));
                Assert.That(schemaOnlyRuleIds, Is.Not.Empty);
            }
        }
        finally
        {
            CommandAppHarness.DeleteSqliteDatabase(dbPath);
            if (Directory.Exists(defaultOutputDir))
                Directory.Delete(defaultOutputDir, recursive: true);
            if (Directory.Exists(schemaOnlyOutputDir))
                Directory.Delete(schemaOnlyOutputDir, recursive: true);
        }
    }

    private const string NoRowsPresentOnTableRuleId = "SCHEMATIC0039";

    private static async Task<int> RunReportAsync(string dbPath, string outputDir, params string[] extraArgs)
    {
        var (console, _) = CommandAppHarness.CreateCapturingConsole();

        var registrar = new CommandAppHarness.InstanceRegistrar();
        registrar.RegisterInstance(typeof(IAnsiConsole), console);
        registrar.RegisterInstance(typeof(IDatabaseCommandDependencyProviderFactory), new DatabaseCommandDependencyProviderFactory());
        registrar.RegisterInstance(typeof(IFileLauncher), new FakeFileLauncher(open: true));

        var app = new CommandApp(registrar);
        app.Configure(config => config.AddCommand<ReportCommand>("report"));

        return await app.RunAsync(
            ["report", "--dialect", "sqlite", "--connection-string", $"Data Source={dbPath}", "--output", outputDir, .. extraArgs]);
    }

    private static async Task<List<string>> ReadLintRuleIdsAsync(string outputDir)
    {
        var lintJson = await File.ReadAllBytesAsync(Path.Combine(outputDir, "data", "lint.json"));
        using var document = JsonDocument.Parse(lintJson);

        return document.RootElement
            .GetProperty("lintRules")
            .EnumerateArray()
            .Select(static rule => rule.GetProperty("ruleId").GetString()!)
            .ToList();
    }

    private sealed class FakeFileLauncher(bool open) : IFileLauncher
    {
        public List<string> OpenedPaths { get; } = [];

        public bool TryOpen(string path)
        {
            OpenedPaths.Add(path);
            return open;
        }
    }
}
