namespace SJP.Schematic.Core;

/// <summary>
/// Defines behaviour for top-level objects within a database.
/// </summary>
public interface IDatabaseEntity
{
    /// <summary>
    /// The name of the database object.
    /// </summary>
    Identifier Name { get; }
}