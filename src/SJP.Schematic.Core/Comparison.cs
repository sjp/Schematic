using System.Threading.Tasks;

namespace SJP.Schema.Core
{
    public interface IDatabaseStructureChange
    {
        void Apply();

        Task ApplyAsync();
    }

    // enables us to provide some options on how we'd like to compare
    // for example do we want to drop missing columns?
    // do we want to rename when objects are the same structure but different name?
    // do we want to always drop triggers beforehand?
    // etc...
    public interface IDatabaseComparisonOptions<T> where T : IDatabaseEntity
    {

    }

    public interface IDatabaseComparable<T> where T : IDatabaseEntity
    {
        IDatabaseStructureChange CompareStructure(T comparison, IDatabaseComparisonOptions<T> options);

        Task<IDatabaseStructureChange> CompareStructureAsync(T comparison, IDatabaseComparisonOptions<T> options);
    }
}
