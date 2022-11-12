#pragma warning disable IDE1006, S101 // Naming Styles
namespace SJP.Schematic.Sqlite.Pragma.Query;

/// <summary>
/// Stores information on collating sequences defined for the current database connection.
/// </summary>
public sealed record pragma_collation_list
{
    /// <summary>
    /// A sequence number assigned to each index for internal tracking purposes.
    /// </summary>
    public required int seq { get; init; }

    /// <summary>
    /// The name of the collating function. Built-in collating functions are <c>BINARY</c>, <c>NOCASE</c>, and <c>RTRIM</c>.
    /// </summary>
    public required string name { get; init; }
}
#pragma warning restore IDE1006, S101 // Naming Styles