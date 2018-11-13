using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SJP.Schematic.Core;
using SJP.Schematic.SqlServer.Query;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Core.Utilities;
using LanguageExt;

namespace SJP.Schematic.SqlServer
{
    public class SqlServerRelationalDatabase : RelationalDatabase, IRelationalDatabase
    {
        public SqlServerRelationalDatabase(IDatabaseDialect dialect, IDbConnection connection)
            : base(dialect, connection)
        {
            _metadata = new AsyncLazy<DatabaseMetadata>(LoadDatabaseMetadataAsync);
        }

        public string ServerName => Metadata.ServerName;

        public string DatabaseName => Metadata.DatabaseName;

        public string DefaultSchema => Metadata.DefaultSchema;

        public string DatabaseVersion => Metadata.DatabaseVersion;

        protected DatabaseMetadata Metadata => _metadata.Task.GetAwaiter().GetResult();

        protected Option<Identifier> GetResolvedTableName(Identifier tableName)
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));

            tableName = CreateQualifiedIdentifier(tableName);
            var qualifiedTableName = Connection.QueryFirstOrDefault<QualifiedName>(
                TableNameQuery,
                new { SchemaName = tableName.Schema, TableName = tableName.LocalName }
            );

            return qualifiedTableName != null
                ? Option<Identifier>.Some(Identifier.CreateQualifiedIdentifier(tableName.Server, tableName.Database, qualifiedTableName.SchemaName, qualifiedTableName.ObjectName))
                : Option<Identifier>.None;
        }

        protected Task<Option<Identifier>> GetResolvedTableNameAsync(Identifier tableName, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));

            return GetResolvedTableNameAsyncCore(tableName, cancellationToken);
        }

        private async Task<Option<Identifier>> GetResolvedTableNameAsyncCore(Identifier tableName, CancellationToken cancellationToken)
        {
            tableName = CreateQualifiedIdentifier(tableName);
            var qualifiedTableName = await Connection.QueryFirstOrDefaultAsync<QualifiedName>(
                TableNameQuery,
                new { SchemaName = tableName.Schema, TableName = tableName.LocalName }
            ).ConfigureAwait(false);

            return qualifiedTableName != null
                ? Option<Identifier>.Some(Identifier.CreateQualifiedIdentifier(tableName.Server, tableName.Database, qualifiedTableName.SchemaName, qualifiedTableName.ObjectName))
                : Option<Identifier>.None;
        }

        protected virtual string TableNameQuery => TableNameQuerySql;

        private const string TableNameQuerySql = @"
select top 1 schema_name(schema_id) as SchemaName, name as ObjectName
from sys.tables
where schema_id = schema_id(@SchemaName) and name = @TableName";

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

        private const string TablesQuerySql = "select schema_name(schema_id) as SchemaName, name as ObjectName from sys.tables order by schema_name(schema_id), name";

        protected virtual Option<IRelationalDatabaseTable> LoadTableSync(Identifier tableName)
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));

            tableName = CreateQualifiedIdentifier(tableName);
            return GetResolvedTableName(tableName)
                .Map<IRelationalDatabaseTable>(name => new SqlServerRelationalDatabaseTable(Connection, this, Dialect.TypeProvider, name));
        }

        protected virtual Task<Option<IRelationalDatabaseTable>> LoadTableAsync(Identifier tableName, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));

            return LoadTableAsyncCore(tableName, cancellationToken);
        }

        private async Task<Option<IRelationalDatabaseTable>> LoadTableAsyncCore(Identifier tableName, CancellationToken cancellationToken)
        {
            tableName = CreateQualifiedIdentifier(tableName);
            var qualifiedName = await GetResolvedTableNameAsync(tableName, cancellationToken).ConfigureAwait(false);

            return qualifiedName
                .Map<IRelationalDatabaseTable>(name => new SqlServerRelationalDatabaseTable(Connection, this, Dialect.TypeProvider, name));
        }

        protected Option<Identifier> GetResolvedViewName(Identifier viewName)
        {
            if (viewName == null)
                throw new ArgumentNullException(nameof(viewName));

            viewName = CreateQualifiedIdentifier(viewName);
            var qualifiedViewName = Connection.QueryFirstOrDefault<QualifiedName>(
                ViewNameQuery,
                new { SchemaName = viewName.Schema, ViewName = viewName.LocalName }
            );

            return qualifiedViewName != null
                ? Option<Identifier>.Some(Identifier.CreateQualifiedIdentifier(viewName.Server, viewName.Database, qualifiedViewName.SchemaName, qualifiedViewName.ObjectName))
                : Option<Identifier>.None;
        }

        protected Task<Option<Identifier>> GetResolvedViewNameAsync(Identifier viewName, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (viewName == null)
                throw new ArgumentNullException(nameof(viewName));

            return GetResolvedViewNameAsyncCore(viewName, cancellationToken);
        }

        private async Task<Option<Identifier>> GetResolvedViewNameAsyncCore(Identifier viewName, CancellationToken cancellationToken)
        {
            viewName = CreateQualifiedIdentifier(viewName);
            var qualifiedViewName = await Connection.QueryFirstOrDefaultAsync<QualifiedName>(
                ViewNameQuery,
                new { SchemaName = viewName.Schema, ViewName = viewName.LocalName }
            ).ConfigureAwait(false);

            return qualifiedViewName != null
                ? Option<Identifier>.Some(Identifier.CreateQualifiedIdentifier(viewName.Server, viewName.Database, qualifiedViewName.SchemaName, qualifiedViewName.ObjectName))
                : Option<Identifier>.None;
        }

        protected virtual string ViewNameQuery => ViewNameQuerySql;

        private const string ViewNameQuerySql = @"
select top 1 schema_name(schema_id) as SchemaName, name as ObjectName
from sys.views
where schema_id = schema_id(@SchemaName) and name = @ViewName";

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

        private const string ViewsQuerySql = "select schema_name(schema_id) as SchemaName, name as ObjectName from sys.views order by schema_name(schema_id), name";

        protected virtual Option<IRelationalDatabaseView> LoadViewSync(Identifier viewName)
        {
            if (viewName == null)
                throw new ArgumentNullException(nameof(viewName));

            viewName = CreateQualifiedIdentifier(viewName);
            return GetResolvedViewName(viewName)
                .Map<IRelationalDatabaseView>(name => new SqlServerRelationalDatabaseView(Connection, Dialect.TypeProvider, name));
        }

        protected virtual Task<Option<IRelationalDatabaseView>> LoadViewAsync(Identifier viewName, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (viewName == null)
                throw new ArgumentNullException(nameof(viewName));

            return LoadViewAsyncCore(viewName, cancellationToken);
        }

        private async Task<Option<IRelationalDatabaseView>> LoadViewAsyncCore(Identifier viewName, CancellationToken cancellationToken)
        {
            viewName = CreateQualifiedIdentifier(viewName);
            var qualifiedName = await GetResolvedViewNameAsync(viewName, cancellationToken).ConfigureAwait(false);

            return qualifiedName
                .Map<IRelationalDatabaseView>(name => new SqlServerRelationalDatabaseView(Connection, Dialect.TypeProvider, name));
        }

        protected Option<Identifier> GetResolvedSequenceName(Identifier sequenceName)
        {
            if (sequenceName == null)
                throw new ArgumentNullException(nameof(sequenceName));

            sequenceName = CreateQualifiedIdentifier(sequenceName);
            var qualifiedSequenceName = Connection.QueryFirstOrDefault<QualifiedName>(
                SequenceNameQuery,
                new { SchemaName = sequenceName.Schema, SequenceName = sequenceName.LocalName }
            );

            return qualifiedSequenceName != null
                ? Option<Identifier>.Some(Identifier.CreateQualifiedIdentifier(sequenceName.Server, sequenceName.Database, qualifiedSequenceName.SchemaName, qualifiedSequenceName.ObjectName))
                : Option<Identifier>.None;
        }

        protected Task<Option<Identifier>> GetResolvedSequenceNameAsync(Identifier sequenceName, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (sequenceName == null)
                throw new ArgumentNullException(nameof(sequenceName));

            return GetResolvedSequenceNameAsyncCore(sequenceName, cancellationToken);
        }

        private async Task<Option<Identifier>> GetResolvedSequenceNameAsyncCore(Identifier sequenceName, CancellationToken cancellationToken)
        {
            sequenceName = CreateQualifiedIdentifier(sequenceName);
            var qualifiedSequenceName = await Connection.QueryFirstOrDefaultAsync<QualifiedName>(
                SequenceNameQuery,
                new { SchemaName = sequenceName.Schema, SequenceName = sequenceName.LocalName }
            ).ConfigureAwait(false);

            return qualifiedSequenceName != null
                ? Option<Identifier>.Some(Identifier.CreateQualifiedIdentifier(sequenceName.Server, sequenceName.Database, qualifiedSequenceName.SchemaName, qualifiedSequenceName.ObjectName))
                : Option<Identifier>.None;
        }

        protected virtual string SequenceNameQuery => SequenceNameQuerySql;

        private const string SequenceNameQuerySql = @"
select top 1 schema_name(schema_id) as SchemaName, name as ObjectName
from sys.sequences
where schema_id = schema_id(@SchemaName) and name = @SequenceName";

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

        private const string SequencesQuerySql = "select schema_name(schema_id) as SchemaName, name as ObjectName from sys.sequences order by schema_name(schema_id), name";

        protected virtual Option<IDatabaseSequence> LoadSequenceSync(Identifier sequenceName)
        {
            if (sequenceName == null)
                throw new ArgumentNullException(nameof(sequenceName));

            sequenceName = CreateQualifiedIdentifier(sequenceName);
            return GetResolvedSequenceName(sequenceName)
                .Map<IDatabaseSequence>(name => new SqlServerDatabaseSequence(Connection, name));
        }

        protected virtual Task<Option<IDatabaseSequence>> LoadSequenceAsync(Identifier sequenceName, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (sequenceName == null)
                throw new ArgumentNullException(nameof(sequenceName));

            return LoadSequenceAsyncCore(sequenceName, cancellationToken);
        }

        private async Task<Option<IDatabaseSequence>> LoadSequenceAsyncCore(Identifier sequenceName, CancellationToken cancellationToken)
        {
            sequenceName = CreateQualifiedIdentifier(sequenceName);
            var qualifiedSequenceName = await GetResolvedSequenceNameAsync(sequenceName, cancellationToken).ConfigureAwait(false);

            return qualifiedSequenceName
                .Map<IDatabaseSequence>(name => new SqlServerDatabaseSequence(Connection, name));
        }

        protected Option<Identifier> GetResolvedSynonymName(Identifier synonymName)
        {
            if (synonymName == null)
                throw new ArgumentNullException(nameof(synonymName));

            synonymName = CreateQualifiedIdentifier(synonymName);
            var qualifiedSynonymName = Connection.QueryFirstOrDefault<QualifiedName>(
                SynonymNameQuery,
                new { SchemaName = synonymName.Schema, SynonymName = synonymName.LocalName }
            );

            return qualifiedSynonymName != null
                ? Option<Identifier>.Some(Identifier.CreateQualifiedIdentifier(synonymName.Server, synonymName.Database, qualifiedSynonymName.SchemaName, qualifiedSynonymName.ObjectName))
                : Option<Identifier>.None;
        }

        protected Task<Option<Identifier>> GetResolvedSynonymNameAsync(Identifier synonymName, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (synonymName == null)
                throw new ArgumentNullException(nameof(synonymName));

            return GetResolvedSynonymNameAsyncCore(synonymName, cancellationToken);
        }

        private async Task<Option<Identifier>> GetResolvedSynonymNameAsyncCore(Identifier synonymName, CancellationToken cancellationToken)
        {
            synonymName = CreateQualifiedIdentifier(synonymName);
            var qualifiedSynonymName = await Connection.QueryFirstOrDefaultAsync<QualifiedName>(
                SynonymNameQuery,
                new { SchemaName = synonymName.Schema, SynonymName = synonymName.LocalName }
            ).ConfigureAwait(false);

            return qualifiedSynonymName != null
                ? Option<Identifier>.Some(Identifier.CreateQualifiedIdentifier(synonymName.Server, synonymName.Database, qualifiedSynonymName.SchemaName, qualifiedSynonymName.ObjectName))
                : Option<Identifier>.None;
        }

        protected virtual string SynonymNameQuery => SynonymNameQuerySql;

        private const string SynonymNameQuerySql = @"
select top 1 schema_name(schema_id) as SchemaName, name as ObjectName
from sys.synonyms
where schema_id = schema_id(@SchemaName) and name = @SynonymName";

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
                    .Select(dto => Identifier.CreateQualifiedIdentifier(dto.SchemaName, dto.ObjectName))
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
                .Select(dto => Identifier.CreateQualifiedIdentifier(dto.SchemaName, dto.ObjectName))
                .ToList();

            var synonyms = synonymNames
                .Select(name => LoadSynonymAsync(name, cancellationToken))
                .Where(s => s.IsSome)
                .Select(s => s.UnwrapSome());
            return new ReadOnlyCollectionSlim<Task<IDatabaseSynonym>>(synonymNames.Count, synonyms);
        }

        protected virtual string SynonymsQuery => SynonymsQuerySql;

        private const string SynonymsQuerySql = "select schema_name(schema_id) as SchemaName, name as ObjectName from sys.synonyms order by schema_name(schema_id), name";

        protected virtual Option<IDatabaseSynonym> LoadSynonymSync(Identifier synonymName)
        {
            if (synonymName == null)
                throw new ArgumentNullException(nameof(synonymName));

            synonymName = CreateQualifiedIdentifier(synonymName);
            var resolvedSynonymNameOption = GetResolvedSynonymName(synonymName);
            if (resolvedSynonymNameOption.IsNone)
                return Option<IDatabaseSynonym>.None;

            var resolvedSynonymName = resolvedSynonymNameOption.UnwrapSome();
            var queryResult = Connection.QuerySingle<SynonymData>(
                LoadSynonymQuery,
                new { SchemaName = resolvedSynonymName.Schema, SynonymName = resolvedSynonymName.LocalName }
            );

            var serverName = !queryResult.TargetServerName.IsNullOrWhiteSpace() ? queryResult.TargetServerName : null;
            var databaseName = !queryResult.TargetDatabaseName.IsNullOrWhiteSpace() ? queryResult.TargetDatabaseName : null;
            var schemaName = !queryResult.TargetSchemaName.IsNullOrWhiteSpace() ? queryResult.TargetSchemaName : null;
            var localName = !queryResult.TargetObjectName.IsNullOrWhiteSpace() ? queryResult.TargetObjectName : null;

            var targetName = Identifier.CreateQualifiedIdentifier(serverName, databaseName, schemaName, localName);
            targetName = CreateQualifiedIdentifier(targetName);

            return Option<IDatabaseSynonym>.Some(new DatabaseSynonym(resolvedSynonymName, targetName));
        }

        protected virtual Task<Option<IDatabaseSynonym>> LoadSynonymAsync(Identifier synonymName, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (synonymName == null)
                throw new ArgumentNullException(nameof(synonymName));

            return LoadSynonymAsyncCore(synonymName, cancellationToken);
        }

        private async Task<Option<IDatabaseSynonym>> LoadSynonymAsyncCore(Identifier synonymName, CancellationToken cancellationToken)
        {
            synonymName = CreateQualifiedIdentifier(synonymName);
            var resolvedSynonymNameOption = await GetResolvedSynonymNameAsync(synonymName, cancellationToken).ConfigureAwait(false);

            if (resolvedSynonymNameOption.IsNone)
                return Option<IDatabaseSynonym>.None;

            var resolvedSynonymName = resolvedSynonymNameOption.UnwrapSome();
            var queryResult = await Connection.QueryFirstOrDefaultAsync<SynonymData>(
                LoadSynonymQuery,
                new { SchemaName = resolvedSynonymName.Schema, SynonymName = resolvedSynonymName.LocalName }
            ).ConfigureAwait(false);

            var serverName = !queryResult.TargetServerName.IsNullOrWhiteSpace() ? queryResult.TargetServerName : null;
            var databaseName = !queryResult.TargetDatabaseName.IsNullOrWhiteSpace() ? queryResult.TargetDatabaseName : null;
            var schemaName = !queryResult.TargetSchemaName.IsNullOrWhiteSpace() ? queryResult.TargetSchemaName : null;
            var localName = !queryResult.TargetObjectName.IsNullOrWhiteSpace() ? queryResult.TargetObjectName : null;

            var targetName = Identifier.CreateQualifiedIdentifier(serverName, databaseName, schemaName, localName);
            targetName = CreateQualifiedIdentifier(targetName);

            return Option<IDatabaseSynonym>.Some(new DatabaseSynonym(resolvedSynonymName, targetName));
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

        private Task<DatabaseMetadata> LoadDatabaseMetadataAsync()
        {
            const string sql = "select @@SERVERNAME as ServerName, db_name() as DatabaseName, schema_name() as DefaultSchema, @@version as DatabaseVersion";
            return Connection.QuerySingleAsync<DatabaseMetadata>(sql);
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

        private readonly AsyncLazy<DatabaseMetadata> _metadata;
    }
}
