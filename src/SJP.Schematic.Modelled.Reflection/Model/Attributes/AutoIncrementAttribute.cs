using System;

namespace SJP.Schematic.Modelled.Reflection.Model
{
    [AttributeUsage(AttributeTargets.Struct | AttributeTargets.Property, AllowMultiple = true, Inherited = true)]
    public sealed class AutoIncrementAttribute : ModelledSchemaAttribute
    {
        public AutoIncrementAttribute()
            : base(new[] { Dialect.All })
        {
            InitialValue = 1;
            Increment = 1;
        }

        public AutoIncrementAttribute(params Type[] dialects)
            : base(dialects)
        {
            InitialValue = 1;
            Increment = 1;
        }

        public AutoIncrementAttribute(decimal initialValue, decimal increment)
            : base(new[] { Dialect.All })
        {
            InitialValue = initialValue;
            Increment = increment;
        }

        public AutoIncrementAttribute(decimal initialValue, decimal increment, params Type[] dialects)
            : base(dialects)
        {
            InitialValue = initialValue;
            Increment = increment;
        }

        public decimal InitialValue { get; }

        public decimal Increment { get; }
    }
}
