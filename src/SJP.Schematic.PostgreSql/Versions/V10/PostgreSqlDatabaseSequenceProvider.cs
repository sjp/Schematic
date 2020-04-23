using SJP.Schematic.Core;

namespace SJP.Schematic.PostgreSql.Versions.V10
{
    public class PostgreSqlDatabaseSequenceProvider : PostgreSqlDatabaseSequenceProviderBase
    {
        public PostgreSqlDatabaseSequenceProvider(IDbConnectionFactory connection, IIdentifierDefaults identifierDefaults, IIdentifierResolutionStrategy identifierResolver)
            : base(connection, identifierDefaults, identifierResolver)
        {
        }

        protected override string SequencesQuery => SequencesQuerySql;

        private const string SequencesQuerySql = @"
select
    schemaname as SchemaName,
    sequencename as SequenceName,
    start_value as StartValue,
    min_value as MinValue,
    max_value as MaxValue,
    increment_by as Increment,
    cycle as Cycle,
    cache_size as CacheSize
from pg_catalog.pg_sequences
order by schemaname, sequencename";

        protected override string SequenceQuery => SequenceQuerySql;

        private const string SequenceQuerySql = @"
select
    start_value as StartValue,
    min_value as MinValue,
    max_value as MaxValue,
    increment_by as Increment,
    cycle as Cycle,
    cache_size as CacheSize
from pg_catalog.pg_sequences
where schemaname = @SchemaName and sequencename = @SequenceName";
    }
}
