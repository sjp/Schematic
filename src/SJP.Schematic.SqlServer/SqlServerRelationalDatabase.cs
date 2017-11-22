﻿using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using SJP.Schematic.Core;
using SJP.Schematic.SqlServer.Query;

namespace SJP.Schematic.SqlServer
{
    public class SqlServerRelationalDatabase : RelationalDatabase, IRelationalDatabase, IDependentRelationalDatabase
    {
        public SqlServerRelationalDatabase(IDatabaseDialect dialect, IDbConnection connection, IEqualityComparer<Identifier> comparer = null)
            : base(dialect, connection)
        {
            _metadata = new Lazy<DatabaseMetadata>(LoadDatabaseMetadata);
            Comparer = comparer ?? new IdentifierComparer(StringComparer.OrdinalIgnoreCase, ServerName, DatabaseName, DefaultSchema);
            _parentDb = this;
        }

        public IRelationalDatabase Parent
        {
            get => _parentDb;
            set => _parentDb = value ?? throw new ArgumentNullException(nameof(Parent));
        }

        protected IEqualityComparer<Identifier> Comparer { get; }

        protected IRelationalDatabase Database => Parent;

        public string ServerName => Metadata.ServerName;

        public string DatabaseName => Metadata.DatabaseName;

        public string DefaultSchema => Metadata.DefaultSchema;

        protected DatabaseMetadata Metadata => _metadata.Value;

        public bool TableExists(Identifier tableName)
        {
            if (tableName == null || tableName.LocalName == null)
                throw new ArgumentNullException(nameof(tableName));

            tableName = CreateQualifiedIdentifier(tableName);

            return Connection.ExecuteScalar<int>(
                TableExistsQuery,
                new { SchemaName = tableName.Schema, TableName = tableName.LocalName }
            ) != 0;
        }

        public async Task<bool> TableExistsAsync(Identifier tableName)
        {
            if (tableName == null || tableName.LocalName == null)
                throw new ArgumentNullException(nameof(tableName));

            tableName = CreateQualifiedIdentifier(tableName);

            return await Connection.ExecuteScalarAsync<int>(
                TableExistsQuery,
                new { SchemaName = tableName.Schema, TableName = tableName.LocalName }
            ).ConfigureAwait(false) != 0;
        }

        protected virtual string TableExistsQuery => TableExistsQuerySql;

        private const string TableExistsQuerySql = "select top 1 1 from sys.tables where schema_id = schema_id(@SchemaName) and name = @TableName";

        public IRelationalDatabaseTable GetTable(Identifier tableName)
        {
            if (tableName == null || tableName.LocalName == null)
                throw new ArgumentNullException(nameof(tableName));

            tableName = CreateQualifiedIdentifier(tableName);
            return LoadTableSync(tableName);
        }

        public Task<IRelationalDatabaseTable> GetTableAsync(Identifier tableName)
        {
            if (tableName == null || tableName.LocalName == null)
                throw new ArgumentNullException(nameof(tableName));

            tableName = CreateQualifiedIdentifier(tableName);
            return LoadTableAsync(tableName);
        }

        public IEnumerable<IRelationalDatabaseTable> Tables
        {
            get
            {
                var tableNames = Connection.Query<QualifiedName>(TablesQuery)
                    .Select(dto => new Identifier(dto.SchemaName, dto.ObjectName));

                foreach (var tableName in tableNames)
                    yield return LoadTableSync(tableName);
            }
        }

        public async Task<IAsyncEnumerable<IRelationalDatabaseTable>> TablesAsync()
        {
            var queryResults = await Connection.QueryAsync<QualifiedName>(TablesQuery).ConfigureAwait(false);
            var tableNames = queryResults.Select(dto => new Identifier(dto.SchemaName, dto.ObjectName));

            return tableNames
                .Select(LoadTableSync)
                .ToAsyncEnumerable();
        }

        protected virtual string TablesQuery => TablesQuerySql;

        private const string TablesQuerySql = "select schema_name(schema_id) as SchemaName, name as ObjectName from sys.tables order by schema_name(schema_id), name";

        protected virtual IRelationalDatabaseTable LoadTableSync(Identifier tableName)
        {
            if (tableName == null || tableName.LocalName == null)
                throw new ArgumentNullException(nameof(tableName));

            tableName = CreateQualifiedIdentifier(tableName);
            return TableExists(tableName)
                ? new SqlServerRelationalDatabaseTable(Connection, Database, tableName, Comparer)
                : null;
        }

        protected virtual async Task<IRelationalDatabaseTable> LoadTableAsync(Identifier tableName)
        {
            if (tableName == null || tableName.LocalName == null)
                throw new ArgumentNullException(nameof(tableName));

            tableName = CreateQualifiedIdentifier(tableName);
            var exists = await TableExistsAsync(tableName).ConfigureAwait(false);
            return exists
                ? new SqlServerRelationalDatabaseTable(Connection, Database, tableName, Comparer)
                : null;
        }

        public bool ViewExists(Identifier viewName)
        {
            if (viewName == null || viewName.LocalName == null)
                throw new ArgumentNullException(nameof(viewName));

            viewName = CreateQualifiedIdentifier(viewName);

            return Connection.ExecuteScalar<int>(
                ViewExistsQuery,
                new { SchemaName = viewName.Schema, ViewName = viewName.LocalName }
            ) != 0;
        }

        public async Task<bool> ViewExistsAsync(Identifier viewName)
        {
            if (viewName == null || viewName.LocalName == null)
                throw new ArgumentNullException(nameof(viewName));

            viewName = CreateQualifiedIdentifier(viewName);

            return await Connection.ExecuteScalarAsync<int>(
                ViewExistsQuery,
                new { SchemaName = viewName.Schema, ViewName = viewName.LocalName }
            ).ConfigureAwait(false) != 0;
        }

        protected virtual string ViewExistsQuery => ViewExistsQuerySql;

        private const string ViewExistsQuerySql = "select top 1 1 from sys.views where schema_id = schema_id(@SchemaName) and name = @ViewName";

        public IRelationalDatabaseView GetView(Identifier viewName)
        {
            if (viewName == null || viewName.LocalName == null)
                throw new ArgumentNullException(nameof(viewName));

            viewName = CreateQualifiedIdentifier(viewName);
            return LoadViewSync(viewName);
        }

        public Task<IRelationalDatabaseView> GetViewAsync(Identifier viewName)
        {
            if (viewName == null || viewName.LocalName == null)
                throw new ArgumentNullException(nameof(viewName));

            viewName = CreateQualifiedIdentifier(viewName);
            return LoadViewAsync(viewName);
        }

        public IEnumerable<IRelationalDatabaseView> Views
        {
            get
            {
                var viewNames = Connection.Query<QualifiedName>(ViewsQuery)
                    .Select(dto => new Identifier(dto.SchemaName, dto.ObjectName));

                foreach (var viewName in viewNames)
                    yield return LoadViewSync(viewName);
            }
        }

        public async Task<IAsyncEnumerable<IRelationalDatabaseView>> ViewsAsync()
        {
            var queryResult = await Connection.QueryAsync<QualifiedName>(ViewsQuery).ConfigureAwait(false);
            var viewNames = queryResult.Select(dto => new Identifier(dto.SchemaName, dto.ObjectName));

            return viewNames
                .Select(LoadViewSync)
                .ToAsyncEnumerable();
        }

        protected virtual string ViewsQuery => ViewsQuerySql;

        private const string ViewsQuerySql = "select schema_name(schema_id) as SchemaName, name as ObjectName from sys.views order by schema_name(schema_id), name";

        protected virtual IRelationalDatabaseView LoadViewSync(Identifier viewName)
        {
            if (viewName == null || viewName.LocalName == null)
                throw new ArgumentNullException(nameof(viewName));

            viewName = CreateQualifiedIdentifier(viewName);
            return ViewExists(viewName)
                ? new SqlServerRelationalDatabaseView(Connection, Database, viewName, Comparer)
                : null;
        }

        protected virtual async Task<IRelationalDatabaseView> LoadViewAsync(Identifier viewName)
        {
            if (viewName == null || viewName.LocalName == null)
                throw new ArgumentNullException(nameof(viewName));

            viewName = CreateQualifiedIdentifier(viewName);
            var exists = await ViewExistsAsync(viewName).ConfigureAwait(false);
            return exists
                ? new SqlServerRelationalDatabaseView(Connection, Database, viewName, Comparer)
                : null;
        }

        public bool SequenceExists(Identifier sequenceName)
        {
            if (sequenceName == null || sequenceName.LocalName == null)
                throw new ArgumentNullException(nameof(sequenceName));

            sequenceName = CreateQualifiedIdentifier(sequenceName);

            return Connection.ExecuteScalar<int>(
                SequenceExistsQuery,
                new { SchemaName = sequenceName.Schema, SequenceName = sequenceName.LocalName }
            ) != 0;
        }

        public async Task<bool> SequenceExistsAsync(Identifier sequenceName)
        {
            if (sequenceName == null || sequenceName.LocalName == null)
                throw new ArgumentNullException(nameof(sequenceName));

            sequenceName = CreateQualifiedIdentifier(sequenceName);

            return await Connection.ExecuteScalarAsync<int>(
                SequenceExistsQuery,
                new { SchemaName = sequenceName.Schema, SequenceName = sequenceName.LocalName }
            ).ConfigureAwait(false) != 0;
        }

        protected virtual string SequenceExistsQuery => SequenceExistsQuerySql;

        private const string SequenceExistsQuerySql = "select top 1 1 from sys.sequences where schema_id = schema_id(@SchemaName) and name = @SequenceName";

        public IDatabaseSequence GetSequence(Identifier sequenceName)
        {
            if (sequenceName == null || sequenceName.LocalName == null)
                throw new ArgumentNullException(nameof(sequenceName));

            sequenceName = CreateQualifiedIdentifier(sequenceName);
            return LoadSequenceSync(sequenceName);
        }

        public Task<IDatabaseSequence> GetSequenceAsync(Identifier sequenceName)
        {
            if (sequenceName == null || sequenceName.LocalName == null)
                throw new ArgumentNullException(nameof(sequenceName));

            sequenceName = CreateQualifiedIdentifier(sequenceName);
            return LoadSequenceAsync(sequenceName);
        }

        public IEnumerable<IDatabaseSequence> Sequences
        {
            get
            {
                var sequenceNames = Connection.Query<QualifiedName>(SequencesQuery)
                    .Select(dto => new Identifier(dto.SchemaName, dto.ObjectName));

                foreach (var sequenceName in sequenceNames)
                    yield return LoadSequenceSync(sequenceName);
            }
        }

        public async Task<IAsyncEnumerable<IDatabaseSequence>> SequencesAsync()
        {
            var queryResult = await Connection.QueryAsync<QualifiedName>(SequencesQuery).ConfigureAwait(false);
            var sequenceNames = queryResult.Select(dto => new Identifier(dto.SchemaName, dto.ObjectName));

            return sequenceNames
                .Select(LoadSequenceSync)
                .ToAsyncEnumerable();
        }

        protected virtual string SequencesQuery => SequencesQuerySql;

        private const string SequencesQuerySql = "select schema_name(schema_id) as SchemaName, name as ObjectName from sys.sequences order by schema_name(schema_id), name";

        protected virtual IDatabaseSequence LoadSequenceSync(Identifier sequenceName)
        {
            if (sequenceName == null || sequenceName.LocalName == null)
                throw new ArgumentNullException(nameof(sequenceName));

            sequenceName = CreateQualifiedIdentifier(sequenceName);
            return SequenceExists(sequenceName)
                ? new SqlServerDatabaseSequence(Connection, Database, sequenceName)
                : null;
        }

        protected virtual async Task<IDatabaseSequence> LoadSequenceAsync(Identifier sequenceName)
        {
            if (sequenceName == null || sequenceName.LocalName == null)
                throw new ArgumentNullException(nameof(sequenceName));

            sequenceName = CreateQualifiedIdentifier(sequenceName);
            var exists = await SequenceExistsAsync(sequenceName).ConfigureAwait(false);
            return exists
                ? new SqlServerDatabaseSequence(Connection, Database, sequenceName)
                : null;
        }

        public bool SynonymExists(Identifier synonymName)
        {
            if (synonymName == null || synonymName.LocalName == null)
                throw new ArgumentNullException(nameof(synonymName));

            synonymName = CreateQualifiedIdentifier(synonymName);

            return Connection.ExecuteScalar<int>(
                SynonymExistsQuery,
                new { SchemaName = synonymName.Schema, SynonymName = synonymName.LocalName }
            ) != 0;
        }

        public async Task<bool> SynonymExistsAsync(Identifier synonymName)
        {
            if (synonymName == null || synonymName.LocalName == null)
                throw new ArgumentNullException(nameof(synonymName));

            synonymName = CreateQualifiedIdentifier(synonymName);

            return await Connection.ExecuteScalarAsync<int>(
                SynonymExistsQuery,
                new { SchemaName = synonymName.Schema, SynonymName = synonymName.LocalName }
            ).ConfigureAwait(false) != 0;
        }

        protected virtual string SynonymExistsQuery => SynonymExistsQuerySql;

        private const string SynonymExistsQuerySql = "select top 1 1 from sys.synonyms where schema_id = schema_id(@SchemaName) and name = @SynonymName";

        public IDatabaseSynonym GetSynonym(Identifier synonymName)
        {
            if (synonymName == null || synonymName.LocalName == null)
                throw new ArgumentNullException(nameof(synonymName));

            synonymName = CreateQualifiedIdentifier(synonymName);
            return LoadSynonymSync(synonymName);
        }

        public Task<IDatabaseSynonym> GetSynonymAsync(Identifier synonymName)
        {
            if (synonymName == null || synonymName.LocalName == null)
                throw new ArgumentNullException(nameof(synonymName));

            synonymName = CreateQualifiedIdentifier(synonymName);
            return LoadSynonymAsync(synonymName);
        }

        public IEnumerable<IDatabaseSynonym> Synonyms
        {
            get
            {
                var synonyms = Connection.Query<QualifiedName>(SynonymsQuery)
                    .Select(dto => new Identifier(dto.SchemaName, dto.ObjectName));

                foreach (var synonym in synonyms)
                    yield return LoadSynonymSync(synonym);
            }
        }

        public async Task<IAsyncEnumerable<IDatabaseSynonym>> SynonymsAsync()
        {
            var queryResult = await Connection.QueryAsync<QualifiedName>(SynonymsQuery).ConfigureAwait(false);
            var synonymNames = queryResult.Select(dto => new Identifier(dto.SchemaName, dto.ObjectName));

            return synonymNames
                .Select(LoadSynonymSync)
                .ToAsyncEnumerable();
        }

        protected virtual string SynonymsQuery => SynonymsQuerySql;

        private const string SynonymsQuerySql = "select schema_name(schema_id) as SchemaName, name as ObjectName from sys.synonyms order by schema_name(schema_id), name";

        protected virtual IDatabaseSynonym LoadSynonymSync(Identifier synonymName)
        {
            if (synonymName == null || synonymName.LocalName == null)
                throw new ArgumentNullException(nameof(synonymName));

            synonymName = CreateQualifiedIdentifier(synonymName);
            if (!SynonymExists(synonymName))
                return null;

            var queryResult = Connection.QuerySingle<SynonymData>(
                LoadSynonymQuery,
                new { SchemaName = synonymName.Schema, SynonymName = synonymName.LocalName }
            );

            var serverName = !queryResult.TargetServerName.IsNullOrWhiteSpace() ? queryResult.TargetServerName : null;
            var databaseName = !queryResult.TargetDatabaseName.IsNullOrWhiteSpace() ? queryResult.TargetDatabaseName : null;
            var schemaName = !queryResult.TargetSchemaName.IsNullOrWhiteSpace() ? queryResult.TargetSchemaName : null;
            var localName = !queryResult.TargetObjectName.IsNullOrWhiteSpace() ? queryResult.TargetObjectName : null;

            var targetName = Identifier.CreateQualifiedIdentifier(serverName, databaseName, schemaName, localName);
            targetName = CreateQualifiedIdentifier(targetName);

            return new SqlServerDatabaseSynonym(Database, synonymName, targetName);
        }

        protected virtual async Task<IDatabaseSynonym> LoadSynonymAsync(Identifier synonymName)
        {
            if (synonymName == null || synonymName.LocalName == null)
                throw new ArgumentNullException(nameof(synonymName));

            synonymName = CreateQualifiedIdentifier(synonymName);
            var exists = await SynonymExistsAsync(synonymName).ConfigureAwait(false);
            if (!exists)
                return null;

            var queryResult = await Connection.QuerySingleAsync<SynonymData>(
                LoadSynonymQuery,
                new { SchemaName = synonymName.Schema, SynonymName = synonymName.LocalName }
            ).ConfigureAwait(false);

            var serverName = !queryResult.TargetServerName.IsNullOrWhiteSpace() ? queryResult.TargetServerName : null;
            var databaseName = !queryResult.TargetDatabaseName.IsNullOrWhiteSpace() ? queryResult.TargetDatabaseName : null;
            var schemaName = !queryResult.TargetSchemaName.IsNullOrWhiteSpace() ? queryResult.TargetSchemaName : null;
            var localName = !queryResult.TargetObjectName.IsNullOrWhiteSpace() ? queryResult.TargetObjectName : null;

            var targetName = Identifier.CreateQualifiedIdentifier(serverName, databaseName, schemaName, localName);
            targetName = CreateQualifiedIdentifier(targetName);

            return new SqlServerDatabaseSynonym(Database, synonymName, targetName);
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

        private DatabaseMetadata LoadDatabaseMetadata()
        {
            const string sql = "select @@SERVERNAME as ServerName, db_name() as DatabaseName, schema_name() as DefaultSchema";
            return Connection.QuerySingle<DatabaseMetadata>(sql);
        }

        /// <summary>
        /// Qualifies an identifier with information from the database. For example, sets the schema if it is missing.
        /// </summary>
        /// <param name="identifier">An identifier which may or may not be fully qualified.</param>
        /// <returns>A new identifier, which will have components present where they were previously missing.</returns>
        /// <remarks>No components of an identifier when present will be modified. For example, when given a fully qualified identifier, a new identifier will be returned that is equal in value to the argument.</remarks>
        protected Identifier CreateQualifiedIdentifier(Identifier identifier)
        {
            if (identifier == null || identifier.LocalName == null)
                throw new ArgumentNullException(nameof(identifier));

            var serverName = identifier.Server ?? ServerName;
            var databaseName = identifier.Database ?? DatabaseName;
            var schema = identifier.Schema ?? DefaultSchema;

            return new Identifier(serverName, databaseName, schema, identifier.LocalName);
        }

        private IRelationalDatabase _parentDb;
        private readonly Lazy<DatabaseMetadata> _metadata;
    }
}
