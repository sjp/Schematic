using LanguageExt;

namespace SJP.Schematic.Migrations
{
    public interface IMigrationOperationAnalyzer<T> where T : IMigrationOperation
    {
        EitherAsync<IMigrationError, IMigrationOperation> GetDependentOperations(T operation);
    }
}
