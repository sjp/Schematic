using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using LanguageExt;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Comments;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.SqlServer.Query;

namespace SJP.Schematic.SqlServer.Comments
{
    /// <summary>
    /// A comment provider for SQL Server database synonyms.
    /// </summary>
    /// <seealso cref="IDatabaseSynonymCommentProvider" />
    public class SqlServerSynonymCommentProvider : IDatabaseSynonymCommentProvider
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SqlServerSynonymCommentProvider"/> class.
        /// </summary>
        /// <param name="connection">A database connection factory.</param>
        /// <param name="identifierDefaults">Database identifier defaults.</param>
        /// <exception cref="ArgumentNullException"><paramref name="connection"/> or <paramref name="identifierDefaults"/> are <c>null</c>.</exception>
        public SqlServerSynonymCommentProvider(IDbConnectionFactory connection, IIdentifierDefaults identifierDefaults)
        {
            Connection = connection ?? throw new ArgumentNullException(nameof(connection));
            IdentifierDefaults = identifierDefaults ?? throw new ArgumentNullException(nameof(identifierDefaults));
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
        /// Retrieves the extended property name used to store comments on an object.
        /// </summary>
        /// <value>The comment property name.</value>
        protected virtual string CommentProperty { get; } = "MS_Description";

        /// <summary>
        /// Retrieves comments for all database synonyms.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A collection of database synonyms comments.</returns>
        public async IAsyncEnumerable<IDatabaseSynonymComments> GetAllSynonymComments([EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var allCommentsData = await Connection.QueryAsync<CommentsData>(
                AllSynonymCommentsQuery,
                new { CommentProperty },
                cancellationToken
            ).ConfigureAwait(false);

            var comments = allCommentsData
                .GroupBy(row => new { row.SchemaName, row.TableName })
                .Select(g =>
                {
                    var synonymName = QualifySynonymName(Identifier.CreateQualifiedIdentifier(g.Key.SchemaName, g.Key.TableName));
                    var comments = g.ToList();

                    var synonymComment = GetFirstCommentByType(comments, Constants.Synonym);
                    return new DatabaseSynonymComments(synonymName, synonymComment);
                });

            foreach (var comment in comments)
                yield return comment;
        }

        /// <summary>
        /// Gets the resolved name of the synonym. This enables non-strict name matching to be applied.
        /// </summary>
        /// <param name="synonymName">A synonym name.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A synonym name that, if available, can be assumed to exist and applied strictly.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="synonymName"/> is <c>null</c>.</exception>
        protected OptionAsync<Identifier> GetResolvedSynonymName(Identifier synonymName, CancellationToken cancellationToken)
        {
            if (synonymName == null)
                throw new ArgumentNullException(nameof(synonymName));

            synonymName = QualifySynonymName(synonymName);
            var qualifiedSynonymName = Connection.QueryFirstOrNone<QualifiedName>(
                SynonymNameQuery,
                new { SchemaName = synonymName.Schema, SynonymName = synonymName.LocalName },
                cancellationToken
            );

            return qualifiedSynonymName.Map(name => Identifier.CreateQualifiedIdentifier(synonymName.Server, synonymName.Database, name.SchemaName, name.ObjectName));
        }

        /// <summary>
        /// Gets a query that retrieves a synonym's name, used for name resolution.
        /// </summary>
        /// <value>A SQL query.</value>
        protected virtual string SynonymNameQuery => SynonymNameQuerySql;

        private static readonly string SynonymNameQuerySql = @$"
select top 1 schema_name(schema_id) as [{ nameof(QualifiedName.SchemaName) }], name as [{ nameof(QualifiedName.ObjectName) }]
from sys.synonyms
where schema_id = schema_id(@SchemaName) and name = @SynonymName and is_ms_shipped = 0";

        /// <summary>
        /// Retrieves comments for a database synonym.
        /// </summary>
        /// <param name="synonymName">A synonym name.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A comments object result in the some state, if found, none otherwise.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="synonymName"/> is <c>null</c>.</exception>
        public OptionAsync<IDatabaseSynonymComments> GetSynonymComments(Identifier synonymName, CancellationToken cancellationToken = default)
        {
            if (synonymName == null)
                throw new ArgumentNullException(nameof(synonymName));

            var candidateSynonymName = QualifySynonymName(synonymName);
            return LoadSynonymComments(candidateSynonymName, cancellationToken);
        }

        /// <summary>
        /// Retrieves comments for a database synonym.
        /// </summary>
        /// <param name="synonymName">A synonym name.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A comments object result in the some state, if found, none otherwise.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="synonymName"/> is <c>null</c>.</exception>
        protected virtual OptionAsync<IDatabaseSynonymComments> LoadSynonymComments(Identifier synonymName, CancellationToken cancellationToken)
        {
            if (synonymName == null)
                throw new ArgumentNullException(nameof(synonymName));

            var candidateSynonymName = QualifySynonymName(synonymName);
            return GetResolvedSynonymName(candidateSynonymName, cancellationToken)
                .MapAsync(name => LoadSynonymCommentsAsyncCore(name, cancellationToken));
        }

        private async Task<IDatabaseSynonymComments> LoadSynonymCommentsAsyncCore(Identifier synonymName, CancellationToken cancellationToken)
        {
            var commentsData = await Connection.QueryAsync<CommentsData>(
                SynonymCommentsQuery,
                new { SchemaName = synonymName.Schema, SynonymName = synonymName.LocalName, CommentProperty },
                cancellationToken
            ).ConfigureAwait(false);

            var synonymComment = GetFirstCommentByType(commentsData, Constants.Synonym);

            return new DatabaseSynonymComments(synonymName, synonymComment);
        }

        /// <summary>
        /// Gets a query that retrieves comment information on all synonyms.
        /// </summary>
        /// <value>A SQL query.</value>
        protected virtual string AllSynonymCommentsQuery => AllSynonymCommentsQuerySql;

        private static readonly string AllSynonymCommentsQuerySql = @$"
select
    SCHEMA_NAME(s.schema_id) as [{ nameof(CommentsData.SchemaName) }],
    s.name as [{ nameof(CommentsData.TableName) }],
    'SYNONYM' as [{ nameof(CommentsData.ObjectType) }],
    s.name as [{ nameof(CommentsData.ObjectName) }],
    ep.value as [{ nameof(CommentsData.Comment) }]
from sys.synonyms s
left join sys.extended_properties ep on s.object_id = ep.major_id and ep.name = @CommentProperty and ep.minor_id = 0
where s.is_ms_shipped = 0
order by SCHEMA_NAME(s.schema_id), s.name
";

        /// <summary>
        /// Gets a query that retrieves comment information on a single synonym.
        /// </summary>
        /// <value>A SQL query.</value>
        protected virtual string SynonymCommentsQuery => SynonymCommentsQuerySql;

        private static readonly string SynonymCommentsQuerySql = @$"
select
    'SYNONYM' as [{ nameof(CommentsData.ObjectType) }],
    s.name as [{ nameof(CommentsData.ObjectName) }],
    ep.value as [{ nameof(CommentsData.Comment) }]
from sys.synonyms s
left join sys.extended_properties ep on s.object_id = ep.major_id and ep.name = @CommentProperty and ep.minor_id = 0
where s.schema_id = SCHEMA_ID(@SchemaName) and s.name = @SynonymName and s.is_ms_shipped = 0
";

        private static Option<string> GetFirstCommentByType(IEnumerable<CommentsData> commentsData, string objectType)
        {
            if (commentsData == null)
                throw new ArgumentNullException(nameof(commentsData));
            if (objectType.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(objectType));

            return commentsData
                .Where(c => string.Equals(c.ObjectType, objectType, StringComparison.Ordinal))
                .Select(c => !c.Comment.IsNullOrWhiteSpace() ? Option<string>.Some(c.Comment) : Option<string>.None)
                .FirstOrDefault();
        }

        /// <summary>
        /// Qualifies the name of a synonym, using known identifier defaults.
        /// </summary>
        /// <param name="synonymName">A synonym name to qualify.</param>
        /// <returns>A synonym name that is at least as qualified as its input.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="synonymName"/> is <c>null</c>.</exception>
        protected Identifier QualifySynonymName(Identifier synonymName)
        {
            if (synonymName == null)
                throw new ArgumentNullException(nameof(synonymName));

            var schema = synonymName.Schema ?? IdentifierDefaults.Schema;
            return Identifier.CreateQualifiedIdentifier(IdentifierDefaults.Server, IdentifierDefaults.Database, schema, synonymName.LocalName);
        }

        private static class Constants
        {
            public const string Synonym = "SYNONYM";
        }
    }
}
