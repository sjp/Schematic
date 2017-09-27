namespace SJP.Schematic.Core
{
    public struct AutoIncrement : IAutoIncrement
    {
        public AutoIncrement(decimal initialValue, decimal increment)
        {
            InitialValue = initialValue;
            Increment = increment;
        }

        public decimal InitialValue { get; }

        public decimal Increment { get; }
    }
}
