using System;
using System.Collections.Generic;
using System.Linq;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Lint;
using Spectre.Console;

namespace SJP.Schematic.Tool.Handlers;

internal sealed class TextLintResultWriter : ILintResultWriter
{
    public void Write(IAnsiConsole console, IReadOnlyCollection<IRuleMessage> results)
    {
        var groupedResults = results
            .GroupAsDictionary(static r => r.RuleId, StringComparer.Ordinal)
            .ToList();

        var hasDisplayedResults = false;

        foreach (var ruleGroup in groupedResults.Select(r => r.Value))
        {
            var ruleTitle = "Rule: " + ruleGroup[0].Title;
            var underline = new string('-', ruleTitle.Length);

            if (hasDisplayedResults)
            {
                console.WriteLine();
                console.WriteLine();
            }
            hasDisplayedResults = true;

            console.WriteLine(underline);
            console.WriteLine(ruleTitle);
            console.WriteLine(underline);
            console.WriteLine();

            foreach (var message in ruleGroup)
            {
                console.WriteLine(" * " + message.Message);
            }
        }
    }
}
