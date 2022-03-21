namespace SJP.Schematic.Lint;

/// <summary>
/// Describes the levels of issues that can be detected during analysis.
/// </summary>
public enum RuleLevel
{
    /// <summary>
    /// Indicates minor or possible issue with a database object.
    /// </summary>
    Information,

    /// <summary>
    /// Indicates a likely issue with a database object.
    /// </summary>
    Warning,

    /// <summary>
    /// Indicates a more severe issue with a database object.
    /// </summary>
    Error
}