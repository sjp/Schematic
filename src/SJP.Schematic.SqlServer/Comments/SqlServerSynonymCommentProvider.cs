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
using SJP.Schematic.SqlServer.QueryResult;

namespace SJP.Schematic.SqlServer.Comments;

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
        var queryResults = await Connection.QueryAsync<GetAllSynonymNamesQueryResult>(SynonymsQuery, cancellationToken).ConfigureAwait(false);
        var synonymNames = queryResults
            .Select(static dto => Identifier.CreateQualifiedIdentifier(dto.SchemaName, dto.SynonymName))
            .Select(QualifySynonymName);

        foreach (var synonymName in synonymNames)
            yield return await LoadSynonymCommentsAsyncCore(synonymName, cancellationToken).ConfigureAwait(false);
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
        var qualifiedSynonymName = Connection.QueryFirstOrNone<GetSynonymNameQueryResult>(
            SynonymNameQuery,
            new GetSynonymNameQuery { SchemaName = synonymName.Schema!, SynonymName = synonymName.LocalName },
            cancellationToken
        );

        return qualifiedSynonymName.Map(name => Identifier.CreateQualifiedIdentifier(synonymName.Server, synonymName.Database, name.SchemaName, name.SynonymName));
    }

    /// <summary>
    /// Gets a query that retrieves a synonym's name, used for name resolution.
    /// </summary>
    /// <value>A SQL query.</value>
    protected virtual string SynonymNameQuery => SynonymNameQuerySql;

    private const string SynonymNameQuerySql = @$"
select top 1 schema_name(schema_id) as [{ nameof(GetSynonymNameQueryResult.SchemaName) }], name as [{ nameof(GetSynonymNameQueryResult.SynonymName) }]
from sys.synonyms
where schema_id = schema_id(@{ nameof(GetSynonymNameQueryResult.SchemaName) }) and name = @{ nameof(GetSynonymNameQueryResult.SynonymName) } and is_ms_shipped = 0";

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
        var queryResult = await Connection.QueryAsync<GetSynonymCommentsQueryResult>(
            SynonymCommentsQuery,
            new GetSynonymCommentsQuery
            {
                SchemaName = synonymName.Schema!,
                SynonymName = synonymName.LocalName,
                CommentProperty = CommentProperty
            },
            cancellationToken
        ).ConfigureAwait(false);

        var commentData = queryResult.Select(r => new CommentData
        {
            ObjectName = r.ObjectName,
            ObjectType = r.ObjectType,
            Comment = r.Comment
        }).ToList();

        var synonymComment = GetFirstCommentByType(commentData, Constants.Synonym);

        return new DatabaseSynonymComments(synonymName, synonymComment);
    }

    /// <summary>
    /// A SQL query that retrieves the names of all synonyms in the database.
    /// </summary>
    /// <value>A SQL query.</value>
    protected virtual string SynonymsQuery => SynonymsQuerySql;

    private const string SynonymsQuerySql = @$"
select
    schema_name(schema_id) as [{ nameof(GetAllSynonymNamesQueryResult.SchemaName) }],
    name as [{ nameof(GetAllSynonymNamesQueryResult.SynonymName) }]
from sys.synonyms
where is_ms_shipped = 0
order by schema_name(schema_id), name";

    /// <summary>
    /// Gets a query that retrieves comment information on a single synonym.
    /// </summary>
    /// <value>A SQL query.</value>
    protected virtual string SynonymCommentsQuery => SynonymCommentsQuerySql;

    private const string SynonymCommentsQuerySql = @$"
select
    'SYNONYM' as [{ nameof(GetSynonymCommentsQueryResult.ObjectType) }],
    s.name as [{ nameof(GetSynonymCommentsQueryResult.ObjectName) }],
    ep.value as [{ nameof(GetSynonymCommentsQueryResult.Comment) }]
from sys.synonyms s
left join sys.extended_properties ep on s.object_id = ep.major_id and ep.name = @{ nameof(GetSynonymCommentsQuery.CommentProperty) } and ep.minor_id = 0
where s.schema_id = SCHEMA_ID(@{ nameof(GetSynonymCommentsQuery.SchemaName) }) and s.name = @{ nameof(GetSynonymCommentsQuery.SynonymName) } and s.is_ms_shipped = 0
";

    private static Option<string> GetFirstCommentByType(IEnumerable<CommentData> commentsData, string objectType)
    {
        if (commentsData == null)
            throw new ArgumentNullException(nameof(commentsData));
        if (objectType.IsNullOrWhiteSpace())
            throw new ArgumentNullException(nameof(objectType));

        return commentsData
            .Where(c => string.Equals(c.ObjectType, objectType, StringComparison.Ordinal))
            .Select(static c => !c.Comment.IsNullOrWhiteSpace() ? Option<string>.Some(c.Comment) : Option<string>.None)
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

    private sealed record CommentData
    {
        public string SchemaName { get; init; } = default!;

        public string SynonymName { get; init; } = default!;

        public string ObjectType { get; init; } = default!;

        public string ObjectName { get; init; } = default!;

        public string? Comment { get; init; }
    }
}
