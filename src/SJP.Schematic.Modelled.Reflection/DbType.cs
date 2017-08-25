using System;
using System.Collections.Generic;
using SJP.Schema.Core;

namespace SJP.Schema.Modelled.Reflection
{
    public class DbType<T> : IDbType<T>
    {
        public DbType()
        {
        }

        public DataType DataType => throw new NotImplementedException();

        public T ClrType => throw new NotImplementedException();

        public bool IsFixedLength => throw new NotImplementedException();

        public int Length => throw new NotImplementedException();

        public IEnumerable<KeyValuePair<IDatabaseDialect, string>> TypeNames { get; }
    }
}
