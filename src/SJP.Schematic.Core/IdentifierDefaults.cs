namespace SJP.Schematic.Core
{
    /// <summary>
    /// Stores default values for <see cref="Identifier"/> instances.
    /// </summary>
    /// <seealso cref="IIdentifierDefaults" />
    public class IdentifierDefaults : IIdentifierDefaults
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="IdentifierDefaults"/> class.
        /// </summary>
        /// <param name="server">A server name.</param>
        /// <param name="database">A database name.</param>
        /// <param name="schema">A schema name.</param>
        public IdentifierDefaults(string? server, string? database, string? schema)
        {
            Server = server;
            Database = database;
            Schema = schema;
        }

        /// <summary>
        /// A server name.
        /// </summary>
        public string? Server { get; }

        /// <summary>
        /// A database name.
        /// </summary>
        public string? Database { get; }

        /// <summary>
        /// A schema name.
        /// </summary>
        public string? Schema { get; }
    }
}
