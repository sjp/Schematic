using Dapper;
using System;
using System.Data;
using SJP.Schematic.Core;
using SJP.Schematic.PostgreSql.Query;
using SJP.Schematic.Core.Utilities;
using System.Threading.Tasks;

namespace SJP.Schematic.PostgreSql
{
    public class PostgreSqlDatabaseSequence : IDatabaseSequence
    {
        public PostgreSqlDatabaseSequence(IDbConnection connection, Identifier sequenceName)
        {
            Connection = connection ?? throw new ArgumentNullException(nameof(connection));
            Name = sequenceName ?? throw new ArgumentNullException(nameof(sequenceName));

            _dataLoader = new AsyncLazy<SequenceData>(LoadSequenceDataAsync);
        }

        public Identifier Name { get; }

        protected IDbConnection Connection { get; }

        public int Cache => SequenceData.CacheSize;

        public bool Cycle => SequenceData.Cycle;

        public decimal Increment => SequenceData.Increment;

        public decimal? MaxValue => SequenceData.MaxValue;

        public decimal? MinValue => SequenceData.MinValue;

        public decimal Start => SequenceData.StartValue;

        protected SequenceData SequenceData => _dataLoader.Task.GetAwaiter().GetResult();

        protected virtual Task<SequenceData> LoadSequenceDataAsync()
        {
            // TODO: for PostgreSQL >= 10, there will be a p.cache_size available
            const string sql = @"
select
    p.start_value as StartValue,
    p.minimum_value as MinValue,
    p.maximum_value as MaxValue,
    p.increment as Increment,
    1 as CacheSize,
    p.cycle_option as Cycle
from pg_namespace nc, pg_class c, lateral pg_sequence_parameters(c.oid) p
where c.relnamespace = nc.oid
    and c.relkind = 'S'
    and nc.nspname = @SchemaName
    and c.relname = @SequenceName";

            return Connection.QuerySingleAsync<SequenceData>(sql, new { SchemaName = Name.Schema, SequenceName = Name.LocalName });
        }

        private readonly AsyncLazy<SequenceData> _dataLoader;
    }
}
