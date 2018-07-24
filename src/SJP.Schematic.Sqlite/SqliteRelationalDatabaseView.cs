using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using SJP.Schematic.Core;
using SJP.Schematic.Sqlite.Pragma;
using SJP.Schematic.Core.Extensions;
using System.Threading;

namespace SJP.Schematic.Sqlite
{
    public class SqliteRelationalDatabaseView : IRelationalDatabaseView
    {
        public SqliteRelationalDatabaseView(IDbConnection connection, IRelationalDatabase database, Identifier viewName)
        {
            if (viewName == null)
                throw new ArgumentNullException(nameof(viewName));

            Connection = connection ?? throw new ArgumentNullException(nameof(connection));
            Database = database ?? throw new ArgumentNullException(nameof(database));
            if (database.Dialect == null)
                throw new ArgumentException("The given database object does not contain a dialect.", nameof(database));
            Dialect = database.Dialect;

            Comparer = new IdentifierComparer(StringComparer.OrdinalIgnoreCase, defaultSchema: Database.DefaultSchema);

            var schemaName = viewName.Schema ?? database.DefaultSchema;
            var localName = viewName.LocalName;

            Name = new Identifier(schemaName, localName);
            Pragma = new DatabasePragma(Dialect, connection, schemaName);
        }

        public IRelationalDatabase Database { get; }

        public Identifier Name { get; }

        protected IDatabaseDialect Dialect { get; }

        protected IDbConnection Connection { get; }

        protected IEqualityComparer<Identifier> Comparer { get; }

        protected ISqliteDatabasePragma Pragma { get; }

        public string Definition => LoadDefinitionSync();

        public Task<string> DefinitionAsync(CancellationToken cancellationToken = default(CancellationToken)) => LoadDefinitionAsync(cancellationToken);

        protected virtual string LoadDefinitionSync()
        {
            var sql = $"select sql from { Dialect.QuoteIdentifier(Name.Schema) }.sqlite_master where type = 'view' and tbl_name = @ViewName";
            return Connection.ExecuteScalar<string>(sql, new { ViewName = Name.LocalName });
        }

        protected virtual Task<string> LoadDefinitionAsync(CancellationToken cancellationToken)
        {
            var sql = $"select sql from { Dialect.QuoteIdentifier(Name.Schema) }.sqlite_master where type = 'view' and tbl_name = @ViewName";
            return Connection.ExecuteScalarAsync<string>(sql, new { ViewName = Name.LocalName });
        }

        public bool IsIndexed => Indexes.Any();

        public IReadOnlyDictionary<Identifier, IDatabaseViewIndex> Index => LoadIndexLookupSync();

        public Task<IReadOnlyDictionary<Identifier, IDatabaseViewIndex>> IndexAsync(CancellationToken cancellationToken = default(CancellationToken)) => LoadIndexLookupAsync(cancellationToken);

        public IReadOnlyCollection<IDatabaseViewIndex> Indexes => LoadIndexesSync();

        public Task<IReadOnlyCollection<IDatabaseViewIndex>> IndexesAsync(CancellationToken cancellationToken = default(CancellationToken)) => LoadIndexesAsync(cancellationToken);

        protected virtual IReadOnlyCollection<IDatabaseViewIndex> LoadIndexesSync() => Array.Empty<IDatabaseViewIndex>();

        protected virtual Task<IReadOnlyCollection<IDatabaseViewIndex>> LoadIndexesAsync(CancellationToken cancellationToken = default(CancellationToken)) => Task.FromResult(LoadIndexesSync());

        protected virtual IReadOnlyDictionary<Identifier, IDatabaseViewIndex> LoadIndexLookupSync() => _emptyIndexLookup;

        protected virtual Task<IReadOnlyDictionary<Identifier, IDatabaseViewIndex>> LoadIndexLookupAsync(CancellationToken cancellationToken = default(CancellationToken)) => _emptyIndexLookupTask;

        private readonly static IReadOnlyDictionary<Identifier, IDatabaseViewIndex> _emptyIndexLookup = new Dictionary<Identifier, IDatabaseViewIndex>();
        private readonly static Task<IReadOnlyDictionary<Identifier, IDatabaseViewIndex>> _emptyIndexLookupTask = Task.FromResult(_emptyIndexLookup);

        public IReadOnlyDictionary<Identifier, IDatabaseViewColumn> Column => LoadColumnLookupSync();

        public Task<IReadOnlyDictionary<Identifier, IDatabaseViewColumn>> ColumnAsync(CancellationToken cancellationToken = default(CancellationToken)) => LoadColumnLookupAsync(cancellationToken);

        public IReadOnlyList<IDatabaseViewColumn> Columns => LoadColumnsSync();

        public Task<IReadOnlyList<IDatabaseViewColumn>> ColumnsAsync(CancellationToken cancellationToken = default(CancellationToken)) => LoadColumnsAsync(cancellationToken);

        protected virtual IReadOnlyDictionary<Identifier, IDatabaseViewColumn> LoadColumnLookupSync()
        {
            var columns = Columns;
            var result = new Dictionary<Identifier, IDatabaseViewColumn>(columns.Count, Comparer);

            foreach (var column in columns.Where(c => c.Name != null))
                result[column.Name.LocalName] = column;

            return result;
        }

        protected virtual async Task<IReadOnlyDictionary<Identifier, IDatabaseViewColumn>> LoadColumnLookupAsync(CancellationToken cancellationToken)
        {
            var columns = await ColumnsAsync(cancellationToken).ConfigureAwait(false);
            var result = new Dictionary<Identifier, IDatabaseViewColumn>(columns.Count, Comparer);

            foreach (var column in columns.Where(c => c.Name != null))
                result[column.Name.LocalName] = column;

            return result;
        }

        protected virtual IReadOnlyList<IDatabaseViewColumn> LoadColumnsSync()
        {
            var tableInfos = Pragma.TableInfo(Name);

            var result = new List<IDatabaseViewColumn>();
            if (tableInfos.Empty())
                return result;

            foreach (var tableInfo in tableInfos)
            {
                var columnTypeName = tableInfo.type;
                if (columnTypeName.IsNullOrWhiteSpace())
                    columnTypeName = GetTypeofColumn(tableInfo.name);

                var affinity = _affinityParser.ParseTypeName(columnTypeName);
                var columnType = new SqliteColumnType(affinity);

                var column = new SqliteDatabaseViewColumn(this, tableInfo.name, columnType, !tableInfo.notnull, tableInfo.dflt_value, null);
                result.Add(column);
            }

            return result.AsReadOnly();
        }

        protected virtual async Task<IReadOnlyList<IDatabaseViewColumn>> LoadColumnsAsync(CancellationToken cancellationToken)
        {
            var tableInfos = await Pragma.TableInfoAsync(Name, cancellationToken).ConfigureAwait(false);

            var result = new List<IDatabaseViewColumn>();
            if (tableInfos.Empty())
                return result;

            foreach (var tableInfo in tableInfos)
            {
                var columnTypeName = tableInfo.type;
                if (columnTypeName.IsNullOrWhiteSpace())
                    columnTypeName = await GetTypeofColumnAsync(tableInfo.name, cancellationToken).ConfigureAwait(false);

                var affinity = _affinityParser.ParseTypeName(columnTypeName);
                var columnType = new SqliteColumnType(affinity);

                var column = new SqliteDatabaseViewColumn(this, tableInfo.name, columnType, !tableInfo.notnull, tableInfo.dflt_value, null);
                result.Add(column);
            }

            return result.AsReadOnly();
        }

        protected virtual string GetTypeofColumn(Identifier columnName)
        {
            if (columnName == null)
                throw new ArgumentNullException(nameof(columnName));

            var sql = $"select typeof({ Dialect.QuoteName(columnName.LocalName) }) from { Dialect.QuoteName(Name) } limit 1";
            return Connection.ExecuteScalar<string>(sql);
        }

        protected virtual Task<string> GetTypeofColumnAsync(Identifier columnName, CancellationToken cancellationToken)
        {
            if (columnName == null)
                throw new ArgumentNullException(nameof(columnName));

            var sql = $"select typeof({ Dialect.QuoteName(columnName.LocalName) }) from { Dialect.QuoteName(Name) } limit 1";
            return Connection.ExecuteScalarAsync<string>(sql);
        }

        private readonly static SqliteTypeAffinityParser _affinityParser = new SqliteTypeAffinityParser();
    }
}
