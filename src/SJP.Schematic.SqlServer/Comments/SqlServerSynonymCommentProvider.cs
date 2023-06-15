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
using SJP.Schematic.SqlServer.Queries;

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
    public IAsyncEnumerable<IDatabaseSynonymComments> GetAllSynonymComments(CancellationToken cancellationToken = default)
    {
        return Connection.QueryEnumerableAsync<GetAllSynonymNames.Result>(GetAllSynonymNames.Sql, cancellationToken)
            .Select(static dto => Identifier.CreateQualifiedIdentifier(dto.SchemaName, dto.SynonymName))
            .Select(QualifySynonymName)
            .SelectAwait(synonymName => LoadSynonymCommentsAsyncCore(synonymName, cancellationToken).ToValue());
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
        ArgumentNullException.ThrowIfNull(synonymName);

        synonymName = QualifySynonymName(synonymName);
        var qualifiedSynonymName = Connection.QueryFirstOrNone(
            GetSynonymName.Sql,
            new GetSynonymName.Query { SchemaName = synonymName.Schema!, SynonymName = synonymName.LocalName },
            cancellationToken
        );

        return qualifiedSynonymName.Map(name => Identifier.CreateQualifiedIdentifier(synonymName.Server, synonymName.Database, name.SchemaName, name.SynonymName));
    }

    /// <summary>
    /// Retrieves comments for a database synonym.
    /// </summary>
    /// <param name="synonymName">A synonym name.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A comments object result in the some state, if found, none otherwise.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="synonymName"/> is <c>null</c>.</exception>
    public OptionAsync<IDatabaseSynonymComments> GetSynonymComments(Identifier synonymName, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(synonymName);

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
        ArgumentNullException.ThrowIfNull(synonymName);

        var candidateSynonymName = QualifySynonymName(synonymName);
        return GetResolvedSynonymName(candidateSynonymName, cancellationToken)
            .MapAsync(name => LoadSynonymCommentsAsyncCore(name, cancellationToken));
    }

    private async Task<IDatabaseSynonymComments> LoadSynonymCommentsAsyncCore(Identifier synonymName, CancellationToken cancellationToken)
    {
        var queryResult = await Connection.QueryAsync(
            Queries.GetSynonymComments.Sql,
            new GetSynonymComments.Query
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

    private static Option<string> GetFirstCommentByType(IEnumerable<CommentData> commentsData, string objectType)
    {
        ArgumentNullException.ThrowIfNull(commentsData);
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
        ArgumentNullException.ThrowIfNull(synonymName);

        var schema = synonymName.Schema ?? IdentifierDefaults.Schema;
        return Identifier.CreateQualifiedIdentifier(IdentifierDefaults.Server, IdentifierDefaults.Database, schema, synonymName.LocalName);
    }

    private static class Constants
    {
        public const string Synonym = "SYNONYM";
    }

    private sealed record CommentData
    {
        public required string ObjectType { get; init; }

        public required string ObjectName { get; init; }

        public required string? Comment { get; init; }
    }
}