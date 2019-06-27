namespace SJP.Schematic.Migrations
{
    public interface IMigrationError
    {
        string Description { get; }

        IMigrationOperation Operation { get; }
    }
}
