using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Core
{
    public abstract class DatabaseDialect<TDialect> : IDatabaseDialect where TDialect : IDatabaseDialect
    {
        protected DatabaseDialect()
        {
        }

        public abstract IDbConnection CreateConnection(string connectionString);

        public abstract Task<IDbConnection> CreateConnectionAsync(string connectionString, CancellationToken cancellationToken = default(CancellationToken));

        public virtual string QuoteName(Identifier name)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));

            var pieces = new List<string>();

            if (name.Server != null)
                pieces.Add(QuoteIdentifier(name.Server));
            if (name.Database != null)
                pieces.Add(QuoteIdentifier(name.Database));
            if (name.Schema != null)
                pieces.Add(QuoteIdentifier(name.Schema));
            if (name.LocalName != null)
                pieces.Add(QuoteIdentifier(name.LocalName));

            return pieces.Join(".");
        }

        public virtual string QuoteIdentifier(string identifier)
        {
            if (identifier.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(identifier));

            return $"\"{ identifier.Replace("\"", "\"\"") }\"";
        }

        public abstract bool IsReservedKeyword(string text);

        public abstract IDbTypeProvider TypeProvider { get; }
    }
}
