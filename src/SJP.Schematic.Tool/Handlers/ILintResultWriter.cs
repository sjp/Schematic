using System.Collections.Generic;
using SJP.Schematic.Lint;
using Spectre.Console;

namespace SJP.Schematic.Tool.Handlers;

internal interface ILintResultWriter
{
    void Write(IAnsiConsole console, IReadOnlyCollection<IRuleMessage> results);
}
