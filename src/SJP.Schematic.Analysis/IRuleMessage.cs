namespace SJP.Schematic.Analysis
{
    public interface IRuleMessage
    {
        RuleLevel Level { get; }

        string Message { get; }

        string Title { get; }
    }
}