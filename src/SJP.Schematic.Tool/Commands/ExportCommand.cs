using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Dbml;
using SJP.Schematic.Serialization;
using SJP.Schematic.Tool.Handlers;
using Spectre.Console;
using Spectre.Console.Cli;

namespace SJP.Schematic.Tool.Commands;

internal sealed class ExportCommand : AsyncCommand<ExportCommand.Settings>
{
    public sealed class Settings : CommonSettings
    {
        // Bound as a string, rather than directly as ExportFormat, so an unrecognized value can be
        // reported with a clear, custom message in Validate() instead of Spectre's generic type
        // conversion failure (which would otherwise leak the underlying Nullable<T> type name).
        [CommandOption("--format <FORMAT>")]
        [Description("The format to export the schema in. One of: dbml, json.")]
        public string? Format { get; init; }

        [CommandOption("--output <FILE>")]
        [Description("The file to save the exported schema to.")]
        public FileInfo? Output { get; init; }

        public override ValidationResult Validate()
        {
            var baseResult = base.Validate();
            if (!baseResult.Successful)
                return baseResult;

            if (string.IsNullOrWhiteSpace(Format))
                return ValidationResult.Error("A format must be specified with --format (dbml or json).");

            if (!Enum.TryParse<ExportFormat>(Format, ignoreCase: true, out _))
                return ValidationResult.Error($"Unrecognized format '{Format}'. Valid values are: dbml, json.");

            return Output == null
                ? ValidationResult.Error("An output file must be specified with --output.")
                : ValidationResult.Success();
        }

        internal ExportFormat ParsedFormat => Enum.Parse<ExportFormat>(Format!, ignoreCase: true);
    }

    private readonly IAnsiConsole _console;
    private readonly IDatabaseCommandDependencyProviderFactory _dependencyProviderFactory;
    private readonly IFileSystem _fileSystem;

    public ExportCommand(
        IAnsiConsole console,
        IDatabaseCommandDependencyProviderFactory dependencyProviderFactory,
        IFileSystem fileSystem)
    {
        ArgumentNullException.ThrowIfNull(console);
        ArgumentNullException.ThrowIfNull(dependencyProviderFactory);
        ArgumentNullException.ThrowIfNull(fileSystem);

        _console = console;
        _dependencyProviderFactory = dependencyProviderFactory;
        _fileSystem = fileSystem;
    }

    protected override async Task<int> ExecuteAsync(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        var dependencyProvider = _dependencyProviderFactory.GetDbDependencies(settings);
        var connection = dependencyProvider.GetSchematicConnection();
        var database = await connection.Dialect.GetRelationalDatabaseAsync(connection, cancellationToken);

        var snapshotDb = await database.SnapshotAsync(cancellationToken);
        var sortedDb = await SortDatabaseAsync(snapshotDb, cancellationToken);

        switch (settings.ParsedFormat)
        {
            case ExportFormat.Dbml:
                var tables = await sortedDb.GetAllTables(cancellationToken);
                var dbml = new DbmlFormatter().RenderTables(tables);
                await using (var writer = _fileSystem.File.CreateText(settings.Output!.FullName))
                    await writer.WriteAsync(dbml.AsMemory(), cancellationToken);
                break;
            case ExportFormat.Json:
                await using (var stream = _fileSystem.File.Create(settings.Output!.FullName))
                    await new JsonRelationalDatabaseSerializer().SerializeAsync(stream, sortedDb, cancellationToken);
                break;
            default:
                throw new NotSupportedException($"Unsupported export format: {settings.Format}");
        }

        _console.Write("Schema exported to: " + settings.Output!.FullName);

        return ErrorCode.Success;
    }

    // Neither DbmlFormatter nor JsonRelationalDatabaseSerializer sort their input, so exports are
    // otherwise only as stable as the underlying provider's enumeration order. Sorting the
    // top-level objects here (but not their nested columns/indexes/keys, whose order is either
    // meaningful or already deterministic per run) keeps diffs on exported files readable across
    // re-exports.
    private static async Task<IRelationalDatabase> SortDatabaseAsync(IRelationalDatabase db, CancellationToken cancellationToken)
    {
        var (tables, views, sequences, synonyms, routines) = await (
            db.GetAllTables(cancellationToken),
            db.GetAllViews(cancellationToken),
            db.GetAllSequences(cancellationToken),
            db.GetAllSynonyms(cancellationToken),
            db.GetAllRoutines(cancellationToken)
        ).WhenAll();

        return new RelationalDatabase(
            db.IdentifierDefaults,
            new VerbatimIdentifierResolutionStrategy(),
            Sort(tables),
            Sort(views),
            Sort(sequences),
            Sort(synonyms),
            Sort(routines));
    }

    private static IReadOnlyCollection<T> Sort<T>(IReadOnlyCollection<T> entities) where T : IDatabaseEntity => entities
        .OrderBy(static e => e.Name.Schema, StringComparer.Ordinal)
        .ThenBy(static e => e.Name.LocalName, StringComparer.Ordinal)
        .ToList();
}
