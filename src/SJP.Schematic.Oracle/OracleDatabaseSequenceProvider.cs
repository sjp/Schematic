using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using LanguageExt;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Oracle.Query;

namespace SJP.Schematic.Oracle
{
    public class OracleDatabaseSequenceProvider : IDatabaseSequenceProvider
    {
        public OracleDatabaseSequenceProvider(IDbConnection connection, IIdentifierDefaults identifierDefaults, IIdentifierResolutionStrategy identifierResolver)
        {
            Connection = connection ?? throw new ArgumentNullException(nameof(connection));
            IdentifierDefaults = identifierDefaults ?? throw new ArgumentNullException(nameof(identifierDefaults));
            IdentifierResolver = identifierResolver ?? throw new ArgumentNullException(nameof(identifierResolver));
        }

        protected IDbConnection Connection { get; }

        protected IIdentifierDefaults IdentifierDefaults { get; }

        protected IIdentifierResolutionStrategy IdentifierResolver { get; }

        public async Task<IReadOnlyCollection<IDatabaseSequence>> SequencesAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            var queryResult = await Connection.QueryAsync<QualifiedName>(SequencesQuery, cancellationToken).ConfigureAwait(false);
            var sequenceNames = queryResult
                .Select(dto => Identifier.CreateQualifiedIdentifier(dto.SchemaName, dto.ObjectName))
                .ToList();

            var sequences = await sequenceNames
                .Select(name => LoadSequenceAsync(name, cancellationToken))
                .Somes()
                .ConfigureAwait(false);

            return sequences.ToList();
        }

        protected virtual string SequencesQuery => SequencesQuerySql;

        private const string SequencesQuerySql = @"
select
    s.SEQUENCE_OWNER as SchemaName,
    s.SEQUENCE_NAME as ObjectName
from ALL_SEQUENCES s
inner join ALL_OBJECTS o on s.SEQUENCE_OWNER = o.OWNER and s.SEQUENCE_NAME = o.OBJECT_NAME
where o.ORACLE_MAINTAINED <> 'Y'
order by s.SEQUENCE_OWNER, s.SEQUENCE_NAME";

        public OptionAsync<IDatabaseSequence> GetSequenceAsync(Identifier sequenceName, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (sequenceName == null)
                throw new ArgumentNullException(nameof(sequenceName));

            var candidateSequenceName = QualifySequenceName(sequenceName);
            return LoadSequenceAsync(candidateSequenceName, cancellationToken);
        }

        public OptionAsync<Identifier> GetResolvedSequenceNameAsync(Identifier sequenceName, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (sequenceName == null)
                throw new ArgumentNullException(nameof(sequenceName));

            var resolvedNames = IdentifierResolver
                .GetResolutionOrder(sequenceName)
                .Select(QualifySequenceName);

            return resolvedNames
                .Select(name => GetResolvedSequenceNameStrictAsync(name, cancellationToken))
                .FirstSomeAsync(cancellationToken);
        }

        protected OptionAsync<Identifier> GetResolvedSequenceNameStrictAsync(Identifier sequenceName, CancellationToken cancellationToken)
        {
            if (sequenceName == null)
                throw new ArgumentNullException(nameof(sequenceName));

            var candidateSequenceName = QualifySequenceName(sequenceName);
            var qualifiedSequenceName = Connection.QueryFirstOrNoneAsync<QualifiedName>(
                SequenceNameQuery,
                new { SchemaName = candidateSequenceName.Schema, SequenceName = candidateSequenceName.LocalName },
                cancellationToken
            );

            return qualifiedSequenceName.Map(name => Identifier.CreateQualifiedIdentifier(candidateSequenceName.Server, candidateSequenceName.Database, name.SchemaName, name.ObjectName));
        }

        protected virtual string SequenceNameQuery => SequenceNameQuerySql;

        private const string SequenceNameQuerySql = @"
select s.SEQUENCE_OWNER as SchemaName, s.SEQUENCE_NAME as ObjectName
from ALL_SEQUENCES s
inner join ALL_OBJECTS o on s.SEQUENCE_OWNER = o.OWNER and s.SEQUENCE_NAME = o.OBJECT_NAME
where s.SEQUENCE_OWNER = :SchemaName and s.SEQUENCE_NAME = :SequenceName and o.ORACLE_MAINTAINED <> 'Y'";

        protected virtual string SequenceQuery => SequenceQuerySql;

        private const string SequenceQuerySql = @"
select
    INCREMENT_BY as ""Increment"",
    MIN_VALUE as ""MinValue"",
    MAX_VALUE as ""MaxValue"",
    CYCLE_FLAG as ""Cycle"",
    CACHE_SIZE as CacheSize
from ALL_SEQUENCES
where SEQUENCE_OWNER = :SchemaName and SEQUENCE_NAME = :SequenceName";

        protected virtual OptionAsync<IDatabaseSequence> LoadSequenceAsync(Identifier sequenceName, CancellationToken cancellationToken)
        {
            if (sequenceName == null)
                throw new ArgumentNullException(nameof(sequenceName));

            var candidateSequenceName = QualifySequenceName(sequenceName);
            return LoadSequenceAsyncCore(candidateSequenceName, cancellationToken).ToAsync();
        }

        private async Task<Option<IDatabaseSequence>> LoadSequenceAsyncCore(Identifier sequenceName, CancellationToken cancellationToken)
        {
            var resolvedSequenceNameOption = GetResolvedSequenceNameAsync(sequenceName);
            var resolvedSequenceNameOptionIsNone = await resolvedSequenceNameOption.IsNone.ConfigureAwait(false);
            if (resolvedSequenceNameOptionIsNone)
                return Option<IDatabaseSequence>.None;

            var resolvedSequenceName = await resolvedSequenceNameOption.UnwrapSomeAsync().ConfigureAwait(false);
            var sequence = LoadSequenceDataAsync(resolvedSequenceName, cancellationToken)
                .Map(seqData => BuildSequenceFromDto(resolvedSequenceName, seqData));

            return await sequence.ToOption().ConfigureAwait(false);
        }

        protected virtual OptionAsync<SequenceData> LoadSequenceDataAsync(Identifier sequenceName, CancellationToken cancellationToken)
        {
            if (sequenceName == null)
                throw new ArgumentNullException(nameof(sequenceName));

            return Connection.QueryFirstOrNoneAsync<SequenceData>(
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

        private static IDatabaseSequence BuildSequenceFromDto(Identifier sequenceName, SequenceData seqData)
        {
            if (sequenceName == null)
                throw new ArgumentNullException(nameof(sequenceName));
            if (seqData == null)
                throw new ArgumentNullException(nameof(seqData));

            var cycle = seqData.Cycle == "Y";
            var start = seqData.Increment >= 0
                ? seqData.MinValue
                : seqData.MaxValue;

            return new DatabaseSequence(
                sequenceName,
                start,
                seqData.Increment,
                Option<decimal>.Some(seqData.MinValue),
                Option<decimal>.Some(seqData.MaxValue),
                cycle,
                seqData.CacheSize
            );
        }
    }
}
