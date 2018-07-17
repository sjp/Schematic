using System.Threading;
using System.Threading.Tasks;
using SJP.Schematic.Core;

namespace SJP.Schematic.Sqlite
{
    public interface ISqliteDatabase : IRelationalDatabase
    {
        void Vacuum();

        Task VacuumAsync(CancellationToken cancellationToken = default(CancellationToken));

        void Vacuum(string schemaName);

        Task VacuumAsync(string schemaName, CancellationToken cancellationToken = default(CancellationToken));

        void AttachDatabase(string schemaName, string fileName);

        Task AttachDatabaseAsync(string schemaName, string fileName, CancellationToken cancellationToken = default(CancellationToken));

        void DetachDatabase(string schemaName);

        Task DetachDatabaseAsync(string schemaName, CancellationToken cancellationToken = default(CancellationToken));
    }
}
