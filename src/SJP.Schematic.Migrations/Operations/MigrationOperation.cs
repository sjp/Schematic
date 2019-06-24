namespace SJP.Schematic.Migrations.Operations
{
    public abstract class MigrationOperation : IMigrationOperation
    {
        public virtual bool IsDestructive { get; }
    }
}
