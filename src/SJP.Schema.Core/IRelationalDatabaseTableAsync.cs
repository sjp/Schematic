using System.Collections.Generic;
using System.Threading.Tasks;

namespace SJP.Schema.Core
{
    // async analogues of every synchronous property
    public interface IRelationalDatabaseTableAsync : IDatabaseEntity
    {
        Task<IDatabaseKey> PrimaryKeyAsync();

        Task<IReadOnlyDictionary<string, IDatabaseTableColumn>> ColumnAsync();
        Task<IList<IDatabaseTableColumn>> ColumnsAsync();

        Task<IReadOnlyDictionary<string, IDatabaseCheckConstraint>> CheckConstraintAsync();
        Task<IEnumerable<IDatabaseCheckConstraint>> CheckConstraintsAsync();

        Task<IReadOnlyDictionary<string, IDatabaseTableIndex>> IndexAsync();
        Task<IEnumerable<IDatabaseTableIndex>> IndexesAsync();

        Task<IReadOnlyDictionary<string, IDatabaseKey>> UniqueKeyAsync();
        Task<IEnumerable<IDatabaseKey>> UniqueKeysAsync();

        Task<IReadOnlyDictionary<string, IDatabaseRelationalKey>> ParentKeyAsync();
        Task<IEnumerable<IDatabaseRelationalKey>> ParentKeysAsync();

        Task<IEnumerable<IDatabaseRelationalKey>> ChildKeysAsync();

        // TRIGGER ON TABLE or DATABASE OR BOTH?
        Task<IReadOnlyDictionary<string, IDatabaseTrigger>> TriggerAsync();
        Task<IEnumerable<IDatabaseTrigger>> TriggersAsync();
    }
}
