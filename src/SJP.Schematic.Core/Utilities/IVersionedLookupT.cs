using System;

namespace SJP.Schematic.Core.Utilities
{
    /// <summary>
    /// A lookup container that is intended to retrieve database implementations for a given version, avoiding the need to provide version ranges manually.
    /// </summary>
    /// <typeparam name="T">The type of value to retrieve from the lookup.</typeparam>
    public interface IVersionedLookup<out T>
    {
        /// <summary>
        /// Retrieves the value that is applicable for the given version.
        /// </summary>
        /// <param name="version">A version.</param>
        /// <returns>An object of type <typeparamref name="T"/>.</returns>
        T GetValue(Version version);
    }
}
