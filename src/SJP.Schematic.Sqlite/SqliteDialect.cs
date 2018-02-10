using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using SJP.Schematic.Core;

namespace SJP.Schematic.Sqlite
{
    public class SqliteDialect : DatabaseDialect<SqliteDialect>
    {
        public override IDbConnection CreateConnection(string connectionString)
        {
            var connection = new SqliteConnection(connectionString);
            connection.Open();
            return connection;
        }

        public override async Task<IDbConnection> CreateConnectionAsync(string connectionString, CancellationToken cancellationToken = default(CancellationToken))
        {
            var connection = new SqliteConnection(connectionString);
            await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
            return connection;
        }

        public override bool IsValidColumnName(Identifier name)
        {
            throw new NotImplementedException();
        }

        public override bool IsValidConstraintName(Identifier name)
        {
            throw new NotImplementedException();
        }

        public override bool IsValidObjectName(Identifier name)
        {
            throw new NotImplementedException();
        }

        public override string QuoteName(Identifier name)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));

            if (name.Schema != null)
            {
                return QuoteIdentifier(name.Schema)
                    + "."
                    + QuoteIdentifier(name.LocalName);
            }
            else
            {
                return QuoteIdentifier(name.LocalName);
            }
        }

        public override IDbTypeProvider TypeProvider => _typeProvider;

        private readonly static IDbTypeProvider _typeProvider = new SqliteDbTypeProvider();
    }
}
