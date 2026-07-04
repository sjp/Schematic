using System;
using System.IO;
using System.Threading.Tasks;
using NUnit.Framework;
using SJP.Schematic.Tool.Commands;
using SJP.Schematic.Tool.Handlers;
using Spectre.Console;
using Spectre.Console.Cli;

namespace SJP.Schematic.Tool.Tests.Commands;

[TestFixture]
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

            var registrar = new CommandAppHarness.InstanceRegistrar();
            registrar.RegisterInstance(typeof(IAnsiConsole), console);
            registrar.RegisterInstance(typeof(IDatabaseCommandDependencyProviderFactory), new DatabaseCommandDependencyProviderFactory());

            var app = new CommandApp(registrar);
            app.Configure(config => config.AddCommand<ReportCommand>("report"));

            var exitCode = await app.RunAsync(
                ["report", "--dialect", "sqlite", "--connection-string", $"Data Source={dbPath}", "--output", outputDir]);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(exitCode, Is.Zero);
                Assert.That(File.Exists(Path.Combine(outputDir, "index.html")), Is.True);
            }
        }
        finally
        {
            CommandAppHarness.DeleteSqliteDatabase(dbPath);
            if (Directory.Exists(outputDir))
                Directory.Delete(outputDir, recursive: true);
        }
    }
}
