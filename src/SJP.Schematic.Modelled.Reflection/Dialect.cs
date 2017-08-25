using System;

namespace SJP.Schematic.Modelled.Reflection
{
    public sealed class Dialect
    {
        // to avoid creating an instance
        // we just want to be able to do Dialect.All for annotation
        private Dialect() { }

        public static Type All { get; } = typeof(Dialect);
    }
}
