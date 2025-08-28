using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Reporting;
using Spectre.Console;

namespace SJP.Schematic.Tool.Handlers;

internal sealed class ReportCommandHandler : DatabaseCommandHandler
{
    private readonly IAnsiConsole _console;

    public ReportCommandHandler(IAnsiConsole console, FileInfo filePath)
        : base(filePath)
    {
        _console = console ?? throw new ArgumentNullException(nameof(console));
        ConfigureDotPath();
    }

    public async Task<int> HandleCommandAsync(DirectoryInfo outputPath, CancellationToken cancellationToken)
    {
        var connection = GetSchematicConnection();
        var database = await connection.Dialect.GetRelationalDatabaseAsync(connection, cancellationToken).ConfigureAwait(false);

        var snapshotDb = await database.SnapshotAsync(cancellationToken).ConfigureAwait(false);

        var reportGenerator = new ReportGenerator(connection, snapshotDb, outputPath);
        await reportGenerator.GenerateAsync(cancellationToken).ConfigureAwait(false);

        _console.Write("Report generated to: " + outputPath.FullName);
        return ErrorCode.Success;
    }

    private void ConfigureDotPath()
    {
        var dotPath = Configuration.GetValue<string>("Graphviz:Dot");
        if (!dotPath.IsNullOrWhiteSpace())
            Environment.SetEnvironmentVariable("SCHEMATIC_GRAPHVIZ_DOT", dotPath, EnvironmentVariableTarget.Process);
    }
}