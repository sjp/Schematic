using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using LanguageExt;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Oracle.Query;

namespace SJP.Schematic.Oracle
{
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
        public async IAsyncEnumerable<IDatabaseSynonym> GetAllSynonyms([EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            // collections create directly instead of via LoadSynonym() methods
            // the main reason is to avoid queries where possible, especially when
            // the SYS.ALL_SYNONYMS data dictionary view is very slow

            var queryResult = await Connection.QueryAsync<SynonymData>(SynonymsQuery, cancellationToken).ConfigureAwait(false);

            foreach (var synonymRow in queryResult)
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

                yield return new DatabaseSynonym(qualifiedSynonymName, qualifiedTargetName);
            }
        }

        /// <summary>
        /// A SQL query that retrieves the definitions of all synonyms in the database.
        /// </summary>
        /// <value>A SQL query.</value>
        protected virtual string SynonymsQuery => SynonymsQuerySql;

        private static readonly string SynonymsQuerySql = @$"
select distinct
    s.OWNER as ""{ nameof(SynonymData.SchemaName) }"",
    s.SYNONYM_NAME as ""{ nameof(SynonymData.SynonymName) }"",
    s.DB_LINK as ""{ nameof(SynonymData.TargetDatabaseName) }"",
    s.TABLE_OWNER as ""{ nameof(SynonymData.TargetSchemaName) }"",
    s.TABLE_NAME as ""{ nameof(SynonymData.TargetObjectName) }""
from SYS.ALL_SYNONYMS s
inner join SYS.ALL_OBJECTS o on s.OWNER = o.OWNER and s.SYNONYM_NAME = o.OBJECT_NAME
where o.ORACLE_MAINTAINED <> 'Y'
order by s.DB_LINK, s.OWNER, s.SYNONYM_NAME";

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
        public OptionAsync<Identifier> GetResolvedSynonymName(Identifier synonymName, CancellationToken cancellationToken = default)
        {
            if (synonymName == null)
                throw new ArgumentNullException(nameof(synonymName));

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
            if (synonymName == null)
                throw new ArgumentNullException(nameof(synonymName));

            var candidateSynonymName = QualifySynonymName(synonymName);

            // fast path, SYS.ALL_SYNONYMS is much slower than SYS.USER_SYNONYMS so prefer the latter where possible
            var isUserSynonym = string.Equals(candidateSynonymName.Database, IdentifierDefaults.Database, StringComparison.Ordinal)
                && string.Equals(candidateSynonymName.Schema, IdentifierDefaults.Schema, StringComparison.Ordinal);
            if (isUserSynonym)
            {
                var userSynonymName = Connection.QueryFirstOrNone<string>(
                    UserSynonymNameQuery,
                    new { SynonymName = candidateSynonymName.LocalName },
                    cancellationToken
                );

                return userSynonymName.Map(name => Identifier.CreateQualifiedIdentifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, name));
            }

            var qualifiedSynonymName = Connection.QueryFirstOrNone<QualifiedName>(
                SynonymNameQuery,
                new { SchemaName = candidateSynonymName.Schema, SynonymName = candidateSynonymName.LocalName },
                cancellationToken
            );

            return qualifiedSynonymName.Map(name => Identifier.CreateQualifiedIdentifier(candidateSynonymName.Server, candidateSynonymName.Database, name.SchemaName, name.ObjectName));
        }

        /// <summary>
        /// Gets a query that retrieves a synonym's name from all visible schemas.
        /// </summary>
        /// <value>A SQL query.</value>
        protected virtual string SynonymNameQuery => SynonymNameQuerySql;

        private static readonly string SynonymNameQuerySql = @$"
select s.OWNER as ""{ nameof(QualifiedName.SchemaName) }"", s.SYNONYM_NAME as ""{ nameof(QualifiedName.ObjectName) }""
from SYS.ALL_SYNONYMS s
inner join SYS.ALL_OBJECTS o on s.OWNER = o.OWNER and s.SYNONYM_NAME = o.OBJECT_NAME
where s.OWNER = :SchemaName and s.SYNONYM_NAME = :SynonymName and o.ORACLE_MAINTAINED <> 'Y'";

        /// <summary>
        /// Gets a query that retrieves a synonym's name from the user's schema.
        /// </summary>
        /// <value>A SQL query.</value>
        protected virtual string UserSynonymNameQuery => UserSynonymNameQuerySql;

        private const string UserSynonymNameQuerySql = @"
select s.SYNONYM_NAME
from SYS.USER_SYNONYMS s
inner join SYS.ALL_OBJECTS o on s.SYNONYM_NAME = o.OBJECT_NAME
where o.OWNER = SYS_CONTEXT('USERENV', 'CURRENT_USER') and s.SYNONYM_NAME = :SynonymName and o.ORACLE_MAINTAINED <> 'Y'";

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
            return GetResolvedSynonymName(candidateSynonymName)
                .Bind(name => LoadSynonymAsyncCore(name, cancellationToken));
        }

        private OptionAsync<IDatabaseSynonym> LoadSynonymAsyncCore(Identifier synonymName, CancellationToken cancellationToken)
        {
            // SYS.ALL_SYNONYMS is much slower than SYS.USER_SYNONYMS so prefer the latter where possible
            var isUserSynonym = string.Equals(synonymName.Database, IdentifierDefaults.Database, StringComparison.Ordinal)
                && string.Equals(synonymName.Schema, IdentifierDefaults.Schema, StringComparison.Ordinal);
            var synonymData = isUserSynonym
                ? LoadUserSynonymData(synonymName.LocalName, cancellationToken)
                : LoadSynonymData(synonymName, cancellationToken);

            return synonymData.Map(synData => BuildSynonymFromDto(synonymName, synData));
        }

        /// <summary>
        /// A SQL query that retrieves a synonym's definition.
        /// </summary>
        /// <value>A SQL query.</value>
        protected virtual string LoadSynonymQuery => LoadSynonymQuerySql;

        private static readonly string LoadSynonymQuerySql = @$"
select distinct
    s.DB_LINK as ""{ nameof(TargetSynonymData.TargetDatabaseName) }"",
    s.TABLE_OWNER as ""{ nameof(TargetSynonymData.TargetSchemaName) }"",
    s.TABLE_NAME as ""{ nameof(TargetSynonymData.TargetObjectName) }""
from SYS.ALL_SYNONYMS s
inner join SYS.ALL_OBJECTS o on s.OWNER = o.OWNER and s.SYNONYM_NAME = o.OBJECT_NAME
where s.OWNER = :SchemaName and s.SYNONYM_NAME = :SynonymName and o.ORACLE_MAINTAINED <> 'Y'";

        /// <summary>
        /// A SQL query that retrieves a synonym's definition for the default/user schema.
        /// </summary>
        /// <value>A SQL query.</value>
        protected virtual string LoadUserSynonymQuery => LoadUserSynonymQuerySql;

        private static readonly string LoadUserSynonymQuerySql = @$"
select distinct
    s.DB_LINK as ""{ nameof(TargetSynonymData.TargetDatabaseName) }"",
    s.TABLE_OWNER as ""{ nameof(TargetSynonymData.TargetSchemaName) }"",
    s.TABLE_NAME as ""{ nameof(TargetSynonymData.TargetObjectName) }""
from SYS.USER_SYNONYMS s
inner join SYS.ALL_OBJECTS o on s.SYNONYM_NAME = o.OBJECT_NAME
where s.SYNONYM_NAME = :SynonymName and o.OWNER = SYS_CONTEXT('USERENV', 'CURRENT_USER') and o.ORACLE_MAINTAINED <> 'Y'";

        private OptionAsync<TargetSynonymData> LoadSynonymData(Identifier synonymName, CancellationToken cancellationToken)
        {
            if (synonymName == null)
                throw new ArgumentNullException(nameof(synonymName));

            return Connection.QueryFirstOrNone<TargetSynonymData>(
                LoadSynonymQuery,
                new { SchemaName = synonymName.Schema, SynonymName = synonymName.LocalName },
                cancellationToken
            );
        }

        private OptionAsync<TargetSynonymData> LoadUserSynonymData(string synonymName, CancellationToken cancellationToken)
        {
            if (synonymName.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(synonymName));

            return Connection.QueryFirstOrNone<TargetSynonymData>(
                LoadUserSynonymQuery,
                new { SynonymName = synonymName },
                cancellationToken
            );
        }

        private IDatabaseSynonym BuildSynonymFromDto(Identifier synonymName, TargetSynonymData synonymData)
        {
            if (synonymName == null)
                throw new ArgumentNullException(nameof(synonymName));
            if (synonymData == null)
                throw new ArgumentNullException(nameof(synonymData));

            var databaseName = !synonymData.TargetDatabaseName.IsNullOrWhiteSpace() ? synonymData.TargetDatabaseName : null;
            var schemaName = !synonymData.TargetSchemaName.IsNullOrWhiteSpace() ? synonymData.TargetSchemaName : null;
            var localName = !synonymData.TargetObjectName.IsNullOrWhiteSpace() ? synonymData.TargetObjectName : null;

            var qualifiedSynonymName = QualifySynonymName(synonymName);
            var targetName = Identifier.CreateQualifiedIdentifier(databaseName, schemaName, localName);
            var qualifiedTargetName = QualifySynonymTargetName(targetName);

            return new DatabaseSynonym(qualifiedSynonymName, qualifiedTargetName);
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

        /// <summary>
        /// Qualifies the name of a synonym's target, using known identifier defaults.
        /// </summary>
        /// <param name="targetName">A synonym's target name to qualify.</param>
        /// <returns>A synonym target name that is at least as qualified as its input.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="targetName"/> is <c>null</c>.</exception>
        private Identifier QualifySynonymTargetName(Identifier targetName)
        {
            if (targetName == null)
                throw new ArgumentNullException(nameof(targetName));

            var database = targetName.Database ?? IdentifierDefaults.Database;
            var schema = targetName.Schema ?? IdentifierDefaults.Schema;
            return Identifier.CreateQualifiedIdentifier(IdentifierDefaults.Server, database, schema, targetName.LocalName);
        }
    }
}
