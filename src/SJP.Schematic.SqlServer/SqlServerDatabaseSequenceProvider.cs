using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LanguageExt;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.SqlServer.Query;

namespace SJP.Schematic.SqlServer
{
    public class SqlServerDatabaseSequenceProvider : IDatabaseSequenceProvider
    {
        public SqlServerDatabaseSequenceProvider(IDbConnection connection, IIdentifierDefaults identifierDefaults)
        {
            Connection = connection ?? throw new ArgumentNullException(nameof(connection));
            IdentifierDefaults = identifierDefaults ?? throw new ArgumentNullException(nameof(identifierDefaults));
        }

        protected IDbConnection Connection { get; }

        protected IIdentifierDefaults IdentifierDefaults { get; }

        public async Task<IReadOnlyCollection<IDatabaseSequence>> GetAllSequences(CancellationToken cancellationToken = default)
        {
            var queryResult = await Connection.QueryAsync<SequenceData>(SequencesQuery, cancellationToken).ConfigureAwait(false);
            if (queryResult.Empty())
                return Array.Empty<IDatabaseSequence>();

            return queryResult
                .Select(row =>
                {
                    var sequenceName = QualifySequenceName(Identifier.CreateQualifiedIdentifier(row.SchemaName, row.ObjectName));
                    return BuildSequenceFromDto(sequenceName, row);
                })
                .ToList();
        }

        protected virtual string SequencesQuery => SequencesQuerySql;

        private const string SequencesQuerySql = @"
select
    schema_name(schema_id) as SchemaName,
    name as ObjectName,
    start_value as StartValue,
    increment as Increment,
    minimum_value as MinValue,
    maximum_value as MaxValue,
    is_cycling as Cycle,
    is_cached as IsCached,
    cache_size as CacheSize
from sys.sequences
where is_ms_shipped = 0
order by schema_name(schema_id), name";

        public OptionAsync<IDatabaseSequence> GetSequence(Identifier sequenceName, CancellationToken cancellationToken = default)
        {
            if (sequenceName == null)
                throw new ArgumentNullException(nameof(sequenceName));

            var candidateSequenceName = QualifySequenceName(sequenceName);
            return LoadSequence(candidateSequenceName, cancellationToken);
        }

        protected OptionAsync<Identifier> GetResolvedSequenceName(Identifier sequenceName, CancellationToken cancellationToken)
        {
            if (sequenceName == null)
                throw new ArgumentNullException(nameof(sequenceName));

            var candidateSequenceName = QualifySequenceName(sequenceName);
            var qualifiedSequenceName = Connection.QueryFirstOrNone<QualifiedName>(
                SequenceNameQuery,
                new { SchemaName = candidateSequenceName.Schema, SequenceName = candidateSequenceName.LocalName },
                cancellationToken
            );

            return qualifiedSequenceName.Map(name => Identifier.CreateQualifiedIdentifier(candidateSequenceName.Server, candidateSequenceName.Database, name.SchemaName, name.ObjectName));
        }

        protected virtual string SequenceNameQuery => SequenceNameQuerySql;

        private const string SequenceNameQuerySql = @"
select top 1 schema_name(schema_id) as SchemaName, name as ObjectName
from sys.sequences
where schema_id = schema_id(@SchemaName) and name = @SequenceName and is_ms_shipped = 0";

        private static IDatabaseSequence BuildSequenceFromDto(Identifier sequenceName, SequenceData seqData)
        {
            if (sequenceName == null)
                throw new ArgumentNullException(nameof(sequenceName));
            if (seqData == null)
                throw new ArgumentNullException(nameof(seqData));

            return new DatabaseSequence(
                sequenceName,
                seqData.StartValue,
                seqData.Increment,
                Option<decimal>.Some(seqData.MinValue),
                Option<decimal>.Some(seqData.MaxValue),
                seqData.Cycle,
                seqData.IsCached ? seqData.CacheSize ?? -1 : 0 // -1 as unknown/database determined
            );
        }

        protected virtual string SequenceQuery => SequenceQuerySql;

        private const string SequenceQuerySql = @"
select
    start_value as StartValue,
    increment as Increment,
    minimum_value as MinValue,
    maximum_value as MaxValue,
    is_cycling as Cycle,
    is_cached as IsCached,
    cache_size as CacheSize
from sys.sequences
where schema_name(schema_id) = @SchemaName and name = @SequenceName and is_ms_shipped = 0";

        protected virtual OptionAsync<IDatabaseSequence> LoadSequence(Identifier sequenceName, CancellationToken cancellationToken)
        {
            if (sequenceName == null)
                throw new ArgumentNullException(nameof(sequenceName));

            var candidateSequenceName = QualifySequenceName(sequenceName);
            return GetResolvedSequenceName(candidateSequenceName, cancellationToken)
                .Bind(name => LoadSequenceData(name, cancellationToken)
                    .Map(seq => BuildSequenceFromDto(name, seq)));
        }

        private OptionAsync<SequenceData> LoadSequenceData(Identifier sequenceName, CancellationToken cancellationToken)
        {
            if (sequenceName == null)
                throw new ArgumentNullException(nameof(sequenceName));

            return Connection.QueryFirstOrNone<SequenceData>(
                SequenceQuery,
                new { SchemaName = sequenceName.Schema, SequenceName = sequenceName.LocalName },
                cancellationToken
            );
        }

        protected Identifier QualifySequenceName(Identifier sequenceName)
        {
            if (sequenceName == null)
                throw new ArgumentNullException(nameof(sequenceName));

            var schema = sequenceName.Schema ?? IdentifierDefaults.Schema;
            return Identifier.CreateQualifiedIdentifier(IdentifierDefaults.Server, IdentifierDefaults.Database, schema, sequenceName.LocalName);
        }
    }
}
