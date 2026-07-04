using System;
using System.IO;
using System.Threading.Tasks;
using NUnit.Framework;
using SJP.Schematic.Tool.Commands;
using Spectre.Console.Cli;

namespace SJP.Schematic.Tool.Tests.Commands;

[TestFixture]
internal static class CompletionCommandTests
{
    [Test]
    public static async Task Execute_GivenBashShell_WritesScriptToStandardOutput()
    {
        var registrar = new CommandAppHarness.InstanceRegistrar();
        var app = new CommandApp(registrar);
        app.Configure(config => config.AddCommand<CompletionCommand>("completion"));

        var originalOut = Console.Out;
        var writer = new StringWriter();
        Console.SetOut(writer);
        try
        {
            var exitCode = await app.RunAsync(["completion", "bash"]);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(exitCode, Is.Zero);
                Assert.That(writer.ToString(), Does.Contain("_schematic"));
            }
        }
        finally
        {
            Console.SetOut(originalOut);
        }
    }

    [Test]
    public static void GetCompletionScript_GivenUnknownShell_ThrowsArgumentOutOfRangeException()
    {
        Assert.That(
            () => CompletionCommand.GetCompletionScript((CompletionCommand.ShellType)int.MaxValue),
            Throws.InstanceOf<ArgumentOutOfRangeException>());
    }

    [TestCaseSource(nameof(AllShells))]
    public static void GetCompletionScript_GivenSupportedShell_ReturnsNonEmptyScript(CompletionCommand.ShellType shell)
    {
        var script = CompletionCommand.GetCompletionScript(shell);

        Assert.That(script, Is.Not.Null.And.Not.Empty);
    }

    [TestCaseSource(nameof(AllShells))]
    public static void GetCompletionScript_GivenSupportedShell_MentionsAllTopLevelCommands(CompletionCommand.ShellType shell)
    {
        var script = CompletionCommand.GetCompletionScript(shell);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(script, Does.Contain("orm"));
            Assert.That(script, Does.Contain("init"));
            Assert.That(script, Does.Contain("lint"));
            Assert.That(script, Does.Contain("report"));
            Assert.That(script, Does.Contain("test"));
            Assert.That(script, Does.Contain("completion"));
        }
    }

    [TestCaseSource(nameof(AllShells))]
    public static void GetCompletionScript_GivenSupportedShell_MentionsAllOrmSubcommands(CompletionCommand.ShellType shell)
    {
        var script = CompletionCommand.GetCompletionScript(shell);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(script, Does.Contain("efcore"));
            Assert.That(script, Does.Contain("ormlite"));
            Assert.That(script, Does.Contain("poco"));
        }
    }

    [Test]
    public static void GetCompletionScript_GivenBashShell_RegistersBashCompletionFunction()
    {
        var script = CompletionCommand.GetCompletionScript(CompletionCommand.ShellType.Bash);

        Assert.That(script, Does.Contain("complete -F _schematic schematic"));
    }

    [Test]
    public static void GetCompletionScript_GivenZshShell_StartsWithCompdefDirective()
    {
        var script = CompletionCommand.GetCompletionScript(CompletionCommand.ShellType.Zsh);

        Assert.That(script, Does.StartWith("#compdef schematic"));
    }

    [Test]
    public static void GetCompletionScript_GivenFishShell_UsesFishCompleteCommands()
    {
        var script = CompletionCommand.GetCompletionScript(CompletionCommand.ShellType.Fish);

        Assert.That(script, Does.Contain("complete -c schematic"));
    }

    [Test]
    public static void GetCompletionScript_GivenPowerShell_RegistersNativeArgumentCompleter()
    {
        var script = CompletionCommand.GetCompletionScript(CompletionCommand.ShellType.PowerShell);

        Assert.That(script, Does.Contain("Register-ArgumentCompleter -Native -CommandName 'schematic'"));
    }

    private static CompletionCommand.ShellType[] AllShells => Enum.GetValues<CompletionCommand.ShellType>();
}
