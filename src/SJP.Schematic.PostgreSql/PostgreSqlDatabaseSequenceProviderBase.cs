using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using LanguageExt;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.PostgreSql.Query;
using SJP.Schematic.PostgreSql.QueryResult;

namespace SJP.Schematic.PostgreSql
{
    /// <summary>
    /// A database sequence provider for PostgreSQL databases.
    /// </summary>
    /// <seealso cref="IDatabaseSequenceProvider" />
    public class PostgreSqlDatabaseSequenceProviderBase : IDatabaseSequenceProvider
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PostgreSqlDatabaseSequenceProviderBase"/> class.
        /// </summary>
        /// <param name="connection">A database connection factory.</param>
        /// <param name="identifierDefaults">Database identifier defaults.</param>
        /// <param name="identifierResolver">An identifier resolver.</param>
        /// <exception cref="ArgumentNullException"><paramref name="connection"/> or <paramref name="identifierDefaults"/> or <paramref name="identifierResolver"/> are <c>null</c>.</exception>
        public PostgreSqlDatabaseSequenceProviderBase(IDbConnectionFactory connection, IIdentifierDefaults identifierDefaults, IIdentifierResolutionStrategy identifierResolver)
        {
            Connection = connection ?? throw new ArgumentNullException(nameof(connection));
            IdentifierDefaults = identifierDefaults ?? throw new ArgumentNullException(nameof(identifierDefaults));
            IdentifierResolver = identifierResolver ?? throw new ArgumentNullException(nameof(identifierResolver));
        }

        /// <summary>
        /// A database connection factory.
        /// </summary>
        /// <value>A database connection factory.</value>
        protected IDbConnectionFactory Connection { get; }

        /// <summary>
        /// Identifier defaults for the associated database.
        /// </summary>
        /// <value>Identifier defaults.</value>
        protected IIdentifierDefaults IdentifierDefaults { get; }

        /// <summary>
        /// Gets an identifier resolver that enables more relaxed matching against database object names.
        /// </summary>
        /// <value>An identifier resolver.</value>
        protected IIdentifierResolutionStrategy IdentifierResolver { get; }

        /// <summary>
        /// Gets all database sequences.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>A collection of database sequences.</returns>
        public async IAsyncEnumerable<IDatabaseSequence> GetAllSequences([EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var queryResult = await Connection.QueryAsync<GetAllSequenceDefinitionsQueryResult>(SequencesQuery, cancellationToken).ConfigureAwait(false);
            var sequences = queryResult
                .Select(row =>
                {
                    var sequenceName = QualifySequenceName(Identifier.CreateQualifiedIdentifier(row.SchemaName, row.SequenceName));
                    return new DatabaseSequence(
                        sequenceName,
                        row.StartValue,
                        row.Increment,
                        Option<decimal>.Some(row.MinValue),
                        Option<decimal>.Some(row.MaxValue),
                        row.Cycle,
                        row.CacheSize
                    );
                });

            foreach (var sequence in sequences)
                yield return sequence;
        }

        /// <summary>
        /// Gets a query that retrieves information on all sequences in the database.
        /// </summary>
        /// <value>A SQL query.</value>
        protected virtual string SequencesQuery => SequencesQuerySql;

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
        /// Gets a database sequence.
        /// </summary>
        /// <param name="sequenceName">A database sequence name.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A database sequence in the 'some' state if found; otherwise 'none'.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="sequenceName"/> is <c>null</c>.</exception>
        public OptionAsync<IDatabaseSequence> GetSequence(Identifier sequenceName, CancellationToken cancellationToken = default)
        {
            if (sequenceName == null)
                throw new ArgumentNullException(nameof(sequenceName));

            var candidateSequenceName = QualifySequenceName(sequenceName);
            return LoadSequence(candidateSequenceName, cancellationToken);
        }

        /// <summary>
        /// Gets the resolved name of the sequence. This enables non-strict name matching to be applied.
        /// </summary>
        /// <param name="sequenceName">A sequence name.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A sequence name that, if available, can be assumed to exist and applied strictly.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="sequenceName"/> is <c>null</c>.</exception>
        protected OptionAsync<Identifier> GetResolvedSequenceName(Identifier sequenceName, CancellationToken cancellationToken = default)
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

        /// <summary>
        /// Gets the resolved name of the sequence without name resolution. i.e. the name must match strictly to return a result.
        /// </summary>
        /// <param name="sequenceName">A sequence name that will be resolved.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A sequence name that, if available, can be assumed to exist and applied strictly.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="sequenceName"/> is <c>null</c>.</exception>
        protected OptionAsync<Identifier> GetResolvedSequenceNameStrict(Identifier sequenceName, CancellationToken cancellationToken)
        {
            if (sequenceName == null)
                throw new ArgumentNullException(nameof(sequenceName));

            var candidateSequenceName = QualifySequenceName(sequenceName);
            var qualifiedSequenceName = Connection.QueryFirstOrNone<GetSequenceNameQueryResult>(
                SequenceNameQuery,
                new GetSequenceNameQuery { SchemaName = candidateSequenceName.Schema!, SequenceName = candidateSequenceName.LocalName },
                cancellationToken
            );

            return qualifiedSequenceName.Map(name => Identifier.CreateQualifiedIdentifier(candidateSequenceName.Server, candidateSequenceName.Database, name.SchemaName, name.SequenceName));
        }

        /// <summary>
        /// Gets a query that resolves the name of a sequence.
        /// </summary>
        /// <value>A SQL query.</value>
        protected virtual string SequenceNameQuery => SequenceNameQuerySql;

        private const string SequenceNameQuerySql = @$"
select sequence_schema as ""{ nameof(GetSequenceNameQueryResult.SchemaName) }"", sequence_name as ""{ nameof(GetSequenceNameQueryResult.SequenceName) }""
from information_schema.sequences
where sequence_schema = @{ nameof(GetSequenceNameQuery.SchemaName) } and sequence_name = @{ nameof(GetSequenceNameQuery.SequenceName) }
    and sequence_schema not in ('pg_catalog', 'information_schema')
limit 1";

        /// <summary>
        /// Gets a query that retrieves all relevant information on a sequence.
        /// </summary>
        /// <value>A SQL query.</value>
        protected virtual string SequenceQuery => SequenceQuerySql;

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

        /// <summary>
        /// Retrieves database sequence information.
        /// </summary>
        /// <param name="sequenceName">A database sequence name.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A database sequence in the 'some' state if found; otherwise 'none'.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="sequenceName"/> is <c>null</c>.</exception>
        protected virtual OptionAsync<IDatabaseSequence> LoadSequence(Identifier sequenceName, CancellationToken cancellationToken)
        {
            if (sequenceName == null)
                throw new ArgumentNullException(nameof(sequenceName));

            var candidateSequenceName = QualifySequenceName(sequenceName);
            return GetResolvedSequenceName(candidateSequenceName, cancellationToken)
                .Bind(name => LoadSequenceData(name, cancellationToken));
        }

        private OptionAsync<IDatabaseSequence> LoadSequenceData(Identifier sequenceName, CancellationToken cancellationToken)
        {
            if (sequenceName == null)
                throw new ArgumentNullException(nameof(sequenceName));

            return Connection.QueryFirstOrNone<GetSequenceDefinitionQueryResult>(
                SequenceQuery,
                new GetSequenceDefinitionQuery { SchemaName = sequenceName.Schema!, SequenceName = sequenceName.LocalName },
                cancellationToken
            ).Map<IDatabaseSequence>(dto => new DatabaseSequence(
                sequenceName,
                dto.StartValue,
                dto.Increment,
                Option<decimal>.Some(dto.MinValue),
                Option<decimal>.Some(dto.MaxValue),
                dto.Cycle,
                dto.CacheSize
            ));
        }

        /// <summary>
        /// Qualifies the name of the sequence.
        /// </summary>
        /// <param name="sequenceName">A view name.</param>
        /// <returns>A sequence name is at least as qualified as the given sequence name.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="sequenceName"/> is <c>null</c>.</exception>
        protected Identifier QualifySequenceName(Identifier sequenceName)
        {
            if (sequenceName == null)
                throw new ArgumentNullException(nameof(sequenceName));

            var schema = sequenceName.Schema ?? IdentifierDefaults.Schema;
            return Identifier.CreateQualifiedIdentifier(IdentifierDefaults.Server, IdentifierDefaults.Database, schema, sequenceName.LocalName);
        }
    }
}
