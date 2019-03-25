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
using SJP.Schematic.SqlServer.Query;

namespace SJP.Schematic.SqlServer.Comments
{
    public class SqlServerSynonymCommentProvider : IDatabaseSynonymCommentProvider
    {
        public SqlServerSynonymCommentProvider(IDbConnection connection, IIdentifierDefaults identifierDefaults)
        {
            Connection = connection ?? throw new ArgumentNullException(nameof(connection));
            IdentifierDefaults = identifierDefaults ?? throw new ArgumentNullException(nameof(identifierDefaults));
        }

        protected IDbConnection Connection { get; }

        protected IIdentifierDefaults IdentifierDefaults { get; }

        protected virtual string CommentProperty { get; } = "MS_Description";

        public async Task<IReadOnlyCollection<IDatabaseSynonymComments>> GetAllSynonymComments(CancellationToken cancellationToken = default(CancellationToken))
        {
            var result = new List<IDatabaseSynonymComments>();

            var allCommentsData = await Connection.QueryAsync<CommentsData>(
                AllSynonymCommentsQuery,
                new { CommentProperty },
                cancellationToken
            ).ConfigureAwait(false);

            var groupedByName = allCommentsData.GroupBy(row => new { row.SchemaName, row.TableName }).ToList();
            foreach (var groupedComment in groupedByName)
            {
                var tmpIdentifier = Identifier.CreateQualifiedIdentifier(groupedComment.Key.SchemaName, groupedComment.Key.TableName);
                var qualifiedName = QualifySynonymName(tmpIdentifier);

                var commentsData = groupedComment.ToList();

                var synonymComment = GetFirstCommentByType(commentsData, Constants.Synonym);

                var comments = new DatabaseSynonymComments(qualifiedName, synonymComment);
                result.Add(comments);
            }

            return result
                .OrderBy(c => c.SynonymName.Schema)
                .ThenBy(c => c.SynonymName.LocalName)
                .ToList();
        }

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

        protected virtual string SynonymNameQuery => SynonymNameQuerySql;

        private const string SynonymNameQuerySql = @"
select top 1 schema_name(schema_id) as SchemaName, name as ObjectName
from sys.synonyms
where schema_id = schema_id(@SchemaName) and name = @SynonymName and is_ms_shipped = 0";

        public OptionAsync<IDatabaseSynonymComments> GetSynonymComments(Identifier synonymName, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (synonymName == null)
                throw new ArgumentNullException(nameof(synonymName));

            var candidateSynonymName = QualifySynonymName(synonymName);
            return LoadSynonymComments(candidateSynonymName, cancellationToken);
        }

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

        protected virtual string AllSynonymCommentsQuery => AllSynonymCommentsQuerySql;

        private const string AllSynonymCommentsQuerySql = @"
select SCHEMA_NAME(s.schema_id) as SchemaName, s.name as TableName, 'SYNONYM' as ObjectType, s.name as ObjectName, ep.value as Comment
from sys.synonyms s
left join sys.extended_properties ep on s.object_id = ep.major_id and ep.name = @CommentProperty and ep.minor_id = 0
where s.is_ms_shipped = 0
";

        protected virtual string SynonymCommentsQuery => SynonymCommentsQuerySql;

        private const string SynonymCommentsQuerySql = @"
select 'SYNONYM' as ObjectType, s.name as ObjectName, ep.value as Comment
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
                .Where(c => c.ObjectType == objectType)
                .Select(c => Option<string>.Some(c.Comment))
                .FirstOrDefault();
        }

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
