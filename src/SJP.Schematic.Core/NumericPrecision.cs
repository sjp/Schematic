using System;

namespace SJP.Schematic.Core
{
    public struct NumericPrecision
    {
        public NumericPrecision(int precision, int scale)
        {
            Precision = precision;
            Scale = scale;
        }

        public int Precision { get; }

        public int Scale { get; }
    }
}
