using System;
using System.Data;

namespace SJP.Schema.Core
{
    public abstract class RelationalDatabase
    {
        protected RelationalDatabase(IDatabaseDialect dialect, IDbConnection connection)
        {
            Dialect = dialect ?? throw new ArgumentNullException(nameof(dialect));
            Connection = connection ?? throw new ArgumentNullException(nameof(connection));
        }

        public IDatabaseDialect Dialect { get; }

        protected IDbConnection Connection { get; }
    }
}
