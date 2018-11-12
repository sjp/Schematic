using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
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
            Connection = connection ?? throw new ArgumentNullException(nameof(connection));

            if (viewName == null)
                throw new ArgumentNullException(nameof(viewName));
            if (viewName.Schema == null)
                throw new ArgumentException("The given view name is missing a required schema name.", nameof(viewName));
            Name = viewName.Database == null
                ? viewName
                : Identifier.CreateQualifiedIdentifier(viewName.Schema, viewName.LocalName);

            if (database == null)
                throw new ArgumentNullException(nameof(database));
            if (database.Dialect == null)
                throw new ArgumentException("The given database object does not contain a dialect.", nameof(database));
            Dialect = database.Dialect;

            Pragma = new DatabasePragma(Dialect, connection, viewName.Schema);
        }

        public Identifier Name { get; }

        protected IDatabaseDialect Dialect { get; }

        protected IDbConnection Connection { get; }

        protected ISqliteDatabasePragma Pragma { get; }

        public string Definition => LoadDefinitionSync();

        public Task<string> DefinitionAsync(CancellationToken cancellationToken = default(CancellationToken)) => LoadDefinitionAsync(cancellationToken);

        protected virtual string LoadDefinitionSync()
        {
            return Connection.ExecuteScalar<string>(DefinitionQuery, new { ViewName = Name.LocalName });
        }

        protected virtual Task<string> LoadDefinitionAsync(CancellationToken cancellationToken)
        {
            return Connection.ExecuteScalarAsync<string>(DefinitionQuery, new { ViewName = Name.LocalName });
        }

        protected virtual string DefinitionQuery => $"select sql from { Dialect.QuoteIdentifier(Name.Schema) }.sqlite_master where type = 'view' and tbl_name = @ViewName";

        public bool IsIndexed => Indexes.Count > 0;

        public IReadOnlyCollection<IDatabaseIndex> Indexes => LoadIndexesSync();

        public Task<IReadOnlyCollection<IDatabaseIndex>> IndexesAsync(CancellationToken cancellationToken = default(CancellationToken)) => LoadIndexesAsync(cancellationToken);

        protected virtual IReadOnlyCollection<IDatabaseIndex> LoadIndexesSync() => Array.Empty<IDatabaseIndex>();

        protected virtual Task<IReadOnlyCollection<IDatabaseIndex>> LoadIndexesAsync(CancellationToken cancellationToken = default(CancellationToken)) => Task.FromResult(LoadIndexesSync());

        public IReadOnlyList<IDatabaseColumn> Columns => LoadColumnsSync();

        public Task<IReadOnlyList<IDatabaseColumn>> ColumnsAsync(CancellationToken cancellationToken = default(CancellationToken)) => LoadColumnsAsync(cancellationToken);

        protected virtual IReadOnlyList<IDatabaseColumn> LoadColumnsSync()
        {
            var tableInfos = Pragma.TableInfo(Name);

            var result = new List<IDatabaseColumn>();
            if (tableInfos.Empty())
                return result;

            foreach (var tableInfo in tableInfos)
            {
                var columnTypeName = tableInfo.type;
                if (columnTypeName.IsNullOrWhiteSpace())
                    columnTypeName = GetTypeofColumn(tableInfo.name);

                var affinity = _affinityParser.ParseTypeName(columnTypeName);
                var columnType = new SqliteColumnType(affinity);

                var column = new DatabaseColumn(tableInfo.name, columnType, !tableInfo.notnull, tableInfo.dflt_value, null);
                result.Add(column);
            }

            return result.AsReadOnly();
        }

        protected virtual async Task<IReadOnlyList<IDatabaseColumn>> LoadColumnsAsync(CancellationToken cancellationToken)
        {
            var tableInfos = await Pragma.TableInfoAsync(Name, cancellationToken).ConfigureAwait(false);

            var result = new List<IDatabaseColumn>();
            if (tableInfos.Empty())
                return result;

            foreach (var tableInfo in tableInfos)
            {
                var columnTypeName = tableInfo.type;
                if (columnTypeName.IsNullOrWhiteSpace())
                    columnTypeName = await GetTypeofColumnAsync(tableInfo.name, cancellationToken).ConfigureAwait(false);

                var affinity = _affinityParser.ParseTypeName(columnTypeName);
                var columnType = new SqliteColumnType(affinity);

                var column = new DatabaseColumn(tableInfo.name, columnType, !tableInfo.notnull, tableInfo.dflt_value, null);
                result.Add(column);
            }

            return result.AsReadOnly();
        }

        protected virtual string GetTypeofColumn(Identifier columnName)
        {
            if (columnName == null)
                throw new ArgumentNullException(nameof(columnName));

            var sql = GetTypeofQuery(columnName.LocalName);
            return Connection.ExecuteScalar<string>(sql);
        }

        protected virtual Task<string> GetTypeofColumnAsync(Identifier columnName, CancellationToken cancellationToken)
        {
            if (columnName == null)
                throw new ArgumentNullException(nameof(columnName));

            var sql = GetTypeofQuery(columnName.LocalName);
            return Connection.ExecuteScalarAsync<string>(sql);
        }

        protected virtual string GetTypeofQuery(string columnName) => $"select typeof({ Dialect.QuoteName(columnName) }) from { Dialect.QuoteName(Name) } limit 1";

        private readonly static SqliteTypeAffinityParser _affinityParser = new SqliteTypeAffinityParser();
    }
}
