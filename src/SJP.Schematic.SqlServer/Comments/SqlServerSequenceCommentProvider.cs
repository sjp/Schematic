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
/// A database sequence comment provider for SQL Server.
/// </summary>
/// <seealso cref="IDatabaseSequenceCommentProvider" />
public class SqlServerSequenceCommentProvider : IDatabaseSequenceCommentProvider
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SqlServerSequenceCommentProvider"/> class.
    /// </summary>
    /// <param name="connection">A database connection factory.</param>
    /// <param name="identifierDefaults">Database identifier defaults.</param>
    /// <exception cref="ArgumentNullException"><paramref name="connection"/> or <paramref name="identifierDefaults"/> are <c>null</c>.</exception>
    public SqlServerSequenceCommentProvider(IDbConnectionFactory connection, IIdentifierDefaults identifierDefaults)
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
    /// Retrieves comments for all database sequences.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of database sequence comments.</returns>
    public async IAsyncEnumerable<IDatabaseSequenceComments> GetAllSequenceComments([EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var queryResults = await Connection.QueryAsync<GetAllSequenceNames.Result>(GetAllSequenceNames.Sql, cancellationToken).ConfigureAwait(false);
        var sequenceNames = queryResults
            .Select(static dto => Identifier.CreateQualifiedIdentifier(dto.SchemaName, dto.SequenceName))
            .Select(QualifySequenceName);

        foreach (var sequenceName in sequenceNames)
            yield return await LoadSequenceCommentsAsyncCore(sequenceName, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Gets the resolved name of the sequence. This enables non-strict name matching to be applied.
    /// </summary>
    /// <param name="sequenceName">A sequence name.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A sequence name that, if available, can be assumed to exist and applied strictly.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="sequenceName"/> is <c>null</c>.</exception>
    protected OptionAsync<Identifier> GetResolvedSequenceName(Identifier sequenceName, CancellationToken cancellationToken)
    {
        if (sequenceName == null)
            throw new ArgumentNullException(nameof(sequenceName));

        sequenceName = QualifySequenceName(sequenceName);
        var qualifiedSequenceName = Connection.QueryFirstOrNone<GetSequenceName.Result>(
            GetSequenceName.Sql,
            new GetSequenceName.Query { SchemaName = sequenceName.Schema!, SequenceName = sequenceName.LocalName },
            cancellationToken
        );

        return qualifiedSequenceName.Map(name => Identifier.CreateQualifiedIdentifier(sequenceName.Server, sequenceName.Database, name.SchemaName, name.SequenceName));
    }

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
            .MapAsync(name => LoadSequenceCommentsAsyncCore(name, cancellationToken));
    }

    private async Task<IDatabaseSequenceComments> LoadSequenceCommentsAsyncCore(Identifier sequenceName, CancellationToken cancellationToken)
    {
        var queryResult = await Connection.QueryAsync<GetSequenceComments.Result>(
            Queries.GetSequenceComments.Sql,
            new GetSequenceComments.Query
            {
                SchemaName = sequenceName.Schema!,
                SequenceName = sequenceName.LocalName,
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

        var sequenceComment = GetFirstCommentByType(commentData, Constants.Sequence);

        return new DatabaseSequenceComments(sequenceName, sequenceComment);
    }

    private static Option<string> GetFirstCommentByType(IEnumerable<CommentData> commentsData, string objectType)
    {
        if (commentsData == null)
            throw new ArgumentNullException(nameof(commentsData));
        if (objectType.IsNullOrWhiteSpace())
            throw new ArgumentNullException(nameof(objectType));

        return commentsData
            .Where(c => c.ObjectType == objectType)
            .Select(static c => !c.Comment.IsNullOrWhiteSpace() ? Option<string>.Some(c.Comment) : Option<string>.None)
            .FirstOrDefault();
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

    private static class Constants
    {
        public const string Sequence = "SEQUENCE";
    }

    private sealed record CommentData
    {
        public string SchemaName { get; init; } = default!;

        public string SequenceName { get; init; } = default!;

        public string ObjectType { get; init; } = default!;

        public string ObjectName { get; init; } = default!;

        public string? Comment { get; init; }
    }
}