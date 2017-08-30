using System;
using System.Collections.Generic;
using System.Data;

namespace SJP.Schematic.Core
{
    public abstract class DatabaseDialect<TDialect> : IDatabaseDialect where TDialect : IDatabaseDialect
    {
        protected DatabaseDialect()
        {
        }

        public abstract IDbConnection CreateConnection(string connectionString, bool openConnection = true);

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

        public abstract bool IsValidColumnName(Identifier name);

        public abstract bool IsValidConstraintName(Identifier name);

        public abstract bool IsValidObjectName(Identifier name);

        // TODO: implement mapping for abstract types to physical types
        public abstract string GetTypeName(DataType dataType);
    }
}
