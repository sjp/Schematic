using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LanguageExt;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.PostgreSql.Query;

namespace SJP.Schematic.PostgreSql.Versions.V9_4
{
    public class PostgreSqlDatabaseSequenceProvider : IDatabaseSequenceProvider
    {
        public PostgreSqlDatabaseSequenceProvider(IDbConnection connection, IIdentifierDefaults identifierDefaults, IIdentifierResolutionStrategy identifierResolver)
        {
            Connection = connection ?? throw new ArgumentNullException(nameof(connection));
            IdentifierDefaults = identifierDefaults ?? throw new ArgumentNullException(nameof(identifierDefaults));
            IdentifierResolver = identifierResolver ?? throw new ArgumentNullException(nameof(identifierResolver));
        }

        protected IDbConnection Connection { get; }

        protected IIdentifierDefaults IdentifierDefaults { get; }

        protected IIdentifierResolutionStrategy IdentifierResolver { get; }

        public async Task<IReadOnlyCollection<IDatabaseSequence>> GetAllSequences(CancellationToken cancellationToken = default(CancellationToken))
        {
            var queryResult = await Connection.QueryAsync<QualifiedName>(SequencesQuery, cancellationToken).ConfigureAwait(false);
            var sequenceNames = queryResult
                .Select(dto => Identifier.CreateQualifiedIdentifier(dto.SchemaName, dto.ObjectName))
                .ToList();

            var sequences = new List<IDatabaseSequence>();

            foreach (var sequenceName in sequenceNames)
            {
                var sequence = LoadSequence(sequenceName, cancellationToken);
                await sequence.IfSome(s => sequences.Add(s)).ConfigureAwait(false);
            }

            return sequences;
        }

        protected virtual string SequencesQuery => SequencesQuerySql;

        private const string SequencesQuerySql = @"
select
    sequence_schema as SchemaName,
    sequence_name as ObjectName
from information_schema.sequences
where sequence_schema not in ('pg_catalog', 'information_schema')";

        public OptionAsync<IDatabaseSequence> GetSequence(Identifier sequenceName, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (sequenceName == null)
                throw new ArgumentNullException(nameof(sequenceName));

            var candidateSequenceName = QualifySequenceName(sequenceName);
            return LoadSequence(candidateSequenceName, cancellationToken);
        }

        protected OptionAsync<Identifier> GetResolvedSequenceName(Identifier sequenceName, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (sequenceName == null)
                throw new ArgumentNullException(nameof(sequenceName));

            var resolvedNames = IdentifierResolver
                .GetResolutionOrder(sequenceName)
                .Select(QualifySequenceName);

            return resolvedNames
                .Select(name => GetResolvedSequenceNameStrict(name, cancellationToken))
                .FirstSome(cancellationToken);
        }

        protected OptionAsync<Identifier> GetResolvedSequenceNameStrict(Identifier sequenceName, CancellationToken cancellationToken)
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
select sequence_schema as SchemaName, sequence_name as ObjectName
from information_schema.sequences
where sequence_schema = @SchemaName and sequence_name = @SequenceName
    and sequence_schema not in ('pg_catalog', 'information_schema')
limit 1";

        protected virtual string SequenceQuery => SequenceQuerySql;

        private const string SequenceQuerySql = @"
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

        protected virtual OptionAsync<IDatabaseSequence> LoadSequence(Identifier sequenceName, CancellationToken cancellationToken)
        {
            if (sequenceName == null)
                throw new ArgumentNullException(nameof(sequenceName));

            var candidateSequenceName = QualifySequenceName(sequenceName);
            return LoadSequenceAsyncCore(candidateSequenceName, cancellationToken).ToAsync();
        }

        private async Task<Option<IDatabaseSequence>> LoadSequenceAsyncCore(Identifier sequenceName, CancellationToken cancellationToken)
        {
            var resolvedSequenceNameOption = GetResolvedSequenceName(sequenceName);
            var resolvedSequenceNameOptionIsNone = await resolvedSequenceNameOption.IsNone.ConfigureAwait(false);
            if (resolvedSequenceNameOptionIsNone)
                return Option<IDatabaseSequence>.None;

            var resolvedSequenceName = await resolvedSequenceNameOption.UnwrapSomeAsync().ConfigureAwait(false);
            var sequence = LoadSequenceData(resolvedSequenceName, cancellationToken)
                .Map(seqData => BuildSequenceFromDto(resolvedSequenceName, seqData));

            return await sequence.ToOption().ConfigureAwait(false);
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
                seqData.CacheSize
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
