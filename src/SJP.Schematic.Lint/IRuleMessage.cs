namespace SJP.Schematic.Lint
{
    public interface IRuleMessage
    {
        RuleLevel Level { get; }

        string Message { get; }

        string Title { get; }
    }
}