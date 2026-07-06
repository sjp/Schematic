using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Lint;
using SJP.Schematic.Tool.Handlers;
using Spectre.Console;
using Spectre.Console.Cli;

namespace SJP.Schematic.Tool.Commands;

internal sealed class LintCommand : AsyncCommand<LintCommand.Settings>
{
    public sealed class Settings : CommonSettings
    {
        [CommandOption("--format <FORMAT>")]
        [Description("The format to render lint results in. One of: text, json, sarif.")]
        [DefaultValue(LintOutputFormat.Text)]
        public LintOutputFormat Format { get; init; }
    }

    private readonly IAnsiConsole _console;
    private readonly IDatabaseCommandDependencyProviderFactory _dependencyProviderFactory;

    public LintCommand(
        IAnsiConsole console,
        IDatabaseCommandDependencyProviderFactory dependencyProviderFactory)
    {
        ArgumentNullException.ThrowIfNull(console);
        ArgumentNullException.ThrowIfNull(dependencyProviderFactory);

        _console = console;
        _dependencyProviderFactory = dependencyProviderFactory;
    }

    protected override async Task<int> ExecuteAsync(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        var dependencyProvider = _dependencyProviderFactory.GetDbDependencies(settings);
        var connection = dependencyProvider.GetSchematicConnection();
        var database = await connection.Dialect.GetRelationalDatabaseAsync(connection, cancellationToken);

        var ruleProvider = new DefaultRuleProvider();
        var rules = ruleProvider.GetRules(connection, RuleLevel.Information);
        var linter = new RelationalDatabaseLinter(rules);

        var snapshotDb = await database.SnapshotAsync(cancellationToken);
        var dbResults = await linter.AnalyseDatabase(snapshotDb, cancellationToken);

        ILintResultWriter writer = settings.Format switch
        {
            LintOutputFormat.Json => new JsonLintResultWriter(),
            LintOutputFormat.Sarif => new SarifLintResultWriter(),
            _ => new TextLintResultWriter(),
        };
        writer.Write(_console, dbResults);

        return ErrorCode.Success;
    }
}