using System;

namespace SJP.Schematic.Core
{
    /// <summary>
    /// Not intended to be used directly. Used to store common information used for <see cref="IRelationalDatabase"/> instances.
    /// </summary>
    public abstract class RelationalDatabase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RelationalDatabase"/> class.
        /// </summary>
        /// <param name="identifierDefaults">Database identifier defaults.</param>
        /// <exception cref="ArgumentNullException"><paramref name="identifierDefaults"/> is <c>null</c>.</exception>
        protected RelationalDatabase(IIdentifierDefaults identifierDefaults)
        {
            IdentifierDefaults = identifierDefaults ?? throw new ArgumentNullException(nameof(identifierDefaults));
        }

        /// <summary>
        /// Gets the database identifier defaults.
        /// </summary>
        /// <value>Database identifier defaults.</value>
        public IIdentifierDefaults IdentifierDefaults { get; }
    }
}
