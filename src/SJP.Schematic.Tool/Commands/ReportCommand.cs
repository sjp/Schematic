using System;
using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
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

        [CommandOption("--open")]
        [Description("Open the generated report in the default browser once it's ready.")]
        [DefaultValue(false)]
        public bool Open { get; init; }

        public override ValidationResult Validate()
        {
            var baseResult = base.Validate();
            if (!baseResult.Successful)
                return baseResult;

            return OutputDirectory == null
                ? ValidationResult.Error("An output directory must be specified with --output.")
                : ValidationResult.Success();
        }
    }

    private readonly IAnsiConsole _console;
    private readonly IDatabaseCommandDependencyProviderFactory _dependencyProviderFactory;
    private readonly IFileLauncher _fileLauncher;

    public ReportCommand(
        IAnsiConsole console,
        IDatabaseCommandDependencyProviderFactory dependencyProviderFactory,
        IFileLauncher fileLauncher)
    {
        ArgumentNullException.ThrowIfNull(console);
        ArgumentNullException.ThrowIfNull(dependencyProviderFactory);
        ArgumentNullException.ThrowIfNull(fileLauncher);

        _console = console;
        _dependencyProviderFactory = dependencyProviderFactory;
        _fileLauncher = fileLauncher;
    }

    protected override async Task<int> ExecuteAsync(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        var dependencyProvider = _dependencyProviderFactory.GetDbDependencies(settings);
        var connection = dependencyProvider.GetSchematicConnection();
        var databaseProvider = dependencyProvider.GetRelationalDatabaseProvider(connection);
        var database = await databaseProvider.GetRelationalDatabaseAsync(cancellationToken);

        var snapshotDb = await database.SnapshotAsync(cancellationToken);

        var reportGenerator = new ReportGenerator(connection, databaseProvider, snapshotDb, settings.OutputDirectory!.FullName);
        await reportGenerator.GenerateAsync(cancellationToken);

        _console.Write("Report generated to: " + settings.OutputDirectory!.FullName);

        if (settings.Open)
        {
            var indexPath = Path.Combine(settings.OutputDirectory!.FullName, "index.html");
            if (!_fileLauncher.TryOpen(indexPath))
                _console.MarkupLine("[yellow]Report was generated, but could not be opened automatically.[/]");
        }

        return ErrorCode.Success;
    }
}