using System.IO.Abstractions;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using SJP.Schematic.Tool.Commands;
using SJP.Schematic.Tool.Handlers;
using Spectre.Console;
using Spectre.Console.Cli;
using Spectre.Console.Cli.Extensions.DependencyInjection;

namespace SJP.Schematic.Tool;

internal static class Program
{
    public static Task<int> Main(string[] args)
    {
        var services = new ServiceCollection();
        services.AddSingleton<IFileSystem, FileSystem>();
        services.AddSingleton<IDatabaseCommandDependencyProvider, DatabaseCommandDependencyProvider>();
        services.AddSingleton<IDatabaseCommandDependencyProviderFactory, DatabaseCommandDependencyProviderFactory>();
        var registrar = new DependencyInjectionRegistrar(services);

        var app = new CommandApp(registrar);

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
            config.AddCommand<InitCommand>("init");
            config.AddCommand<LintCommand>("lint");
            config.AddCommand<ReportCommand>("report");
            config.AddCommand<TestCommand>("test");
            config.AddCommand<CompletionCommand>("completion")
                .WithExample("completion", "bash");

            config.ValidateExamples();
            config.SetExceptionHandler((ex, _) =>
            {
                // Parse/validation failures carry a user-actionable message; render it plainly
                // rather than dumping a stack trace. Anything else is unexpected, so show detail.
                if (ex is CommandParseException or CommandRuntimeException)
                    AnsiConsole.MarkupLineInterpolated($"[red]{ex.Message}[/]");
                else
                    AnsiConsole.WriteException(ex, ExceptionFormats.ShortenEverything);

                return ErrorCode.Error;
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
}