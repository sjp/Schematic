using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO.Abstractions;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using SJP.Schematic.Tool.Handlers;
using Spectre.Console;
using Spectre.Console.Cli;

namespace SJP.Schematic.Tool.Commands;

[Description("Interactively create a schematic configuration file.")]
internal sealed class InitCommand : AsyncCommand<InitCommand.Settings>
{
    private static readonly string[] Dialects = ["sqlserver", "postgresql", "mysql", "oracle", "sqlite"];

    public sealed class Settings : CommandSettings
    {
        [CommandOption("-o|--output <FILE>")]
        [Description("The path to write the generated configuration file to.")]
        [DefaultValue(CommonSettings.DefaultConfigFileName)]
        public string OutputPath { get; init; } = CommonSettings.DefaultConfigFileName;

        [CommandOption("--force")]
        [Description("Overwrite the output file if it already exists.")]
        public bool Force { get; init; }
    }

    private readonly IAnsiConsole _console;
    private readonly IFileSystem _fileSystem;

    public InitCommand(IAnsiConsole console, IFileSystem fileSystem)
    {
        ArgumentNullException.ThrowIfNull(console);
        ArgumentNullException.ThrowIfNull(fileSystem);

        _console = console;
        _fileSystem = fileSystem;
    }

    protected override async Task<int> ExecuteAsync(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        if (_fileSystem.File.Exists(settings.OutputPath) && !settings.Force)
        {
            var overwrite = _console.Confirm($"[yellow]'{settings.OutputPath}' already exists. Overwrite?[/]", false);
            if (!overwrite)
            {
                _console.MarkupLine("[yellow]Aborted.[/]");
                return ErrorCode.Error;
            }
        }

        var dialect = _console.Prompt(
            new SelectionPrompt<string>()
                .Title("Which database [green]dialect[/] are you connecting to?")
                .AddChoices(Dialects));

        var connectionString = PromptForConnectionString(dialect);

        if (_console.Confirm("Test the connection now?"))
            await TestConnectionAsync(dialect, connectionString, cancellationToken);

        WriteConfig(settings.OutputPath, dialect, connectionString);

        _console.MarkupLineInterpolated($"[green]Configuration written to {settings.OutputPath}[/]");
        _console.MarkupLineInterpolated($"Run a report with: [grey]schematic report -c {settings.OutputPath} --output ./report[/]");
        return ErrorCode.Success;
    }

    private string PromptForConnectionString(string dialect)
    {
        if (string.Equals(dialect, "sqlite", StringComparison.Ordinal))
        {
            var path = _console.Prompt(new TextPrompt<string>("Database [green]file path[/]:"));
            return ConnectionStringFactory.ForSqlite(path);
        }

        var mode = _console.Prompt(
            new SelectionPrompt<string>()
                .Title("How would you like to provide the connection details?")
                .AddChoices("Guided (enter host, credentials, etc.)", "Paste a full connection string"));

        if (mode.StartsWith("Paste", StringComparison.Ordinal))
            return _console.Prompt(new TextPrompt<string>("[green]Connection string[/]:").Secret());

        var host = _console.Prompt(new TextPrompt<string>("Host:"));

        var portText = _console.Prompt(new TextPrompt<string>("Port [grey](leave blank for the provider default)[/]:").AllowEmpty());
        int? port = int.TryParse(portText, out var parsedPort) ? parsedPort : null;

        var user = _console.Prompt(new TextPrompt<string>("User:").AllowEmpty());
        var password = _console.Prompt(new TextPrompt<string>("Password:").Secret().AllowEmpty());

        var databaseLabel = string.Equals(dialect, "oracle", StringComparison.Ordinal) ? "Service name:" : "Database:";
        var database = _console.Prompt(new TextPrompt<string>(databaseLabel).AllowEmpty());

        return ConnectionStringFactory.BuildGuided(
            dialect,
            new ConnectionStringFactory.ConnectionDetails(host, port, user, password, database));
    }

    private async Task TestConnectionAsync(string dialect, string connectionString, CancellationToken cancellationToken)
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Dialect"] = dialect,
                ["ConnectionStrings:Schematic"] = connectionString,
            })
            .Build();

        var connectionFactory = new DatabaseCommandDependencyProvider(config).GetConnectionFactory();

        try
        {
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(TimeSpan.FromSeconds(10));

            _ = await connectionFactory.OpenConnectionAsync(cts.Token);

            _console.MarkupLine("[green]Successfully connected to the database.[/]");
        }
        catch (OperationCanceledException)
        {
            cancellationToken.ThrowIfCancellationRequested();
            _console.MarkupLine("[red]Database connection request timed out.[/] The configuration will still be saved.");
        }
        catch (Exception ex)
        {
            _console.MarkupLine("[red]Failed to connect to the database.[/] The configuration will still be saved.");
            _console.MarkupLineInterpolated($"    [red]{ex.Message}[/]");
        }
    }

    private void WriteConfig(string outputPath, string dialect, string connectionString)
    {
        var config = new JsonObject
        {
            ["Dialect"] = dialect,
            ["ConnectionStrings"] = new JsonObject
            {
                ["Schematic"] = connectionString,
            },
        };

        var json = config.ToJsonString(new JsonSerializerOptions { WriteIndented = true });
        _fileSystem.File.WriteAllText(outputPath, json);
    }
}
