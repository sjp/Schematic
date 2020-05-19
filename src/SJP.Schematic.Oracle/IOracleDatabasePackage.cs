using LanguageExt;
using SJP.Schematic.Core;

namespace SJP.Schematic.Oracle
{
    /// <summary>
    /// Defines an Oracle package, which is a collection of functions and procedures.
    /// </summary>
    /// <seealso cref="IDatabaseRoutine" />
    public interface IOracleDatabasePackage : IDatabaseRoutine
    {
        /// <summary>
        /// Gets the specification of the package. Roughly equivalent to an interface or headers.
        /// </summary>
        /// <value>The specification.</value>
        string Specification { get; }

        /// <summary>
        /// The body of the package, i.e. the implementation.
        /// </summary>
        /// <value>The package body.</value>
        Option<string> Body { get; }
    }
}