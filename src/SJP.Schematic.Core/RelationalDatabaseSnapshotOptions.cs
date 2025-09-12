namespace SJP.Schematic.Core;

/// <summary>
/// Defines which database entities will be populated when snapshotting a database.
/// </summary>
public sealed record RelationalDatabaseSnapshotOptions
{
    /// <summary>
    /// Indicates whether tables should be exported. The default is <see langword="true" />.
    /// </summary>
    public bool IncludeTables { get; init; } = true;

    /// <summary>
    /// Indicates whether views should be exported. The default is <see langword="true" />.
    /// </summary>
    public bool IncludeViews { get; init; } = true;

    /// <summary>
    /// Indicates whether sequences should be exported. The default is <see langword="true" />.
    /// </summary>
    public bool IncludeSequences { get; init; } = true;

    /// <summary>
    /// Indicates whether synonyms should be exported. The default is <see langword="true" />.
    /// </summary>
    public bool IncludeSynonyms { get; init; } = true;

    /// <summary>
    /// Indicates whether routines should be exported. The default is <see langword="true" />.
    /// </summary>
    public bool IncludeRoutines { get; init; } = true;

    /// <summary>
    /// Configures options so that no database entities will be exported.
    /// This is intended to be used to enable opt-in approaches, rather than the default of opt-out.
    /// </summary>
    public static RelationalDatabaseSnapshotOptions Empty => new RelationalDatabaseSnapshotOptions
    {
        IncludeTables = false,
        IncludeViews = false,
        IncludeSequences = false,
        IncludeRoutines = false,
        IncludeSynonyms = false,
    };
}