using Dapper;
using System;
using System.Data;
using SJP.Schematic.Core;
using SJP.Schematic.SqlServer.Query;

namespace SJP.Schematic.SqlServer
{
    public class SqlServerDatabaseSequence : IDatabaseSequence
    {
        public SqlServerDatabaseSequence(IRelationalDatabase database, IDbConnection connection, Identifier sequenceName)
        {
            if (sequenceName == null || sequenceName.LocalName == null)
                throw new ArgumentNullException(nameof(sequenceName));

            Database = database ?? throw new ArgumentNullException(nameof(database));
            Connection = connection ?? throw new ArgumentNullException(nameof(connection));

            if (sequenceName.Schema == null && database.DefaultSchema != null)
                sequenceName = new Identifier(database.DefaultSchema, sequenceName.LocalName);
            Name = sequenceName.LocalName;

            _dataLoader = new Lazy<SequenceData>(LoadSequenceData);
        }

        public IRelationalDatabase Database { get; }

        public Identifier Name { get; }

        protected IDbConnection Connection { get; }

        public int Cache => SequenceData.IsCached
            ? SequenceData.CacheSize ?? -1 // -1 as unknown/database determined
            : 0;

        public bool Cycle => SequenceData.Cycle;

        public decimal Increment => SequenceData.Increment;

        public decimal? MaxValue => SequenceData.MaxValue;

        public decimal? MinValue => SequenceData.MinValue;

        public decimal Start => SequenceData.StartValue;

        protected SequenceData SequenceData => _dataLoader.Value;

        protected virtual SequenceData LoadSequenceData()
        {
            return Connection.QuerySingle<SequenceData>(@"
select start_value as StartValue, increment as Increment, minimum_value as MinValue, maximum_value as MaxValue, is_cycling as Cycle, is_cached as IsCached, cache_size as CacheSize
from sys.sequences
where schema_name(schema_id) = @SchemaName and name = @SequenceName
", new { SchemaName = Name.Schema, SequenceName = Name.LocalName });
        }

        private readonly Lazy<SequenceData> _dataLoader;
    }
}
