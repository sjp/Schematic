namespace SJP.Schematic.Core
{
    public interface IDatabaseSequence : IDatabaseEntity
    {
        int Cache { get; }

        bool Cycle { get; }

        decimal Increment { get; }

        decimal? MaxValue { get; }

        decimal? MinValue { get; }

        decimal Start { get; }
    }
}
