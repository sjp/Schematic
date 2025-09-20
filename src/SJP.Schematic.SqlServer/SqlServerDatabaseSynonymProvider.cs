using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LanguageExt;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.SqlServer.Queries;

namespace SJP.Schematic.SqlServer;

/// <summary>
/// A database synonym provider for SQL Server.
/// </summary>
/// <seealso cref="IDatabaseSynonymProvider" />
public class SqlServerDatabaseSynonymProvider : IDatabaseSynonymProvider
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SqlServerDatabaseSynonymProvider"/> class.
    /// </summary>
    /// <param name="connection">A database connection.</param>
    /// <param name="identifierDefaults">Database identifier defaults.</param>
    /// <exception cref="ArgumentNullException"><paramref name="connection"/> or <paramref name="identifierDefaults"/> are <see langword="null" />.</exception>
    public SqlServerDatabaseSynonymProvider(IDbConnectionFactory connection, IIdentifierDefaults identifierDefaults)
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
    /// Enumerates all database synonyms.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A collection of database synonyms.</returns>
    public IAsyncEnumerable<IDatabaseSynonym> GetAllSynonyms(CancellationToken cancellationToken = default)
    {
        return Connection.QueryEnumerableAsync<GetAllSynonymDefinitions.Result>(GetAllSynonymDefinitions.Sql, cancellationToken)
            .Select(row =>
            {
                var synonymName = QualifySynonymName(Identifier.CreateQualifiedIdentifier(row.SchemaName, row.SynonymName));

                var serverName = !row.TargetServerName.IsNullOrWhiteSpace() ? row.TargetServerName : null;
                var databaseName = !row.TargetDatabaseName.IsNullOrWhiteSpace() ? row.TargetDatabaseName : null;
                var schemaName = !row.TargetSchemaName.IsNullOrWhiteSpace() ? row.TargetSchemaName : null;
                var localName = row.TargetObjectName;

                var targetName = Identifier.CreateQualifiedIdentifier(serverName, databaseName, schemaName, localName);
                var qualifiedTargetName = QualifySynonymTargetName(targetName);

                return new DatabaseSynonym(synonymName, qualifiedTargetName);
            });
    }

    /// <summary>
    /// Gets all database synonyms.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A collection of database synonyms.</returns>
    public async Task<IReadOnlyCollection<IDatabaseSynonym>> GetAllSynonyms2(CancellationToken cancellationToken = default)
    {
        return await Connection.QueryEnumerableAsync<GetAllSynonymDefinitions.Result>(GetAllSynonymDefinitions.Sql, cancellationToken)
            .Select(row =>
            {
                var synonymName = QualifySynonymName(Identifier.CreateQualifiedIdentifier(row.SchemaName, row.SynonymName));

                var serverName = !row.TargetServerName.IsNullOrWhiteSpace() ? row.TargetServerName : null;
                var databaseName = !row.TargetDatabaseName.IsNullOrWhiteSpace() ? row.TargetDatabaseName : null;
                var schemaName = !row.TargetSchemaName.IsNullOrWhiteSpace() ? row.TargetSchemaName : null;
                var localName = row.TargetObjectName;

                var targetName = Identifier.CreateQualifiedIdentifier(serverName, databaseName, schemaName, localName);
                var qualifiedTargetName = QualifySynonymTargetName(targetName);

                return new DatabaseSynonym(synonymName, qualifiedTargetName);
            })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Gets a database synonym.
    /// </summary>
    /// <param name="synonymName">A database synonym name.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A database synonym in the 'some' state if found; otherwise 'none'.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="synonymName"/> is <see langword="null" />.</exception>
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
    /// <exception cref="ArgumentNullException"><paramref name="synonymName"/> is <see langword="null" />.</exception>
    protected OptionAsync<Identifier> GetResolvedSynonymName(Identifier synonymName, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(synonymName);

        var candidateSynonymName = QualifySynonymName(synonymName);
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
    /// <exception cref="ArgumentNullException"><paramref name="synonymName"/> is <see langword="null" />.</exception>
    protected OptionAsync<IDatabaseSynonym> LoadSynonym(Identifier synonymName, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(synonymName);

        var candidateSynonymName = QualifySynonymName(synonymName);
        return GetResolvedSynonymName(candidateSynonymName, cancellationToken)
            .Bind(name =>
            {
                return Connection.QueryFirstOrNone(
                    GetSynonymDefinition.Sql,
                    new GetSynonymDefinition.Query { SchemaName = synonymName.Schema!, SynonymName = synonymName.LocalName },
                    cancellationToken
                ).Map<IDatabaseSynonym>(synonymData =>
                {
                    var serverName = !synonymData.TargetServerName.IsNullOrWhiteSpace() ? synonymData.TargetServerName : null;
                    var databaseName = !synonymData.TargetDatabaseName.IsNullOrWhiteSpace() ? synonymData.TargetDatabaseName : null;
                    var schemaName = !synonymData.TargetSchemaName.IsNullOrWhiteSpace() ? synonymData.TargetSchemaName : null;
                    var localName = synonymData.TargetObjectName;

                    var targetName = Identifier.CreateQualifiedIdentifier(serverName, databaseName, schemaName, localName);
                    var qualifiedTargetName = QualifySynonymTargetName(targetName);

                    return new DatabaseSynonym(name, qualifiedTargetName);
                });
            });
    }

    /// <summary>
    /// Qualifies the name of a synonym, using known identifier defaults.
    /// </summary>
    /// <param name="synonymName">A synonym name to qualify.</param>
    /// <returns>A synonym name that is at least as qualified as its input.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="synonymName"/> is <see langword="null" />.</exception>
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
    /// <exception cref="ArgumentNullException"><paramref name="targetName"/> is <see langword="null" />.</exception>
    protected Identifier QualifySynonymTargetName(Identifier targetName)
    {
        ArgumentNullException.ThrowIfNull(targetName);

        var database = targetName.Database ?? IdentifierDefaults.Database;
        var schema = targetName.Schema ?? IdentifierDefaults.Schema;
        return Identifier.CreateQualifiedIdentifier(IdentifierDefaults.Server, database, schema, targetName.LocalName);
    }
}