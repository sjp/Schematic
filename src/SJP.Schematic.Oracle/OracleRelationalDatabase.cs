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
    public class OracleRelationalDatabase : RelationalDatabase, IDependentRelationalDatabase
    {
        public OracleRelationalDatabase(IDatabaseDialect dialect, IDbConnection connection, INameResolverStrategy nameResolver = null)
            : base(dialect, connection)
        {
            _metadata = new AsyncLazy<DatabaseMetadata>(LoadDatabaseMetadataAsync);
            NameResolver = nameResolver ?? new DefaultOracleNameResolverStrategy();
            _parentDb = this;
        }

        public IRelationalDatabase Parent
        {
            get => _parentDb;
            set => _parentDb = value ?? throw new ArgumentNullException(nameof(value));
        }

        protected INameResolverStrategy NameResolver { get; }

        protected IRelationalDatabase Database => Parent;

        public string ServerName => Metadata.ServerName;

        public string DatabaseName => Metadata.DatabaseName;

        public string DefaultSchema => Metadata.DefaultSchema;

        public string DatabaseVersion => Metadata.DatabaseVersion;

        protected DatabaseMetadata Metadata => _metadata.Task.GetAwaiter().GetResult();

        public bool TableExists(Identifier tableName)
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));

            var resolvedName = ResolveFirstObjectExistsName(tableName, TableExistsStrict);
            return resolvedName != null;
        }

        public Task<bool> TableExistsAsync(Identifier tableName, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));

            return TableExistsAsyncCore(tableName, cancellationToken);
        }

        private async Task<bool> TableExistsAsyncCore(Identifier tableName, CancellationToken cancellationToken)
        {
            var resolvedName = await ResolveFirstObjectExistsNameAsync(tableName, TableExistsStrictAsync, cancellationToken).ConfigureAwait(false);
            return resolvedName != null;
        }

        protected bool TableExistsStrict(Identifier tableName)
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));

            tableName = CreateQualifiedIdentifier(tableName);

            return Connection.ExecuteScalar<int>(
                TableExistsQuery,
                new { SchemaName = tableName.Schema, TableName = tableName.LocalName }
            ) != 0;
        }

        protected Task<bool> TableExistsStrictAsync(Identifier tableName, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));

            return TableExistsStrictAsyncCore(tableName, cancellationToken);
        }

        private async Task<bool> TableExistsStrictAsyncCore(Identifier tableName, CancellationToken cancellationToken)
        {
            tableName = CreateQualifiedIdentifier(tableName);

            return await Connection.ExecuteScalarAsync<int>(
                TableExistsQuery,
                new { SchemaName = tableName.Schema, TableName = tableName.LocalName }
            ).ConfigureAwait(false) != 0;
        }

        protected virtual string TableExistsQuery => TableExistsQuerySql;

        private const string TableExistsQuerySql = @"
select COUNT(*)
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

            tableName = ResolveFirstObjectExistsName(tableName, TableExists);
            return tableName != null
                ? new OracleRelationalDatabaseTable(Connection, Database, tableName, NameResolver)
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
            tableName = await ResolveFirstObjectExistsNameAsync(tableName, TableExistsAsync, cancellationToken).ConfigureAwait(false);
            return tableName != null
                ? new OracleRelationalDatabaseTable(Connection, Database, tableName, NameResolver)
                : null;
        }

        public bool ViewExists(Identifier viewName)
        {
            if (viewName == null)
                throw new ArgumentNullException(nameof(viewName));

            var resolvedName = ResolveFirstObjectExistsName(viewName, ViewExistsStrict);
            return resolvedName != null;
        }

        public Task<bool> ViewExistsAsync(Identifier viewName, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (viewName == null)
                throw new ArgumentNullException(nameof(viewName));

            return ViewExistsAsyncCore(viewName, cancellationToken);
        }

        private async Task<bool> ViewExistsAsyncCore(Identifier viewName, CancellationToken cancellationToken)
        {
            var resolvedName = await ResolveFirstObjectExistsNameAsync(viewName, ViewExistsStrictAsync, cancellationToken).ConfigureAwait(false);
            return resolvedName != null;
        }

        protected bool ViewExistsStrict(Identifier viewName)
        {
            if (viewName == null)
                throw new ArgumentNullException(nameof(viewName));

            viewName = CreateQualifiedIdentifier(viewName);

            return Connection.ExecuteScalar<int>(
                ViewExistsQuery,
                new { SchemaName = viewName.Schema, ViewName = viewName.LocalName }
            ) != 0;
        }

        protected Task<bool> ViewExistsStrictAsync(Identifier viewName, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (viewName == null)
                throw new ArgumentNullException(nameof(viewName));

            return ViewExistsStrictAsyncCore(viewName, cancellationToken);
        }

        private async Task<bool> ViewExistsStrictAsyncCore(Identifier viewName, CancellationToken cancellationToken)
        {
            viewName = CreateQualifiedIdentifier(viewName);

            return await Connection.ExecuteScalarAsync<int>(
                ViewExistsQuery,
                new { SchemaName = viewName.Schema, ViewName = viewName.LocalName }
            ).ConfigureAwait(false) != 0;
        }

        protected virtual string ViewExistsQuery => ViewExistsQuerySql;

        private const string ViewExistsQuerySql = @"
select COUNT(*)
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

            viewName = ResolveFirstObjectExistsName(viewName, ViewExists);
            return viewName != null
                ? new OracleRelationalDatabaseView(Connection, Database, viewName, NameResolver)
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
            viewName = await ResolveFirstObjectExistsNameAsync(viewName, ViewExistsAsync, cancellationToken).ConfigureAwait(false);
            return viewName != null
                ? new OracleRelationalDatabaseView(Connection, Database, viewName, NameResolver)
                : null;
        }

        public bool SequenceExists(Identifier sequenceName)
        {
            if (sequenceName == null)
                throw new ArgumentNullException(nameof(sequenceName));

            var resolvedName = ResolveFirstObjectExistsName(sequenceName, SequenceExistsStrict);
            return resolvedName != null;
        }

        public Task<bool> SequenceExistsAsync(Identifier sequenceName, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (sequenceName == null)
                throw new ArgumentNullException(nameof(sequenceName));

            return SequenceExistsAsyncCore(sequenceName, cancellationToken);
        }

        private async Task<bool> SequenceExistsAsyncCore(Identifier sequenceName, CancellationToken cancellationToken)
        {
            var resolvedName = await ResolveFirstObjectExistsNameAsync(sequenceName, SequenceExistsStrictAsync, cancellationToken).ConfigureAwait(false);
            return resolvedName != null;
        }

        protected bool SequenceExistsStrict(Identifier sequenceName)
        {
            if (sequenceName == null)
                throw new ArgumentNullException(nameof(sequenceName));

            sequenceName = CreateQualifiedIdentifier(sequenceName);

            return Connection.ExecuteScalar<int>(
                SequenceExistsQuery,
                new { SchemaName = sequenceName.Schema, SequenceName = sequenceName.LocalName }
            ) != 0;
        }

        protected Task<bool> SequenceExistsStrictAsync(Identifier sequenceName, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (sequenceName == null)
                throw new ArgumentNullException(nameof(sequenceName));

            return SequenceExistsStrictAsyncCore(sequenceName, cancellationToken);
        }

        private async Task<bool> SequenceExistsStrictAsyncCore(Identifier sequenceName, CancellationToken cancellationToken)
        {
            sequenceName = CreateQualifiedIdentifier(sequenceName);

            return await Connection.ExecuteScalarAsync<int>(
                SequenceExistsQuery,
                new { SchemaName = sequenceName.Schema, SequenceName = sequenceName.LocalName }
            ).ConfigureAwait(false) != 0;
        }

        protected virtual string SequenceExistsQuery => SequenceExistsQuerySql;

        private const string SequenceExistsQuerySql = @"
select COUNT(*)
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

            sequenceName = ResolveFirstObjectExistsName(sequenceName, SequenceExists);
            return sequenceName != null
                ? new OracleDatabaseSequence(Connection, Database, sequenceName)
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
            sequenceName = await ResolveFirstObjectExistsNameAsync(sequenceName, SequenceExistsAsync, cancellationToken).ConfigureAwait(false);
            return sequenceName != null
                ? new OracleDatabaseSequence(Connection, Database, sequenceName)
                : null;
        }

        public bool SynonymExists(Identifier synonymName)
        {
            if (synonymName == null)
                throw new ArgumentNullException(nameof(synonymName));

            var resolvedName = ResolveFirstObjectExistsName(synonymName, SynonymExistsStrict);
            return resolvedName != null;
        }

        public Task<bool> SynonymExistsAsync(Identifier synonymName, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (synonymName == null)
                throw new ArgumentNullException(nameof(synonymName));

            return SynonymExistsAsyncCore(synonymName, cancellationToken);
        }

        private async Task<bool> SynonymExistsAsyncCore(Identifier synonymName, CancellationToken cancellationToken)
        {
            var resolvedName = await ResolveFirstObjectExistsNameAsync(synonymName, SynonymExistsStrictAsync, cancellationToken).ConfigureAwait(false);
            return resolvedName != null;
        }

        protected bool SynonymExistsStrict(Identifier synonymName)
        {
            if (synonymName == null)
                throw new ArgumentNullException(nameof(synonymName));

            synonymName = CreateQualifiedIdentifier(synonymName);

            return Connection.ExecuteScalar<int>(
                SynonymExistsQuery,
                new { SchemaName = synonymName.Schema, SynonymName = synonymName.LocalName }
            ) != 0;
        }

        protected Task<bool> SynonymExistsStrictAsync(Identifier synonymName, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (synonymName == null)
                throw new ArgumentNullException(nameof(synonymName));

            return SynonymExistsStrictAsyncCore(synonymName, cancellationToken);
        }

        private async Task<bool> SynonymExistsStrictAsyncCore(Identifier synonymName, CancellationToken cancellationToken)
        {
            synonymName = CreateQualifiedIdentifier(synonymName);

            return await Connection.ExecuteScalarAsync<int>(
                SynonymExistsQuery,
                new { SchemaName = synonymName.Schema, SynonymName = synonymName.LocalName }
            ).ConfigureAwait(false) != 0;
        }

        protected virtual string SynonymExistsQuery => SynonymExistsQuerySql;

        private const string SynonymExistsQuerySql = @"
select COUNT(*)
from ALL_SYNONYMS s
inner join ALL_OBJECTS o on s.OWNER = o.OWNER and s.SYNONYM_NAME = o.OBJECT_NAME
where s.OWNER = :SchemaName and s.SYNONYM_NAME = :SynonymName and o.ORACLE_MAINTAINED <> 'Y'";

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

            synonymName = ResolveFirstObjectExistsName(synonymName, SynonymExistsStrict);
            if (synonymName == null)
                return null;

            var queryResult = Connection.QuerySingle<SynonymData>(
                LoadSynonymQuery,
                new { SchemaName = synonymName.Schema, SynonymName = synonymName.LocalName }
            );

            var databaseName = !queryResult.TargetDatabaseName.IsNullOrWhiteSpace() ? queryResult.TargetDatabaseName : null;
            var schemaName = !queryResult.TargetSchemaName.IsNullOrWhiteSpace() ? queryResult.TargetSchemaName : null;
            var localName = !queryResult.TargetObjectName.IsNullOrWhiteSpace() ? queryResult.TargetObjectName : null;

            var targetName = Identifier.CreateQualifiedIdentifier(databaseName, schemaName, localName);
            targetName = CreateQualifiedIdentifier(targetName);

            return new OracleDatabaseSynonym(Database, synonymName, targetName);
        }

        protected virtual Task<IDatabaseSynonym> LoadSynonymAsync(Identifier synonymName, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (synonymName == null)
                throw new ArgumentNullException(nameof(synonymName));

            return LoadSynonymAsyncCore(synonymName, cancellationToken);
        }

        private async Task<IDatabaseSynonym> LoadSynonymAsyncCore(Identifier synonymName, CancellationToken cancellationToken)
        {
            synonymName = await ResolveFirstObjectExistsNameAsync(synonymName, SynonymExistsStrictAsync, cancellationToken).ConfigureAwait(false);
            if (synonymName == null)
                return null;

            var queryResult = await Connection.QuerySingleAsync<SynonymData>(
                LoadSynonymQuery,
                new { SchemaName = synonymName.Schema, SynonymName = synonymName.LocalName }
            ).ConfigureAwait(false);

            var databaseName = !queryResult.TargetDatabaseName.IsNullOrWhiteSpace() ? queryResult.TargetDatabaseName : null;
            var schemaName = !queryResult.TargetSchemaName.IsNullOrWhiteSpace() ? queryResult.TargetSchemaName : null;
            var localName = !queryResult.TargetObjectName.IsNullOrWhiteSpace() ? queryResult.TargetObjectName : null;

            var targetName = Identifier.CreateQualifiedIdentifier(databaseName, schemaName, localName);
            targetName = CreateQualifiedIdentifier(targetName);

            return new OracleDatabaseSynonym(Database, synonymName, targetName);
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

        protected Identifier ResolveFirstObjectExistsName(Identifier objectName, Func<Identifier, bool> objectExistsFunc)
        {
            if (objectName == null)
                throw new ArgumentNullException(nameof(objectName));
            if (objectExistsFunc == null)
                throw new ArgumentNullException(nameof(objectExistsFunc));

            var resolvedNames = NameResolver
                .GetResolutionOrder(objectName)
                .Select(CreateQualifiedIdentifier);
            return resolvedNames.FirstOrDefault(objectExistsFunc);
        }

        protected Task<Identifier> ResolveFirstObjectExistsNameAsync(Identifier objectName, Func<Identifier, CancellationToken, Task<bool>> objectExistsFunc, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (objectName == null)
                throw new ArgumentNullException(nameof(objectName));
            if (objectExistsFunc == null)
                throw new ArgumentNullException(nameof(objectExistsFunc));

            return ResolveFirstObjectExistsNameAsyncCore(objectName, objectExistsFunc, cancellationToken);
        }

        private async Task<Identifier> ResolveFirstObjectExistsNameAsyncCore(Identifier objectName, Func<Identifier, CancellationToken, Task<bool>> objectExistsFunc, CancellationToken cancellationToken)
        {
            var resolvedNames = NameResolver
                .GetResolutionOrder(objectName)
                .Select(CreateQualifiedIdentifier);

            foreach (var resolvedName in resolvedNames)
            {
                var exists = await objectExistsFunc(resolvedName, cancellationToken).ConfigureAwait(false);
                if (exists)
                    return resolvedName;
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
    USER as DefaultSchema
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

        private IRelationalDatabase _parentDb;
        private readonly AsyncLazy<DatabaseMetadata> _metadata;
    }
}
