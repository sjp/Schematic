using System;
using System.ComponentModel;
using System.Linq;
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

        [CommandOption("--level <LEVEL>")]
        [Description("The reporting level applied to all lint rules. One of: information, warning, error. Defaults to information.")]
        [DefaultValue(RuleLevel.Information)]
        public RuleLevel Level { get; init; }

        [CommandOption("--fail-on <LEVEL>")]
        [Description("The minimum rule level that causes the command to exit with a non-zero exit code. One of: information, warning, error. If not set, the command always exits successfully.")]
        public RuleLevel? FailOn { get; init; }
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
        var databaseProvider = dependencyProvider.GetRelationalDatabaseProvider(connection);
        var database = await databaseProvider.GetRelationalDatabaseAsync(cancellationToken);

        var ruleProvider = new DefaultRuleProvider();
        var rules = ruleProvider.GetRules(connection, settings.Level);
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

        if (settings.FailOn.HasValue && dbResults.Any(r => r.Level >= settings.FailOn.Value))
            return ErrorCode.Error;

        return ErrorCode.Success;
    }
}