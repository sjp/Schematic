using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Core;

/// <summary>
/// Stores default values for <see cref="Identifier"/> instances.
/// </summary>
/// <seealso cref="IIdentifierDefaults" />
public class IdentifierDefaults : IIdentifierDefaults
{
    /// <summary>
    /// Initializes a new instance of the <see cref="IdentifierDefaults"/> class.
    /// </summary>
    /// <param name="server">A server name.</param>
    /// <param name="database">A database name.</param>
    /// <param name="schema">A schema name.</param>
    /// <remarks>Empty and whitespace-only values are stored as <see langword="null" />, i.e. as an absent default.</remarks>
    public IdentifierDefaults(string? server, string? database, string? schema)
    {
        Server = server.IsNullOrWhiteSpace() ? null : server;
        Database = database.IsNullOrWhiteSpace() ? null : database;
        Schema = schema.IsNullOrWhiteSpace() ? null : schema;
    }

    /// <summary>
    /// Defaults where no server, database or schema name is present.
    /// </summary>
    public static IdentifierDefaults Empty { get; } = new IdentifierDefaults(null, null, null);

    /// <summary>
    /// A server name.
    /// </summary>
    public string? Server { get; }

    /// <summary>
    /// A database name.
    /// </summary>
    public string? Database { get; }

    /// <summary>
    /// A schema name.
    /// </summary>
    public string? Schema { get; }
}
