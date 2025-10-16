using System;
using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Reporting;
using SJP.Schematic.Tool.Handlers;
using Spectre.Console;
using Spectre.Console.Cli;

namespace SJP.Schematic.Tool.Commands;

internal sealed class ReportCommand : AsyncCommand<ReportCommand.Settings>
{
    public sealed class Settings : CommonSettings
    {
        [CommandOption("--output <DIRECTORY>")]
        [Description("The directory to save the generated report.")]
        public DirectoryInfo? OutputDirectory { get; init; }
    }

    private readonly IAnsiConsole _console;
    private readonly IDatabaseCommandDependencyProviderFactory _dependencyProviderFactory;

    public ReportCommand(
        IAnsiConsole console,
        IDatabaseCommandDependencyProviderFactory dependencyProviderFactory)
    {
        ArgumentNullException.ThrowIfNull(console);
        ArgumentNullException.ThrowIfNull(dependencyProviderFactory);

        _console = console;
        _dependencyProviderFactory = dependencyProviderFactory;
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        var cancellationToken = CancellationToken.None;

        var dependencyProvider = _dependencyProviderFactory.GetDbDependencies(settings.ConfigFile!.FullName);
        ConfigureDotPath(dependencyProvider.Configuration);
        var connection = dependencyProvider.GetSchematicConnection();
        var database = await connection.Dialect.GetRelationalDatabaseAsync(connection, cancellationToken);

        var snapshotDb = await database.SnapshotAsync(cancellationToken);

        var reportGenerator = new ReportGenerator(connection, snapshotDb, settings.OutputDirectory!.FullName);
        await reportGenerator.GenerateAsync(cancellationToken);

        _console.Write("Report generated to: " + settings.OutputDirectory!.FullName);
        return ErrorCode.Success;
    }

    private static void ConfigureDotPath(IConfiguration configuration)
    {
        var dotPath = configuration["Graphviz:Dot"];
        if (!dotPath.IsNullOrWhiteSpace())
            Environment.SetEnvironmentVariable("SCHEMATIC_GRAPHVIZ_DOT", dotPath, EnvironmentVariableTarget.Process);
    }
}