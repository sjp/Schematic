using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using LanguageExt;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Comments;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.PostgreSql.Query;

namespace SJP.Schematic.PostgreSql.Comments
{
    /// <summary>
    /// A database sequence comment provider for PostgreSQL.
    /// </summary>
    /// <seealso cref="IDatabaseSequenceCommentProvider" />
    public class PostgreSqlSequenceCommentProvider : IDatabaseSequenceCommentProvider
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PostgreSqlSequenceCommentProvider"/> class.
        /// </summary>
        /// <param name="connection">A database connection factory.</param>
        /// <param name="identifierDefaults">Database identifier defaults.</param>
        /// <param name="identifierResolver">An identifier resolver.</param>
        /// <exception cref="ArgumentNullException"><paramref name="connection"/> or <paramref name="identifierDefaults"/> or <paramref name="identifierResolver"/> are <c>null</c>.</exception>
        public PostgreSqlSequenceCommentProvider(IDbConnectionFactory connection, IIdentifierDefaults identifierDefaults, IIdentifierResolutionStrategy identifierResolver)
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
        /// Retrieves comments for all database sequences.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A collection of database sequence comments.</returns>
        public async IAsyncEnumerable<IDatabaseSequenceComments> GetAllSequenceComments([EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var allCommentsData = await Connection.QueryAsync<CommentsData>(AllSequenceCommentsQuery, cancellationToken).ConfigureAwait(false);

            foreach (var commentData in allCommentsData)
            {
                var tmpIdentifier = Identifier.CreateQualifiedIdentifier(commentData.SchemaName, commentData.ObjectName);
                var qualifiedName = QualifySequenceName(tmpIdentifier);

                var sequenceComment = !commentData.Comment.IsNullOrWhiteSpace()
                    ? Option<string>.Some(commentData.Comment)
                    : Option<string>.None;

                yield return new DatabaseSequenceComments(qualifiedName, sequenceComment);
            }
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
            var qualifiedSequenceName = Connection.QueryFirstOrNone<QualifiedName>(
                SequenceNameQuery,
                new { SchemaName = candidateSequenceName.Schema, SequenceName = candidateSequenceName.LocalName },
                cancellationToken
            );

            return qualifiedSequenceName.Map(name => Identifier.CreateQualifiedIdentifier(candidateSequenceName.Server, candidateSequenceName.Database, name.SchemaName, name.ObjectName));
        }

        /// <summary>
        /// Gets a query that resolves the name of a sequence.
        /// </summary>
        /// <value>A SQL query.</value>
        protected virtual string SequenceNameQuery => SequenceNameQuerySql;

        private static readonly string SequenceNameQuerySql = @$"
select sequence_schema as ""{ nameof(QualifiedName.SchemaName) }"", sequence_name as ""{ nameof(QualifiedName.ObjectName) }""
from information_schema.sequences
where sequence_schema = @SchemaName and sequence_name = @SequenceName
    and sequence_schema not in ('pg_catalog', 'information_schema')
limit 1";

        /// <summary>
        /// Retrieves comments for a particular database sequence.
        /// </summary>
        /// <param name="sequenceName">The name of a database sequence.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>An <see cref="OptionAsync{A}" /> instance which holds the value of the sequence's comments, if available.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="sequenceName"/> is <c>null</c>.</exception>
        public OptionAsync<IDatabaseSequenceComments> GetSequenceComments(Identifier sequenceName, CancellationToken cancellationToken = default)
        {
            if (sequenceName == null)
                throw new ArgumentNullException(nameof(sequenceName));

            var candidateSequenceName = QualifySequenceName(sequenceName);
            return LoadSequenceComments(candidateSequenceName, cancellationToken);
        }

        /// <summary>
        /// Retrieves comments for a particular database sequence.
        /// </summary>
        /// <param name="sequenceName">The name of a database sequence.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>An <see cref="OptionAsync{A}" /> instance which holds the value of the sequence's comments, if available.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="sequenceName"/> is <c>null</c>.</exception>
        protected virtual OptionAsync<IDatabaseSequenceComments> LoadSequenceComments(Identifier sequenceName, CancellationToken cancellationToken)
        {
            if (sequenceName == null)
                throw new ArgumentNullException(nameof(sequenceName));

            var candidateSequenceName = QualifySequenceName(sequenceName);
            return GetResolvedSequenceName(candidateSequenceName, cancellationToken)
                .Bind(name =>
                {
                    return Connection.QueryFirstOrNone<CommentsData>(
                        SequenceCommentsQuery,
                        new { SchemaName = name.Schema, SequenceName = name.LocalName },
                        cancellationToken
                    ).Map<IDatabaseSequenceComments>(c =>
                    {
                        var comment = !c.Comment.IsNullOrWhiteSpace()
                            ? Option<string>.Some(c.Comment)
                            : Option<string>.None;
                        return new DatabaseSequenceComments(name, comment);
                    });
                });
        }

        /// <summary>
        /// Gets a query that retrieves comment information on all sequences.
        /// </summary>
        /// <value>A SQL query.</value>
        protected virtual string AllSequenceCommentsQuery => AllSequenceCommentsQuerySql;

        private static readonly string AllSequenceCommentsQuerySql = @$"
select
    nc.nspname as ""{ nameof(CommentsData.SchemaName) }"",
    c.relname as ""{ nameof(CommentsData.ObjectName) }"",
    d.description as ""{ nameof(CommentsData.Comment) }""
from pg_catalog.pg_namespace nc
inner join pg_catalog.pg_class c on c.relnamespace = nc.oid
left join pg_catalog.pg_description d on d.objoid = c.oid
where nc.nspname not in ('pg_catalog', 'information_schema') and c.relkind = 'S'
order by nc.nspname, c.relname";

        /// <summary>
        /// Gets a query that retrieves comment information on a single comment.
        /// </summary>
        /// <value>A SQL query.</value>
        protected virtual string SequenceCommentsQuery => SequenceCommentsQuerySql;

        private static readonly string SequenceCommentsQuerySql = @$"
select
    nc.nspname as ""{ nameof(CommentsData.SchemaName) }"",
    c.relname as ""{ nameof(CommentsData.ObjectName) }"",
    d.description as ""{ nameof(CommentsData.Comment) }""
from pg_catalog.pg_namespace nc
inner join pg_catalog.pg_class c on c.relnamespace = nc.oid
left join pg_catalog.pg_description d on d.objoid = c.oid
where nc.nspname = @SchemaName and c.relname = @SequenceName
    and nc.nspname not in ('pg_catalog', 'information_schema')
    and c.relkind = 'S'
";

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
