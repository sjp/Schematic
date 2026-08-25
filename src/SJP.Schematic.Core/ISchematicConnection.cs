namespace SJP.Schematic.Core;

/// <summary>
/// Represents a database connection that is used within Schematic.
/// </summary>
public interface ISchematicConnection
{
    /// <summary>
    /// Gets a database connection factory.
    /// </summary>
    /// <value>A database connection factory.</value>
    IDbConnectionFactory ConnectionFactory { get; }

    /// <summary>
    /// Gets the database dialect.
    /// </summary>
    /// <value>A dialect.</value>
    IDatabaseDialect Dialect { get; }
}
