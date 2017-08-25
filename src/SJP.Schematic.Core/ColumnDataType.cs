using System;

namespace SJP.Schematic.Core
{
    public abstract class ColumnDataType : IDbType
    {
        public virtual DataType Type => throw new NotImplementedException();

        public virtual bool IsFixedLength => throw new NotImplementedException();

        public virtual int Length => throw new NotImplementedException();

        public virtual Type ClrType => throw new NotImplementedException();

        // expose via IDbStringType
        public virtual bool IsUnicode => false;

        public virtual string Collation => null;

        // expose via IDbNumericType
        public virtual int Precision => UnknownLength;

        public virtual int Scale => UnknownLength;

        protected static int UnknownLength = -1;
    }
}
