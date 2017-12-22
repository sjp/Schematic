using Dapper;
using System;
using System.Data;
using SJP.Schematic.Core;
using SJP.Schematic.PostgreSql.Query;

namespace SJP.Schematic.PostgreSql
{
    public class PostgreSqlDatabaseSequence : IDatabaseSequence
    {
        public PostgreSqlDatabaseSequence(IDbConnection connection, IRelationalDatabase database, Identifier sequenceName)
        {
            if (sequenceName == null || sequenceName.LocalName == null)
                throw new ArgumentNullException(nameof(sequenceName));

            Database = database ?? throw new ArgumentNullException(nameof(database));
            Connection = connection ?? throw new ArgumentNullException(nameof(connection));

            var serverName = sequenceName.Server ?? database.ServerName;
            var databaseName = sequenceName.Database ?? database.DatabaseName;
            var schemaName = sequenceName.Schema ?? database.DefaultSchema;

            Name = new Identifier(serverName, databaseName, schemaName, sequenceName.LocalName);

            _dataLoader = new Lazy<SequenceData>(LoadSequenceData);
        }

        public IRelationalDatabase Database { get; }

        public Identifier Name { get; }

        protected IDbConnection Connection { get; }

        public int Cache => SequenceData.CacheSize;

        public bool Cycle => SequenceData.Cycle;

        public decimal Increment => SequenceData.Increment;

        public decimal? MaxValue => SequenceData.MaxValue;

        public decimal? MinValue => SequenceData.MinValue;

        public decimal Start => SequenceData.StartValue;

        protected SequenceData SequenceData => _dataLoader.Value;

        protected virtual SequenceData LoadSequenceData()
        {
            return Connection.QuerySingle<SequenceData>(@"
select
    s.start_value as StartValue,
    s.increment_by as Increment,
    s.min_value as MinValue,
    s.max_value as MaxValue,
    s.cycle as Cycle,
    s.cache_size as CacheSize
from pg_catalog.pg_sequences s
inner join pg_catalog.pg_namespace ns on s.schemaname = ns.nspname
where ns.nspname = @SchemaName and s.sequencename = @SequenceName
", new { SchemaName = Name.Schema, SequenceName = Name.LocalName });
        }

        private readonly Lazy<SequenceData> _dataLoader;
    }
}
