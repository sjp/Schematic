using SJP.Schematic.Core;
using SJP.Schematic.PostgreSql.Query;

namespace SJP.Schematic.PostgreSql.Versions.V10
{
    /// <summary>
    /// A database sequence provider for PostgreSQL v10 (and higher) databases.
    /// </summary>
    /// <seealso cref="IDatabaseSequenceProvider" />
    public class PostgreSqlDatabaseSequenceProvider : PostgreSqlDatabaseSequenceProviderBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PostgreSqlDatabaseSequenceProvider"/> class.
        /// </summary>
        /// <param name="connection">A database connection factory.</param>
        /// <param name="identifierDefaults">Database identifier defaults.</param>
        /// <param name="identifierResolver">An identifier resolver.</param>
        public PostgreSqlDatabaseSequenceProvider(IDbConnectionFactory connection, IIdentifierDefaults identifierDefaults, IIdentifierResolutionStrategy identifierResolver)
            : base(connection, identifierDefaults, identifierResolver)
        {
        }

        /// <summary>
        /// Gets a query that retrieves information on all sequences in the database.
        /// </summary>
        /// <value>A SQL query.</value>
        protected override string SequencesQuery => SequencesQuerySql;

        private static readonly string SequencesQuerySql = @$"
select
    schemaname as ""{ nameof(SequenceData.SchemaName) }"",
    sequencename as ""{ nameof(SequenceData.SequenceName) }"",
    start_value as ""{ nameof(SequenceData.StartValue) }"",
    min_value as ""{ nameof(SequenceData.MinValue) }"",
    max_value as ""{ nameof(SequenceData.MaxValue) }"",
    increment_by as ""{ nameof(SequenceData.Increment) }"",
    cycle as ""{ nameof(SequenceData.Cycle) }"",
    cache_size as ""{ nameof(SequenceData.CacheSize) }""
from pg_catalog.pg_sequences
order by schemaname, sequencename";

        /// <summary>
        /// Gets a query that retrieves all relevant information on a sequence.
        /// </summary>
        /// <value>A SQL query.</value>
        protected override string SequenceQuery => SequenceQuerySql;

        private static readonly string SequenceQuerySql = @$"
select
    start_value as ""{ nameof(SequenceData.StartValue) }"",
    min_value as ""{ nameof(SequenceData.MinValue) }"",
    max_value as ""{ nameof(SequenceData.MaxValue) }"",
    increment_by as ""{ nameof(SequenceData.Increment) }"",
    cycle as ""{ nameof(SequenceData.Cycle) }"",
    cache_size as ""{ nameof(SequenceData.CacheSize) }""
from pg_catalog.pg_sequences
where schemaname = @SchemaName and sequencename = @SequenceName";
    }
}
