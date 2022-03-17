namespace SJP.Schematic.Core;

/// <summary>
/// Defines a database routine.
/// </summary>
/// <seealso cref="IDatabaseEntity" />
public interface IDatabaseRoutine : IDatabaseEntity
{
    /// <summary>
    /// The definition of the routine.
    /// </summary>
    /// <value>A textual routine definition.</value>
    string Definition { get; }
}
