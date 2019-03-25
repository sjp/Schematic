using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LanguageExt;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Comments;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.PostgreSql.Query;

namespace SJP.Schematic.PostgreSql.Comments
{
    public class PostgreSqlSequenceCommentProvider : IDatabaseSequenceCommentProvider
    {
        public PostgreSqlSequenceCommentProvider(IDbConnection connection, IIdentifierDefaults identifierDefaults, IIdentifierResolutionStrategy identifierResolver)
        {
            Connection = connection ?? throw new ArgumentNullException(nameof(connection));
            IdentifierDefaults = identifierDefaults ?? throw new ArgumentNullException(nameof(identifierDefaults));
            IdentifierResolver = identifierResolver ?? throw new ArgumentNullException(nameof(identifierResolver));
        }

        protected IDbConnection Connection { get; }

        protected IIdentifierDefaults IdentifierDefaults { get; }

        protected IIdentifierResolutionStrategy IdentifierResolver { get; }

        public async Task<IReadOnlyCollection<IDatabaseSequenceComments>> GetAllSequenceComments(CancellationToken cancellationToken = default(CancellationToken))
        {
            var allCommentsData = await Connection.QueryAsync<CommentsData>(AllSequenceCommentsQuery, cancellationToken).ConfigureAwait(false);

            var result = new List<IDatabaseSequenceComments>();

            foreach (var commentData in allCommentsData)
            {
                var tmpIdentifier = Identifier.CreateQualifiedIdentifier(commentData.SchemaName, commentData.ObjectName);
                var qualifiedName = QualifySequenceName(tmpIdentifier);

                var sequenceComment = !commentData.Comment.IsNullOrWhiteSpace()
                    ? Option<string>.Some(commentData.Comment)
                    : Option<string>.None;

                var comments = new DatabaseSequenceComments(qualifiedName, sequenceComment);
                result.Add(comments);
            }

            return result;
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

        public OptionAsync<IDatabaseSequenceComments> GetSequenceComments(Identifier sequenceName, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (sequenceName == null)
                throw new ArgumentNullException(nameof(sequenceName));

            var candidateSequenceName = QualifySequenceName(sequenceName);
            return LoadSequenceComments(candidateSequenceName, cancellationToken);
        }

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

        protected virtual string AllSequenceCommentsQuery => AllSequenceCommentsQuerySql;

        private const string AllSequenceCommentsQuerySql = @"
select
    nc.nspname as SchemaName,
    c.relname as ObjectName,
    d.description as Comment
from pg_catalog.pg_namespace nc
inner join pg_catalog.pg_class c on c.relnamespace = nc.oid
left join pg_catalog.pg_description d on d.objoid = c.oid
where nc.nspname not in ('pg_catalog', 'information_schema') and c.relkind = 'S'
order by nc.nspname, c.relname";

        protected virtual string SequenceCommentsQuery => SequenceCommentsQuerySql;

        private const string SequenceCommentsQuerySql = @"
select
    nc.nspname as SchemaName,
    c.relname as ObjectName,
    d.description as Comment
from pg_catalog.pg_namespace nc
inner join pg_catalog.pg_class c on c.relnamespace = nc.oid
left join pg_catalog.pg_description d on d.objoid = c.oid
where nc.nspname = @SchemaName and c.relname = @SequenceName
    and nc.nspname not in ('pg_catalog', 'information_schema')
    and c.relkind = 'S'
";

        protected Identifier QualifySequenceName(Identifier sequenceName)
        {
            if (sequenceName == null)
                throw new ArgumentNullException(nameof(sequenceName));

            var schema = sequenceName.Schema ?? IdentifierDefaults.Schema;
            return Identifier.CreateQualifiedIdentifier(IdentifierDefaults.Server, IdentifierDefaults.Database, schema, sequenceName.LocalName);
        }
    }
}
