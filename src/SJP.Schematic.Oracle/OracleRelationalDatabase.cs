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
using LanguageExt;

namespace SJP.Schematic.Oracle
{
    public class OracleRelationalDatabase : RelationalDatabase, IRelationalDatabase
    {
        public OracleRelationalDatabase(IDatabaseDialect dialect, IDbConnection connection, IIdentifierResolutionStrategy identifierResolver)
            : base(dialect, connection)
        {
            IdentifierResolver = identifierResolver ?? throw new ArgumentNullException(nameof(identifierResolver));
            _metadata = new AsyncLazy<DatabaseMetadata>(LoadDatabaseMetadataAsync);
        }

        protected IIdentifierResolutionStrategy IdentifierResolver { get; }

        public string ServerName => Metadata.ServerName;

        public string DatabaseName => Metadata.DatabaseName;

        public string DefaultSchema => Metadata.DefaultSchema;

        public string DatabaseVersion => Metadata.DatabaseVersion;

        protected DatabaseMetadata Metadata => _metadata.Task.GetAwaiter().GetResult();

        protected Option<Identifier> GetResolvedTableName(Identifier tableName)
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));

            return ResolveFirstExistingObjectName(tableName, GetResolvedTableNameStrict);
        }

        protected Task<Option<Identifier>> GetResolvedTableNameAsync(Identifier tableName, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));

            return GetResolvedTableNameAsyncCore(tableName, cancellationToken);
        }

        private Task<Option<Identifier>> GetResolvedTableNameAsyncCore(Identifier tableName, CancellationToken cancellationToken)
        {
            return ResolveFirstExistingObjectNameAsync(tableName, GetResolvedTableNameStrictAsync, cancellationToken);
        }

        protected Option<Identifier> GetResolvedTableNameStrict(Identifier tableName)
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));

            tableName = CreateQualifiedIdentifier(tableName);
            var qualifiedTableName = Connection.QueryFirstOrNone<QualifiedName>(
                TableNameQuery,
                new { SchemaName = tableName.Schema, TableName = tableName.LocalName }
            );

            return qualifiedTableName.Map(name => Identifier.CreateQualifiedIdentifier(tableName.Server, tableName.Database, name.SchemaName, name.ObjectName));
        }

        protected Task<Option<Identifier>> GetResolvedTableNameStrictAsync(Identifier tableName, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));

            return GetResolvedTableNameStrictAsyncCore(tableName, cancellationToken);
        }

        private async Task<Option<Identifier>> GetResolvedTableNameStrictAsyncCore(Identifier tableName, CancellationToken cancellationToken)
        {
            tableName = CreateQualifiedIdentifier(tableName);
            var qualifiedTableName = await Connection.QueryFirstOrNoneAsync<QualifiedName>(
                TableNameQuery,
                new { SchemaName = tableName.Schema, TableName = tableName.LocalName }
            ).ConfigureAwait(false);

            return qualifiedTableName.Map(name => Identifier.CreateQualifiedIdentifier(tableName.Server, tableName.Database, name.SchemaName, name.ObjectName));
        }

        protected virtual string TableNameQuery => TableNameQuerySql;

        private const string TableNameQuerySql = @"
select t.OWNER as SchemaName, t.TABLE_NAME as ObjectName
from ALL_TABLES t
inner join ALL_OBJECTS o on t.OWNER = o.OWNER and t.TABLE_NAME = o.OBJECT_NAME
where t.OWNER = :SchemaName and t.TABLE_NAME = :TableName and o.ORACLE_MAINTAINED <> 'Y'";

        public Option<IRelationalDatabaseTable> GetTable(Identifier tableName)
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));

            tableName = CreateQualifiedIdentifier(tableName);
            return LoadTableSync(tableName);
        }

        public Task<Option<IRelationalDatabaseTable>> GetTableAsync(Identifier tableName, CancellationToken cancellationToken = default(CancellationToken))
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

                var tables = tableNames
                    .Select(LoadTableSync)
                    .Where(t => t.IsSome)
                    .Select(t => t.UnwrapSome());
                return new ReadOnlyCollectionSlim<IRelationalDatabaseTable>(tableNames.Count, tables);
            }
        }

        public async Task<IReadOnlyCollection<Task<IRelationalDatabaseTable>>> TablesAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            var queryResults = await Connection.QueryAsync<QualifiedName>(TablesQuery).ConfigureAwait(false);
            var tableNames = queryResults
                .Select(dto => Identifier.CreateQualifiedIdentifier(dto.SchemaName, dto.ObjectName))
                .ToList();

            var tables = tableNames
                .Select(name => LoadTableAsync(name, cancellationToken))
                .Where(t => t.IsSome)
                .Select(t => t.UnwrapSome());
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

        protected virtual Option<IRelationalDatabaseTable> LoadTableSync(Identifier tableName)
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));

            var resolvedTableName = ResolveFirstExistingObjectName(tableName, GetResolvedTableName);
            return resolvedTableName
                .Map<IRelationalDatabaseTable>(name => new OracleRelationalDatabaseTable(Connection, this, Dialect.TypeProvider, name, IdentifierResolver));
        }

        protected virtual Task<Option<IRelationalDatabaseTable>> LoadTableAsync(Identifier tableName, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));

            return LoadTableAsyncCore(tableName, cancellationToken);
        }

        private async Task<Option<IRelationalDatabaseTable>> LoadTableAsyncCore(Identifier tableName, CancellationToken cancellationToken)
        {
            var resolvedTableName = await ResolveFirstExistingObjectNameAsync(tableName, GetResolvedTableNameAsync, cancellationToken).ConfigureAwait(false);
            return resolvedTableName
                .Map<IRelationalDatabaseTable>(name => new OracleRelationalDatabaseTable(Connection, this, Dialect.TypeProvider, name, IdentifierResolver));
        }

        protected Option<Identifier> GetResolvedViewName(Identifier viewName)
        {
            if (viewName == null)
                throw new ArgumentNullException(nameof(viewName));

            return ResolveFirstExistingObjectName(viewName, GetResolvedViewNameStrict);
        }

        protected Task<Option<Identifier>> GetResolvedViewNameAsync(Identifier viewName, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (viewName == null)
                throw new ArgumentNullException(nameof(viewName));

            return GetResolvedViewNameAsyncCore(viewName, cancellationToken);
        }

        private Task<Option<Identifier>> GetResolvedViewNameAsyncCore(Identifier viewName, CancellationToken cancellationToken)
        {
            return ResolveFirstExistingObjectNameAsync(viewName, GetResolvedViewNameStrictAsync, cancellationToken);
        }

        protected Option<Identifier> GetResolvedViewNameStrict(Identifier viewName)
        {
            if (viewName == null)
                throw new ArgumentNullException(nameof(viewName));

            viewName = CreateQualifiedIdentifier(viewName);
            var qualifiedViewName = Connection.QueryFirstOrNone<QualifiedName>(
                ViewNameQuery,
                new { SchemaName = viewName.Schema, ViewName = viewName.LocalName }
            );

            return qualifiedViewName.Map(name => Identifier.CreateQualifiedIdentifier(viewName.Server, viewName.Database, name.SchemaName, name.ObjectName));
        }

        protected Task<Option<Identifier>> GetResolvedViewNameStrictAsync(Identifier viewName, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (viewName == null)
                throw new ArgumentNullException(nameof(viewName));

            return GetResolvedViewNameStrictAsyncCore(viewName, cancellationToken);
        }

        private async Task<Option<Identifier>> GetResolvedViewNameStrictAsyncCore(Identifier viewName, CancellationToken cancellationToken)
        {
            viewName = CreateQualifiedIdentifier(viewName);

            var qualifiedViewName = await Connection.QueryFirstOrNoneAsync<QualifiedName>(
                ViewNameQuery,
                new { SchemaName = viewName.Schema, ViewName = viewName.LocalName }
            ).ConfigureAwait(false);

            return qualifiedViewName.Map(name => Identifier.CreateQualifiedIdentifier(viewName.Server, viewName.Database, name.SchemaName, name.ObjectName));
        }

        protected virtual string ViewNameQuery => ViewNameQuerySql;

        private const string ViewNameQuerySql = @"
select v.OWNER as SchemaName, v.VIEW_NAME as ObjectName
from ALL_VIEWS v
inner join ALL_OBJECTS o on v.OWNER = o.OWNER and v.VIEW_NAME = o.OBJECT_NAME
where v.OWNER = :SchemaName and v.VIEW_NAME = :ViewName and o.ORACLE_MAINTAINED <> 'Y'";

        public Option<IRelationalDatabaseView> GetView(Identifier viewName)
        {
            if (viewName == null)
                throw new ArgumentNullException(nameof(viewName));

            viewName = CreateQualifiedIdentifier(viewName);
            return LoadViewSync(viewName);
        }

        public Task<Option<IRelationalDatabaseView>> GetViewAsync(Identifier viewName, CancellationToken cancellationToken = default(CancellationToken))
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

                var views = viewNames
                    .Select(LoadViewSync)
                    .Where(v => v.IsSome)
                    .Select(v => v.UnwrapSome());
                return new ReadOnlyCollectionSlim<IRelationalDatabaseView>(viewNames.Count, views);
            }
        }

        public async Task<IReadOnlyCollection<Task<IRelationalDatabaseView>>> ViewsAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            var queryResult = await Connection.QueryAsync<QualifiedName>(ViewsQuery).ConfigureAwait(false);
            var viewNames = queryResult
                .Select(dto => Identifier.CreateQualifiedIdentifier(dto.SchemaName, dto.ObjectName))
                .ToList();

            var views = viewNames
                .Select(name => LoadViewAsync(name, cancellationToken))
                .Where(v => v.IsSome)
                .Select(v => v.UnwrapSome());
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

        protected virtual Option<IRelationalDatabaseView> LoadViewSync(Identifier viewName)
        {
            if (viewName == null)
                throw new ArgumentNullException(nameof(viewName));

            var resolvedViewName = ResolveFirstExistingObjectName(viewName, GetResolvedViewName);
            return resolvedViewName
                .Map<IRelationalDatabaseView>(name => new OracleRelationalDatabaseView(Connection, Dialect.TypeProvider, name, IdentifierResolver));
        }

        protected virtual Task<Option<IRelationalDatabaseView>> LoadViewAsync(Identifier viewName, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (viewName == null)
                throw new ArgumentNullException(nameof(viewName));

            return LoadViewAsyncCore(viewName, cancellationToken);
        }

        private async Task<Option<IRelationalDatabaseView>> LoadViewAsyncCore(Identifier viewName, CancellationToken cancellationToken)
        {
            var resolvedViewName = await ResolveFirstExistingObjectNameAsync(viewName, GetResolvedViewNameAsync, cancellationToken).ConfigureAwait(false);
            return resolvedViewName
                .Map<IRelationalDatabaseView>(name => new OracleRelationalDatabaseView(Connection, Dialect.TypeProvider, name, IdentifierResolver));
        }

        public Option<Identifier> GetResolvedSequenceName(Identifier sequenceName)
        {
            if (sequenceName == null)
                throw new ArgumentNullException(nameof(sequenceName));

            return ResolveFirstExistingObjectName(sequenceName, GetResolvedSequenceNameStrict);
        }

        public Task<Option<Identifier>> GetResolvedSequenceNameAsync(Identifier sequenceName, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (sequenceName == null)
                throw new ArgumentNullException(nameof(sequenceName));

            return GetResolvedSequenceNameAsyncCore(sequenceName, cancellationToken);
        }

        private Task<Option<Identifier>> GetResolvedSequenceNameAsyncCore(Identifier sequenceName, CancellationToken cancellationToken)
        {
            return ResolveFirstExistingObjectNameAsync(sequenceName, GetResolvedSequenceNameStrictAsync, cancellationToken);
        }

        protected Option<Identifier> GetResolvedSequenceNameStrict(Identifier sequenceName)
        {
            if (sequenceName == null)
                throw new ArgumentNullException(nameof(sequenceName));

            sequenceName = CreateQualifiedIdentifier(sequenceName);
            var qualifiedSequenceName = Connection.QueryFirstOrNone<QualifiedName>(
                SequenceNameQuery,
                new { SchemaName = sequenceName.Schema, SequenceName = sequenceName.LocalName }
            );

            return qualifiedSequenceName.Map(name => Identifier.CreateQualifiedIdentifier(sequenceName.Server, sequenceName.Database, name.SchemaName, name.ObjectName));
        }

        protected Task<Option<Identifier>> GetResolvedSequenceNameStrictAsync(Identifier sequenceName, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (sequenceName == null)
                throw new ArgumentNullException(nameof(sequenceName));

            return GetResolvedSequenceNameStrictAsyncCore(sequenceName, cancellationToken);
        }

        private async Task<Option<Identifier>> GetResolvedSequenceNameStrictAsyncCore(Identifier sequenceName, CancellationToken cancellationToken)
        {
            sequenceName = CreateQualifiedIdentifier(sequenceName);

            var qualifiedSequenceName = await Connection.QueryFirstOrNoneAsync<QualifiedName>(
                SequenceNameQuery,
                new { SchemaName = sequenceName.Schema, SequenceName = sequenceName.LocalName }
            ).ConfigureAwait(false);

            return qualifiedSequenceName.Map(name => Identifier.CreateQualifiedIdentifier(sequenceName.Server, sequenceName.Database, name.SchemaName, name.ObjectName));
        }

        protected virtual string SequenceNameQuery => SequenceNameQuerySql;

        private const string SequenceNameQuerySql = @"
select s.SEQUENCE_OWNER as SchemaName, s.SEQUENCE_NAME as ObjectName
from ALL_SEQUENCES s
inner join ALL_OBJECTS o on s.SEQUENCE_OWNER = o.OWNER and s.SEQUENCE_NAME = o.OBJECT_NAME
where s.SEQUENCE_OWNER = :SchemaName and s.SEQUENCE_NAME = :SequenceName and o.ORACLE_MAINTAINED <> 'Y'";

        public Option<IDatabaseSequence> GetSequence(Identifier sequenceName)
        {
            if (sequenceName == null)
                throw new ArgumentNullException(nameof(sequenceName));

            sequenceName = CreateQualifiedIdentifier(sequenceName);
            return LoadSequenceSync(sequenceName);
        }

        public Task<Option<IDatabaseSequence>> GetSequenceAsync(Identifier sequenceName, CancellationToken cancellationToken = default(CancellationToken))
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

                var sequences = sequenceNames
                    .Select(LoadSequenceSync)
                    .Where(s => s.IsSome)
                    .Select(s => s.UnwrapSome());
                return new ReadOnlyCollectionSlim<IDatabaseSequence>(sequenceNames.Count, sequences);
            }
        }

        public async Task<IReadOnlyCollection<Task<IDatabaseSequence>>> SequencesAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            var queryResult = await Connection.QueryAsync<QualifiedName>(SequencesQuery).ConfigureAwait(false);
            var sequenceNames = queryResult
                .Select(dto => Identifier.CreateQualifiedIdentifier(dto.SchemaName, dto.ObjectName))
                .ToList();

            var sequences = sequenceNames
                .Select(name => LoadSequenceAsync(name, cancellationToken))
                .Where(s => s.IsSome)
                .Select(s => s.UnwrapSome());
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

        protected virtual Option<IDatabaseSequence> LoadSequenceSync(Identifier sequenceName)
        {
            if (sequenceName == null)
                throw new ArgumentNullException(nameof(sequenceName));

            return ResolveFirstExistingObjectName(sequenceName, GetResolvedSequenceName)
                .Map<IDatabaseSequence>(name => new OracleDatabaseSequence(Connection, name));
        }

        protected virtual Task<Option<IDatabaseSequence>> LoadSequenceAsync(Identifier sequenceName, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (sequenceName == null)
                throw new ArgumentNullException(nameof(sequenceName));

            return LoadSequenceAsyncCore(sequenceName, cancellationToken);
        }

        private async Task<Option<IDatabaseSequence>> LoadSequenceAsyncCore(Identifier sequenceName, CancellationToken cancellationToken)
        {
            var resolvedSequenceName = await ResolveFirstExistingObjectNameAsync(sequenceName, GetResolvedSequenceNameAsync, cancellationToken).ConfigureAwait(false);
            return resolvedSequenceName
                .Map<IDatabaseSequence>(name => new OracleDatabaseSequence(Connection, name));
        }

        public Option<Identifier> GetResolvedSynonymName(Identifier synonymName)
        {
            if (synonymName == null)
                throw new ArgumentNullException(nameof(synonymName));

            return ResolveFirstExistingObjectName(synonymName, GetResolvedSynonymNameStrict);
        }

        public Task<Option<Identifier>> GetResolvedSynonymNameAsync(Identifier synonymName, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (synonymName == null)
                throw new ArgumentNullException(nameof(synonymName));

            return GetResolvedSynonymNameAsyncCore(synonymName, cancellationToken);
        }

        private Task<Option<Identifier>> GetResolvedSynonymNameAsyncCore(Identifier synonymName, CancellationToken cancellationToken)
        {
            return ResolveFirstExistingObjectNameAsync(synonymName, GetResolvedSynonymNameStrictAsync, cancellationToken);
        }

        protected Option<Identifier> GetResolvedSynonymNameStrict(Identifier synonymName)
        {
            if (synonymName == null)
                throw new ArgumentNullException(nameof(synonymName));

            synonymName = CreateQualifiedIdentifier(synonymName);

            if (synonymName.Database == DatabaseName && synonymName.Schema == DefaultSchema)
                return GetResolvedUserSynonymNameStrict(synonymName.LocalName);

            var qualifiedSynonymName = Connection.QueryFirstOrNone<QualifiedName>(
                SynonymNameQuery,
                new { SchemaName = synonymName.Schema, SynonymName = synonymName.LocalName }
            );

            return qualifiedSynonymName.Map(name => Identifier.CreateQualifiedIdentifier(synonymName.Server, synonymName.Database, name.SchemaName, name.ObjectName));
        }

        protected Option<Identifier> GetResolvedUserSynonymNameStrict(string synonymName)
        {
            if (synonymName.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(synonymName));

            var name = Connection.QueryFirstOrNone<string>(UserSynonymNameQuery, new { SynonymName = synonymName });
            return name.Map(n => Identifier.CreateQualifiedIdentifier(ServerName, DatabaseName, DefaultSchema, n));
        }

        protected Task<Option<Identifier>> GetResolvedSynonymNameStrictAsync(Identifier synonymName, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (synonymName == null)
                throw new ArgumentNullException(nameof(synonymName));

            return GetResolvedSynonymNameStrictAsyncCore(synonymName, cancellationToken);
        }

        private async Task<Option<Identifier>> GetResolvedSynonymNameStrictAsyncCore(Identifier synonymName, CancellationToken cancellationToken)
        {
            synonymName = CreateQualifiedIdentifier(synonymName);

            if (synonymName.Database == DatabaseName && synonymName.Schema == DefaultSchema)
                return await GetResolvedUserSynonymNameStrictAsyncCore(synonymName.LocalName, cancellationToken).ConfigureAwait(false);

            var qualifiedSynonymName = await Connection.QueryFirstOrNoneAsync<QualifiedName>(
                SynonymNameQuery,
                new { SchemaName = synonymName.Schema, SynonymName = synonymName.LocalName }
            ).ConfigureAwait(false);

            return qualifiedSynonymName.Map(name => Identifier.CreateQualifiedIdentifier(synonymName.Server, synonymName.Database, name.SchemaName, name.ObjectName));
        }

        private async Task<Option<Identifier>> GetResolvedUserSynonymNameStrictAsyncCore(string synonymName, CancellationToken cancellationToken)
        {
            var name = await Connection.QueryFirstOrNoneAsync<string>(
                UserSynonymNameQuery,
                new { SynonymName = synonymName }
            ).ConfigureAwait(false);

            return name.Map(n => Identifier.CreateQualifiedIdentifier(ServerName, DatabaseName, DefaultSchema, n));
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

        public Option<IDatabaseSynonym> GetSynonym(Identifier synonymName)
        {
            if (synonymName == null)
                throw new ArgumentNullException(nameof(synonymName));

            synonymName = CreateQualifiedIdentifier(synonymName);
            return LoadSynonymSync(synonymName);
        }

        public Task<Option<IDatabaseSynonym>> GetSynonymAsync(Identifier synonymName, CancellationToken cancellationToken = default(CancellationToken))
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

                var synonyms = synonymNames
                    .Select(LoadSynonymSync)
                    .Where(s => s.IsSome)
                    .Select(s => s.UnwrapSome());
                return new ReadOnlyCollectionSlim<IDatabaseSynonym>(synonymNames.Count, synonyms);
            }
        }

        public async Task<IReadOnlyCollection<Task<IDatabaseSynonym>>> SynonymsAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            var queryResult = await Connection.QueryAsync<QualifiedName>(SynonymsQuery).ConfigureAwait(false);
            var synonymNames = queryResult
                .Select(dto => Identifier.CreateQualifiedIdentifier(dto.DatabaseName, dto.SchemaName, dto.ObjectName))
                .ToList();

            var synonyms = synonymNames
                .Select(name => LoadSynonymAsync(name, cancellationToken))
                .Where(s => s.IsSome)
                .Select(s => s.UnwrapSome());
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

        protected virtual Option<IDatabaseSynonym> LoadSynonymSync(Identifier synonymName)
        {
            if (synonymName == null)
                throw new ArgumentNullException(nameof(synonymName));

            var resolvedSynonym = ResolveFirstExistingObjectName(synonymName, GetResolvedSynonymNameStrict);
            if (resolvedSynonym.IsNone)
                return Option<IDatabaseSynonym>.None;

            var resolvedSynonymName = resolvedSynonym.UnwrapSome();
            if (resolvedSynonymName.Database == DatabaseName && resolvedSynonymName.Schema == DefaultSchema)
                return LoadUserSynonymSync(resolvedSynonymName.LocalName);

            var queryResult = Connection.QueryFirstOrNone<SynonymData>(
                LoadSynonymQuery,
                new { SchemaName = resolvedSynonymName.Schema, SynonymName = resolvedSynonymName.LocalName }
            );

            return queryResult.Map<IDatabaseSynonym>(synonymData =>
            {
                var databaseName = !synonymData.TargetDatabaseName.IsNullOrWhiteSpace() ? synonymData.TargetDatabaseName : null;
                var schemaName = !synonymData.TargetSchemaName.IsNullOrWhiteSpace() ? synonymData.TargetSchemaName : null;
                var localName = !synonymData.TargetObjectName.IsNullOrWhiteSpace() ? synonymData.TargetObjectName : null;

                var qualifiedSynonymName = CreateQualifiedIdentifier(synonymName);
                var targetName = Identifier.CreateQualifiedIdentifier(databaseName, schemaName, localName);
                var qualifiedTargetName = CreateQualifiedIdentifier(targetName);

                return new DatabaseSynonym(qualifiedSynonymName, qualifiedTargetName);
            });
        }

        private Option<IDatabaseSynonym> LoadUserSynonymSync(string synonymName)
        {
            var queryResult = Connection.QueryFirstOrNone<SynonymData>(
                LoadUserSynonymQuery,
                new { SynonymName = synonymName }
            );

            return queryResult.Map<IDatabaseSynonym>(synonymData =>
            {
                var databaseName = !synonymData.TargetDatabaseName.IsNullOrWhiteSpace() ? synonymData.TargetDatabaseName : null;
                var schemaName = !synonymData.TargetSchemaName.IsNullOrWhiteSpace() ? synonymData.TargetSchemaName : null;
                var localName = !synonymData.TargetObjectName.IsNullOrWhiteSpace() ? synonymData.TargetObjectName : null;

                var qualifiedSynonymName = CreateQualifiedIdentifier(synonymName);
                var targetName = Identifier.CreateQualifiedIdentifier(databaseName, schemaName, localName);
                var qualifiedTargetName = CreateQualifiedIdentifier(targetName);

                return new DatabaseSynonym(qualifiedSynonymName, qualifiedTargetName);
            });
        }

        protected virtual Task<Option<IDatabaseSynonym>> LoadSynonymAsync(Identifier synonymName, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (synonymName == null)
                throw new ArgumentNullException(nameof(synonymName));

            return LoadSynonymAsyncCore(synonymName, cancellationToken);
        }

        private async Task<Option<IDatabaseSynonym>> LoadSynonymAsyncCore(Identifier synonymName, CancellationToken cancellationToken)
        {
            var resolvedSynonymNameOption = await ResolveFirstExistingObjectNameAsync(synonymName, GetResolvedSynonymNameStrictAsync, cancellationToken).ConfigureAwait(false);
            if (resolvedSynonymNameOption.IsNone)
                return Option<IDatabaseSynonym>.None;

            var resolvedSynonymName = resolvedSynonymNameOption.UnwrapSome();
            if (resolvedSynonymName.Database == DatabaseName && resolvedSynonymName.Schema == DefaultSchema)
                return await LoadUserSynonymAsyncCore(resolvedSynonymName.LocalName, cancellationToken).ConfigureAwait(false);

            var queryResult = await Connection.QueryFirstOrNoneAsync<SynonymData>(
                LoadSynonymQuery,
                new { SchemaName = resolvedSynonymName.Schema, SynonymName = resolvedSynonymName.LocalName }
            ).ConfigureAwait(false);

            return queryResult.Map<IDatabaseSynonym>(synonymData =>
            {
                var databaseName = !synonymData.TargetDatabaseName.IsNullOrWhiteSpace() ? synonymData.TargetDatabaseName : null;
                var schemaName = !synonymData.TargetSchemaName.IsNullOrWhiteSpace() ? synonymData.TargetSchemaName : null;
                var localName = !synonymData.TargetObjectName.IsNullOrWhiteSpace() ? synonymData.TargetObjectName : null;

                var qualifiedSynonymName = CreateQualifiedIdentifier(synonymName);
                var targetName = Identifier.CreateQualifiedIdentifier(databaseName, schemaName, localName);
                var qualifiedTargetName = CreateQualifiedIdentifier(targetName);

                return new DatabaseSynonym(qualifiedSynonymName, qualifiedTargetName);
            });
        }

        private async Task<Option<IDatabaseSynonym>> LoadUserSynonymAsyncCore(string synonymName, CancellationToken cancellationToken)
        {
            var queryResult = await Connection.QueryFirstOrNoneAsync<SynonymData>(
                LoadUserSynonymQuery,
                new { SynonymName = synonymName }
            ).ConfigureAwait(false);

            return queryResult.Map<IDatabaseSynonym>(synonymData =>
            {
                var databaseName = !synonymData.TargetDatabaseName.IsNullOrWhiteSpace() ? synonymData.TargetDatabaseName : null;
                var schemaName = !synonymData.TargetSchemaName.IsNullOrWhiteSpace() ? synonymData.TargetSchemaName : null;
                var localName = !synonymData.TargetObjectName.IsNullOrWhiteSpace() ? synonymData.TargetObjectName : null;

                var qualifiedSynonymName = CreateQualifiedIdentifier(synonymName);
                var targetName = Identifier.CreateQualifiedIdentifier(databaseName, schemaName, localName);
                var qualifiedTargetName = CreateQualifiedIdentifier(targetName);

                return new DatabaseSynonym(qualifiedSynonymName, qualifiedTargetName);
            });
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

        protected Option<Identifier> ResolveFirstExistingObjectName(Identifier objectName, Func<Identifier, Option<Identifier>> objectExistsFunc)
        {
            if (objectName == null)
                throw new ArgumentNullException(nameof(objectName));
            if (objectExistsFunc == null)
                throw new ArgumentNullException(nameof(objectExistsFunc));

            var resolvedNames = IdentifierResolver
                .GetResolutionOrder(objectName)
                .Select(CreateQualifiedIdentifier);

            return resolvedNames
                .Select(objectExistsFunc)
                .FirstOrDefault(name => name.IsSome);
        }

        protected Task<Option<Identifier>> ResolveFirstExistingObjectNameAsync(Identifier objectName, Func<Identifier, CancellationToken, Task<Option<Identifier>>> objectExistsFunc, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (objectName == null)
                throw new ArgumentNullException(nameof(objectName));
            if (objectExistsFunc == null)
                throw new ArgumentNullException(nameof(objectExistsFunc));

            return ResolveFirstExistingObjectNameAsyncCore(objectName, objectExistsFunc, cancellationToken);
        }

        private async Task<Option<Identifier>> ResolveFirstExistingObjectNameAsyncCore(Identifier objectName, Func<Identifier, CancellationToken, Task<Option<Identifier>>> objectExistsFunc, CancellationToken cancellationToken)
        {
            var resolvedNames = IdentifierResolver
                .GetResolutionOrder(objectName)
                .Select(CreateQualifiedIdentifier);

            foreach (var resolvedName in resolvedNames)
            {
                var existingName = await objectExistsFunc(resolvedName, cancellationToken).ConfigureAwait(false);
                if (existingName.IsSome)
                    return existingName;
            }

            return Option<Identifier>.None;
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
            var hostInfoTask = Connection.QueryFirstOrNoneAsync<DatabaseHost>(hostSql);

            const string versionSql = @"
select
    PRODUCT as ProductName,
    VERSION as VersionNumber
from PRODUCT_COMPONENT_VERSION
where PRODUCT like 'Oracle Database%'";
            var versionInfoTask = Connection.QueryFirstOrNoneAsync<DatabaseVersion>(versionSql);

            await Task.WhenAll(hostInfoTask, versionInfoTask).ConfigureAwait(false);

            var hostInfo = hostInfoTask.GetAwaiter().GetResult();
            var versionInfo = versionInfoTask.GetAwaiter().GetResult();

            var qualifiedServerName = hostInfo.MatchUnsafe(
                dbHost => dbHost.ServerHost + "/" + dbHost.ServerSid,
                () => null
            );
            var dbName = hostInfo.MatchUnsafe(h => h.DatabaseName, () => null);
            var defaultSchema = hostInfo.MatchUnsafe(h => h.DefaultSchema, () => null);
            var versionName = versionInfo.MatchUnsafe(
                vInfo => vInfo.ProductName + vInfo.VersionNumber,
                () => null
            );

            return new DatabaseMetadata
            {
                 ServerName = qualifiedServerName,
                 DatabaseName = dbName,
                 DefaultSchema = defaultSchema,
                 DatabaseVersion = versionName
            };
        }

        private readonly AsyncLazy<DatabaseMetadata> _metadata;
    }
}
