using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using LanguageExt;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.SqlServer.Query;

namespace SJP.Schematic.SqlServer
{
    public class SqlServerDatabaseSynonymProvider : IDatabaseSynonymProvider
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SqlServerDatabaseSynonymProvider"/> class.
        /// </summary>
        /// <param name="connection">A database connection.</param>
        /// <param name="identifierDefaults">Database identifier defaults.</param>
        /// <exception cref="ArgumentNullException"><paramref name="connection"/> or <paramref name="identifierDefaults"/> are <c>null</c>.</exception>
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

        public async IAsyncEnumerable<IDatabaseSynonym> GetAllSynonyms([EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var queryResult = await Connection.QueryAsync<SynonymData>(SynonymsQuery, cancellationToken).ConfigureAwait(false);

            foreach (var row in queryResult)
            {
                var synonymName = QualifySynonymName(Identifier.CreateQualifiedIdentifier(row.SchemaName, row.ObjectName));

                var serverName = !row.TargetServerName.IsNullOrWhiteSpace() ? row.TargetServerName : null;
                var databaseName = !row.TargetDatabaseName.IsNullOrWhiteSpace() ? row.TargetDatabaseName : null;
                var schemaName = !row.TargetSchemaName.IsNullOrWhiteSpace() ? row.TargetSchemaName : null;
                var localName = row.TargetObjectName;

                var targetName = Identifier.CreateQualifiedIdentifier(serverName, databaseName, schemaName, localName);
                var qualifiedTargetName = QualifySynonymTargetName(targetName);

                yield return new DatabaseSynonym(synonymName, qualifiedTargetName);
            }
        }

        /// <summary>
        /// A SQL query that retrieves the definitions of all synonyms in the database.
        /// </summary>
        /// <value>A SQL query.</value>
        protected virtual string SynonymsQuery => SynonymsQuerySql;

        private const string SynonymsQuerySql = @"
select
    schema_name(schema_id) as SchemaName,
    name as ObjectName,
    PARSENAME(base_object_name, 4) as TargetServerName,
    PARSENAME(base_object_name, 3) as TargetDatabaseName,
    PARSENAME(base_object_name, 2) as TargetSchemaName,
    PARSENAME(base_object_name, 1) as TargetObjectName
from sys.synonyms
where is_ms_shipped = 0
order by schema_name(schema_id), name";

        /// <summary>
        /// Gets a database synonym.
        /// </summary>
        /// <param name="synonymName">A database synonym name.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A database synonym in the 'some' state if found; otherwise 'none'.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="synonymName"/> is <c>null</c>.</exception>
        public OptionAsync<IDatabaseSynonym> GetSynonym(Identifier synonymName, CancellationToken cancellationToken = default)
        {
            if (synonymName == null)
                throw new ArgumentNullException(nameof(synonymName));

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
        protected OptionAsync<Identifier> GetResolvedSynonymName(Identifier synonymName, CancellationToken cancellationToken)
        {
            if (synonymName == null)
                throw new ArgumentNullException(nameof(synonymName));

            var candidateSynonymName = QualifySynonymName(synonymName);
            var qualifiedSynonymName = Connection.QueryFirstOrNone<QualifiedName>(
                SynonymNameQuery,
                new { SchemaName = candidateSynonymName.Schema, SynonymName = candidateSynonymName.LocalName },
                cancellationToken
            );

            return qualifiedSynonymName.Map(name => Identifier.CreateQualifiedIdentifier(candidateSynonymName.Server, candidateSynonymName.Database, name.SchemaName, name.ObjectName));
        }

        protected virtual string SynonymNameQuery => SynonymNameQuerySql;

        private const string SynonymNameQuerySql = @"
select top 1 schema_name(schema_id) as SchemaName, name as ObjectName
from sys.synonyms
where schema_id = schema_id(@SchemaName) and name = @SynonymName and is_ms_shipped = 0";

        /// <summary>
        /// Retrieves a database synonym, if available.
        /// </summary>
        /// <param name="synonymName">A synonym name.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A synonym definition, if available.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="synonymName"/> is <c>null</c>.</exception>
        protected virtual OptionAsync<IDatabaseSynonym> LoadSynonym(Identifier synonymName, CancellationToken cancellationToken)
        {
            if (synonymName == null)
                throw new ArgumentNullException(nameof(synonymName));

            var candidateSynonymName = QualifySynonymName(synonymName);
            return GetResolvedSynonymName(candidateSynonymName, cancellationToken)
                .Bind(name =>
                {
                    return Connection.QueryFirstOrNone<SynonymData>(
                        LoadSynonymQuery,
                        new { SchemaName = synonymName.Schema, SynonymName = synonymName.LocalName },
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
        /// A SQL query that retrieves a synonym's definition.
        /// </summary>
        /// <value>A SQL query.</value>
        protected virtual string LoadSynonymQuery => LoadSynonymQuerySql;

        private const string LoadSynonymQuerySql = @"
select
    PARSENAME(base_object_name, 4) as TargetServerName,
    PARSENAME(base_object_name, 3) as TargetDatabaseName,
    PARSENAME(base_object_name, 2) as TargetSchemaName,
    PARSENAME(base_object_name, 1) as TargetObjectName
from sys.synonyms
where schema_id = schema_id(@SchemaName) and name = @SynonymName and is_ms_shipped = 0";

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

        /// <summary>
        /// Qualifies the name of a synonym's target, using known identifier defaults.
        /// </summary>
        /// <param name="targetName">A synonym's target name to qualify.</param>
        /// <returns>A synonym target name that is at least as qualified as its input.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="targetName"/> is <c>null</c>.</exception>
        protected Identifier QualifySynonymTargetName(Identifier targetName)
        {
            if (targetName == null)
                throw new ArgumentNullException(nameof(targetName));

            var database = targetName.Database ?? IdentifierDefaults.Database;
            var schema = targetName.Schema ?? IdentifierDefaults.Schema;
            return Identifier.CreateQualifiedIdentifier(IdentifierDefaults.Server, database, schema, targetName.LocalName);
        }
    }
}
