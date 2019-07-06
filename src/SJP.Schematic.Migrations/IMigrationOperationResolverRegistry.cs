namespace SJP.Schematic.Migrations
{
    public interface IMigrationOperationResolverRegistry
    {
        void AddResolver<TOperation>(IMigrationOperationResolver<TOperation> resolver) where TOperation : IMigrationOperation;
        IMigrationOperationResolver<TOperation> GetResolver<TOperation>() where TOperation : IMigrationOperation;
    }
}