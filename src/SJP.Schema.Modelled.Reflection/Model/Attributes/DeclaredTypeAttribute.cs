﻿using System;
using SJP.Schema.Core;

namespace SJP.Schema.Modelled.Reflection.Model
{
    [AttributeUsage(AttributeTargets.Struct, AllowMultiple = true, Inherited = true)]
    public abstract class DeclaredTypeAttribute : ModelledSchemaAttribute
    {
        protected DeclaredTypeAttribute(DataType dataType)
            : base(new[] { Dialect.All })
        {
            DataType = dataType;
        }

        protected DeclaredTypeAttribute(DataType dataType, params Type[] dialects)
            : base(dialects)
        {
            DataType = dataType;
        }

        protected DeclaredTypeAttribute(DataType dataType, int length, bool isFixedLength = false)
            : base(new[] { Dialect.All })
        {
            if (length < 0)
                throw new ArgumentOutOfRangeException(nameof(length), "A non-negative length must be provided");

            DataType = dataType;
            Length = length;
            IsFixedLength = isFixedLength;
        }

        protected DeclaredTypeAttribute(DataType dataType, int length, bool isFixedLength = false, params Type[] dialects)
            : base(dialects)
        {
            if (length < 0)
                throw new ArgumentOutOfRangeException(nameof(length), "A non-negative length must be provided");

            DataType = dataType;
            Length = length;
            IsFixedLength = isFixedLength;
        }

        protected DeclaredTypeAttribute(DataType dataType, int precision, int scale = 0)
            : base(new[] { Dialect.All })
        {
            if (precision < 0)
                throw new ArgumentOutOfRangeException(nameof(precision), "A non-negative precision must be provided");
            if (scale < 0)
                throw new ArgumentOutOfRangeException(nameof(scale), "A non-negative scale must be provided");

            DataType = dataType;
            Length = precision;
            Precision = precision;
            Scale = scale;
        }

        protected DeclaredTypeAttribute(DataType dataType, int precision, int scale = 0, params Type[] dialects)
            : base(dialects)
        {
            if (precision < 0)
                throw new ArgumentOutOfRangeException(nameof(precision), "A non-negative precision must be provided");
            if (scale < 0)
                throw new ArgumentOutOfRangeException(nameof(scale), "A non-negative scale must be provided");

            DataType = dataType;
            Length = precision;
            Precision = precision;
            Scale = scale;
        }

        public virtual DataType DataType { get; }

        public virtual int Length { get; } = UnknownLength;

        public virtual int Precision { get; } = UnknownLength;

        public virtual int Scale { get; } = UnknownLength;

        public virtual bool IsFixedLength { get; }

        protected static int UnknownLength = -1;
    }
}
