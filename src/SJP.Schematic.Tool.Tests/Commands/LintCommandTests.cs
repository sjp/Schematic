#nullable enable
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
}
