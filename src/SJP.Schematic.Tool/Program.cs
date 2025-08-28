using System;
using System.Reflection;
using System.Threading.Tasks;
using Oracle.ManagedDataAccess.Client;
using SJP.Schematic.Tool.Commands;
using Spectre.Console;
using Spectre.Console.Cli;

namespace SJP.Schematic.Tool;

internal static class Program
{
    public static Task<int> Main(string[] args)
    {
        var app = new CommandApp();

        app.Configure(config =>
        {
            config.SetApplicationName("schematic");
            config.SetApplicationVersion(GetVersion());

            config.AddBranch<OrmCommand.Settings>("orm", orm =>
            {
                orm.SetDescription("Generate ORM projects to interact with a database.");
                orm.AddCommand<GenerateEfCoreCommand>("efcore");
                orm.AddCommand<GenerateOrmLiteCommand>("ormlite");
                orm.AddCommand<GeneratePocoCommand>("poco");
            });
            config.AddCommand<LintCommand>("lint");
            config.AddCommand<ReportCommand>("report");
            config.AddCommand<TestCommand>("test");

            config.PropagateExceptions();
            config.ValidateExamples();
            config.SetExceptionHandler((ex, resolver) =>
            {
                AnsiConsole.WriteException(ex, ExceptionFormats.ShortenEverything);
            });
        });

        return app.RunAsync(args);
    }

    private static string GetVersion()
    {
        var assembly = Assembly.GetEntryAssembly()!;
        var assemblyVersion = assembly.GetName().Version!;
        return $"v{assemblyVersion.Major}.{assemblyVersion.Minor}.{assemblyVersion.Build}";
    }

    private static void HandleException(Exception exception)
    {
        AnsiConsole.Foreground = ConsoleColor.Red;

        if (exception is TargetInvocationException tie &&
            tie.InnerException is not null)
        {
            exception = tie.InnerException;
        }

        // take the first if present
        if (exception is AggregateException ae && ae.InnerExceptions.Count > 0)
        {
            exception = ae.InnerExceptions[0];
        }

        if (exception is OperationCanceledException)
        {
            AnsiConsole.WriteLine("...canceled.");
        }
        // don't know why, but oracle wraps cancellation in its own exception
        else if (exception is OracleException oex && oex.Number == 1013)
        {
            AnsiConsole.WriteLine("...canceled.");
        }
        else if (exception is CommandException command)
        {
            AnsiConsole.WriteLine($"The command failed:");
            AnsiConsole.WriteLine($"    {command.Message}");

            if (command.InnerException != null)
            {
                AnsiConsole.WriteLine();
                AnsiConsole.WriteLine(command.InnerException.ToString());
            }
        }
        else
        {
            AnsiConsole.WriteLine("An unhandled exception has occurred: ");
            AnsiConsole.WriteLine(exception.ToString());
        }

        AnsiConsole.ResetColors();
    }
}