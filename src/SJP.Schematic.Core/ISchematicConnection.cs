using System;

namespace SJP.Schematic.Core;

/// <summary>
/// Represents a database connection that is used within Schematic.
/// </summary>
public interface ISchematicConnection
{
    /// <summary>
    /// An arbitrary identifier. Intended to be used to identify a connection when multiple are available.
    /// </summary>
    /// <value>A connection identifier.</value>
    Guid ConnectionId { get; }

    /// <summary>
    /// Gets a database connection factory.
    /// </summary>
    /// <value>A database connection factory.</value>
    IDbConnectionFactory DbConnection { get; }

    /// <summary>
    /// Gets the database dialect.
    /// </summary>
    /// <value>A dialect.</value>
    IDatabaseDialect Dialect { get; }
}