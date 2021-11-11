using SJP.Schematic.Core;
using SJP.Schematic.PostgreSql.Query;
using SJP.Schematic.PostgreSql.QueryResult;

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

        private const string SequencesQuerySql = @$"
select
    schemaname as ""{ nameof(GetAllSequenceDefinitionsQueryResult.SchemaName) }"",
    sequencename as ""{ nameof(GetAllSequenceDefinitionsQueryResult.SequenceName) }"",
    start_value as ""{ nameof(GetAllSequenceDefinitionsQueryResult.StartValue) }"",
    min_value as ""{ nameof(GetAllSequenceDefinitionsQueryResult.MinValue) }"",
    max_value as ""{ nameof(GetAllSequenceDefinitionsQueryResult.MaxValue) }"",
    increment_by as ""{ nameof(GetAllSequenceDefinitionsQueryResult.Increment) }"",
    cycle as ""{ nameof(GetAllSequenceDefinitionsQueryResult.Cycle) }"",
    cache_size as ""{ nameof(GetAllSequenceDefinitionsQueryResult.CacheSize) }""
from pg_catalog.pg_sequences
order by schemaname, sequencename";

        /// <summary>
        /// Gets a query that retrieves all relevant information on a sequence.
        /// </summary>
        /// <value>A SQL query.</value>
        protected override string SequenceQuery => SequenceQuerySql;

        private const string SequenceQuerySql = @$"
select
    start_value as ""{ nameof(GetSequenceDefinitionQueryResult.StartValue) }"",
    min_value as ""{ nameof(GetSequenceDefinitionQueryResult.MinValue) }"",
    max_value as ""{ nameof(GetSequenceDefinitionQueryResult.MaxValue) }"",
    increment_by as ""{ nameof(GetSequenceDefinitionQueryResult.Increment) }"",
    cycle as ""{ nameof(GetSequenceDefinitionQueryResult.Cycle) }"",
    cache_size as ""{ nameof(GetSequenceDefinitionQueryResult.CacheSize) }""
from pg_catalog.pg_sequences
where schemaname = @{ nameof(GetSequenceDefinitionQuery.SchemaName) } and sequencename = @{ nameof(GetSequenceDefinitionQuery.SequenceName) }";
    }
}
