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
    ///  A routine comment provider for SQL Server.
    /// </summary>
    /// <seealso cref="IDatabaseRoutineCommentProvider" />
    public class SqlServerRoutineCommentProvider : IDatabaseRoutineCommentProvider
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SqlServerDatabaseCommentProvider"/> class.
        /// </summary>
        /// <param name="connection">A database connection.</param>
        /// <param name="identifierDefaults">Identifier defaults for the associated database.</param>
        /// <exception cref="ArgumentNullException"><paramref name="connection"/> or <paramref name="identifierDefaults"/> is <c>null</c>.</exception>
        public SqlServerRoutineCommentProvider(IDbConnectionFactory connection, IIdentifierDefaults identifierDefaults)
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
        /// Retrieves comments for all database routines.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A collection of database routine comments, where available.</returns>
        public async IAsyncEnumerable<IDatabaseRoutineComments> GetAllRoutineComments([EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var allCommentsData = await Connection.QueryAsync<CommentsData>(
                AllRoutineCommentsQuery,
                new { CommentProperty },
                cancellationToken
            ).ConfigureAwait(false);

            var comments = allCommentsData
                .GroupBy(row => new { row.SchemaName, row.TableName })
                .Select(g =>
                {
                    var qualifiedName = QualifyRoutineName(Identifier.CreateQualifiedIdentifier(g.Key.SchemaName, g.Key.TableName));
                    var commentsData = g.ToList();
                    var routineComment = GetFirstCommentByType(commentsData, Constants.Routine);

                    return new DatabaseRoutineComments(qualifiedName, routineComment);
                });

            foreach (var comment in comments)
                yield return comment;
        }

        /// <summary>
        /// Gets the resolved name of the routine. This enables non-strict name matching to be applied.
        /// </summary>
        /// <param name="routineName">A routine name that will be resolved.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A routine name that, if available, can be assumed to exist and applied strictly.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="routineName"/> is <c>null</c>.</exception>
        protected OptionAsync<Identifier> GetResolvedRoutineName(Identifier routineName, CancellationToken cancellationToken)
        {
            if (routineName == null)
                throw new ArgumentNullException(nameof(routineName));

            routineName = QualifyRoutineName(routineName);
            var qualifiedRoutineName = Connection.QueryFirstOrNone<QualifiedName>(
                RoutineNameQuery,
                new { SchemaName = routineName.Schema, RoutineName = routineName.LocalName },
                cancellationToken
            );

            return qualifiedRoutineName.Map(name => Identifier.CreateQualifiedIdentifier(routineName.Server, routineName.Database, name.SchemaName, name.ObjectName));
        }

        /// <summary>
        /// A SQL query that retrieves the resolved routine name.
        /// </summary>
        /// <value>A SQL query.</value>
        protected virtual string RoutineNameQuery => RoutineNameQuerySql;

        private static readonly string RoutineNameQuerySql = @$"
select top 1 schema_name(schema_id) as [{ nameof(QualifiedName.SchemaName) }], name as [{ nameof(QualifiedName.ObjectName) }]
from sys.objects
where schema_id = schema_id(@SchemaName) and name = @RoutineName
    and type in ('P', 'FN', 'IF', 'TF') and is_ms_shipped = 0";

        /// <summary>
        /// Retrieves comments for a database routine, if available.
        /// </summary>
        /// <param name="routineName">A routine name.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Comments for the given database routine, if available.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="routineName"/> is <c>null</c>.</exception>
        public OptionAsync<IDatabaseRoutineComments> GetRoutineComments(Identifier routineName, CancellationToken cancellationToken = default)
        {
            if (routineName == null)
                throw new ArgumentNullException(nameof(routineName));

            var candidateRoutineName = QualifyRoutineName(routineName);
            return LoadRoutineComments(candidateRoutineName, cancellationToken);
        }

        /// <summary>
        /// Retrieves comments for a database routine, if available.
        /// </summary>
        /// <param name="routineName">A routine name.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Comments for the given database routine, if available.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="routineName"/> is <c>null</c>.</exception>
        protected virtual OptionAsync<IDatabaseRoutineComments> LoadRoutineComments(Identifier routineName, CancellationToken cancellationToken)
        {
            if (routineName == null)
                throw new ArgumentNullException(nameof(routineName));

            var candidateRoutineName = QualifyRoutineName(routineName);
            return GetResolvedRoutineName(candidateRoutineName, cancellationToken)
                .MapAsync(name => LoadRoutineCommentsAsyncCore(name, cancellationToken));
        }

        private async Task<IDatabaseRoutineComments> LoadRoutineCommentsAsyncCore(Identifier routineName, CancellationToken cancellationToken)
        {
            var commentsData = await Connection.QueryAsync<CommentsData>(
                RoutineCommentsQuery,
                new { SchemaName = routineName.Schema, RoutineName = routineName.LocalName, CommentProperty },
                cancellationToken
            ).ConfigureAwait(false);

            var routineComment = GetFirstCommentByType(commentsData, Constants.Routine);

            return new DatabaseRoutineComments(routineName, routineComment);
        }

        /// <summary>
        /// Gets a query that retrieves comments for all routines in the database.
        /// </summary>
        /// <value>A SQL query.</value>
        protected virtual string AllRoutineCommentsQuery => AllRoutineCommentsQuerySql;

        private static readonly string AllRoutineCommentsQuerySql = @$"
select
    SCHEMA_NAME(r.schema_id) as [{ nameof(CommentsData.SchemaName) }],
    r.name as [{ nameof(CommentsData.TableName) }],
    'ROUTINE' as [{ nameof(CommentsData.ObjectType) }],
    r.name as [{ nameof(CommentsData.ObjectName) }],
    ep.value as [{ nameof(CommentsData.Comment) }]
from sys.objects r
left join sys.extended_properties ep on r.object_id = ep.major_id and ep.name = @CommentProperty and ep.minor_id = 0
where r.is_ms_shipped = 0 and r.type in ('P', 'FN', 'IF', 'TF')
order by SCHEMA_NAME(r.schema_id), r.name
";

        /// <summary>
        /// Gets a query that retrieves comments for a single routine.
        /// </summary>
        /// <value>A SQL query.</value>
        protected virtual string RoutineCommentsQuery => RoutineCommentsQuerySql;

        private static readonly string RoutineCommentsQuerySql = @$"
select
    'ROUTINE' as [{ nameof(CommentsData.ObjectType) }],
    r.name as [{ nameof(CommentsData.ObjectName) }],
    ep.value as [{ nameof(CommentsData.Comment) }]
from sys.objects r
left join sys.extended_properties ep on r.object_id = ep.major_id and ep.name = @CommentProperty and ep.minor_id = 0
where r.schema_id = SCHEMA_ID(@SchemaName) and r.name = @RoutineName and r.is_ms_shipped = 0
    and r.type in ('P', 'FN', 'IF', 'TF')
";

        private static Option<string> GetFirstCommentByType(IEnumerable<CommentsData> commentsData, string objectType)
        {
            if (commentsData == null)
                throw new ArgumentNullException(nameof(commentsData));
            if (objectType.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(objectType));

            return commentsData
                .Where(c => c.ObjectType == objectType)
                .Select(c => !c.Comment.IsNullOrWhiteSpace() ? Option<string>.Some(c.Comment) : Option<string>.None)
                .FirstOrDefault();
        }

        /// <summary>
        /// Qualifies the name of a routine, using known identifier defaults.
        /// </summary>
        /// <param name="routineName">A routine name to qualify.</param>
        /// <returns>A routine name that is at least as qualified as its input.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="routineName"/> is <c>null</c>.</exception>
        protected Identifier QualifyRoutineName(Identifier routineName)
        {
            if (routineName == null)
                throw new ArgumentNullException(nameof(routineName));

            var schema = routineName.Schema ?? IdentifierDefaults.Schema;
            return Identifier.CreateQualifiedIdentifier(IdentifierDefaults.Server, IdentifierDefaults.Database, schema, routineName.LocalName);
        }

        private static class Constants
        {
            public const string Routine = "ROUTINE";
        }
    }
}
