using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Utilities;
using SJP.Schematic.MySql.Query;
using LanguageExt;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.MySql
{
    public class MySqlRelationalDatabase : RelationalDatabase, IRelationalDatabase
    {
        public MySqlRelationalDatabase(IDatabaseDialect dialect, IDbConnection connection)
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
            var qualifiedTableName = Connection.QueryFirstOrNone<QualifiedName>(
                TableNameQuery,
                new { SchemaName = tableName.Schema, TableName = tableName.LocalName }
            );

            return qualifiedTableName.Map(name => Identifier.CreateQualifiedIdentifier(tableName.Server, tableName.Database, name.SchemaName, name.ObjectName));
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

            var qualifiedTableName = await Connection.QueryFirstOrNoneAsync<QualifiedName>(
                TableNameQuery,
                new { SchemaName = tableName.Schema, TableName = tableName.LocalName }
            ).ConfigureAwait(false);

            return qualifiedTableName.Map(name => Identifier.CreateQualifiedIdentifier(tableName.Server, tableName.Database, name.SchemaName, name.ObjectName));
        }

        protected virtual string TableNameQuery => TableNameQuerySql;

        private const string TableNameQuerySql = @"
select table_schema as SchemaName, table_name as ObjectName
from information_schema.tables
where table_schema = @SchemaName and table_name = @TableName
limit 1";

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
                var tableNames = Connection.Query<QualifiedName>(TablesQuery, new { SchemaName = DefaultSchema })
                    .Select(dto => Identifier.CreateQualifiedIdentifier(dto.SchemaName, dto.ObjectName))
                    .ToList();

                var tables = tableNames
                    .Select(LoadTableSync)
                    .Somes();
                return new ReadOnlyCollectionSlim<IRelationalDatabaseTable>(tableNames.Count, tables);
            }
        }

        public async Task<IReadOnlyCollection<Task<IRelationalDatabaseTable>>> TablesAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            var queryResults = await Connection.QueryAsync<QualifiedName>(TablesQuery, new { SchemaName = DefaultSchema }).ConfigureAwait(false);
            var tableNames = queryResults
                .Select(dto => Identifier.CreateQualifiedIdentifier(dto.SchemaName, dto.ObjectName))
                .ToList();

            var tables = tableNames
                .Select(name => LoadTableAsync(name, cancellationToken))
                .Somes();
            return new ReadOnlyCollectionSlim<Task<IRelationalDatabaseTable>>(tableNames.Count, tables);
        }

        protected virtual string TablesQuery => TablesQuerySql;

        private const string TablesQuerySql = @"
select
    TABLE_SCHEMA as SchemaName,
    TABLE_NAME as ObjectName
from information_schema.tables
where TABLE_SCHEMA = @SchemaName order by TABLE_NAME";

        protected virtual Option<IRelationalDatabaseTable> LoadTableSync(Identifier tableName)
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));

            tableName = CreateQualifiedIdentifier(tableName);
            return GetResolvedTableName(tableName)
                .Map<IRelationalDatabaseTable>(name => new MySqlRelationalDatabaseTable(Connection, this, Dialect.TypeProvider, name));
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
            var qualifiedTableName = await GetResolvedTableNameAsync(tableName, cancellationToken).ConfigureAwait(false);

            return qualifiedTableName.Map<IRelationalDatabaseTable>(name => new MySqlRelationalDatabaseTable(Connection, this, Dialect.TypeProvider, name));
        }

        protected Option<Identifier> GetResolvedViewName(Identifier viewName)
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

        protected Task<Option<Identifier>> GetResolvedViewNameAsync(Identifier viewName, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (viewName == null)
                throw new ArgumentNullException(nameof(viewName));

            return GetResolvedViewNameAsyncCore(viewName, cancellationToken);
        }

        private async Task<Option<Identifier>> GetResolvedViewNameAsyncCore(Identifier viewName, CancellationToken cancellationToken)
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
select table_schema as SchemaName, table_name as ObjectName
from information_schema.views
where table_schema = @SchemaName and table_name = @ViewName
limit 1";

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
                var viewNames = Connection.Query<QualifiedName>(ViewsQuery, new { SchemaName = DefaultSchema })
                    .Select(dto => Identifier.CreateQualifiedIdentifier(dto.SchemaName, dto.ObjectName))
                    .ToList();

                var views = viewNames
                    .Select(LoadViewSync)
                    .Somes();
                return new ReadOnlyCollectionSlim<IRelationalDatabaseView>(viewNames.Count, views);
            }
        }

        public async Task<IReadOnlyCollection<Task<IRelationalDatabaseView>>> ViewsAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            var queryResult = await Connection.QueryAsync<QualifiedName>(ViewsQuery, new { SchemaName = DefaultSchema }).ConfigureAwait(false);
            var viewNames = queryResult
                .Select(dto => Identifier.CreateQualifiedIdentifier(dto.SchemaName, dto.ObjectName))
                .ToList();

            var views = viewNames
                .Select(name => LoadViewAsync(name, cancellationToken))
                .Somes();
            return new ReadOnlyCollectionSlim<Task<IRelationalDatabaseView>>(viewNames.Count, views);
        }

        protected virtual string ViewsQuery => ViewsQuerySql;

        private const string ViewsQuerySql = @"
select
    TABLE_SCHEMA as SchemaName,
    TABLE_NAME as ObjectName
from information_schema.views
where TABLE_SCHEMA = @SchemaName order by TABLE_NAME";

        protected virtual Option<IRelationalDatabaseView> LoadViewSync(Identifier viewName)
        {
            if (viewName == null)
                throw new ArgumentNullException(nameof(viewName));

            viewName = CreateQualifiedIdentifier(viewName);
            return GetResolvedViewName(viewName)
                .Map<IRelationalDatabaseView>(name => new MySqlRelationalDatabaseView(Connection, Dialect.TypeProvider, name));
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
            var qualifiedViewName = await GetResolvedViewNameAsync(viewName, cancellationToken).ConfigureAwait(false);

            return qualifiedViewName
                .Map<IRelationalDatabaseView>(name => new MySqlRelationalDatabaseView(Connection, Dialect.TypeProvider, name));
        }

        public Option<IDatabaseSequence> GetSequence(Identifier sequenceName)
        {
            if (sequenceName == null)
                throw new ArgumentNullException(nameof(sequenceName));

            return _missingSequence;
        }

        public Task<Option<IDatabaseSequence>> GetSequenceAsync(Identifier sequenceName, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (sequenceName == null)
                throw new ArgumentNullException(nameof(sequenceName));

            return _missingSequenceTask;
        }

        public IReadOnlyCollection<IDatabaseSequence> Sequences { get; } = Array.Empty<IDatabaseSequence>();

        public Task<IReadOnlyCollection<Task<IDatabaseSequence>>> SequencesAsync(CancellationToken cancellationToken = default(CancellationToken)) => _emptySequences;

        public Option<IDatabaseSynonym> GetSynonym(Identifier synonymName)
        {
            if (synonymName == null)
                throw new ArgumentNullException(nameof(synonymName));

            return _missingSynonym;
        }

        public Task<Option<IDatabaseSynonym>> GetSynonymAsync(Identifier synonymName, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (synonymName == null)
                throw new ArgumentNullException(nameof(synonymName));

            return _missingSynonymTask;
        }

        public IReadOnlyCollection<IDatabaseSynonym> Synonyms { get; } = Array.Empty<IDatabaseSynonym>();

        public Task<IReadOnlyCollection<Task<IDatabaseSynonym>>> SynonymsAsync(CancellationToken cancellationToken = default(CancellationToken)) => _emptySynonyms;

        private async Task<DatabaseMetadata> LoadDatabaseMetadataAsync()
        {
            const string sql = @"
select
    @@hostname as ServerName,
    database() as DatabaseName,
    schema() as DefaultSchema,
    version() as DatabaseVersion";
            var metadata = await Connection.QuerySingleAsync<DatabaseMetadata>(sql).ConfigureAwait(false);
            metadata.DatabaseVersion = "MySQL " + metadata.DatabaseVersion;
            return metadata;
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

            var schema = identifier.Schema ?? DefaultSchema;
            return Identifier.CreateQualifiedIdentifier(ServerName, DatabaseName, schema, identifier.LocalName);
        }

        private readonly AsyncLazy<DatabaseMetadata> _metadata;

        private readonly static Option<IDatabaseSequence> _missingSequence = Option<IDatabaseSequence>.None;
        private readonly static Task<Option<IDatabaseSequence>> _missingSequenceTask = Task.FromResult(Option<IDatabaseSequence>.None);
        private readonly static Task<IReadOnlyCollection<Task<IDatabaseSequence>>> _emptySequences = Task.FromResult<IReadOnlyCollection<Task<IDatabaseSequence>>>(Array.Empty<Task<IDatabaseSequence>>());
        private readonly static Option<IDatabaseSynonym> _missingSynonym = Option<IDatabaseSynonym>.None;
        private readonly static Task<Option<IDatabaseSynonym>> _missingSynonymTask = Task.FromResult(Option<IDatabaseSynonym>.None);
        private readonly static Task<IReadOnlyCollection<Task<IDatabaseSynonym>>> _emptySynonyms = Task.FromResult<IReadOnlyCollection<Task<IDatabaseSynonym>>>(Array.Empty<Task<IDatabaseSynonym>>());
    }
}
