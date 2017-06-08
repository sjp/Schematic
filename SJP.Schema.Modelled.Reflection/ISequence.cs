namespace SJP.Schema.Modelled.Reflection
{
    public interface ISequence
    {
        int Cache { get; }

        bool Cycle { get; }

        decimal Increment { get; }

        decimal? MaxValue { get; }

        decimal? MinValue { get; }

        decimal Start { get; }
    }
}
