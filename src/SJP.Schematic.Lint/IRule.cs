using System.Collections.Generic;
using SJP.Schematic.Core;

namespace SJP.Schematic.Lint
{
    public interface IRule
    {
        RuleLevel Level { get; }

        string Title { get; }

        IEnumerable<IRuleMessage> AnalyseDatabase(IRelationalDatabase database);
    }
}
