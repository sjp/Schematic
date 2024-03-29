﻿using System;
using System.Collections.Generic;
using System.Threading;
using LanguageExt;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Comments;

namespace SJP.Schematic.SqlServer.Comments;

/// <summary>
/// A comment provider for SQL Server database objects.
/// </summary>
/// <seealso cref="IRelationalDatabaseCommentProvider" />
public class SqlServerDatabaseCommentProvider : IRelationalDatabaseCommentProvider
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SqlServerDatabaseCommentProvider"/> class.
    /// </summary>
    /// <param name="connection">A database connection.</param>
    /// <param name="identifierDefaults">Identifier defaults for the associated database.</param>
    /// <exception cref="ArgumentNullException"><paramref name="connection"/> or <paramref name="identifierDefaults"/> is <c>null</c>.</exception>
    public SqlServerDatabaseCommentProvider(IDbConnectionFactory connection, IIdentifierDefaults identifierDefaults)
    {
        ArgumentNullException.ThrowIfNull(connection);

        IdentifierDefaults = identifierDefaults ?? throw new ArgumentNullException(nameof(identifierDefaults));
        _tableCommentProvider = new SqlServerTableCommentProvider(connection, identifierDefaults);
        _viewCommentProvider = new SqlServerViewCommentProvider(connection, identifierDefaults);
        _sequenceCommentProvider = new SqlServerSequenceCommentProvider(connection, identifierDefaults);
        _synonymCommentProvider = new SqlServerSynonymCommentProvider(connection, identifierDefaults);
        _routineCommentProvider = new SqlServerRoutineCommentProvider(connection, identifierDefaults);
    }

    /// <summary>
    /// Default values for identifiers in a database.
    /// </summary>
    /// <value>Identifier defaults.</value>
    public IIdentifierDefaults IdentifierDefaults { get; }

    /// <summary>
    /// Retrieves comments for a database table, if available.
    /// </summary>
    /// <param name="tableName">A table name.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Comments for the given database table, if available.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="tableName"/> is <c>null</c>.</exception>
    public OptionAsync<IRelationalDatabaseTableComments> GetTableComments(Identifier tableName, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(tableName);

        return _tableCommentProvider.GetTableComments(tableName, cancellationToken);
    }

    /// <summary>
    /// Retrieves comments for all database tables.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of database table comments, where available.</returns>
    public IAsyncEnumerable<IRelationalDatabaseTableComments> GetAllTableComments(CancellationToken cancellationToken = default)
    {
        return _tableCommentProvider.GetAllTableComments(cancellationToken);
    }

    /// <summary>
    /// Retrieves comments for a particular database view.
    /// </summary>
    /// <param name="viewName">The name of a database view.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>An <see cref="OptionAsync{A}" /> instance which holds the value of the view's comments, if available.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="viewName"/> is <c>null</c>.</exception>
    public OptionAsync<IDatabaseViewComments> GetViewComments(Identifier viewName, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(viewName);

        return _viewCommentProvider.GetViewComments(viewName, cancellationToken);
    }

    /// <summary>
    /// Retrieves all database view comments defined within a database.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of view comments.</returns>
    public IAsyncEnumerable<IDatabaseViewComments> GetAllViewComments(CancellationToken cancellationToken = default)
    {
        return _viewCommentProvider.GetAllViewComments(cancellationToken);
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
        ArgumentNullException.ThrowIfNull(sequenceName);

        return _sequenceCommentProvider.GetSequenceComments(sequenceName, cancellationToken);
    }

    /// <summary>
    /// Retrieves comments for all database sequences.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of database sequence comments.</returns>
    public IAsyncEnumerable<IDatabaseSequenceComments> GetAllSequenceComments(CancellationToken cancellationToken = default)
    {
        return _sequenceCommentProvider.GetAllSequenceComments(cancellationToken);
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

        return _synonymCommentProvider.GetSynonymComments(synonymName, cancellationToken);
    }

    /// <summary>
    /// Retrieves comments for all database synonyms.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of database synonyms comments.</returns>
    public IAsyncEnumerable<IDatabaseSynonymComments> GetAllSynonymComments(CancellationToken cancellationToken = default)
    {
        return _synonymCommentProvider.GetAllSynonymComments(cancellationToken);
    }

    /// <summary>
    /// Retrieves comments for a database routine, if available.
    /// </summary>
    /// <param name="routineName">A routine name.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Comments for the given database routine, if available.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="routineName"/> is <c>null</c>.</exception>
    public OptionAsync<IDatabaseRoutineComments> GetRoutineComments(Identifier routineName, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(routineName);

        return _routineCommentProvider.GetRoutineComments(routineName, cancellationToken);
    }

    /// <summary>
    /// Retrieves all database routine comments defined within a database.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of routine comments.</returns>
    public IAsyncEnumerable<IDatabaseRoutineComments> GetAllRoutineComments(CancellationToken cancellationToken = default)
    {
        return _routineCommentProvider.GetAllRoutineComments(cancellationToken);
    }

    private readonly IRelationalDatabaseTableCommentProvider _tableCommentProvider;
    private readonly IDatabaseViewCommentProvider _viewCommentProvider;
    private readonly IDatabaseSequenceCommentProvider _sequenceCommentProvider;
    private readonly IDatabaseSynonymCommentProvider _synonymCommentProvider;
    private readonly IDatabaseRoutineCommentProvider _routineCommentProvider;
}