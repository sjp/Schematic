using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LanguageExt;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Comments;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.PostgreSql.Queries;

namespace SJP.Schematic.PostgreSql.Comments;

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
    /// <exception cref="ArgumentNullException"><paramref name="connection"/> or <paramref name="identifierDefaults"/> or <paramref name="identifierResolver"/> are <see langword="null" />.</exception>
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
    /// Enumerates comments for all database sequences.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of database sequence comments.</returns>
    public IAsyncEnumerable<IDatabaseSequenceComments> GetAllSequenceComments(CancellationToken cancellationToken = default)
    {
        return Connection.QueryEnumerableAsync<GetAllSequenceComments.Result>(Queries.GetAllSequenceComments.Sql, cancellationToken)
            .Select(commentData =>
            {
                var tmpIdentifier = Identifier.CreateQualifiedIdentifier(commentData.SchemaName, commentData.SequenceName);
                var qualifiedName = QualifySequenceName(tmpIdentifier);

                var sequenceComment = !commentData.Comment.IsNullOrWhiteSpace()
                    ? Option<string>.Some(commentData.Comment)
                    : Option<string>.None;

                return new DatabaseSequenceComments(qualifiedName, sequenceComment);
            });
    }

    /// <summary>
    /// Retrieves comments for all database sequences.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of database sequence comments.</returns>
    public async Task<IReadOnlyCollection<IDatabaseSequenceComments>> GetAllSequenceComments2(CancellationToken cancellationToken = default)
    {
        return await Connection.QueryEnumerableAsync<GetAllSequenceComments.Result>(Queries.GetAllSequenceComments.Sql, cancellationToken)
            .Select(commentData =>
            {
                var tmpIdentifier = Identifier.CreateQualifiedIdentifier(commentData.SchemaName, commentData.SequenceName);
                var qualifiedName = QualifySequenceName(tmpIdentifier);

                var sequenceComment = !commentData.Comment.IsNullOrWhiteSpace()
                    ? Option<string>.Some(commentData.Comment)
                    : Option<string>.None;

                return new DatabaseSequenceComments(qualifiedName, sequenceComment);
            })
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Gets the resolved name of the sequence. This enables non-strict name matching to be applied.
    /// </summary>
    /// <param name="sequenceName">A sequence name.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A sequence name that, if available, can be assumed to exist and applied strictly.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="sequenceName"/> is <see langword="null" />.</exception>
    protected OptionAsync<Identifier> GetResolvedSequenceName(Identifier sequenceName, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(sequenceName);

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
    /// <exception cref="ArgumentNullException"><paramref name="sequenceName"/> is <see langword="null" />.</exception>
    protected OptionAsync<Identifier> GetResolvedSequenceNameStrict(Identifier sequenceName, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(sequenceName);

        var candidateSequenceName = QualifySequenceName(sequenceName);
        var qualifiedSequenceName = Connection.QueryFirstOrNone(
            GetSequenceName.Sql,
            new GetSequenceName.Query { SchemaName = candidateSequenceName.Schema!, SequenceName = candidateSequenceName.LocalName },
            cancellationToken
        );

        return qualifiedSequenceName.Map(name => Identifier.CreateQualifiedIdentifier(candidateSequenceName.Server, candidateSequenceName.Database, name.SchemaName, name.SequenceName));
    }

    /// <summary>
    /// Retrieves comments for a particular database sequence.
    /// </summary>
    /// <param name="sequenceName">The name of a database sequence.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>An <see cref="OptionAsync{IDatabaseSequenceComments}" /> instance which holds the value of the sequence's comments, if available.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="sequenceName"/> is <see langword="null" />.</exception>
    public OptionAsync<IDatabaseSequenceComments> GetSequenceComments(Identifier sequenceName, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(sequenceName);

        var candidateSequenceName = QualifySequenceName(sequenceName);
        return LoadSequenceComments(candidateSequenceName, cancellationToken);
    }

    /// <summary>
    /// Retrieves comments for a particular database sequence.
    /// </summary>
    /// <param name="sequenceName">The name of a database sequence.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>An <see cref="OptionAsync{IDatabaseSequenceComments}" /> instance which holds the value of the sequence's comments, if available.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="sequenceName"/> is <see langword="null" />.</exception>
    protected OptionAsync<IDatabaseSequenceComments> LoadSequenceComments(Identifier sequenceName, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(sequenceName);

        var candidateSequenceName = QualifySequenceName(sequenceName);
        return GetResolvedSequenceName(candidateSequenceName, cancellationToken)
            .Bind(name =>
            {
                return Connection.QueryFirstOrNone(
                    Queries.GetSequenceComments.Sql,
                    new GetSequenceComments.Query { SchemaName = name.Schema!, SequenceName = name.LocalName },
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
    /// Qualifies the name of the sequence.
    /// </summary>
    /// <param name="sequenceName">A view name.</param>
    /// <returns>A sequence name is at least as qualified as the given sequence name.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="sequenceName"/> is <see langword="null" />.</exception>
    protected Identifier QualifySequenceName(Identifier sequenceName)
    {
        ArgumentNullException.ThrowIfNull(sequenceName);

        var schema = sequenceName.Schema ?? IdentifierDefaults.Schema;
        return Identifier.CreateQualifiedIdentifier(IdentifierDefaults.Server, IdentifierDefaults.Database, schema, sequenceName.LocalName);
    }
}