namespace SJP.Schematic.Migrations
{
    public interface IMigrationOperation
    {
        bool IsDestructive { get; }
    }
}
