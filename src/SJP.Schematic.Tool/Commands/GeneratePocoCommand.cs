using System;
using System.IO.Abstractions;
using System.Threading;
using System.Threading.Tasks;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.DataAccess.Poco;
using SJP.Schematic.Tool.Handlers;
using Spectre.Console;
using Spectre.Console.Cli;

namespace SJP.Schematic.Tool.Commands;

internal sealed class GeneratePocoCommand : AsyncCommand<GeneratePocoCommand.Settings>
{
    public sealed class Settings : OrmCommand.Settings
    {
    }

    private readonly IFileSystem _fileSystem;
    private readonly IAnsiConsole _console;
    private readonly IDatabaseCommandDependencyProviderFactory _dependencyProviderFactory;

    public GeneratePocoCommand(
        IFileSystem fileSystem,
        IAnsiConsole console,
        IDatabaseCommandDependencyProviderFactory dependencyProviderFactory)
    {
        ArgumentNullException.ThrowIfNull(fileSystem);
        ArgumentNullException.ThrowIfNull(console);
        ArgumentNullException.ThrowIfNull(dependencyProviderFactory);

        _fileSystem = fileSystem;
        _console = console;
        _dependencyProviderFactory = dependencyProviderFactory;
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        var cancellationToken = CancellationToken.None;

        var dependencyProvider = _dependencyProviderFactory.GetDbDependencies(settings.ConfigFile!.FullName);
        var connection = dependencyProvider.GetSchematicConnection();
        var nameTranslator = dependencyProvider.GetNameTranslator(settings.NamingConvention);

        var (database, commentProvider) = await (
            connection.Dialect.GetRelationalDatabaseAsync(connection, cancellationToken),
            connection.Dialect.GetRelationalDatabaseCommentProviderAsync(connection, cancellationToken)
        ).WhenAll();

        var generator = new PocoDataAccessGenerator(_fileSystem, database, commentProvider, nameTranslator);
        await generator.GenerateAsync(settings.ProjectPath!.FullName, settings.BaseNamespace!, cancellationToken);

        _console.MarkupLineInterpolated($"[green]Project generated at: {settings.ProjectPath.FullName}[/]");
        return ErrorCode.Success;
    }
}