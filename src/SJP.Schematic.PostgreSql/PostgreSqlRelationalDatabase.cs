using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SJP.Schematic.Core;
using SJP.Schematic.PostgreSql.Query;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Core.Utilities;
using LanguageExt;

namespace SJP.Schematic.PostgreSql
{
    public class PostgreSqlRelationalDatabase : RelationalDatabase, IRelationalDatabase
    {
        public PostgreSqlRelationalDatabase(IDatabaseDialect dialect, IDbConnection connection, IIdentifierResolutionStrategy identifierResolver)
            : base(dialect, connection)
        {
            IdentifierResolver = identifierResolver ?? throw new ArgumentNullException(nameof(identifierResolver));
            _metadata = new Lazy<DatabaseMetadata>(LoadDatabaseMetadata);
        }

        protected IIdentifierResolutionStrategy IdentifierResolver { get; }

        public string ServerName => Metadata.ServerName;

        public string DatabaseName => Metadata.DatabaseName;

        public string DefaultSchema => Metadata.DefaultSchema;

        public string DatabaseVersion => Metadata.DatabaseVersion;

        protected DatabaseMetadata Metadata => _metadata.Value;

        protected Option<Identifier> GetResolvedTableName(Identifier tableName)
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));

            return ResolveFirstExistingObjectName(tableName, GetResolvedTableNameStrict);
        }

        protected OptionAsync<Identifier> GetResolvedTableNameAsync(Identifier tableName, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));

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

        protected OptionAsync<Identifier> GetResolvedTableNameStrictAsync(Identifier tableName, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));

            tableName = CreateQualifiedIdentifier(tableName);
            var qualifiedTableName = Connection.QueryFirstOrNoneAsync<QualifiedName>(
                TableNameQuery,
                new { SchemaName = tableName.Schema, TableName = tableName.LocalName }
            );

            return qualifiedTableName.Map(name => Identifier.CreateQualifiedIdentifier(tableName.Server, tableName.Database, name.SchemaName, name.ObjectName));
        }

        protected virtual string TableNameQuery => TableNameQuerySql;

        private const string TableNameQuerySql = @"
select schemaname as SchemaName, tablename as ObjectName
from pg_catalog.pg_tables
where schemaname = @SchemaName and tablename = @TableName
    and schemaname not in ('pg_catalog', 'information_schema')
limit 1";

        public Option<IRelationalDatabaseTable> GetTable(Identifier tableName)
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));

            tableName = CreateQualifiedIdentifier(tableName);
            return LoadTableSync(tableName);
        }

        public OptionAsync<IRelationalDatabaseTable> GetTableAsync(Identifier tableName, CancellationToken cancellationToken = default(CancellationToken))
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
                    .Somes();
                return new ReadOnlyCollectionSlim<IRelationalDatabaseTable>(tableNames.Count, tables);
            }
        }

        public async Task<IReadOnlyCollection<IRelationalDatabaseTable>> TablesAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            var queryResults = await Connection.QueryAsync<QualifiedName>(TablesQuery).ConfigureAwait(false);
            var tableNames = queryResults
                .Select(dto => Identifier.CreateQualifiedIdentifier(dto.SchemaName, dto.ObjectName))
                .ToList();

            var tables = await tableNames
                .Select(name => LoadTableAsync(name, cancellationToken))
                .Somes()
                .ConfigureAwait(false);

            return tables.ToList();
        }

        protected virtual string TablesQuery => TablesQuerySql;

        private const string TablesQuerySql = @"
select
    schemaname as SchemaName,
    tablename as ObjectName
from pg_catalog.pg_tables
where schemaname not in ('pg_catalog', 'information_schema')";

        protected virtual Option<IRelationalDatabaseTable> LoadTableSync(Identifier tableName)
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));

            var resolvedTableName = ResolveFirstExistingObjectName(tableName, GetResolvedTableName);
            return resolvedTableName
                .Map<IRelationalDatabaseTable>(name => new PostgreSqlRelationalDatabaseTable(Connection, this, Dialect.TypeProvider, name, IdentifierResolver));
        }

        protected virtual OptionAsync<IRelationalDatabaseTable> LoadTableAsync(Identifier tableName, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));

            var resolvedTableName = ResolveFirstExistingObjectNameAsync(tableName, GetResolvedTableNameAsync, cancellationToken);
            return resolvedTableName
                .Map<IRelationalDatabaseTable>(name => new PostgreSqlRelationalDatabaseTable(Connection, this, Dialect.TypeProvider, name, IdentifierResolver));
        }

        protected Option<Identifier> GetResolvedViewName(Identifier viewName)
        {
            if (viewName == null)
                throw new ArgumentNullException(nameof(viewName));

            return ResolveFirstExistingObjectName(viewName, GetResolvedViewNameStrict);
        }

        protected OptionAsync<Identifier> GetResolvedViewNameAsync(Identifier viewName, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (viewName == null)
                throw new ArgumentNullException(nameof(viewName));

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

        protected OptionAsync<Identifier> GetResolvedViewNameStrictAsync(Identifier viewName, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (viewName == null)
                throw new ArgumentNullException(nameof(viewName));

            viewName = CreateQualifiedIdentifier(viewName);

            var qualifiedViewName = Connection.QueryFirstOrNoneAsync<QualifiedName>(
                ViewNameQuery,
                new { SchemaName = viewName.Schema, ViewName = viewName.LocalName }
            );

            return qualifiedViewName.Map(name => Identifier.CreateQualifiedIdentifier(viewName.Server, viewName.Database, name.SchemaName, name.ObjectName));
        }

        protected virtual string ViewNameQuery => ViewNameQuerySql;

        private const string ViewNameQuerySql = @"
select schemaname as SchemaName, viewname as ObjectName
from pg_catalog.pg_views
where schemaname = @SchemaName and viewname = @ViewName
    and schemaname not in ('pg_catalog', 'information_schema')
limit 1";

        public Option<IRelationalDatabaseView> GetView(Identifier viewName)
        {
            if (viewName == null)
                throw new ArgumentNullException(nameof(viewName));

            viewName = CreateQualifiedIdentifier(viewName);
            return LoadViewSync(viewName);
        }

        public OptionAsync<IRelationalDatabaseView> GetViewAsync(Identifier viewName, CancellationToken cancellationToken = default(CancellationToken))
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
                    .Somes();
                return new ReadOnlyCollectionSlim<IRelationalDatabaseView>(viewNames.Count, views);
            }
        }

        public async Task<IReadOnlyCollection<IRelationalDatabaseView>> ViewsAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            var queryResult = await Connection.QueryAsync<QualifiedName>(ViewsQuery).ConfigureAwait(false);
            var viewNames = queryResult
                .Select(dto => Identifier.CreateQualifiedIdentifier(dto.SchemaName, dto.ObjectName))
                .ToList();

            var views = await viewNames
                .Select(name => LoadViewAsync(name, cancellationToken))
                .Somes()
                .ConfigureAwait(false);

            return views.ToList();
        }

        protected virtual string ViewsQuery => ViewsQuerySql;

        private const string ViewsQuerySql = @"
select
    schemaname as SchemaName,
    viewname as ObjectName
from pg_catalog.pg_views
where schemaname not in ('pg_catalog', 'information_schema')";

        protected virtual Option<IRelationalDatabaseView> LoadViewSync(Identifier viewName)
        {
            if (viewName == null)
                throw new ArgumentNullException(nameof(viewName));

            var resolvedViewName = ResolveFirstExistingObjectName(viewName, GetResolvedViewName);
            return resolvedViewName
                .Map<IRelationalDatabaseView>(name => new PostgreSqlRelationalDatabaseView(Connection, Dialect.TypeProvider, name, IdentifierResolver));
        }

        protected virtual OptionAsync<IRelationalDatabaseView> LoadViewAsync(Identifier viewName, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (viewName == null)
                throw new ArgumentNullException(nameof(viewName));

            var resolvedViewName = ResolveFirstExistingObjectNameAsync(viewName, GetResolvedViewNameAsync, cancellationToken);
            return resolvedViewName
                .Map<IRelationalDatabaseView>(name => new PostgreSqlRelationalDatabaseView(Connection, Dialect.TypeProvider, name, IdentifierResolver));
        }

        public Option<Identifier> GetResolvedSequenceName(Identifier sequenceName)
        {
            if (sequenceName == null)
                throw new ArgumentNullException(nameof(sequenceName));

            return ResolveFirstExistingObjectName(sequenceName, GetResolvedSequenceNameStrict);
        }

        public OptionAsync<Identifier> GetResolvedSequenceNameAsync(Identifier sequenceName, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (sequenceName == null)
                throw new ArgumentNullException(nameof(sequenceName));

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

        protected OptionAsync<Identifier> GetResolvedSequenceNameStrictAsync(Identifier sequenceName, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (sequenceName == null)
                throw new ArgumentNullException(nameof(sequenceName));

            sequenceName = CreateQualifiedIdentifier(sequenceName);

            var qualifiedSequenceName = Connection.QueryFirstOrNoneAsync<QualifiedName>(
                SequenceNameQuery,
                new { SchemaName = sequenceName.Schema, SequenceName = sequenceName.LocalName }
            );

            return qualifiedSequenceName.Map(name => Identifier.CreateQualifiedIdentifier(sequenceName.Server, sequenceName.Database, name.SchemaName, name.ObjectName));
        }

        protected virtual string SequenceNameQuery => SequenceNameQuerySql;

        private const string SequenceNameQuerySql = @"
select sequence_schema as SchemaName, sequence_name as ObjectName
from information_schema.sequences
where sequence_schema = @SchemaName and sequence_name = @SequenceName
    and sequence_schema not in ('pg_catalog', 'information_schema')
limit 1";

        public Option<IDatabaseSequence> GetSequence(Identifier sequenceName)
        {
            if (sequenceName == null)
                throw new ArgumentNullException(nameof(sequenceName));

            sequenceName = CreateQualifiedIdentifier(sequenceName);
            return LoadSequenceSync(sequenceName);
        }

        public OptionAsync<IDatabaseSequence> GetSequenceAsync(Identifier sequenceName, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (sequenceName == null)
                throw new ArgumentNullException(nameof(sequenceName));

            sequenceName = CreateQualifiedIdentifier(sequenceName);
            return LoadSequenceAsync(sequenceName, cancellationToken);
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
                    .Somes();
                return new ReadOnlyCollectionSlim<IDatabaseSequence>(sequenceNames.Count, sequences);
            }
        }

        public async Task<IReadOnlyCollection<IDatabaseSequence>> SequencesAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            var queryResult = await Connection.QueryAsync<QualifiedName>(SequencesQuery).ConfigureAwait(false);
            var sequenceNames = queryResult
                .Select(dto => Identifier.CreateQualifiedIdentifier(dto.SchemaName, dto.ObjectName))
                .ToList();

            var sequences = await sequenceNames
                .Select(name => LoadSequenceAsync(name, cancellationToken))
                .Somes()
                .ConfigureAwait(false);

            return sequences.ToList();
        }

        protected virtual string SequencesQuery => SequencesQuerySql;

        private const string SequencesQuerySql = @"
select
    sequence_schema as SchemaName,
    sequence_name as ObjectName
from information_schema.sequences
where sequence_schema not in ('pg_catalog', 'information_schema')";

        protected virtual Option<IDatabaseSequence> LoadSequenceSync(Identifier sequenceName)
        {
            if (sequenceName == null)
                throw new ArgumentNullException(nameof(sequenceName));

            return ResolveFirstExistingObjectName(sequenceName, GetResolvedSequenceName)
                .Map<IDatabaseSequence>(name => new PostgreSqlDatabaseSequence(Connection, name));
        }

        protected virtual OptionAsync<IDatabaseSequence> LoadSequenceAsync(Identifier sequenceName, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (sequenceName == null)
                throw new ArgumentNullException(nameof(sequenceName));

            var resolvedSequenceName = ResolveFirstExistingObjectNameAsync(sequenceName, GetResolvedSequenceNameAsync, cancellationToken);
            return resolvedSequenceName
                .Map<IDatabaseSequence>(name => new PostgreSqlDatabaseSequence(Connection, name));
        }

        public Option<IDatabaseSynonym> GetSynonym(Identifier synonymName)
        {
            if (synonymName == null)
                throw new ArgumentNullException(nameof(synonymName));

            return Option<IDatabaseSynonym>.None;
        }

        public OptionAsync<IDatabaseSynonym> GetSynonymAsync(Identifier synonymName, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (synonymName == null)
                throw new ArgumentNullException(nameof(synonymName));

            return OptionAsync<IDatabaseSynonym>.None;
        }

        public IReadOnlyCollection<IDatabaseSynonym> Synonyms { get; } = Array.Empty<IDatabaseSynonym>();

        public Task<IReadOnlyCollection<IDatabaseSynonym>> SynonymsAsync(CancellationToken cancellationToken = default(CancellationToken)) => _emptySynonyms;

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

        protected OptionAsync<Identifier> ResolveFirstExistingObjectNameAsync(Identifier objectName, Func<Identifier, CancellationToken, OptionAsync<Identifier>> objectExistsFunc, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (objectName == null)
                throw new ArgumentNullException(nameof(objectName));
            if (objectExistsFunc == null)
                throw new ArgumentNullException(nameof(objectExistsFunc));

            return ResolveFirstExistingObjectNameAsyncCore(objectName, objectExistsFunc, cancellationToken).ToAsync();
        }

        private async Task<Option<Identifier>> ResolveFirstExistingObjectNameAsyncCore(Identifier objectName, Func<Identifier, CancellationToken, OptionAsync<Identifier>> objectExistsFunc, CancellationToken cancellationToken)
        {
            var resolvedNames = IdentifierResolver
                .GetResolutionOrder(objectName)
                .Select(CreateQualifiedIdentifier);

            foreach (var resolvedName in resolvedNames)
            {
                var nameOption = objectExistsFunc.Invoke(resolvedName, cancellationToken);
                var nameIsSome = await nameOption.IsSome.ConfigureAwait(false);
                if (nameIsSome)
                    return await nameOption.UnwrapSomeAsync().ConfigureAwait(false);
            }

            return Option<Identifier>.None;
        }

        private DatabaseMetadata LoadDatabaseMetadata()
        {
            const string sql = @"
select
    pg_catalog.host(pg_catalog.inet_server_addr()) as ServerName,
    pg_catalog.current_database() as DatabaseName,
    pg_catalog.current_schema() as DefaultSchema,
    pg_catalog.version() as DatabaseVersion";
            var result = Connection.QuerySingle<DatabaseMetadata>(sql);

            if (result.ServerName.IsNullOrWhiteSpace())
                result.ServerName = "127.0.0.1";

            return result;
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

        private readonly Lazy<DatabaseMetadata> _metadata;

        private readonly static Task<IReadOnlyCollection<IDatabaseSynonym>> _emptySynonyms = Task.FromResult<IReadOnlyCollection<IDatabaseSynonym>>(Array.Empty<IDatabaseSynonym>());
    }
}