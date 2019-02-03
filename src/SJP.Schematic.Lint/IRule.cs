namespace SJP.Schematic.Lint
{
    public interface IRule
    {
        RuleLevel Level { get; }

        string Title { get; }
    }
}
