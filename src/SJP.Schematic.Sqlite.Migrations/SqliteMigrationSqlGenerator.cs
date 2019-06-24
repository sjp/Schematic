using System;
using System.Collections.Generic;
using System.Data;
using SJP.Schematic.Core;
using SJP.Schematic.Migrations;

namespace SJP.Schematic.Sqlite.Migrations
{
    public class SqliteMigrationSqlGenerator : IMigrationsSqlGenerator
    {
        public SqliteMigrationSqlGenerator(IDbConnection connection, IDatabaseDialect dialect, IRelationalDatabase database)
        {
            Connection = connection ?? throw new ArgumentNullException(nameof(connection));
            Dialect = dialect ?? throw new ArgumentNullException(nameof(dialect));
            Database = database ?? throw new ArgumentNullException(nameof(database));
        }

        protected IDbConnection Connection { get; }

        protected IDatabaseDialect Dialect { get; }

        protected IRelationalDatabase Database { get; }

        public IReadOnlyList<ISqlCommand> GenerateSql(IEnumerable<IMigrationOperation> operations)
        {
            if (operations == null)
                throw new ArgumentNullException(nameof(operations));

            return Array.Empty<ISqlCommand>();
        }
    }
}
