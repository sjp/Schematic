namespace SJP.Schematic.Tests.Utilities
{
    public static class OptionIs
    {
        public static NoneConstraint None => new NoneConstraint();

        public static SomeConstraint Some => new SomeConstraint();

#pragma warning disable CA1034 // Nested types should not be visible
        public static class Not
        {
#pragma warning disable S3218 // Inner class members should not shadow outer class "static" or type members
            public static SomeConstraint None => new SomeConstraint();

            public static NoneConstraint Some => new NoneConstraint();
#pragma warning restore S3218 // Inner class members should not shadow outer class "static" or type members
        }
#pragma warning restore CA1034 // Nested types should not be visible
    }
}
