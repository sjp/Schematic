using System.Threading;
using System.Threading.Tasks;
using SJP.Schematic.Core;

namespace SJP.Schematic.SqlServer
{
    /// <summary>
    /// Defines a database dialect containing functionality specific to SQL Server.
    /// </summary>
    /// <seealso cref="IDatabaseDialect" />
    public interface ISqlServerDialect : IDatabaseDialect
    {
        /// <summary>
        /// Gets the server properties available on SQL Server 2008.
        /// </summary>
        /// <param name="connection">A database connection.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Server properties available on SQL Server 2008.</returns>
        Task<IServerProperties2008?> GetServerProperties2008(IDbConnectionFactory connection, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the server properties available on SQL Server 2012.
        /// </summary>
        /// <param name="connection">A database connection.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Server properties available on SQL Server 2012.</returns>
        Task<IServerProperties2012?> GetServerProperties2012(IDbConnectionFactory connection, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the server properties available on SQL Server 2014.
        /// </summary>
        /// <param name="connection">A database connection.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Server properties available on SQL Server 2014.</returns>
        Task<IServerProperties2014?> GetServerProperties2014(IDbConnectionFactory connection, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the server properties available on SQL Server 2017.
        /// </summary>
        /// <param name="connection">A database connection.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Server properties available on SQL Server 2017.</returns>
        Task<IServerProperties2017?> GetServerProperties2017(IDbConnectionFactory connection, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the server properties available on SQL Server 2019.
        /// </summary>
        /// <param name="connection">A database connection.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Server properties available on SQL Server 2019.</returns>
        Task<IServerProperties2019?> GetServerProperties2019(IDbConnectionFactory connection, CancellationToken cancellationToken = default);
    }
}