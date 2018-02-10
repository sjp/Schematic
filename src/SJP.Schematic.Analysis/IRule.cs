using System.Collections.Generic;
using SJP.Schematic.Core;

namespace SJP.Schematic.Analysis
{
    public interface IRule
    {
        RuleLevel Level { get; }

        string Title { get; }

        IEnumerable<RuleMessage> AnalyseDatabase(IRelationalDatabase database);
    }
}
