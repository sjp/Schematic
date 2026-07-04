#nullable enable
using System;
using System.IO;
using System.IO.Abstractions;
using System.Threading.Tasks;
using NUnit.Framework;
using SJP.Schematic.Tool.Commands;
using SJP.Schematic.Tool.Handlers;
using Spectre.Console;
using Spectre.Console.Cli;

namespace SJP.Schematic.Tool.Tests.Commands;

[TestFixture]
internal static class OrmCommandsTests
{
    private static async Task<int> RunOrmAsync(string subcommand, string projectPath)
    {
        var dbPath = CommandAppHarness.CreateSampleSqliteDatabase();
        try
        {
            var (console, _) = CommandAppHarness.CreateCapturingConsole();

            var registrar = new CommandAppHarness.InstanceRegistrar();
            registrar.RegisterInstance(typeof(IAnsiConsole), console);
            registrar.RegisterInstance(typeof(IFileSystem), new FileSystem());
            registrar.RegisterInstance(typeof(IDatabaseCommandDependencyProviderFactory), new DatabaseCommandDependencyProviderFactory());

            var app = new CommandApp(registrar);
            app.Configure(config => config.AddBranch<OrmCommand.Settings>("orm", orm =>
            {
                orm.AddCommand<GenerateEfCoreCommand>("efcore");
                orm.AddCommand<GenerateOrmLiteCommand>("ormlite");
                orm.AddCommand<GeneratePocoCommand>("poco");
            }));

            // For a branch command, options bind at the branch level and precede the subcommand.
            return await app.RunAsync([
                "orm",
                "--dialect", "sqlite",
                "--connection-string", $"Data Source={dbPath}",
                "--project-path", projectPath,
                "--base-namespace", "Schematic.Test.DataAccess",
                subcommand,
            ]);
        }
        finally
        {
            CommandAppHarness.DeleteSqliteDatabase(dbPath);
        }
    }

    [TestCase("efcore")]
    [TestCase("ormlite")]
    [TestCase("poco")]
    public static async Task ExecuteAsync_GivenSqliteDatabase_GeneratesProject(string subcommand)
    {
        var outputDir = Path.Combine(Path.GetTempPath(), $"schematic-orm-{Guid.NewGuid():N}");
        Directory.CreateDirectory(outputDir);
        var projectPath = Path.Combine(outputDir, "Generated.csproj");
        try
        {
            var exitCode = await RunOrmAsync(subcommand, projectPath);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(exitCode, Is.Zero);
                Assert.That(File.Exists(projectPath), Is.True);
            }
        }
        finally
        {
            if (Directory.Exists(outputDir))
                Directory.Delete(outputDir, recursive: true);
        }
    }
}
