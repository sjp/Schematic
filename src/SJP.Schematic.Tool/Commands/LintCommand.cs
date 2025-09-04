using System;
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

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        var cancellationToken = CancellationToken.None;

        var dependencyProvider = _dependencyProviderFactory.GetDbDependencies(settings.ConfigFile!.FullName);
        var connection = dependencyProvider.GetSchematicConnection();
        var database = await connection.Dialect.GetRelationalDatabaseAsync(connection, cancellationToken).ConfigureAwait(false);

        var ruleProvider = new DefaultRuleProvider();
        var rules = ruleProvider.GetRules(connection, RuleLevel.Information);
        var linter = new RelationalDatabaseLinter(rules);

        var snapshotDb = await database.SnapshotAsync(cancellationToken);
        var dbResults = await linter.AnalyseDatabase(snapshotDb, cancellationToken).ToListAsync(cancellationToken).ConfigureAwait(false);
        var groupedResults = dbResults
            .GroupAsDictionary(static r => r.RuleId, StringComparer.Ordinal)
            .ToList();

        var hasDisplayedResults = false;

        foreach (var ruleGroup in groupedResults.Select(r => r.Value))
        {
            var ruleTitle = "Rule: " + ruleGroup[0].Title;
            var underline = new string('-', ruleTitle.Length);

            if (hasDisplayedResults)
            {
                _console.WriteLine();
                _console.WriteLine();
            }
            hasDisplayedResults = true;

            _console.WriteLine(underline);
            _console.WriteLine(ruleTitle);
            _console.WriteLine(underline);
            _console.WriteLine();

            foreach (var message in ruleGroup)
            {
                _console.WriteLine(" * " + message.Message);
            }
        }

        return ErrorCode.Success;
    }
}