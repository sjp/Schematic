namespace SJP.Schematic.Core;

/// <summary>
/// Default values for identifiers in a database.
/// </summary>
public interface IIdentifierDefaults
{
    /// <summary>
    /// A server name.
    /// </summary>
    string? Server { get; }

    /// <summary>
    /// A database name.
    /// </summary>
    string? Database { get; }

    /// <summary>
    /// A schema name.
    /// </summary>
    string? Schema { get; }
}