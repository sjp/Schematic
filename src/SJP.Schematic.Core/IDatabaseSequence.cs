using LanguageExt;

namespace SJP.Schematic.Core
{
    public interface IDatabaseSequence : IDatabaseEntity
    {
        int Cache { get; }

        bool Cycle { get; }

        decimal Increment { get; }

        Option<decimal> MaxValue { get; }

        Option<decimal> MinValue { get; }

        decimal Start { get; }
    }
}
