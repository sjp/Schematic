#nullable enable
using System;
using System.IO;
using System.IO.Abstractions;
using System.Text.Json;
using System.Threading.Tasks;
using NUnit.Framework;
using SJP.Schematic.Tool.Commands;
using SJP.Schematic.Tool.Handlers;
using Spectre.Console;
using Spectre.Console.Cli;

namespace SJP.Schematic.Tool.Tests.Commands;

[TestFixture]
internal static class ExportCommandTests
{
    [Test]
    public static void Validate_GivenConnectionButNoFormat_ReturnsError()
    {
        var settings = new ExportCommand.Settings
        {
            Dialect = "sqlite",
            ConnectionString = "Data Source=:memory:",
            Format = null,
            Output = new FileInfo("./schema.dbml"),
        };

        var result = settings.Validate();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Successful, Is.False);
            Assert.That(result.Message, Does.Contain("--format"));
        }
    }

    [Test]
    public static void Validate_GivenConnectionAndFormatButNoOutput_ReturnsError()
    {
        var settings = new ExportCommand.Settings
        {
            Dialect = "sqlite",
            ConnectionString = "Data Source=:memory:",
            Format = "dbml",
            Output = null,
        };

        var result = settings.Validate();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Successful, Is.False);
            Assert.That(result.Message, Does.Contain("--output"));
        }
    }

    [Test]
    public static void Validate_GivenUnrecognizedFormat_ReturnsError()
    {
        var settings = new ExportCommand.Settings
        {
            Dialect = "sqlite",
            ConnectionString = "Data Source=:memory:",
            Format = "bogus",
            Output = new FileInfo("./schema.bogus"),
        };

        var result = settings.Validate();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Successful, Is.False);
            Assert.That(result.Message, Does.Contain("bogus"));
        }
    }

    [TestCase("dbml")]
    [TestCase("DBML")]
    [TestCase("json")]
    [TestCase("JSON")]
    public static void Validate_GivenConnectionFormatAndOutput_ReturnsSuccess(string format)
    {
        var settings = new ExportCommand.Settings
        {
            Dialect = "sqlite",
            ConnectionString = "Data Source=:memory:",
            Format = format,
            Output = new FileInfo("./schema.out"),
        };

        var result = settings.Validate();

        Assert.That(result.Successful, Is.True);
    }

    private static async Task<(int exitCode, string output)> RunExportAsync(string dbPath, string format, string outputPath)
    {
        var (console, writer) = CommandAppHarness.CreateCapturingConsole();

        var registrar = new CommandAppHarness.InstanceRegistrar();
        registrar.RegisterInstance(typeof(IAnsiConsole), console);
        registrar.RegisterInstance(typeof(IFileSystem), new FileSystem());
        registrar.RegisterInstance(typeof(IDatabaseCommandDependencyProviderFactory), new DatabaseCommandDependencyProviderFactory());

        var app = new CommandApp(registrar);
        app.Configure(config => config.AddCommand<ExportCommand>("export"));

        var exitCode = await app.RunAsync([
            "export",
            "--dialect", "sqlite",
            "--connection-string", $"Data Source={dbPath}",
            "--format", format,
            "--output", outputPath,
        ]);
        return (exitCode, writer.ToString());
    }

    [Test]
    public static async Task ExecuteAsync_GivenDbmlFormat_WritesNonEmptyDbmlFile()
    {
        var dbPath = CommandAppHarness.CreateSampleSqliteDatabase();
        var outputPath = Path.Combine(Path.GetTempPath(), $"schematic-export-{Guid.NewGuid():N}.dbml");
        try
        {
            var (exitCode, _) = await RunExportAsync(dbPath, "dbml", outputPath);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(exitCode, Is.Zero);
                Assert.That(File.Exists(outputPath), Is.True);
                Assert.That(File.ReadAllText(outputPath), Is.Not.Empty);
            }
        }
        finally
        {
            CommandAppHarness.DeleteSqliteDatabase(dbPath);
            if (File.Exists(outputPath))
                File.Delete(outputPath);
        }
    }

    [Test]
    public static async Task ExecuteAsync_GivenJsonFormat_WritesValidJsonFile()
    {
        var dbPath = CommandAppHarness.CreateSampleSqliteDatabase();
        var outputPath = Path.Combine(Path.GetTempPath(), $"schematic-export-{Guid.NewGuid():N}.json");
        try
        {
            var (exitCode, _) = await RunExportAsync(dbPath, "json", outputPath);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(exitCode, Is.Zero);
                Assert.That(File.Exists(outputPath), Is.True);
                Assert.DoesNotThrow(() => JsonDocument.Parse(File.ReadAllText(outputPath)));
            }
        }
        finally
        {
            CommandAppHarness.DeleteSqliteDatabase(dbPath);
            if (File.Exists(outputPath))
                File.Delete(outputPath);
        }
    }

    [TestCase("dbml")]
    [TestCase("json")]
    public static async Task ExecuteAsync_GivenRepeatedExport_ProducesIdenticalOutput(string format)
    {
        var dbPath = CommandAppHarness.CreateSampleSqliteDatabase();
        var firstPath = Path.Combine(Path.GetTempPath(), $"schematic-export-{Guid.NewGuid():N}.{format}");
        var secondPath = Path.Combine(Path.GetTempPath(), $"schematic-export-{Guid.NewGuid():N}.{format}");
        try
        {
            var (firstExitCode, _) = await RunExportAsync(dbPath, format, firstPath);
            var (secondExitCode, _) = await RunExportAsync(dbPath, format, secondPath);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(firstExitCode, Is.Zero);
                Assert.That(secondExitCode, Is.Zero);
                Assert.That(File.ReadAllText(secondPath), Is.EqualTo(File.ReadAllText(firstPath)));
            }
        }
        finally
        {
            CommandAppHarness.DeleteSqliteDatabase(dbPath);
            foreach (var path in new[] { firstPath, secondPath })
            {
                if (File.Exists(path))
                    File.Delete(path);
            }
        }
    }

    [Test]
    public static async Task ExecuteAsync_GivenUnrecognizedFormat_ReturnsClearValidationError()
    {
        var dbPath = CommandAppHarness.CreateSampleSqliteDatabase();
        var outputPath = Path.Combine(Path.GetTempPath(), $"schematic-export-{Guid.NewGuid():N}.txt");
        try
        {
            var (exitCode, _) = await RunExportAsync(dbPath, "bogus", outputPath);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(exitCode, Is.Not.Zero);
                Assert.That(File.Exists(outputPath), Is.False);
            }
        }
        finally
        {
            CommandAppHarness.DeleteSqliteDatabase(dbPath);
            if (File.Exists(outputPath))
                File.Delete(outputPath);
        }
    }
}
