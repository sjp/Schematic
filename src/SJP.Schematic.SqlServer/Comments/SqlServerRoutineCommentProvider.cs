using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LanguageExt;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Comments;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.SqlServer.Queries;

namespace SJP.Schematic.SqlServer.Comments;

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
    /// <exception cref="ArgumentNullException"><paramref name="connection"/> or <paramref name="identifierDefaults"/> is <see langword="null" />.</exception>
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
    protected string CommentProperty { get; } = "MS_Description";

    /// <summary>
    /// Enumerates comments for all database routines.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of database routine comments, where available.</returns>
    public IAsyncEnumerable<IDatabaseRoutineComments> EnumerateAllRoutineComments(CancellationToken cancellationToken = default)
    {
        return Connection.QueryEnumerableAsync<GetAllRoutineNames.Result>(GetAllRoutineNames.Sql, cancellationToken)
            .Select(static dto => Identifier.CreateQualifiedIdentifier(dto.SchemaName, dto.RoutineName))
            .Select(QualifyRoutineName)
            .SelectAwait(routineName => LoadRoutineCommentsAsyncCore(routineName, cancellationToken).ToValue());
    }

    /// <summary>
    /// Retrieves comments for all database routines.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of database routine comments, where available.</returns>
    public async Task<IReadOnlyCollection<IDatabaseRoutineComments>> GetAllRoutineComments(CancellationToken cancellationToken = default)
    {
        var routineNames = await Connection.QueryEnumerableAsync<GetAllRoutineNames.Result>(GetAllRoutineNames.Sql, cancellationToken)
            .Select(static dto => Identifier.CreateQualifiedIdentifier(dto.SchemaName, dto.RoutineName))
            .Select(QualifyRoutineName)
            .ToListAsync(cancellationToken);

        return await routineNames
            .Select(routineName => LoadRoutineCommentsAsyncCore(routineName, cancellationToken))
            .ToArray()
            .WhenAll();
    }

    /// <summary>
    /// Gets the resolved name of the routine. This enables non-strict name matching to be applied.
    /// </summary>
    /// <param name="routineName">A routine name that will be resolved.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A routine name that, if available, can be assumed to exist and applied strictly.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="routineName"/> is <see langword="null" />.</exception>
    protected OptionAsync<Identifier> GetResolvedRoutineName(Identifier routineName, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(routineName);

        routineName = QualifyRoutineName(routineName);
        var qualifiedRoutineName = Connection.QueryFirstOrNone(
            GetRoutineName.Sql,
            new GetRoutineName.Query { SchemaName = routineName.Schema!, RoutineName = routineName.LocalName },
            cancellationToken
        );

        return qualifiedRoutineName.Map(name => Identifier.CreateQualifiedIdentifier(routineName.Server, routineName.Database, name.SchemaName, name.RoutineName));
    }

    /// <summary>
    /// Retrieves comments for a database routine, if available.
    /// </summary>
    /// <param name="routineName">A routine name.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Comments for the given database routine, if available.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="routineName"/> is <see langword="null" />.</exception>
    public OptionAsync<IDatabaseRoutineComments> GetRoutineComments(Identifier routineName, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(routineName);

        var candidateRoutineName = QualifyRoutineName(routineName);
        return LoadRoutineComments(candidateRoutineName, cancellationToken);
    }

    /// <summary>
    /// Retrieves comments for a database routine, if available.
    /// </summary>
    /// <param name="routineName">A routine name.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Comments for the given database routine, if available.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="routineName"/> is <see langword="null" />.</exception>
    protected OptionAsync<IDatabaseRoutineComments> LoadRoutineComments(Identifier routineName, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(routineName);

        var candidateRoutineName = QualifyRoutineName(routineName);
        return GetResolvedRoutineName(candidateRoutineName, cancellationToken)
            .MapAsync(name => LoadRoutineCommentsAsyncCore(name, cancellationToken));
    }

    private async Task<IDatabaseRoutineComments> LoadRoutineCommentsAsyncCore(Identifier routineName, CancellationToken cancellationToken)
    {
        var queryResult = await Connection.QueryAsync(
            Queries.GetRoutineComments.Sql,
            new GetRoutineComments.Query
            {
                SchemaName = routineName.Schema!,
                RoutineName = routineName.LocalName,
                CommentProperty = CommentProperty,
            },
            cancellationToken
        );

        var commentData = queryResult.Select(r => new CommentData
        {
            ObjectName = r.ObjectName,
            ObjectType = r.ObjectType,
            Comment = r.Comment,
        }).ToList();

        var routineComment = GetFirstCommentByType(commentData, Constants.Routine);

        return new DatabaseRoutineComments(routineName, routineComment);
    }

    private static Option<string> GetFirstCommentByType(IEnumerable<CommentData> commentsData, string objectType)
    {
        ArgumentNullException.ThrowIfNull(commentsData);
        ArgumentException.ThrowIfNullOrWhiteSpace(objectType);

        return commentsData
            .Where(c => c.ObjectType == objectType)
            .Select(static c => !c.Comment.IsNullOrWhiteSpace() ? Option<string>.Some(c.Comment) : Option<string>.None)
            .FirstOrDefault();
    }

    /// <summary>
    /// Qualifies the name of a routine, using known identifier defaults.
    /// </summary>
    /// <param name="routineName">A routine name to qualify.</param>
    /// <returns>A routine name that is at least as qualified as its input.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="routineName"/> is <see langword="null" />.</exception>
    protected Identifier QualifyRoutineName(Identifier routineName)
    {
        ArgumentNullException.ThrowIfNull(routineName);

        var schema = routineName.Schema ?? IdentifierDefaults.Schema;
        return Identifier.CreateQualifiedIdentifier(IdentifierDefaults.Server, IdentifierDefaults.Database, schema, routineName.LocalName);
    }

    private static class Constants
    {
        public const string Routine = "ROUTINE";
    }

    private sealed record CommentData
    {
        public required string ObjectType { get; init; }

        public required string ObjectName { get; init; }

        public required string? Comment { get; init; }
    }
}