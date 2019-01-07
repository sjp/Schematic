using System.Threading;
using System.Threading.Tasks;
using SJP.Schematic.Core;

namespace SJP.Schematic.SqlServer
{
    public interface ISqlServerDialect : IDatabaseDialect
    {
        Task<IServerProperties2008> GetServerProperties2008(CancellationToken cancellationToken = default(CancellationToken));

        Task<IServerProperties2012> GetServerProperties2012(CancellationToken cancellationToken = default(CancellationToken));

        Task<IServerProperties2014> GetServerProperties2014(CancellationToken cancellationToken = default(CancellationToken));

        Task<IServerProperties2017> GetServerProperties2017(CancellationToken cancellationToken = default(CancellationToken));
    }
}