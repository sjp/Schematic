using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using SJP.Schema.Core;

namespace SJP.Schema.SqlServer
{
    public class SqlServerDialect : DatabaseDialect<SqlServerDialect>
    {
        public override IDbConnection CreateConnection(string connectionString, bool openConnection = true)
        {
            var connection = new SqlConnection(connectionString);
            if (openConnection)
                connection.Open();
            return connection;
        }

        public override string GetTypeName(DataType dataType)
        {
            throw new NotImplementedException();
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

        public override string QuoteIdentifier(string identifier)
        {
            if (identifier.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(identifier));

            return $"[{ identifier.Replace("]", "]]") }]";
        }

        public override string QuoteName(Identifier name)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));

            var pieces = new List<string>();

            if (name.Database != null)
                pieces.Add(QuoteIdentifier(name.Database));
            if (name.Schema != null)
                pieces.Add(QuoteIdentifier(name.Schema));
            if (name.LocalName != null)
                pieces.Add(QuoteIdentifier(name.LocalName));

            return pieces.Join(".");
        }
    }
}
