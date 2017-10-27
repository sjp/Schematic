using System.Collections.Generic;
using System.Threading.Tasks;

namespace SJP.Schematic.Core
{
    public interface IRelationalDatabaseTableAsync : IDatabaseEntity
    {
        Task<IDatabaseKey> PrimaryKeyAsync();

        Task<IReadOnlyDictionary<Identifier, IDatabaseTableColumn>> ColumnAsync();
        Task<IReadOnlyList<IDatabaseTableColumn>> ColumnsAsync();

        Task<IReadOnlyDictionary<Identifier, IDatabaseCheckConstraint>> CheckAsync();
        Task<IEnumerable<IDatabaseCheckConstraint>> ChecksAsync();

        Task<IReadOnlyDictionary<Identifier, IDatabaseTableIndex>> IndexAsync();
        Task<IEnumerable<IDatabaseTableIndex>> IndexesAsync();

        Task<IReadOnlyDictionary<Identifier, IDatabaseKey>> UniqueKeyAsync();
        Task<IEnumerable<IDatabaseKey>> UniqueKeysAsync();

        Task<IReadOnlyDictionary<Identifier, IDatabaseRelationalKey>> ParentKeyAsync();
        Task<IEnumerable<IDatabaseRelationalKey>> ParentKeysAsync();

        Task<IEnumerable<IDatabaseRelationalKey>> ChildKeysAsync();

        Task<IReadOnlyDictionary<Identifier, IDatabaseTrigger>> TriggerAsync();
        Task<IEnumerable<IDatabaseTrigger>> TriggersAsync();
    }
}
