﻿using System;
using SJP.Schematic.Core;

namespace SJP.Schematic.Modelled.Reflection.Model
{
    public static partial class ColumnType
    {
        [AttributeUsage(AttributeTargets.Struct, AllowMultiple = true, Inherited = true)]
        public sealed class IntervalAttribute : DeclaredTypeAttribute
        {
            public IntervalAttribute(int secondPrecision)
            : base(DataType.Interval, secondPrecision, false, new[] { Dialect.All })
            {
            }

            public IntervalAttribute(int secondPrecision, params Type[] dialects)
                : base(DataType.Interval, secondPrecision, false, dialects)
            {
            }
        }
    }
}
