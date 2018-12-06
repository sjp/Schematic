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
using SJP.Schematic.SqlServer.Query;

namespace SJP.Schematic.SqlServer
{
    public class SqlServerDatabaseSynonymProvider : IDatabaseSynonymProvider
    {
        public SqlServerDatabaseSynonymProvider(IDbConnection connection, IIdentifierDefaults identifierDefaults)
        {
            Connection = connection ?? throw new ArgumentNullException(nameof(connection));
            IdentifierDefaults = identifierDefaults ?? throw new ArgumentNullException(nameof(identifierDefaults));
        }

        protected IDbConnection Connection { get; }

        protected IIdentifierDefaults IdentifierDefaults { get; }

        public IReadOnlyCollection<IDatabaseSynonym> Synonyms
        {
            get
            {
                var synonymNames = Connection.Query<QualifiedName>(SynonymsQuery)
                    .Select(dto => Identifier.CreateQualifiedIdentifier(dto.SchemaName, dto.ObjectName))
                    .ToList();

                var synonyms = synonymNames
                    .Select(LoadSynonymSync)
                    .Somes();
                return new ReadOnlyCollectionSlim<IDatabaseSynonym>(synonymNames.Count, synonyms);
            }
        }

        public async Task<IReadOnlyCollection<IDatabaseSynonym>> SynonymsAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            var queryResult = await Connection.QueryAsync<QualifiedName>(SynonymsQuery, cancellationToken).ConfigureAwait(false);
            var synonymNames = queryResult
                .Select(dto => Identifier.CreateQualifiedIdentifier(dto.SchemaName, dto.ObjectName))
                .ToList();

            var synonyms = await synonymNames
                .Select(name => LoadSynonymAsync(name, cancellationToken))
                .Somes()
                .ConfigureAwait(false);

            return synonyms.ToList();
        }

        protected virtual string SynonymsQuery => SynonymsQuerySql;

        private const string SynonymsQuerySql = "select schema_name(schema_id) as SchemaName, name as ObjectName from sys.synonyms order by schema_name(schema_id), name";

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

        protected Option<Identifier> GetResolvedSynonymName(Identifier synonymName)
        {
            if (synonymName == null)
                throw new ArgumentNullException(nameof(synonymName));

            var candidateSynonymName = QualifySynonymName(synonymName);
            var qualifiedSynonymName = Connection.QueryFirstOrNone<QualifiedName>(
                SynonymNameQuery,
                new { SchemaName = candidateSynonymName.Schema, SynonymName = candidateSynonymName.LocalName }
            );

            return qualifiedSynonymName.Map(name => Identifier.CreateQualifiedIdentifier(candidateSynonymName.Server, candidateSynonymName.Database, name.SchemaName, name.ObjectName));
        }

        protected OptionAsync<Identifier> GetResolvedSynonymNameAsync(Identifier synonymName, CancellationToken cancellationToken)
        {
            if (synonymName == null)
                throw new ArgumentNullException(nameof(synonymName));

            var candidateSynonymName = QualifySynonymName(synonymName);
            var qualifiedSynonymName = Connection.QueryFirstOrNoneAsync<QualifiedName>(
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
where schema_id = schema_id(@SchemaName) and name = @SynonymName";

        protected virtual Option<IDatabaseSynonym> LoadSynonymSync(Identifier synonymName)
        {
            if (synonymName == null)
                throw new ArgumentNullException(nameof(synonymName));

            var candidateSynonymName = QualifySynonymName(synonymName);

            return GetResolvedSynonymName(candidateSynonymName)
                .Bind(name => Connection.QueryFirstOrNone<SynonymData>(
                    LoadSynonymQuery,
                    new { SchemaName = name.Schema, SynonymName = name.LocalName }
                ))
                .Map<IDatabaseSynonym>(synonymData =>
                {
                    var serverName = !synonymData.TargetServerName.IsNullOrWhiteSpace() ? synonymData.TargetServerName : null;
                    var databaseName = !synonymData.TargetDatabaseName.IsNullOrWhiteSpace() ? synonymData.TargetDatabaseName : null;
                    var schemaName = !synonymData.TargetSchemaName.IsNullOrWhiteSpace() ? synonymData.TargetSchemaName : null;
                    var localName = !synonymData.TargetObjectName.IsNullOrWhiteSpace() ? synonymData.TargetObjectName : null;

                    var targetName = Identifier.CreateQualifiedIdentifier(serverName, databaseName, schemaName, localName);
                    var qualifiedTargetName = QualifySynonymTargetName(targetName);

                    return new DatabaseSynonym(candidateSynonymName, qualifiedTargetName);
                });
        }

        protected virtual OptionAsync<IDatabaseSynonym> LoadSynonymAsync(Identifier synonymName, CancellationToken cancellationToken)
        {
            if (synonymName == null)
                throw new ArgumentNullException(nameof(synonymName));

            var candidateSynonymName = QualifySynonymName(synonymName);
            return LoadSynonymAsyncCore(candidateSynonymName, cancellationToken).ToAsync();
        }

        private async Task<Option<IDatabaseSynonym>> LoadSynonymAsyncCore(Identifier synonymName, CancellationToken cancellationToken)
        {
            var candidateSynonymName = QualifySynonymName(synonymName);
            var resolvedSynonymNameOption = GetResolvedSynonymNameAsync(candidateSynonymName, cancellationToken);
            var resolvedSynonymNameOptionIsNone = await resolvedSynonymNameOption.IsNone.ConfigureAwait(false);
            if (resolvedSynonymNameOptionIsNone)
                return Option<IDatabaseSynonym>.None;

            var resolvedSynonymName = await resolvedSynonymNameOption.UnwrapSomeAsync().ConfigureAwait(false);
            var queryResult = Connection.QueryFirstOrNoneAsync<SynonymData>(
                LoadSynonymQuery,
                new { SchemaName = resolvedSynonymName.Schema, SynonymName = resolvedSynonymName.LocalName },
                cancellationToken
            );

            var synonymOption = queryResult.Map<IDatabaseSynonym>(synonymData =>
            {
                var serverName = !synonymData.TargetServerName.IsNullOrWhiteSpace() ? synonymData.TargetServerName : null;
                var databaseName = !synonymData.TargetDatabaseName.IsNullOrWhiteSpace() ? synonymData.TargetDatabaseName : null;
                var schemaName = !synonymData.TargetSchemaName.IsNullOrWhiteSpace() ? synonymData.TargetSchemaName : null;
                var localName = !synonymData.TargetObjectName.IsNullOrWhiteSpace() ? synonymData.TargetObjectName : null;

                var targetName = Identifier.CreateQualifiedIdentifier(serverName, databaseName, schemaName, localName);
                var qualifiedTargetName = QualifySynonymTargetName(targetName);

                return new DatabaseSynonym(resolvedSynonymName, qualifiedTargetName);
            });

            return await synonymOption.ToOption().ConfigureAwait(false);
        }

        protected virtual string LoadSynonymQuery => LoadSynonymQuerySql;

        private const string LoadSynonymQuerySql = @"
select
    PARSENAME(base_object_name, 4) as TargetServerName,
    PARSENAME(base_object_name, 3) as TargetDatabaseName,
    PARSENAME(base_object_name, 2) as TargetSchemaName,
    PARSENAME(base_object_name, 1) as TargetObjectName
from sys.synonyms
where schema_id = schema_id(@SchemaName) and name = @SynonymName";

        protected Identifier QualifySynonymName(Identifier synonymName)
        {
            if (synonymName == null)
                throw new ArgumentNullException(nameof(synonymName));

            var schema = synonymName.Schema ?? IdentifierDefaults.Schema;
            return Identifier.CreateQualifiedIdentifier(IdentifierDefaults.Server, IdentifierDefaults.Database, schema, synonymName.LocalName);
        }

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
