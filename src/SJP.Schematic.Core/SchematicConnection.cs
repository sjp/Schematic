using System;

namespace SJP.Schematic.Core;

/// <summary>
/// A representation of a database connection used within Schematic.
/// </summary>
/// <seealso cref="ISchematicConnection" />
public sealed class SchematicConnection : ISchematicConnection
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SchematicConnection"/> class.
    /// </summary>
    /// <param name="connectionFactory">A database connection factory.</param>
    /// <param name="dialect">The dialect used for <paramref name="connectionFactory"/>.</param>
    /// <exception cref="ArgumentNullException"><paramref name="connectionFactory"/> or <paramref name="dialect"/> is <see langword="null" />.</exception>
    public SchematicConnection(IDbConnectionFactory connectionFactory, IDatabaseDialect dialect)
    {
        ConnectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
        Dialect = dialect ?? throw new ArgumentNullException(nameof(dialect));
    }

    /// <inheritdoc />
    public IDbConnectionFactory ConnectionFactory { get; }

    /// <inheritdoc />
    public IDatabaseDialect Dialect { get; }
}
