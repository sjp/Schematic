using System.Data;
using System.Threading;
using System.Threading.Tasks;

namespace SJP.Schematic.Core
{
    public interface IDbConnectionFactory
    {
        IDbConnection CreateConnection(string connectionString);

        Task<IDbConnection> CreateConnectionAsync(string connectionString, CancellationToken cancellationToken = default);
    }
}
