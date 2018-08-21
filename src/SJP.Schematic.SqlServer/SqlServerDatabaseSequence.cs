using Dapper;
using System;
using System.Data;
using SJP.Schematic.Core;
using SJP.Schematic.SqlServer.Query;
using System.Threading.Tasks;
using SJP.Schematic.Core.Utilities;

namespace SJP.Schematic.SqlServer
{
    public class SqlServerDatabaseSequence : IDatabaseSequence
    {
        public SqlServerDatabaseSequence(IDbConnection connection, IRelationalDatabase database, Identifier sequenceName)
        {
            if (sequenceName == null)
                throw new ArgumentNullException(nameof(sequenceName));

            Database = database ?? throw new ArgumentNullException(nameof(database));
            Connection = connection ?? throw new ArgumentNullException(nameof(connection));

            var serverName = sequenceName.Server ?? database.ServerName;
            var databaseName = sequenceName.Database ?? database.DatabaseName;
            var schemaName = sequenceName.Schema ?? database.DefaultSchema;

            Name = Identifier.CreateQualifiedIdentifier(serverName, databaseName, schemaName, sequenceName.LocalName);

            _dataLoader = new AsyncLazy<SequenceData>(LoadSequenceDataAsync);
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

        protected SequenceData SequenceData => _dataLoader.Task.GetAwaiter().GetResult();

        protected virtual Task<SequenceData> LoadSequenceDataAsync()
        {
            return Connection.QuerySingleAsync<SequenceData>(@"
select start_value as StartValue, increment as Increment, minimum_value as MinValue, maximum_value as MaxValue, is_cycling as Cycle, is_cached as IsCached, cache_size as CacheSize
from sys.sequences
where schema_name(schema_id) = @SchemaName and name = @SequenceName
", new { SchemaName = Name.Schema, SequenceName = Name.LocalName });
        }

        private readonly AsyncLazy<SequenceData> _dataLoader;
    }
}
