using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SJP.Schematic.Core;
using SJP.Schematic.Oracle.Query;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Core.Utilities;

namespace SJP.Schematic.Oracle
{
    public class OracleRelationalDatabase : RelationalDatabase, IRelationalDatabase
    {
        public OracleRelationalDatabase(IDatabaseDialect dialect, IDbConnection connection, IIdentifierResolutionStrategy identifierResolver = null)
            : base(dialect, connection)
        {
            _metadata = new AsyncLazy<DatabaseMetadata>(LoadDatabaseMetadataAsync);
            IdentifierResolver = identifierResolver ?? new DefaultOracleIdentifierResolutionStrategy();
        }

        protected IIdentifierResolutionStrategy IdentifierResolver { get; }

        public string ServerName => Metadata.ServerName;

        public string DatabaseName => Metadata.DatabaseName;

        public string DefaultSchema => Metadata.DefaultSchema;

        public string DatabaseVersion => Metadata.DatabaseVersion;

        protected DatabaseMetadata Metadata => _metadata.Task.GetAwaiter().GetResult();

        protected Identifier GetResolvedTableName(Identifier tableName)
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));

            return ResolveFirstExistingObjectName(tableName, GetResolvedTableNameStrict);
        }

        protected Task<Identifier> GetResolvedTableNameAsync(Identifier tableName, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));

            return GetResolvedTableNameAsyncCore(tableName, cancellationToken);
        }

        private Task<Identifier> GetResolvedTableNameAsyncCore(Identifier tableName, CancellationToken cancellationToken)
        {
            return ResolveFirstExistingObjectNameAsync(tableName, GetResolvedTableNameStrictAsync, cancellationToken);
        }

        protected Identifier GetResolvedTableNameStrict(Identifier tableName)
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));

            tableName = CreateQualifiedIdentifier(tableName);
            var qualifiedTableName = Connection.Query<QualifiedName>(
                TableNameQuery,
                new { SchemaName = tableName.Schema, TableName = tableName.LocalName }
            ).FirstOrDefault();

            return qualifiedTableName != null
                ? Identifier.CreateQualifiedIdentifier(ServerName, DatabaseName, qualifiedTableName.SchemaName, qualifiedTableName.ObjectName)
                : null;
        }

        protected Task<Identifier> GetResolvedTableNameStrictAsync(Identifier tableName, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));

            return GetResolvedTableNameStrictAsyncCore(tableName, cancellationToken);
        }

        private async Task<Identifier> GetResolvedTableNameStrictAsyncCore(Identifier tableName, CancellationToken cancellationToken)
        {
            tableName = CreateQualifiedIdentifier(tableName);
            var qualifiedTableNames = await Connection.QueryAsync<QualifiedName>(
                TableNameQuery,
                new { SchemaName = tableName.Schema, TableName = tableName.LocalName }
            ).ConfigureAwait(false);
            var qualifiedTableName = qualifiedTableNames.FirstOrDefault();

            return qualifiedTableName != null
                ? Identifier.CreateQualifiedIdentifier(ServerName, DatabaseName, qualifiedTableName.SchemaName, qualifiedTableName.ObjectName)
                : null;
        }

        protected virtual string TableNameQuery => TableNameQuerySql;

        private const string TableNameQuerySql = @"
select t.OWNER as SchemaName, t.TABLE_NAME as ObjectName
from ALL_TABLES t
inner join ALL_OBJECTS o on t.OWNER = o.OWNER and t.TABLE_NAME = o.OBJECT_NAME
where t.OWNER = :SchemaName and t.TABLE_NAME = :TableName and o.ORACLE_MAINTAINED <> 'Y'";

        public IRelationalDatabaseTable GetTable(Identifier tableName)
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));

            tableName = CreateQualifiedIdentifier(tableName);
            return LoadTableSync(tableName);
        }

        public Task<IRelationalDatabaseTable> GetTableAsync(Identifier tableName, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));

            tableName = CreateQualifiedIdentifier(tableName);
            return LoadTableAsync(tableName, cancellationToken);
        }

        public IReadOnlyCollection<IRelationalDatabaseTable> Tables
        {
            get
            {
                var tableNames = Connection.Query<QualifiedName>(TablesQuery)
                    .Select(dto => Identifier.CreateQualifiedIdentifier(dto.SchemaName, dto.ObjectName))
                    .ToList();

                var tables = tableNames.Select(LoadTableSync);
                return new ReadOnlyCollectionSlim<IRelationalDatabaseTable>(tableNames.Count, tables);
            }
        }

        public async Task<IReadOnlyCollection<Task<IRelationalDatabaseTable>>> TablesAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            var queryResults = await Connection.QueryAsync<QualifiedName>(TablesQuery).ConfigureAwait(false);
            var tableNames = queryResults
                .Select(dto => Identifier.CreateQualifiedIdentifier(dto.SchemaName, dto.ObjectName))
                .ToList();

            var tables = tableNames.Select(name => LoadTableAsync(name, cancellationToken));
            return new ReadOnlyCollectionSlim<Task<IRelationalDatabaseTable>>(tableNames.Count, tables);
        }

        protected virtual string TablesQuery => TablesQuerySql;

        private const string TablesQuerySql = @"
select
    t.OWNER as SchemaName,
    t.TABLE_NAME as ObjectName
from ALL_TABLES t
inner join ALL_OBJECTS o on t.OWNER = o.OWNER and t.TABLE_NAME = o.OBJECT_NAME
where o.ORACLE_MAINTAINED <> 'Y'
order by t.OWNER, t.TABLE_NAME";

        protected virtual IRelationalDatabaseTable LoadTableSync(Identifier tableName)
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));

            tableName = ResolveFirstExistingObjectName(tableName, GetResolvedTableName);
            return tableName != null
                ? new OracleRelationalDatabaseTable(Connection, this, Dialect.TypeProvider, tableName, IdentifierResolver)
                : null;
        }

        protected virtual Task<IRelationalDatabaseTable> LoadTableAsync(Identifier tableName, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));

            return LoadTableAsyncCore(tableName, cancellationToken);
        }

        private async Task<IRelationalDatabaseTable> LoadTableAsyncCore(Identifier tableName, CancellationToken cancellationToken)
        {
            tableName = await ResolveFirstExistingObjectNameAsync(tableName, GetResolvedTableNameAsync, cancellationToken).ConfigureAwait(false);
            return tableName != null
                ? new OracleRelationalDatabaseTable(Connection, this, Dialect.TypeProvider, tableName, IdentifierResolver)
                : null;
        }

        protected Identifier GetResolvedViewName(Identifier viewName)
        {
            if (viewName == null)
                throw new ArgumentNullException(nameof(viewName));

            return ResolveFirstExistingObjectName(viewName, GetResolvedViewNameStrict);
        }

        protected Task<Identifier> GetResolvedViewNameAsync(Identifier viewName, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (viewName == null)
                throw new ArgumentNullException(nameof(viewName));

            return GetResolvedViewNameAsyncCore(viewName, cancellationToken);
        }

        private Task<Identifier> GetResolvedViewNameAsyncCore(Identifier viewName, CancellationToken cancellationToken)
        {
            return ResolveFirstExistingObjectNameAsync(viewName, GetResolvedViewNameStrictAsync, cancellationToken);
        }

        protected Identifier GetResolvedViewNameStrict(Identifier viewName)
        {
            if (viewName == null)
                throw new ArgumentNullException(nameof(viewName));

            viewName = CreateQualifiedIdentifier(viewName);
            var qualifiedViewName = Connection.Query<QualifiedName>(
                ViewNameQuery,
                new { SchemaName = viewName.Schema, ViewName = viewName.LocalName }
            ).FirstOrDefault();

            return qualifiedViewName != null
                ? Identifier.CreateQualifiedIdentifier(ServerName, DatabaseName, qualifiedViewName.SchemaName, qualifiedViewName.ObjectName)
                : null;
        }

        protected Task<Identifier> GetResolvedViewNameStrictAsync(Identifier viewName, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (viewName == null)
                throw new ArgumentNullException(nameof(viewName));

            return GetResolvedViewNameStrictAsyncCore(viewName, cancellationToken);
        }

        private async Task<Identifier> GetResolvedViewNameStrictAsyncCore(Identifier viewName, CancellationToken cancellationToken)
        {
            viewName = CreateQualifiedIdentifier(viewName);

            var qualifiedViewNames = await Connection.QueryAsync<QualifiedName>(
                ViewNameQuery,
                new { SchemaName = viewName.Schema, ViewName = viewName.LocalName }
            ).ConfigureAwait(false);
            var qualifiedViewName = qualifiedViewNames.FirstOrDefault();

            return qualifiedViewName != null
                ? Identifier.CreateQualifiedIdentifier(ServerName, DatabaseName, qualifiedViewName.SchemaName, qualifiedViewName.ObjectName)
                : null;
        }

        protected virtual string ViewNameQuery => ViewNameQuerySql;

        private const string ViewNameQuerySql = @"
select v.OWNER as SchemaName, v.VIEW_NAME as ObjectName
from ALL_VIEWS v
inner join ALL_OBJECTS o on v.OWNER = o.OWNER and v.VIEW_NAME = o.OBJECT_NAME
where v.OWNER = :SchemaName and v.VIEW_NAME = :ViewName and o.ORACLE_MAINTAINED <> 'Y'";

        public IRelationalDatabaseView GetView(Identifier viewName)
        {
            if (viewName == null)
                throw new ArgumentNullException(nameof(viewName));

            viewName = CreateQualifiedIdentifier(viewName);
            return LoadViewSync(viewName);
        }

        public Task<IRelationalDatabaseView> GetViewAsync(Identifier viewName, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (viewName == null)
                throw new ArgumentNullException(nameof(viewName));

            viewName = CreateQualifiedIdentifier(viewName);
            return LoadViewAsync(viewName, cancellationToken);
        }

        public IReadOnlyCollection<IRelationalDatabaseView> Views
        {
            get
            {
                var viewNames = Connection.Query<QualifiedName>(ViewsQuery)
                    .Select(dto => Identifier.CreateQualifiedIdentifier(dto.SchemaName, dto.ObjectName))
                    .ToList();

                var views = viewNames.Select(LoadViewSync);
                return new ReadOnlyCollectionSlim<IRelationalDatabaseView>(viewNames.Count, views);
            }
        }

        public async Task<IReadOnlyCollection<Task<IRelationalDatabaseView>>> ViewsAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            var queryResult = await Connection.QueryAsync<QualifiedName>(ViewsQuery).ConfigureAwait(false);
            var viewNames = queryResult
                .Select(dto => Identifier.CreateQualifiedIdentifier(dto.SchemaName, dto.ObjectName))
                .ToList();

            var views = viewNames.Select(name => LoadViewAsync(name, cancellationToken));
            return new ReadOnlyCollectionSlim<Task<IRelationalDatabaseView>>(viewNames.Count, views);
        }

        protected virtual string ViewsQuery => ViewsQuerySql;

        private const string ViewsQuerySql = @"
select
    v.OWNER as SchemaName,
    v.VIEW_NAME as ObjectName
from ALL_VIEWS v
inner join ALL_OBJECTS o on v.OWNER = o.OWNER and v.VIEW_NAME = o.OBJECT_NAME
where o.ORACLE_MAINTAINED <> 'Y'
order by v.OWNER, v.VIEW_NAME";

        protected virtual IRelationalDatabaseView LoadViewSync(Identifier viewName)
        {
            if (viewName == null)
                throw new ArgumentNullException(nameof(viewName));

            viewName = ResolveFirstExistingObjectName(viewName, GetResolvedViewName);
            return viewName != null
                ? new OracleRelationalDatabaseView(Connection, Dialect.TypeProvider, viewName, IdentifierResolver)
                : null;
        }

        protected virtual Task<IRelationalDatabaseView> LoadViewAsync(Identifier viewName, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (viewName == null)
                throw new ArgumentNullException(nameof(viewName));

            return LoadViewAsyncCore(viewName, cancellationToken);
        }

        private async Task<IRelationalDatabaseView> LoadViewAsyncCore(Identifier viewName, CancellationToken cancellationToken)
        {
            viewName = await ResolveFirstExistingObjectNameAsync(viewName, GetResolvedViewNameAsync, cancellationToken).ConfigureAwait(false);
            return viewName != null
                ? new OracleRelationalDatabaseView(Connection, Dialect.TypeProvider, viewName, IdentifierResolver)
                : null;
        }

        public Identifier GetResolvedSequenceName(Identifier sequenceName)
        {
            if (sequenceName == null)
                throw new ArgumentNullException(nameof(sequenceName));

            return ResolveFirstExistingObjectName(sequenceName, GetResolvedSequenceNameStrict);
        }

        public Task<Identifier> GetResolvedSequenceNameAsync(Identifier sequenceName, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (sequenceName == null)
                throw new ArgumentNullException(nameof(sequenceName));

            return GetResolvedSequenceNameAsyncCore(sequenceName, cancellationToken);
        }

        private Task<Identifier> GetResolvedSequenceNameAsyncCore(Identifier sequenceName, CancellationToken cancellationToken)
        {
            return ResolveFirstExistingObjectNameAsync(sequenceName, GetResolvedSequenceNameStrictAsync, cancellationToken);
        }

        protected Identifier GetResolvedSequenceNameStrict(Identifier sequenceName)
        {
            if (sequenceName == null)
                throw new ArgumentNullException(nameof(sequenceName));

            sequenceName = CreateQualifiedIdentifier(sequenceName);
            var qualifiedSequenceName = Connection.Query<QualifiedName>(
                SequenceNameQuery,
                new { SchemaName = sequenceName.Schema, SequenceName = sequenceName.LocalName }
            ).FirstOrDefault();

            return qualifiedSequenceName != null
                ? Identifier.CreateQualifiedIdentifier(ServerName, DatabaseName, qualifiedSequenceName.SchemaName, qualifiedSequenceName.ObjectName)
                : null;
        }

        protected Task<Identifier> GetResolvedSequenceNameStrictAsync(Identifier sequenceName, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (sequenceName == null)
                throw new ArgumentNullException(nameof(sequenceName));

            return GetResolvedSequenceNameStrictAsyncCore(sequenceName, cancellationToken);
        }

        private async Task<Identifier> GetResolvedSequenceNameStrictAsyncCore(Identifier sequenceName, CancellationToken cancellationToken)
        {
            sequenceName = CreateQualifiedIdentifier(sequenceName);

            var qualifiedSequenceNames = await Connection.QueryAsync<QualifiedName>(
                SequenceNameQuery,
                new { SchemaName = sequenceName.Schema, SequenceName = sequenceName.LocalName }
            ).ConfigureAwait(false);
            var qualifiedSequenceName = qualifiedSequenceNames.FirstOrDefault();

            return qualifiedSequenceName != null
                ? Identifier.CreateQualifiedIdentifier(ServerName, DatabaseName, qualifiedSequenceName.SchemaName, qualifiedSequenceName.ObjectName)
                : null;
        }

        protected virtual string SequenceNameQuery => SequenceNameQuerySql;

        private const string SequenceNameQuerySql = @"
select s.SEQUENCE_OWNER as SchemaName, s.SEQUENCE_NAME as ObjectName
from ALL_SEQUENCES s
inner join ALL_OBJECTS o on s.SEQUENCE_OWNER = o.OWNER and s.SEQUENCE_NAME = o.OBJECT_NAME
where s.SEQUENCE_OWNER = :SchemaName and s.SEQUENCE_NAME = :SequenceName and o.ORACLE_MAINTAINED <> 'Y'";

        public IDatabaseSequence GetSequence(Identifier sequenceName)
        {
            if (sequenceName == null)
                throw new ArgumentNullException(nameof(sequenceName));

            sequenceName = CreateQualifiedIdentifier(sequenceName);
            return LoadSequenceSync(sequenceName);
        }

        public Task<IDatabaseSequence> GetSequenceAsync(Identifier sequenceName, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (sequenceName == null)
                throw new ArgumentNullException(nameof(sequenceName));

            sequenceName = CreateQualifiedIdentifier(sequenceName);
            return LoadSequenceAsync(sequenceName);
        }

        public IReadOnlyCollection<IDatabaseSequence> Sequences
        {
            get
            {
                var sequenceNames = Connection.Query<QualifiedName>(SequencesQuery)
                    .Select(dto => Identifier.CreateQualifiedIdentifier(dto.SchemaName, dto.ObjectName))
                    .ToList();

                var sequences = sequenceNames.Select(LoadSequenceSync);
                return new ReadOnlyCollectionSlim<IDatabaseSequence>(sequenceNames.Count, sequences);
            }
        }

        public async Task<IReadOnlyCollection<Task<IDatabaseSequence>>> SequencesAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            var queryResult = await Connection.QueryAsync<QualifiedName>(SequencesQuery).ConfigureAwait(false);
            var sequenceNames = queryResult
                .Select(dto => Identifier.CreateQualifiedIdentifier(dto.SchemaName, dto.ObjectName))
                .ToList();

            var sequences = sequenceNames.Select(name => LoadSequenceAsync(name, cancellationToken));
            return new ReadOnlyCollectionSlim<Task<IDatabaseSequence>>(sequenceNames.Count, sequences);
        }

        protected virtual string SequencesQuery => SequencesQuerySql;

        private const string SequencesQuerySql = @"
select
    s.SEQUENCE_OWNER as SchemaName,
    s.SEQUENCE_NAME as ObjectName
from ALL_SEQUENCES s
inner join ALL_OBJECTS o on s.SEQUENCE_OWNER = o.OWNER and s.SEQUENCE_NAME = o.OBJECT_NAME
where o.ORACLE_MAINTAINED <> 'Y'
order by s.SEQUENCE_OWNER, s.SEQUENCE_NAME";

        protected virtual IDatabaseSequence LoadSequenceSync(Identifier sequenceName)
        {
            if (sequenceName == null)
                throw new ArgumentNullException(nameof(sequenceName));

            sequenceName = ResolveFirstExistingObjectName(sequenceName, GetResolvedSequenceName);
            return sequenceName != null
                ? new OracleDatabaseSequence(Connection, sequenceName)
                : null;
        }

        protected virtual Task<IDatabaseSequence> LoadSequenceAsync(Identifier sequenceName, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (sequenceName == null)
                throw new ArgumentNullException(nameof(sequenceName));

            return LoadSequenceAsyncCore(sequenceName, cancellationToken);
        }

        private async Task<IDatabaseSequence> LoadSequenceAsyncCore(Identifier sequenceName, CancellationToken cancellationToken)
        {
            sequenceName = await ResolveFirstExistingObjectNameAsync(sequenceName, GetResolvedSequenceNameAsync, cancellationToken).ConfigureAwait(false);
            return sequenceName != null
                ? new OracleDatabaseSequence(Connection, sequenceName)
                : null;
        }

        public Identifier GetResolvedSynonymName(Identifier synonymName)
        {
            if (synonymName == null)
                throw new ArgumentNullException(nameof(synonymName));

            return ResolveFirstExistingObjectName(synonymName, GetResolvedSynonymNameStrict);
        }

        public Task<Identifier> GetResolvedSynonymNameAsync(Identifier synonymName, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (synonymName == null)
                throw new ArgumentNullException(nameof(synonymName));

            return GetResolvedSynonymNameAsyncCore(synonymName, cancellationToken);
        }

        private Task<Identifier> GetResolvedSynonymNameAsyncCore(Identifier synonymName, CancellationToken cancellationToken)
        {
            return ResolveFirstExistingObjectNameAsync(synonymName, GetResolvedSynonymNameStrictAsync, cancellationToken);
        }

        protected Identifier GetResolvedSynonymNameStrict(Identifier synonymName)
        {
            if (synonymName == null)
                throw new ArgumentNullException(nameof(synonymName));

            synonymName = CreateQualifiedIdentifier(synonymName);

            if (synonymName.Database == DatabaseName && synonymName.Schema == DefaultSchema)
                return GetResolvedUserSynonymNameStrict(synonymName.LocalName);

            var qualifiedSynonymName = Connection.Query<QualifiedName>(
                SynonymNameQuery,
                new { SchemaName = synonymName.Schema, SynonymName = synonymName.LocalName }
            ).FirstOrDefault();

            return qualifiedSynonymName != null
                ? Identifier.CreateQualifiedIdentifier(ServerName, DatabaseName, qualifiedSynonymName.SchemaName, qualifiedSynonymName.ObjectName)
                : null;
        }

        protected Identifier GetResolvedUserSynonymNameStrict(string synonymName)
        {
            if (synonymName.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(synonymName));

            var name = Connection.ExecuteScalar<string>(UserSynonymNameQuery, new { SynonymName = synonymName });
            return name != null
                ? Identifier.CreateQualifiedIdentifier(ServerName, DatabaseName, DefaultSchema, name)
                : null;
        }

        protected Task<Identifier> GetResolvedSynonymNameStrictAsync(Identifier synonymName, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (synonymName == null)
                throw new ArgumentNullException(nameof(synonymName));

            return GetResolvedSynonymNameStrictAsyncCore(synonymName, cancellationToken);
        }

        private async Task<Identifier> GetResolvedSynonymNameStrictAsyncCore(Identifier synonymName, CancellationToken cancellationToken)
        {
            synonymName = CreateQualifiedIdentifier(synonymName);

            if (synonymName.Database == DatabaseName && synonymName.Schema == DefaultSchema)
                return await GetResolvedUserSynonymNameStrictAsyncCore(synonymName.LocalName, cancellationToken).ConfigureAwait(false);

            var qualifiedSynonymNames = await Connection.QueryAsync<QualifiedName>(
                SynonymNameQuery,
                new { SchemaName = synonymName.Schema, SynonymName = synonymName.LocalName }
            ).ConfigureAwait(false);
            var qualifiedSynonymName = qualifiedSynonymNames.FirstOrDefault();

            return qualifiedSynonymName != null
                ? Identifier.CreateQualifiedIdentifier(ServerName, DatabaseName, qualifiedSynonymName.SchemaName, qualifiedSynonymName.ObjectName)
                : null;
        }

        private async Task<Identifier> GetResolvedUserSynonymNameStrictAsyncCore(string synonymName, CancellationToken cancellationToken)
        {
            var name = await Connection.ExecuteScalarAsync<string>(
                UserSynonymNameQuery,
                new { SynonymName = synonymName }
            ).ConfigureAwait(false);
            return name != null
                ? Identifier.CreateQualifiedIdentifier(ServerName, DatabaseName, DefaultSchema, name)
                : null;
        }

        protected virtual string SynonymNameQuery => SynonymNameQuerySql;

        private const string SynonymNameQuerySql = @"
select s.OWNER as SchemaName, s.SYNONYM_NAME as ObjectName
from ALL_SYNONYMS s
inner join ALL_OBJECTS o on s.OWNER = o.OWNER and s.SYNONYM_NAME = o.OBJECT_NAME
where s.OWNER = :SchemaName and s.SYNONYM_NAME = :SynonymName and o.ORACLE_MAINTAINED <> 'Y'";

        protected virtual string UserSynonymNameQuery => UserSynonymNameQuerySql;

        private const string UserSynonymNameQuerySql = @"
select COUNT(*)
from USER_SYNONYMS s
inner join ALL_OBJECTS o on s.SYNONYM_NAME = o.OBJECT_NAME
where o.OWNER = SYS_CONTEXT('USERENV', 'CURRENT_USER') and s.SYNONYM_NAME = :SynonymName and o.ORACLE_MAINTAINED <> 'Y'";

        public IDatabaseSynonym GetSynonym(Identifier synonymName)
        {
            if (synonymName == null)
                throw new ArgumentNullException(nameof(synonymName));

            synonymName = CreateQualifiedIdentifier(synonymName);
            return LoadSynonymSync(synonymName);
        }

        public Task<IDatabaseSynonym> GetSynonymAsync(Identifier synonymName, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (synonymName == null)
                throw new ArgumentNullException(nameof(synonymName));

            synonymName = CreateQualifiedIdentifier(synonymName);
            return LoadSynonymAsync(synonymName, cancellationToken);
        }

        public IReadOnlyCollection<IDatabaseSynonym> Synonyms
        {
            get
            {
                var synonymNames = Connection.Query<QualifiedName>(SynonymsQuery)
                    .Select(dto => Identifier.CreateQualifiedIdentifier(dto.DatabaseName, dto.SchemaName, dto.ObjectName))
                    .ToList();

                var synonyms = synonymNames.Select(LoadSynonymSync);
                return new ReadOnlyCollectionSlim<IDatabaseSynonym>(synonymNames.Count, synonyms);
            }
        }

        public async Task<IReadOnlyCollection<Task<IDatabaseSynonym>>> SynonymsAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            var queryResult = await Connection.QueryAsync<QualifiedName>(SynonymsQuery).ConfigureAwait(false);
            var synonymNames = queryResult
                .Select(dto => Identifier.CreateQualifiedIdentifier(dto.DatabaseName, dto.SchemaName, dto.ObjectName))
                .ToList();

            var synonyms = synonymNames.Select(name => LoadSynonymAsync(name, cancellationToken));
            return new ReadOnlyCollectionSlim<Task<IDatabaseSynonym>>(synonymNames.Count, synonyms);
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

        protected virtual IDatabaseSynonym LoadSynonymSync(Identifier synonymName)
        {
            if (synonymName == null)
                throw new ArgumentNullException(nameof(synonymName));

            synonymName = ResolveFirstExistingObjectName(synonymName, GetResolvedSynonymNameStrict);
            if (synonymName == null)
                return null;

            if (synonymName.Database == DatabaseName && synonymName.Schema == DefaultSchema)
                return LoadUserSynonymSync(synonymName.LocalName);

            var queryResult = Connection.QuerySingle<SynonymData>(
                LoadSynonymQuery,
                new { SchemaName = synonymName.Schema, SynonymName = synonymName.LocalName }
            );

            var databaseName = !queryResult.TargetDatabaseName.IsNullOrWhiteSpace() ? queryResult.TargetDatabaseName : null;
            var schemaName = !queryResult.TargetSchemaName.IsNullOrWhiteSpace() ? queryResult.TargetSchemaName : null;
            var localName = !queryResult.TargetObjectName.IsNullOrWhiteSpace() ? queryResult.TargetObjectName : null;

            var targetName = Identifier.CreateQualifiedIdentifier(databaseName, schemaName, localName);
            targetName = CreateQualifiedIdentifier(targetName);

            return new DatabaseSynonym(synonymName, targetName);
        }

        private IDatabaseSynonym LoadUserSynonymSync(string synonymName)
        {
            var queryResult = Connection.QuerySingle<SynonymData>(
                LoadUserSynonymQuery,
                new { SynonymName = synonymName }
            );

            var databaseName = !queryResult.TargetDatabaseName.IsNullOrWhiteSpace() ? queryResult.TargetDatabaseName : null;
            var schemaName = !queryResult.TargetSchemaName.IsNullOrWhiteSpace() ? queryResult.TargetSchemaName : null;
            var localName = !queryResult.TargetObjectName.IsNullOrWhiteSpace() ? queryResult.TargetObjectName : null;

            var targetName = Identifier.CreateQualifiedIdentifier(databaseName, schemaName, localName);
            targetName = CreateQualifiedIdentifier(targetName);

            return new DatabaseSynonym(synonymName, targetName);
        }

        protected virtual Task<IDatabaseSynonym> LoadSynonymAsync(Identifier synonymName, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (synonymName == null)
                throw new ArgumentNullException(nameof(synonymName));

            return LoadSynonymAsyncCore(synonymName, cancellationToken);
        }

        private async Task<IDatabaseSynonym> LoadSynonymAsyncCore(Identifier synonymName, CancellationToken cancellationToken)
        {
            synonymName = await ResolveFirstExistingObjectNameAsync(synonymName, GetResolvedSynonymNameStrictAsync, cancellationToken).ConfigureAwait(false);
            if (synonymName == null)
                return null;

            if (synonymName.Database == DatabaseName && synonymName.Schema == DefaultSchema)
                return await LoadUserSynonymAsyncCore(synonymName.LocalName, cancellationToken).ConfigureAwait(false);

            var queryResult = await Connection.QuerySingleAsync<SynonymData>(
                LoadSynonymQuery,
                new { SchemaName = synonymName.Schema, SynonymName = synonymName.LocalName }
            ).ConfigureAwait(false);

            var databaseName = !queryResult.TargetDatabaseName.IsNullOrWhiteSpace() ? queryResult.TargetDatabaseName : null;
            var schemaName = !queryResult.TargetSchemaName.IsNullOrWhiteSpace() ? queryResult.TargetSchemaName : null;
            var localName = !queryResult.TargetObjectName.IsNullOrWhiteSpace() ? queryResult.TargetObjectName : null;

            var targetName = Identifier.CreateQualifiedIdentifier(databaseName, schemaName, localName);
            targetName = CreateQualifiedIdentifier(targetName);

            return new DatabaseSynonym(synonymName, targetName);
        }

        private async Task<IDatabaseSynonym> LoadUserSynonymAsyncCore(string synonymName, CancellationToken cancellationToken)
        {
            var queryResult = await Connection.QuerySingleAsync<SynonymData>(
                LoadUserSynonymQuery,
                new { SynonymName = synonymName }
            ).ConfigureAwait(false);

            var databaseName = !queryResult.TargetDatabaseName.IsNullOrWhiteSpace() ? queryResult.TargetDatabaseName : null;
            var schemaName = !queryResult.TargetSchemaName.IsNullOrWhiteSpace() ? queryResult.TargetSchemaName : null;
            var localName = !queryResult.TargetObjectName.IsNullOrWhiteSpace() ? queryResult.TargetObjectName : null;

            var targetName = Identifier.CreateQualifiedIdentifier(databaseName, schemaName, localName);
            targetName = CreateQualifiedIdentifier(targetName);

            return new DatabaseSynonym(synonymName, targetName);
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

        protected Identifier ResolveFirstExistingObjectName(Identifier objectName, Func<Identifier, Identifier> objectExistsFunc)
        {
            if (objectName == null)
                throw new ArgumentNullException(nameof(objectName));
            if (objectExistsFunc == null)
                throw new ArgumentNullException(nameof(objectExistsFunc));

            var resolvedNames = IdentifierResolver
                .GetResolutionOrder(objectName)
                .Select(CreateQualifiedIdentifier);
            return resolvedNames.FirstOrDefault(name => objectExistsFunc(name) != null);
        }

        protected Task<Identifier> ResolveFirstExistingObjectNameAsync(Identifier objectName, Func<Identifier, CancellationToken, Task<Identifier>> objectExistsFunc, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (objectName == null)
                throw new ArgumentNullException(nameof(objectName));
            if (objectExistsFunc == null)
                throw new ArgumentNullException(nameof(objectExistsFunc));

            return ResolveFirstExistingObjectNameAsyncCore(objectName, objectExistsFunc, cancellationToken);
        }

        private async Task<Identifier> ResolveFirstExistingObjectNameAsyncCore(Identifier objectName, Func<Identifier, CancellationToken, Task<Identifier>> objectExistsFunc, CancellationToken cancellationToken)
        {
            var resolvedNames = IdentifierResolver
                .GetResolutionOrder(objectName)
                .Select(CreateQualifiedIdentifier);

            foreach (var resolvedName in resolvedNames)
            {
                var existingName = await objectExistsFunc(resolvedName, cancellationToken).ConfigureAwait(false);
                if (existingName != null)
                    return existingName;
            }

            return null;
        }

        /// <summary>
        /// Qualifies an identifier with information from the database. For example, sets the schema if it is missing.
        /// </summary>
        /// <param name="identifier">An identifier which may or may not be fully qualified.</param>
        /// <returns>A new identifier, which will have components present where they were previously missing.</returns>
        /// <remarks>No components of an identifier when present will be modified. For example, when given a fully qualified identifier, a new identifier will be returned that is equal in value to the argument.</remarks>
        protected Identifier CreateQualifiedIdentifier(Identifier identifier)
        {
            if (identifier == null)
                throw new ArgumentNullException(nameof(identifier));

            var serverName = identifier.Server ?? ServerName;
            var databaseName = identifier.Database ?? DatabaseName;
            var schema = identifier.Schema ?? DefaultSchema;

            return Identifier.CreateQualifiedIdentifier(serverName, databaseName, schema, identifier.LocalName);
        }

        private async Task<DatabaseMetadata> LoadDatabaseMetadataAsync()
        {
            const string hostSql = @"
select
    SYS_CONTEXT('USERENV', 'SERVER_HOST') as ServerHost,
    SYS_CONTEXT('USERENV', 'INSTANCE_NAME') as ServerSid,
    SYS_CONTEXT('USERENV', 'DB_NAME') as DatabaseName,
    SYS_CONTEXT('USERENV', 'CURRENT_USER') as DefaultSchema
from DUAL";
            var hostInfoTask = Connection.QueryFirstOrDefaultAsync<DatabaseHost>(hostSql);

            const string versionSql = @"
select
    PRODUCT as ProductName,
    VERSION as VersionNumber
from PRODUCT_COMPONENT_VERSION
where PRODUCT like 'Oracle Database%'";
            var versionInfoTask = Connection.QueryFirstOrDefaultAsync<DatabaseVersion>(versionSql);

            await Task.WhenAll(hostInfoTask, versionInfoTask).ConfigureAwait(false);

            var hostInfo = hostInfoTask.Result;
            var versionInfo = versionInfoTask.Result;

            var qualifiedServerName = hostInfo.ServerHost + "/" + hostInfo.ServerSid;
            var versionName = versionInfo.ProductName + versionInfo.VersionNumber;

            return new DatabaseMetadata
            {
                 ServerName = qualifiedServerName,
                 DatabaseName = hostInfo.DatabaseName,
                 DefaultSchema = hostInfo.DefaultSchema,
                 DatabaseVersion = versionName
            };
        }

        private readonly AsyncLazy<DatabaseMetadata> _metadata;
    }
}
