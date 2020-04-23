using System.Threading;
using System.Threading.Tasks;
using SJP.Schematic.Core;

namespace SJP.Schematic.SqlServer
{
    public interface ISqlServerDialect : IDatabaseDialect
    {
        Task<IServerProperties2008?> GetServerProperties2008(IDbConnectionFactory connection, CancellationToken cancellationToken = default);

        Task<IServerProperties2012?> GetServerProperties2012(IDbConnectionFactory connection, CancellationToken cancellationToken = default);

        Task<IServerProperties2014?> GetServerProperties2014(IDbConnectionFactory connection, CancellationToken cancellationToken = default);

        Task<IServerProperties2017?> GetServerProperties2017(IDbConnectionFactory connection, CancellationToken cancellationToken = default);

        Task<IServerProperties2019?> GetServerProperties2019(IDbConnectionFactory connection, CancellationToken cancellationToken = default);
    }
}