namespace SJP.Schematic.Core;

/// <summary>
/// Defines a database synonym, which is an object that is an alias for another object.
/// </summary>
/// <seealso cref="IDatabaseEntity" />
public interface IDatabaseSynonym : IDatabaseEntity
{
    /// <summary>
    /// The target of the synonym, i.e. the name of the object being aliased.
    /// </summary>
    /// <value>The synonym target name.</value>
    /// <remarks>The target does not have to be an object present in the database.</remarks>
    Identifier Target { get; }
}