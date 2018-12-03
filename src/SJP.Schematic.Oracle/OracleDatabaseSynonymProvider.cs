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
using SJP.Schematic.Core.Utilities;
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

        public IReadOnlyCollection<IDatabaseSynonym> Synonyms
        {
            get
            {
                var synonymNames = Connection.Query<QualifiedName>(SynonymsQuery)
                    .Select(dto => Identifier.CreateQualifiedIdentifier(dto.DatabaseName, dto.SchemaName, dto.ObjectName))
                    .ToList();

                var synonyms = synonymNames
                    .Select(LoadSynonymSync)
                    .Somes();
                return new ReadOnlyCollectionSlim<IDatabaseSynonym>(synonymNames.Count, synonyms);
            }
        }

        public async Task<IReadOnlyCollection<IDatabaseSynonym>> SynonymsAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            var queryResult = await Connection.QueryAsync<QualifiedName>(SynonymsQuery).ConfigureAwait(false);
            var synonymNames = queryResult
                .Select(dto => Identifier.CreateQualifiedIdentifier(dto.DatabaseName, dto.SchemaName, dto.ObjectName))
                .ToList();

            var synonyms = await synonymNames
                .Select(name => LoadSynonymAsync(name, cancellationToken))
                .Somes()
                .ConfigureAwait(false);

            return synonyms.ToList();
        }

        protected virtual string SynonymsQuery => SynonymsQuerySql;

        private const string SynonymsQuerySql = @"
select distinct
    s.DB_LINK as DatabaseName,
    s.OWNER as SchemaName,
    s.SYNONYM_NAME as ObjectName
from ALL_SYNONYMS s
inner join ALL_OBJECTS o on s.OWNER = o.OWNER and s.SYNONYM_NAME = o.OBJECT_NAME
where o.ORACLE_MAINTAINED <> 'Y'
order by s.DB_LINK, s.OWNER, s.SYNONYM_NAME";

        public Option<IDatabaseSynonym> GetSynonym(Identifier synonymName)
        {
            if (synonymName == null)
                throw new ArgumentNullException(nameof(synonymName));

            var candidateSynonymName = QualifySynonymName(synonymName);
            return LoadSynonymSync(candidateSynonymName);
        }

        public OptionAsync<IDatabaseSynonym> GetSynonymAsync(Identifier synonymName, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (synonymName == null)
                throw new ArgumentNullException(nameof(synonymName));

            var candidateSynonymName = QualifySynonymName(synonymName);
            return LoadSynonymAsync(candidateSynonymName, cancellationToken);
        }

        public Option<Identifier> GetResolvedSynonymName(Identifier synonymName)
        {
            if (synonymName == null)
                throw new ArgumentNullException(nameof(synonymName));

            var resolvedNames = IdentifierResolver
                .GetResolutionOrder(synonymName)
                .Select(QualifySynonymName);

            return resolvedNames
                .Select(GetResolvedSynonymNameStrict)
                .FirstSome();
        }

        public OptionAsync<Identifier> GetResolvedSynonymNameAsync(Identifier synonymName, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (synonymName == null)
                throw new ArgumentNullException(nameof(synonymName));

            var resolvedNames = IdentifierResolver
                .GetResolutionOrder(synonymName)
                .Select(QualifySynonymName);

            return resolvedNames
                .Select(name => GetResolvedSynonymNameStrictAsync(name, cancellationToken))
                .FirstSomeAsync(cancellationToken);
        }

        protected Option<Identifier> GetResolvedSynonymNameStrict(Identifier synonymName)
        {
            if (synonymName == null)
                throw new ArgumentNullException(nameof(synonymName));

            var candidateSynonymName = QualifySynonymName(synonymName);

            // fast path, ALL_SYNONYMS is much slower than USER_SYNONYMS so prefer the latter where possible
            if (candidateSynonymName.Database == IdentifierDefaults.Database && candidateSynonymName.Schema == IdentifierDefaults.Schema)
            {
                var userSynonymName = Connection.QueryFirstOrNone<string>(
                    UserSynonymNameQuery,
                    new { SynonymName = candidateSynonymName.LocalName }
                );

                return userSynonymName.Map(name => Identifier.CreateQualifiedIdentifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, name));
            }

            var qualifiedSynonymName = Connection.QueryFirstOrNone<QualifiedName>(
                SynonymNameQuery,
                new { SchemaName = candidateSynonymName.Schema, SynonymName = candidateSynonymName.LocalName }
            );

            return qualifiedSynonymName.Map(name => Identifier.CreateQualifiedIdentifier(candidateSynonymName.Server, candidateSynonymName.Database, name.SchemaName, name.ObjectName));
        }

        protected OptionAsync<Identifier> GetResolvedSynonymNameStrictAsync(Identifier synonymName, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (synonymName == null)
                throw new ArgumentNullException(nameof(synonymName));

            var candidateSynonymName = QualifySynonymName(synonymName);

            // fast path, ALL_SYNONYMS is much slower than USER_SYNONYMS so prefer the latter where possible
            if (candidateSynonymName.Database == IdentifierDefaults.Database && candidateSynonymName.Schema == IdentifierDefaults.Schema)
            {
                var userSynonymName = Connection.QueryFirstOrNoneAsync<string>(
                    UserSynonymNameQuery,
                    new { SynonymName = candidateSynonymName.LocalName }
                );

                return userSynonymName.Map(name => Identifier.CreateQualifiedIdentifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, name));
            }

            var qualifiedSynonymName = Connection.QueryFirstOrNoneAsync<QualifiedName>(
                SynonymNameQuery,
                new { SchemaName = candidateSynonymName.Schema, SynonymName = candidateSynonymName.LocalName }
            );

            return qualifiedSynonymName.Map(name => Identifier.CreateQualifiedIdentifier(candidateSynonymName.Server, candidateSynonymName.Database, name.SchemaName, name.ObjectName));
        }

        protected virtual string SynonymNameQuery => SynonymNameQuerySql;

        private const string SynonymNameQuerySql = @"
select s.OWNER as SchemaName, s.SYNONYM_NAME as ObjectName
from ALL_SYNONYMS s
inner join ALL_OBJECTS o on s.OWNER = o.OWNER and s.SYNONYM_NAME = o.OBJECT_NAME
where s.OWNER = :SchemaName and s.SYNONYM_NAME = :SynonymName and o.ORACLE_MAINTAINED <> 'Y'";

        protected virtual string UserSynonymNameQuery => UserSynonymNameQuerySql;

        private const string UserSynonymNameQuerySql = @"
select s.SYNONYM_NAME
from USER_SYNONYMS s
inner join ALL_OBJECTS o on s.SYNONYM_NAME = o.OBJECT_NAME
where o.OWNER = SYS_CONTEXT('USERENV', 'CURRENT_USER') and s.SYNONYM_NAME = :SynonymName and o.ORACLE_MAINTAINED <> 'Y'";

        protected virtual Option<IDatabaseSynonym> LoadSynonymSync(Identifier synonymName)
        {
            if (synonymName == null)
                throw new ArgumentNullException(nameof(synonymName));

            var resolvedSynonymNameOption = GetResolvedSynonymName(synonymName);
            if (resolvedSynonymNameOption.IsNone)
                return Option<IDatabaseSynonym>.None;

            var resolvedSynonymName = resolvedSynonymNameOption.UnwrapSome();

            // ALL_SYNONYMS is much slower than USER_SYNONYMS so prefer the latter where possible
            var isUserSynonym = resolvedSynonymName.Database == IdentifierDefaults.Database && resolvedSynonymName.Schema == IdentifierDefaults.Schema;
            var synonymData = isUserSynonym
                ? LoadUserSynonymDataSync(resolvedSynonymName.LocalName)
                : LoadSynonymDataSync(resolvedSynonymName);

            return synonymData.Map(synData => BuildSynonymFromDto(resolvedSynonymName, synData));
        }

        protected virtual OptionAsync<IDatabaseSynonym> LoadSynonymAsync(Identifier synonymName, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (synonymName == null)
                throw new ArgumentNullException(nameof(synonymName));

            var candidateSynonymName = QualifySynonymName(synonymName);
            return LoadSynonymAsyncCore(candidateSynonymName, cancellationToken).ToAsync();
        }

        private async Task<Option<IDatabaseSynonym>> LoadSynonymAsyncCore(Identifier synonymName, CancellationToken cancellationToken)
        {
            var resolvedSynonymNameOption = GetResolvedSynonymNameAsync(synonymName);
            var resolvedSynonymNameOptionIsNone = await resolvedSynonymNameOption.IsNone.ConfigureAwait(false);
            if (resolvedSynonymNameOptionIsNone)
                return Option<IDatabaseSynonym>.None;

            var resolvedSynonymName = await resolvedSynonymNameOption.UnwrapSomeAsync().ConfigureAwait(false);

            // ALL_SYNONYMS is much slower than USER_SYNONYMS so prefer the latter where possible
            var isUserSynonym = resolvedSynonymName.Database == IdentifierDefaults.Database && resolvedSynonymName.Schema == IdentifierDefaults.Schema;
            var synonymData = isUserSynonym
                ? LoadUserSynonymDataAsync(resolvedSynonymName.LocalName, cancellationToken)
                : LoadSynonymDataAsync(resolvedSynonymName, cancellationToken);

            var result = synonymData.Map(synData => BuildSynonymFromDto(resolvedSynonymName, synData));
            return await result.ToOption().ConfigureAwait(false);
        }

        protected virtual string LoadSynonymQuery => LoadSynonymQuerySql;

        private const string LoadSynonymQuerySql = @"
select distinct
    s.DB_LINK as TargetDatabaseName,
    s.TABLE_OWNER as TargetSchemaName,
    s.TABLE_NAME as TargetObjectName
from ALL_SYNONYMS s
inner join ALL_OBJECTS o on s.OWNER = o.OWNER and s.SYNONYM_NAME = o.OBJECT_NAME
where s.OWNER = :SchemaName and s.SYNONYM_NAME = :SynonymName and o.ORACLE_MAINTAINED <> 'Y'";

        protected virtual string LoadUserSynonymQuery => LoadUserSynonymQuerySql;

        private const string LoadUserSynonymQuerySql = @"
select distinct
    s.DB_LINK as TargetDatabaseName,
    s.TABLE_OWNER as TargetSchemaName,
    s.TABLE_NAME as TargetObjectName
from USER_SYNONYMS s
inner join ALL_OBJECTS o on s.SYNONYM_NAME = o.OBJECT_NAME
where s.SYNONYM_NAME = :SynonymName and o.OWNER = SYS_CONTEXT('USERENV', 'CURRENT_USER') and o.ORACLE_MAINTAINED <> 'Y'";

        protected virtual Option<SynonymData> LoadSynonymDataSync(Identifier synonymName)
        {
            if (synonymName == null)
                throw new ArgumentNullException(nameof(synonymName));

            return Connection.QueryFirstOrNone<SynonymData>(
                LoadSynonymQuery,
                new { SchemaName = synonymName.Schema, SynonymName = synonymName.LocalName }
            );
        }

        protected virtual OptionAsync<SynonymData> LoadSynonymDataAsync(Identifier synonymName, CancellationToken cancellationToken)
        {
            if (synonymName == null)
                throw new ArgumentNullException(nameof(synonymName));

            return Connection.QueryFirstOrNoneAsync<SynonymData>(
                LoadSynonymQuery,
                new { SchemaName = synonymName.Schema, SynonymName = synonymName.LocalName }
            );
        }

        protected virtual Option<SynonymData> LoadUserSynonymDataSync(string synonymName)
        {
            if (synonymName.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(synonymName));

            return Connection.QueryFirstOrNone<SynonymData>(
                LoadUserSynonymQuery,
                new { SynonymName = synonymName }
            );
        }

        protected virtual OptionAsync<SynonymData> LoadUserSynonymDataAsync(string synonymName, CancellationToken cancellationToken)
        {
            if (synonymName.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(synonymName));

            return Connection.QueryFirstOrNoneAsync<SynonymData>(
                LoadUserSynonymQuery,
                new { SynonymName = synonymName }
            );
        }

        private IDatabaseSynonym BuildSynonymFromDto(Identifier synonymName, SynonymData synonymData)
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
