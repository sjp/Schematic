namespace SJP.Schematic.Core;

/// <summary>
/// Defines a database object that can be disabled.
/// </summary>
public interface IDatabaseOptional
{
    /// <summary>
    /// Indicates whether this instance is enabled.
    /// </summary>
    /// <value><see langword="true" /> if this object is enabled; otherwise, <see langword="false" />.</value>
    bool IsEnabled { get; }
}