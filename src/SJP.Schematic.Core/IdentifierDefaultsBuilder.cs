using System;

namespace SJP.Schematic.Core
{
    /// <summary>
    /// A builder for constructing database identifier defaults.
    /// </summary>
    public sealed class IdentifierDefaultsBuilder
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="IdentifierDefaultsBuilder"/> class.
        /// </summary>
        public IdentifierDefaultsBuilder()
        {
            _defaults = new IdentifierDefaults(null, null, null);
        }

        private IdentifierDefaultsBuilder(IdentifierDefaults identifierDefaults)
        {
            _defaults = identifierDefaults ?? throw new ArgumentNullException(nameof(identifierDefaults));
        }

        /// <summary>
        /// Constructs a new builder that has a provided server name set in addition to any other existing defaults.
        /// </summary>
        /// <param name="server">A server name.</param>
        /// <returns>A new builder with the given server name set.</returns>
        public IdentifierDefaultsBuilder WithServer(string server)
        {
            return new IdentifierDefaultsBuilder(
                new IdentifierDefaults(server, _defaults.Database, _defaults.Schema)
            );
        }

        /// <summary>
        /// Constructs a new builder that has a provided database name set in addition to any other existing defaults.
        /// </summary>
        /// <param name="database">A database name.</param>
        /// <returns>A new builder with the given database name set.</returns>
        public IdentifierDefaultsBuilder WithDatabase(string database)
        {
            return new IdentifierDefaultsBuilder(
                new IdentifierDefaults(_defaults.Server, database, _defaults.Schema)
            );
        }

        /// <summary>
        /// Constructs a new builder that has a provided schema name set in addition to any other existing defaults.
        /// </summary>
        /// <param name="schema">A schema name.</param>
        /// <returns>A new builder with the given schema name set.</returns>
        public IdentifierDefaultsBuilder WithSchema(string schema)
        {
            return new IdentifierDefaultsBuilder(
                new IdentifierDefaults(_defaults.Server, _defaults.Database, schema)
            );
        }

        /// <summary>
        /// Constructs a resulting set of identifier defaults.
        /// </summary>
        /// <returns>Database identifier defaults.</returns>
        public IIdentifierDefaults Build() => _defaults;

        private readonly IdentifierDefaults _defaults;
    }
}
