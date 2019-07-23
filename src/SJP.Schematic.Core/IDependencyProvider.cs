using System.Collections.Generic;

namespace SJP.Schematic.Core
{
    /// <summary>
    /// Provides references to dependent objects within an expression
    /// </summary>
    public interface IDependencyProvider
    {
        /// <summary>
        /// Retrieves all dependencies for an expression.
        /// </summary>
        /// <param name="objectName">The name of an object defined by an expression (e.g. a computed column definition).</param>
        /// <param name="expression">A SQL expression that may contain dependent object names.</param>
        /// <returns>A collection of identifiers found in the expression.</returns>
        /// <remarks>This will also return unqualified identifiers, which may cause ambiguity between object names and column names. Additionally it may return other identifiers, such as aliases or type names.</remarks>
        IReadOnlyCollection<Identifier> GetDependencies(Identifier objectName, string expression);
    }
}
