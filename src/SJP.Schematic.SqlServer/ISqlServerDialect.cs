using System.Data;
using System.Threading;
using System.Threading.Tasks;
using SJP.Schematic.Core;

namespace SJP.Schematic.SqlServer
{
    public interface ISqlServerDialect : IDatabaseDialect
    {
        Task<IServerProperties2008?> GetServerProperties2008(IDbConnection connection, CancellationToken cancellationToken = default);

        Task<IServerProperties2012?> GetServerProperties2012(IDbConnection connection, CancellationToken cancellationToken = default);

        Task<IServerProperties2014?> GetServerProperties2014(IDbConnection connection, CancellationToken cancellationToken = default);

        Task<IServerProperties2017?> GetServerProperties2017(IDbConnection connection, CancellationToken cancellationToken = default);
    }
}