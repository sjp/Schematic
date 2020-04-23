using System.CommandLine;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SJP.Schematic.Lint;

namespace SJP.Schematic.Tool.Handlers
{
    internal class LintCommandHandler : DatabaseCommandHandler
    {
        public LintCommandHandler(FileInfo filePath)
            : base(filePath)
        {
        }

        public async Task<int> HandleCommand(IConsole console, CancellationToken cancellationToken)
        {
            var connection = GetSchematicConnection();
            var database = await connection.Dialect.GetRelationalDatabaseAsync(connection, cancellationToken).ConfigureAwait(false);
            var ruleProvider = new DefaultRuleProvider();
            var rules = ruleProvider.GetRules(connection, RuleLevel.Information);

            var tables = await database.GetAllTables(cancellationToken).ToListAsync(cancellationToken).ConfigureAwait(false);
            var views = await database.GetAllViews(cancellationToken).ToListAsync(cancellationToken).ConfigureAwait(false);
            var sequences = await database.GetAllSequences(cancellationToken).ToListAsync(cancellationToken).ConfigureAwait(false);
            var synonyms = await database.GetAllSynonyms(cancellationToken).ToListAsync(cancellationToken).ConfigureAwait(false);
            var routines = await database.GetAllRoutines(cancellationToken).ToListAsync(cancellationToken).ConfigureAwait(false);

            var linter = new RelationalDatabaseLinter(rules);

            var tableResults = await linter.AnalyseTables(tables, cancellationToken).ToListAsync(cancellationToken).ConfigureAwait(false);
            var viewResults = await linter.AnalyseViews(views, cancellationToken).ToListAsync(cancellationToken).ConfigureAwait(false);
            var sequenceResults = await linter.AnalyseSequences(sequences, cancellationToken).ToListAsync(cancellationToken).ConfigureAwait(false);
            var synonymResults = await linter.AnalyseSynonyms(synonyms, cancellationToken).ToListAsync(cancellationToken).ConfigureAwait(false);
            var routineResults = await linter.AnalyseRoutines(routines, cancellationToken).ToListAsync(cancellationToken).ConfigureAwait(false);

            var groupedResults = tableResults
                .Append(viewResults)
                .Append(sequenceResults)
                .Append(synonymResults)
                .Append(routineResults)
                .GroupBy(r => r.RuleId)
                .ToList();

            var hasDisplayedResults = false;

            foreach (var group in groupedResults)
            {
                var ruleTitle = "Rule: " + group.First().Title;
                var underline = new string('-', ruleTitle.Length);

                if (hasDisplayedResults)
                {
                    console.Out.WriteLine();
                    console.Out.WriteLine();
                }
                hasDisplayedResults = true;

                console.Out.WriteLine(underline);
                console.Out.WriteLine(ruleTitle);
                console.Out.WriteLine(underline);
                console.Out.WriteLine();

                foreach (var message in group)
                {
                    console.Out.WriteLine(" * " + message.Message);
                }
            }

            return ErrorCode.Success;
        }
    }
}
