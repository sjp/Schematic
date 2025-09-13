using EnumsNET;

namespace SJP.Schematic.SqlServer;

/// <summary>
/// A representation of the compatibility level assigned to a given SQL Server database.
/// The compatibility level affects runtime behaviour in addition to the availability of
/// SQL functionality.
/// </summary>
public sealed record CompatibilityLevel
{
    /// <summary>
    /// Initialises a compatibility level via an integer, as represented in the database.
    /// </summary>
    /// <param name="compatLevelValue">The compatibility level as stored in SQL Server, typically an integer incrementing by 10 for each major version. <see cref="SqlServerCompatibilityLevel"/>.</param>
    public CompatibilityLevel(int compatLevelValue)
    {
        Value = compatLevelValue;

        var compatLevel = (SqlServerCompatibilityLevel)compatLevelValue;
        SqlServerVersion = compatLevel.IsValid()
            ? compatLevel
            : SqlServerCompatibilityLevel.Unknown;
    }

    /// <summary>
    /// The compatibility level value as known to SQL Server.
    /// </summary>
    public int Value { get; }

    /// <summary>
    /// A named SQL Server version that is associated with a given compatibility level.
    /// </summary>
    public SqlServerCompatibilityLevel SqlServerVersion { get; }
}