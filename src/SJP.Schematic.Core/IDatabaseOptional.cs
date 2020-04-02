namespace SJP.Schematic.Core
{
    /// <summary>
    /// Defines a database object that can be disabled.
    /// </summary>
    public interface IDatabaseOptional
    {
        /// <summary>
        /// Indicates whether this instance is enabled.
        /// </summary>
        /// <value><c>true</c> if this object is enabled; otherwise, <c>false</c>.</value>
        bool IsEnabled { get; }
    }
}
