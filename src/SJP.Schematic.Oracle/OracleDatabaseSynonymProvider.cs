using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using LanguageExt;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Oracle.Query;

namespace SJP.Schematic.Oracle
{
    public class OracleDatabaseSynonymProvider : IDatabaseSynonymProvider
    {
        public OracleDatabaseSynonymProvider(IDbConnection connection, IIdentifierDefaults identifierDefaults, IIdentifierResolutionStrategy identifierResolver)
        {
            Connection = connection ?? throw new ArgumentNullException(nameof(connection));
            IdentifierDefaults = identifierDefaults ?? throw new ArgumentNullException(nameof(identifierDefaults));
            IdentifierResolver = identifierResolver ?? throw new ArgumentNullException(nameof(identifierResolver));
        }

        protected IDbConnection Connection { get; }

        protected IIdentifierDefaults IdentifierDefaults { get; }

        protected IIdentifierResolutionStrategy IdentifierResolver { get; }

        // collections create directly instead of via LoadSynonym() methods
        // the main reason is to avoid queries where possible, especially when
        // the SYS.ALL_SYNONYMS data dictionary view is very slow

        public async Task<IReadOnlyCollection<IDatabaseSynonym>> GetAllSynonyms(CancellationToken cancellationToken = default(CancellationToken))
        {
            var queryResult = await Connection.QueryAsync<SynonymData>(SynonymsQuery).ConfigureAwait(false);
            var synonymQueryRows = queryResult.ToList();

            var result = new List<IDatabaseSynonym>(synonymQueryRows.Count);
            foreach (var synonymRow in synonymQueryRows)
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

                var synonym = new DatabaseSynonym(qualifiedSynonymName, qualifiedTargetName);
                result.Add(synonym);
            }

            return result;
        }

        protected virtual string SynonymsQuery => SynonymsQuerySql;

        private const string SynonymsQuerySql = @"
select distinct
    s.OWNER as SchemaName,
    s.SYNONYM_NAME as SynonymName,
    s.DB_LINK as TargetDatabaseName,
    s.TABLE_OWNER as TargetSchemaName,
    s.TABLE_NAME as TargetObjectName
from SYS.ALL_SYNONYMS s
inner join SYS.ALL_OBJECTS o on s.OWNER = o.OWNER and s.SYNONYM_NAME = o.OBJECT_NAME
where o.ORACLE_MAINTAINED <> 'Y'
order by s.DB_LINK, s.OWNER, s.SYNONYM_NAME";

        public OptionAsync<IDatabaseSynonym> GetSynonym(Identifier synonymName, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (synonymName == null)
                throw new ArgumentNullException(nameof(synonymName));

            var candidateSynonymName = QualifySynonymName(synonymName);
            return LoadSynonym(candidateSynonymName, cancellationToken);
        }

        public OptionAsync<Identifier> GetResolvedSynonymName(Identifier synonymName, CancellationToken cancellationToken = default(CancellationToken))
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

        protected OptionAsync<Identifier> GetResolvedSynonymNameStrict(Identifier synonymName, CancellationToken cancellationToken)
        {
            if (synonymName == null)
                throw new ArgumentNullException(nameof(synonymName));

            var candidateSynonymName = QualifySynonymName(synonymName);

            // fast path, SYS.ALL_SYNONYMS is much slower than SYS.USER_SYNONYMS so prefer the latter where possible
            var isUserSynonym = candidateSynonymName.Database == IdentifierDefaults.Database && candidateSynonymName.Schema == IdentifierDefaults.Schema;
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

        protected virtual string SynonymNameQuery => SynonymNameQuerySql;

        private const string SynonymNameQuerySql = @"
select s.OWNER as SchemaName, s.SYNONYM_NAME as ObjectName
from SYS.ALL_SYNONYMS s
inner join SYS.ALL_OBJECTS o on s.OWNER = o.OWNER and s.SYNONYM_NAME = o.OBJECT_NAME
where s.OWNER = :SchemaName and s.SYNONYM_NAME = :SynonymName and o.ORACLE_MAINTAINED <> 'Y'";

        protected virtual string UserSynonymNameQuery => UserSynonymNameQuerySql;

        private const string UserSynonymNameQuerySql = @"
select s.SYNONYM_NAME
from SYS.USER_SYNONYMS s
inner join SYS.ALL_OBJECTS o on s.SYNONYM_NAME = o.OBJECT_NAME
where o.OWNER = SYS_CONTEXT('USERENV', 'CURRENT_USER') and s.SYNONYM_NAME = :SynonymName and o.ORACLE_MAINTAINED <> 'Y'";

        protected virtual OptionAsync<IDatabaseSynonym> LoadSynonym(Identifier synonymName, CancellationToken cancellationToken)
        {
            if (synonymName == null)
                throw new ArgumentNullException(nameof(synonymName));

            var candidateSynonymName = QualifySynonymName(synonymName);
            return LoadSynonymAsyncCore(candidateSynonymName, cancellationToken).ToAsync();
        }

        private async Task<Option<IDatabaseSynonym>> LoadSynonymAsyncCore(Identifier synonymName, CancellationToken cancellationToken)
        {
            var resolvedSynonymNameOption = GetResolvedSynonymName(synonymName);
            var resolvedSynonymNameOptionIsNone = await resolvedSynonymNameOption.IsNone.ConfigureAwait(false);
            if (resolvedSynonymNameOptionIsNone)
                return Option<IDatabaseSynonym>.None;

            var resolvedSynonymName = await resolvedSynonymNameOption.UnwrapSomeAsync().ConfigureAwait(false);

            // SYS.ALL_SYNONYMS is much slower than SYS.USER_SYNONYMS so prefer the latter where possible
            var isUserSynonym = resolvedSynonymName.Database == IdentifierDefaults.Database && resolvedSynonymName.Schema == IdentifierDefaults.Schema;
            var synonymData = isUserSynonym
                ? LoadUserSynonymData(resolvedSynonymName.LocalName, cancellationToken)
                : LoadSynonymData(resolvedSynonymName, cancellationToken);

            var result = synonymData.Map(synData => BuildSynonymFromDto(resolvedSynonymName, synData));
            return await result.ToOption().ConfigureAwait(false);
        }

        protected virtual string LoadSynonymQuery => LoadSynonymQuerySql;

        private const string LoadSynonymQuerySql = @"
select distinct
    s.DB_LINK as TargetDatabaseName,
    s.TABLE_OWNER as TargetSchemaName,
    s.TABLE_NAME as TargetObjectName
from SYS.ALL_SYNONYMS s
inner join SYS.ALL_OBJECTS o on s.OWNER = o.OWNER and s.SYNONYM_NAME = o.OBJECT_NAME
where s.OWNER = :SchemaName and s.SYNONYM_NAME = :SynonymName and o.ORACLE_MAINTAINED <> 'Y'";

        protected virtual string LoadUserSynonymQuery => LoadUserSynonymQuerySql;

        private const string LoadUserSynonymQuerySql = @"
select distinct
    s.DB_LINK as TargetDatabaseName,
    s.TABLE_OWNER as TargetSchemaName,
    s.TABLE_NAME as TargetObjectName
from SYS.USER_SYNONYMS s
inner join SYS.ALL_OBJECTS o on s.SYNONYM_NAME = o.OBJECT_NAME
where s.SYNONYM_NAME = :SynonymName and o.OWNER = SYS_CONTEXT('USERENV', 'CURRENT_USER') and o.ORACLE_MAINTAINED <> 'Y'";

        protected virtual OptionAsync<TargetSynonymData> LoadSynonymData(Identifier synonymName, CancellationToken cancellationToken)
        {
            if (synonymName == null)
                throw new ArgumentNullException(nameof(synonymName));

            return Connection.QueryFirstOrNone<TargetSynonymData>(
                LoadSynonymQuery,
                new { SchemaName = synonymName.Schema, SynonymName = synonymName.LocalName },
                cancellationToken
            );
        }

        protected virtual OptionAsync<TargetSynonymData> LoadUserSynonymData(string synonymName, CancellationToken cancellationToken)
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

        protected Identifier QualifySynonymName(Identifier synonymName)
        {
            if (synonymName == null)
                throw new ArgumentNullException(nameof(synonymName));

            var schema = synonymName.Schema ?? IdentifierDefaults.Schema;
            return Identifier.CreateQualifiedIdentifier(IdentifierDefaults.Server, IdentifierDefaults.Database, schema, synonymName.LocalName);
        }

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
