using Dapper;
using System;
using System.Data;
using SJP.Schematic.Core;
using SJP.Schematic.Oracle.Query;
using System.Threading.Tasks;
using SJP.Schematic.Core.Utilities;

namespace SJP.Schematic.Oracle
{
    public class OracleDatabaseSequence : IDatabaseSequence
    {
        public OracleDatabaseSequence(IDbConnection connection, Identifier sequenceName)
        {
            Connection = connection ?? throw new ArgumentNullException(nameof(connection));
            Name = sequenceName ?? throw new ArgumentNullException(nameof(sequenceName));

            _dataLoader = new AsyncLazy<SequenceData>(LoadSequenceDataAsync);
        }

        public Identifier Name { get; }

        protected IDbConnection Connection { get; }

        public int Cache => SequenceData.CacheSize;

        public bool Cycle => SequenceData.Cycle == "Y";

        public decimal Increment => SequenceData.Increment;

        public decimal? MaxValue => SequenceData.MaxValue;

        public decimal? MinValue => SequenceData.MinValue;

        // inferred
        public decimal Start => SequenceData.Increment >= 0
            ? MinValue.GetValueOrDefault()
            : MaxValue.GetValueOrDefault();

        protected SequenceData SequenceData => _dataLoader.Task.GetAwaiter().GetResult();

        protected virtual Task<SequenceData> LoadSequenceDataAsync()
        {
            return Connection.QuerySingleAsync<SequenceData>(@"
select
    INCREMENT_BY as ""Increment"",
    MIN_VALUE as ""MinValue"",
    MAX_VALUE as ""MaxValue"",
    CYCLE_FLAG as ""Cycle"",
    CACHE_SIZE as CacheSize
from ALL_SEQUENCES
where SEQUENCE_OWNER = :SchemaName and SEQUENCE_NAME = :SequenceName
", new { SchemaName = Name.Schema, SequenceName = Name.LocalName });
        }

        private readonly AsyncLazy<SequenceData> _dataLoader;
    }
}
