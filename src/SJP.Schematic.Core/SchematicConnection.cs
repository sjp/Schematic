using System;
using System.Data;

namespace SJP.Schematic.Core
{
    /// <summary>
    /// A representation of a database connection used within Schematic.
    /// </summary>
    /// <seealso cref="ISchematicConnection" />
    public class SchematicConnection : ISchematicConnection
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SchematicConnection"/> class.
        /// </summary>
        /// <param name="connection">A database connection.</param>
        /// <param name="dialect">The dialect used for <paramref name="connection"/>.</param>
        /// <exception cref="ArgumentNullException"><paramref name="connection"/> or <paramref name="dialect"/> is <c>null</c>.</exception>
        public SchematicConnection(IDbConnection connection, IDatabaseDialect dialect)
            : this(Guid.NewGuid(), connection, dialect)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SchematicConnection"/> class.
        /// </summary>
        /// <param name="connectionId">A connection identifier.</param>
        /// <param name="connection">A database connection.</param>
        /// <param name="dialect">The dialect used for <paramref name="connection"/>.</param>
        /// <exception cref="ArgumentException">An empty connection ID was provided.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="connection"/> or <paramref name="dialect"/> is <c>null</c>.</exception>
        public SchematicConnection(Guid connectionId, IDbConnection connection, IDatabaseDialect dialect)
        {
            if (connectionId == Guid.Empty)
                throw new ArgumentException("An empty connection ID was provided. Consider using Guid.NewGuid() instead.", nameof(connectionId));

            ConnectionId = connectionId;
            DbConnection = connection ?? throw new ArgumentNullException(nameof(connection));
            Dialect = dialect ?? throw new ArgumentNullException(nameof(dialect));

            ConnectionRegistry.RegisterConnection(connectionId, connection);
        }

        /// <inheritdoc />
        public Guid ConnectionId { get; }

        /// <inheritdoc />
        public IDbConnection DbConnection { get; }

        /// <inheritdoc />
        public IDatabaseDialect Dialect { get; }
    }
}
