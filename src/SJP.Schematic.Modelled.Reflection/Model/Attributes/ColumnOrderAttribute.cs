using System;

namespace SJP.Schematic.Modelled.Reflection.Model
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true, Inherited = true)]
    public sealed class ColumnOrderAttribute : ModelledSchemaAttribute
    {
        public ColumnOrderAttribute(int columnNumber)
            : base(new[] { Dialect.All })
        {
            if (columnNumber < 0)
                throw new ArgumentOutOfRangeException(nameof(columnNumber), "The assigned column number must be non-negative. Instead given: " + columnNumber.ToString());

            ColumnNumber = columnNumber;
        }

        public ColumnOrderAttribute(int columnNumber, params Type[] dialects)
            : base(dialects)
        {
            if (columnNumber < 0)
                throw new ArgumentOutOfRangeException(nameof(columnNumber), "The assigned column number must be non-negative. Instead given: " + columnNumber.ToString());

            ColumnNumber = columnNumber;
        }

        public int ColumnNumber { get; }
    }
}
