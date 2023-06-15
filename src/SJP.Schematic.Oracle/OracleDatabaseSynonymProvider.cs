using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using LanguageExt;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Oracle.Queries;

namespace SJP.Schematic.Oracle;

/// <summary>
/// A database synonym provider for Oracle databases.
/// </summary>
/// <seealso cref="IDatabaseSynonymProvider" />
public class OracleDatabaseSynonymProvider : IDatabaseSynonymProvider
{
    /// <summary>
    /// Initializes a new instance of the <see cref="OracleDatabaseSynonymProvider"/> class.
    /// </summary>
    /// <param name="connection">A database connection factory.</param>
    /// <param name="identifierDefaults">Database identifier defaults.</param>
    /// <param name="identifierResolver">An identifier resolver.</param>
    /// <exception cref="ArgumentNullException"><paramref name="connection"/> or <paramref name="identifierDefaults"/> or <paramref name="identifierResolver"/> are <c>null</c>.</exception>
    public OracleDatabaseSynonymProvider(IDbConnectionFactory connection, IIdentifierDefaults identifierDefaults, IIdentifierResolutionStrategy identifierResolver)
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
    /// Gets all database synonyms.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A collection of database synonyms.</returns>
    public IAsyncEnumerable<IDatabaseSynonym> GetAllSynonyms(CancellationToken cancellationToken = default)
    {
        // collections created directly instead of via LoadSynonym() methods
        // the main reason is to avoid queries where possible, especially when
        // the SYS.ALL_SYNONYMS data dictionary view is very slow

        return Connection.QueryEnumerableAsync<GetAllSynonyms.Result>(Queries.GetAllSynonyms.Sql, cancellationToken)
            .Select(synonymRow =>
            {
                var synonymSchema = !synonymRow.SchemaName.IsNullOrWhiteSpace() ? synonymRow.SchemaName : null;
                var synonymName = !synonymRow.SynonymName.IsNullOrWhiteSpace() ? synonymRow.SynonymName : null;

                var targetDatabaseName = !synonymRow.TargetDatabaseName.IsNullOrWhiteSpace() ? synonymRow.TargetDatabaseName : null;
                var targetSchemaName = !synonymRow.TargetSchemaName.IsNullOrWhiteSpace() ? synonymRow.TargetSchemaName : null;
                var targetLocalName = !synonymRow.TargetObjectName.IsNullOrWhiteSpace() ? synonymRow.TargetObjectName : null;

                var fullSynonymName = Identifier.CreateQualifiedIdentifier(synonymSchema, synonymName);
                var targetName = Identifier.CreateQualifiedIdentifier(targetDatabaseName, targetSchemaName, targetLocalName);

                var qualifiedSynonymName = QualifySynonymName(fullSynonymName);
                var qualifiedTargetName = QualifySynonymTargetName(targetName);

                return new DatabaseSynonym(qualifiedSynonymName, qualifiedTargetName);
            });
    }

    /// <summary>
    /// Gets a database synonym.
    /// </summary>
    /// <param name="synonymName">A database synonym name.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A database synonym in the 'some' state if found; otherwise 'none'.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="synonymName"/> is <c>null</c>.</exception>
    public OptionAsync<IDatabaseSynonym> GetSynonym(Identifier synonymName, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(synonymName);

        var candidateSynonymName = QualifySynonymName(synonymName);
        return LoadSynonym(candidateSynonymName, cancellationToken);
    }

    /// <summary>
    /// Gets the resolved name of the synonym. This enables non-strict name matching to be applied.
    /// </summary>
    /// <param name="synonymName">A synonym name.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A synonym name that, if available, can be assumed to exist and applied strictly.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="synonymName"/> is <c>null</c>.</exception>
    public OptionAsync<Identifier> GetResolvedSynonymName(Identifier synonymName, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(synonymName);

        var resolvedNames = IdentifierResolver
            .GetResolutionOrder(synonymName)
            .Select(QualifySynonymName);

        return resolvedNames
            .Select(name => GetResolvedSynonymNameStrict(name, cancellationToken))
            .FirstSome(cancellationToken);
    }

    /// <summary>
    /// Gets the resolved synonym name strictly. If there is no match, the synonym does not exist.
    /// </summary>
    /// <param name="synonymName">A synonym name to be strictly matched.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A resolved synonym name, if available.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="synonymName"/> is <c>null</c>.</exception>
    protected OptionAsync<Identifier> GetResolvedSynonymNameStrict(Identifier synonymName, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(synonymName);

        var candidateSynonymName = QualifySynonymName(synonymName);

        // fast path, SYS.ALL_SYNONYMS is much slower than SYS.USER_SYNONYMS so prefer the latter where possible
        var isUserSynonym = string.Equals(candidateSynonymName.Database, IdentifierDefaults.Database, StringComparison.Ordinal)
            && string.Equals(candidateSynonymName.Schema, IdentifierDefaults.Schema, StringComparison.Ordinal);
        if (isUserSynonym)
        {
            var userSynonymName = Connection.QueryFirstOrNone(
                GetUserSynonymName.Sql,
                new GetUserSynonymName.Query { SynonymName = candidateSynonymName.LocalName },
                cancellationToken
            );

            return userSynonymName.Map(name => Identifier.CreateQualifiedIdentifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, name));
        }

        var qualifiedSynonymName = Connection.QueryFirstOrNone(
            GetSynonymName.Sql,
            new GetSynonymName.Query { SchemaName = candidateSynonymName.Schema!, SynonymName = candidateSynonymName.LocalName },
            cancellationToken
        );

        return qualifiedSynonymName.Map(name => Identifier.CreateQualifiedIdentifier(candidateSynonymName.Server, candidateSynonymName.Database, name.SchemaName, name.SynonymName));
    }

    /// <summary>
    /// Retrieves a database synonym, if available.
    /// </summary>
    /// <param name="synonymName">A synonym name.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A synonym definition, if available.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="synonymName"/> is <c>null</c>.</exception>
    protected OptionAsync<IDatabaseSynonym> LoadSynonym(Identifier synonymName, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(synonymName);

        var candidateSynonymName = QualifySynonymName(synonymName);
        return GetResolvedSynonymName(candidateSynonymName, cancellationToken)
            .Bind(name => LoadSynonymAsyncCore(name, cancellationToken));
    }

    private OptionAsync<IDatabaseSynonym> LoadSynonymAsyncCore(Identifier synonymName, CancellationToken cancellationToken)
    {
        // SYS.ALL_SYNONYMS is much slower than SYS.USER_SYNONYMS so prefer the latter where possible
        var isUserSynonym = string.Equals(synonymName.Database, IdentifierDefaults.Database, StringComparison.Ordinal)
            && string.Equals(synonymName.Schema, IdentifierDefaults.Schema, StringComparison.Ordinal);
        return isUserSynonym
            ? LoadUserSynonymData(synonymName.LocalName, cancellationToken)
            : LoadSynonymData(synonymName, cancellationToken);
    }

    private OptionAsync<IDatabaseSynonym> LoadSynonymData(Identifier synonymName, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(synonymName);

        return Connection.QueryFirstOrNone(
            GetSynonymDefinition.Sql,
            new GetSynonymDefinition.Query { SchemaName = synonymName.Schema!, SynonymName = synonymName.LocalName },
            cancellationToken
        ).Map<IDatabaseSynonym>(row =>
        {
            var databaseName = !row.TargetDatabaseName.IsNullOrWhiteSpace() ? row.TargetDatabaseName : null;
            var schemaName = !row.TargetSchemaName.IsNullOrWhiteSpace() ? row.TargetSchemaName : null;
            var localName = !row.TargetObjectName.IsNullOrWhiteSpace() ? row.TargetObjectName : null;

            var qualifiedSynonymName = QualifySynonymName(synonymName);
            var targetName = Identifier.CreateQualifiedIdentifier(databaseName, schemaName, localName);
            var qualifiedTargetName = QualifySynonymTargetName(targetName);

            return new DatabaseSynonym(qualifiedSynonymName, qualifiedTargetName);
        });
    }

    private OptionAsync<IDatabaseSynonym> LoadUserSynonymData(string synonymName, CancellationToken cancellationToken)
    {
        if (synonymName.IsNullOrWhiteSpace())
            throw new ArgumentNullException(nameof(synonymName));

        return Connection.QueryFirstOrNone(
            GetUserSynonymDefinition.Sql,
            new GetUserSynonymDefinition.Query { SynonymName = synonymName },
            cancellationToken
        ).Map<IDatabaseSynonym>(row =>
        {
            var databaseName = !row.TargetDatabaseName.IsNullOrWhiteSpace() ? row.TargetDatabaseName : null;
            var schemaName = !row.TargetSchemaName.IsNullOrWhiteSpace() ? row.TargetSchemaName : null;
            var localName = !row.TargetObjectName.IsNullOrWhiteSpace() ? row.TargetObjectName : null;

            var qualifiedSynonymName = QualifySynonymName(synonymName);
            var targetName = Identifier.CreateQualifiedIdentifier(databaseName, schemaName, localName);
            var qualifiedTargetName = QualifySynonymTargetName(targetName);

            return new DatabaseSynonym(qualifiedSynonymName, qualifiedTargetName);
        });
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

    /// <summary>
    /// Qualifies the name of a synonym's target, using known identifier defaults.
    /// </summary>
    /// <param name="targetName">A synonym's target name to qualify.</param>
    /// <returns>A synonym target name that is at least as qualified as its input.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="targetName"/> is <c>null</c>.</exception>
    private Identifier QualifySynonymTargetName(Identifier targetName)
    {
        ArgumentNullException.ThrowIfNull(targetName);

        var database = targetName.Database ?? IdentifierDefaults.Database;
        var schema = targetName.Schema ?? IdentifierDefaults.Schema;
        return Identifier.CreateQualifiedIdentifier(IdentifierDefaults.Server, database, schema, targetName.LocalName);
    }
}