namespace SJP.Schematic.Core
{
    public interface IDatabaseSynonym : IDatabaseEntity
    {
        Identifier Target { get; }
    }
}
