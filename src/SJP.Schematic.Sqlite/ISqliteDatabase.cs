using System.Threading.Tasks;
using SJP.Schematic.Core;

namespace SJP.Schematic.Sqlite
{
    public interface ISqliteDatabase : IRelationalDatabaseSync, IRelationalDatabaseAsync
    {
        void Vacuum();

        Task VacuumAsync();

        void Vacuum(string schemaName);

        Task VacuumAsync(string schemaName);

        void AttachDatabase(string schemaName, string fileName);

        Task AttachDatabaseAsync(string schemaName, string fileName);

        void DetachDatabase(string schemaName);

        Task DetachDatabaseAsync(string schemaName);
    }
}
