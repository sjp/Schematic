using System;
using EnumsNET;
using SJP.Schematic.Core;

namespace SJP.Schematic.Modelled.Reflection.Model
{
    [AttributeUsage(AttributeTargets.Struct, AllowMultiple = true, Inherited = true)]
    public abstract class DeclaredTypeAttribute : ModelledSchemaAttribute
    {
        protected DeclaredTypeAttribute(DataType dataType)
            : base(new[] { Dialect.All })
        {
            if (!dataType.IsValid())
                throw new ArgumentException($"The { nameof(DataType) } provided must be a valid enum.", nameof(dataType));

            DataType = dataType;
        }

        protected DeclaredTypeAttribute(DataType dataType, params Type[] dialects)
            : base(dialects)
        {
            if (!dataType.IsValid())
                throw new ArgumentException($"The { nameof(DataType) } provided must be a valid enum.", nameof(dataType));

            DataType = dataType;
        }

        protected DeclaredTypeAttribute(DataType dataType, int length, bool isFixedLength = false)
            : base(new[] { Dialect.All })
        {
            if (!dataType.IsValid())
                throw new ArgumentException($"The { nameof(DataType) } provided must be a valid enum.", nameof(dataType));
            if (length < 0)
                throw new ArgumentOutOfRangeException(nameof(length), "A non-negative length must be provided.");

            DataType = dataType;
            Length = length;
            IsFixedLength = isFixedLength;
        }

        protected DeclaredTypeAttribute(DataType dataType, int length, bool isFixedLength = false, params Type[] dialects)
            : base(dialects)
        {
            if (!dataType.IsValid())
                throw new ArgumentException($"The { nameof(DataType) } provided must be a valid enum.", nameof(dataType));
            if (length < 0)
                throw new ArgumentOutOfRangeException(nameof(length), "A non-negative length must be provided.");

            DataType = dataType;
            Length = length;
            IsFixedLength = isFixedLength;
        }

        protected DeclaredTypeAttribute(DataType dataType, int precision, int scale = 0)
            : base(new[] { Dialect.All })
        {
            if (!dataType.IsValid())
                throw new ArgumentException($"The { nameof(DataType) } provided must be a valid enum.", nameof(dataType));
            if (precision < 0)
                throw new ArgumentOutOfRangeException(nameof(precision), "A non-negative precision must be provided.");
            if (scale < 0)
                throw new ArgumentOutOfRangeException(nameof(scale), "A non-negative scale must be provided.");

            DataType = dataType;
            Length = precision;
            Precision = precision;
            Scale = scale;
        }

        protected DeclaredTypeAttribute(DataType dataType, int precision, int scale = 0, params Type[] dialects)
            : base(dialects)
        {
            if (!dataType.IsValid())
                throw new ArgumentException($"The { nameof(DataType) } provided must be a valid enum.", nameof(dataType));
            if (precision < 0)
                throw new ArgumentOutOfRangeException(nameof(precision), "A non-negative precision must be provided.");
            if (scale < 0)
                throw new ArgumentOutOfRangeException(nameof(scale), "A non-negative scale must be provided.");

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
