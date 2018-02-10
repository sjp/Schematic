using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using SJP.Schematic.Core;
using SJP.Schematic.Sqlite.Pragma;

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

        public Task<string> DefinitionAsync() => LoadDefinitionAsync();

        protected virtual string LoadDefinitionSync()
        {
            var sql = $"select sql from { Dialect.QuoteIdentifier(Name.Schema) }.sqlite_master where type = 'view' and tbl_name = @ViewName";
            return Connection.ExecuteScalar<string>(sql, new { ViewName = Name.LocalName });
        }

        protected virtual Task<string> LoadDefinitionAsync()
        {
            var sql = $"select sql from { Dialect.QuoteIdentifier(Name.Schema) }.sqlite_master where type = 'view' and tbl_name = @ViewName";
            return Connection.ExecuteScalarAsync<string>(sql, new { ViewName = Name.LocalName });
        }

        public bool IsIndexed => Indexes.Any();

        public IReadOnlyDictionary<Identifier, IDatabaseViewIndex> Index => LoadIndexLookupSync();

        public Task<IReadOnlyDictionary<Identifier, IDatabaseViewIndex>> IndexAsync() => LoadIndexLookupAsync();

        public IEnumerable<IDatabaseViewIndex> Indexes => LoadIndexesSync();

        public Task<IEnumerable<IDatabaseViewIndex>> IndexesAsync() => LoadIndexesAsync();

        protected virtual IEnumerable<IDatabaseViewIndex> LoadIndexesSync() => Enumerable.Empty<IDatabaseViewIndex>();

        protected virtual Task<IEnumerable<IDatabaseViewIndex>> LoadIndexesAsync() => Task.FromResult(LoadIndexesSync());

        protected virtual IReadOnlyDictionary<Identifier, IDatabaseViewIndex> LoadIndexLookupSync()
        {
            var result = new Dictionary<Identifier, IDatabaseViewIndex>(Comparer);
            return result.AsReadOnlyDictionary();
        }

        protected virtual Task<IReadOnlyDictionary<Identifier, IDatabaseViewIndex>> LoadIndexLookupAsync() => Task.FromResult(LoadIndexLookupSync());

        public IReadOnlyDictionary<Identifier, IDatabaseViewColumn> Column => LoadColumnLookupSync();

        public Task<IReadOnlyDictionary<Identifier, IDatabaseViewColumn>> ColumnAsync() => LoadColumnLookupAsync();

        public IReadOnlyList<IDatabaseViewColumn> Columns => LoadColumnsSync();

        public Task<IReadOnlyList<IDatabaseViewColumn>> ColumnsAsync() => LoadColumnsAsync();

        protected virtual IReadOnlyDictionary<Identifier, IDatabaseViewColumn> LoadColumnLookupSync()
        {
            var result = new Dictionary<Identifier, IDatabaseViewColumn>(Comparer);

            foreach (var column in Columns.Where(c => c.Name != null))
                result[column.Name.LocalName] = column;

            return result.AsReadOnlyDictionary();
        }

        protected virtual async Task<IReadOnlyDictionary<Identifier, IDatabaseViewColumn>> LoadColumnLookupAsync()
        {
            var result = new Dictionary<Identifier, IDatabaseViewColumn>(Comparer);

            var columns = await ColumnsAsync().ConfigureAwait(false);
            foreach (var column in columns.Where(c => c.Name != null))
                result[column.Name.LocalName] = column;

            return result.AsReadOnlyDictionary();
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

        protected virtual async Task<IReadOnlyList<IDatabaseViewColumn>> LoadColumnsAsync()
        {
            var tableInfos = await Pragma.TableInfoAsync(Name).ConfigureAwait(false);

            var result = new List<IDatabaseViewColumn>();
            if (tableInfos.Empty())
                return result;

            foreach (var tableInfo in tableInfos)
            {
                var columnTypeName = tableInfo.type;
                if (columnTypeName.IsNullOrWhiteSpace())
                    columnTypeName = await GetTypeofColumnAsync(tableInfo.name).ConfigureAwait(false);

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

        protected virtual Task<string> GetTypeofColumnAsync(Identifier columnName)
        {
            if (columnName == null)
                throw new ArgumentNullException(nameof(columnName));

            var sql = $"select typeof({ Dialect.QuoteName(columnName.LocalName) }) from { Dialect.QuoteName(Name) } limit 1";
            return Connection.ExecuteScalarAsync<string>(sql);
        }

        private readonly static SqliteTypeAffinityParser _affinityParser = new SqliteTypeAffinityParser();
    }
}
