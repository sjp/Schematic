using System;

namespace SJP.Schematic.Core;

/// <summary>
/// A representation of a database connection used within Schematic.
/// </summary>
/// <seealso cref="ISchematicConnection" />
public class SchematicConnection : ISchematicConnection
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SchematicConnection"/> class.
    /// </summary>
    /// <param name="connectionFactory">A database connection factory.</param>
    /// <param name="dialect">The dialect used for <paramref name="connectionFactory"/>.</param>
    /// <exception cref="ArgumentNullException"><paramref name="connectionFactory"/> or <paramref name="dialect"/> is <see langword="null" />.</exception>
    public SchematicConnection(IDbConnectionFactory connectionFactory, IDatabaseDialect dialect)
        : this(Guid.NewGuid(), connectionFactory, dialect)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SchematicConnection"/> class.
    /// </summary>
    /// <param name="connectionId">A connection identifier.</param>
    /// <param name="connectionFactory">A database connection factory.</param>
    /// <param name="dialect">The dialect used for <paramref name="connectionFactory"/>.</param>
    /// <exception cref="ArgumentException">An empty connection ID was provided.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="connectionFactory"/> or <paramref name="dialect"/> is <see langword="null" />.</exception>
    public SchematicConnection(Guid connectionId, IDbConnectionFactory connectionFactory, IDatabaseDialect dialect)
    {
        if (connectionId == Guid.Empty)
            throw new ArgumentException("An empty connection ID was provided. Consider using Guid.NewGuid() instead.", nameof(connectionId));

        ConnectionId = connectionId;
        DbConnection = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
        Dialect = dialect ?? throw new ArgumentNullException(nameof(dialect));

        ConnectionRegistry.RegisterConnection(connectionId, connectionFactory);
    }

    /// <inheritdoc />
    public Guid ConnectionId { get; }

    /// <inheritdoc />
    public IDbConnectionFactory DbConnection { get; }

    /// <inheritdoc />
    public IDatabaseDialect Dialect { get; }
}