using System.Data;
using System.Threading;
using System.Threading.Tasks;

namespace SJP.Schematic.Core
{
    public interface IDbConnectionFactory
    {
        IDbConnection CreateConnection();

        IDbConnection OpenConnection();

        Task<IDbConnection> OpenConnectionAsync(CancellationToken cancellationToken = default);

        bool DisposeConnection { get; }
    }
}
