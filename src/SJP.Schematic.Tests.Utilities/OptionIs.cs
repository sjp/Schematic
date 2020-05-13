using LanguageExt;

namespace SJP.Schematic.Tests.Utilities
{
    /// <summary>
    /// A helper class for retrieving constraints for <see cref="Option{A}"/> values.
    /// </summary>
    public static class OptionIs
    {
        /// <summary>
        /// A constraint which asserts that a result must be a none value.
        /// </summary>
        /// <value>The none asserting constraint.</value>
        public static NoneConstraint None => new NoneConstraint();

        /// <summary>
        /// A constraint which asserts that a result must be a some value.
        /// </summary>
        /// <value>The some asserting constraint.</value>
        public static SomeConstraint Some => new SomeConstraint();

#pragma warning disable CA1034 // Nested types should not be visible

        /// <summary>
        /// A helper class for retrieving constraints for <see cref="Option{A}"/> values.
        /// </summary>
        public static class Not
        {
#pragma warning disable S3218 // Inner class members should not shadow outer class "static" or type members

            /// <summary>
            /// A constraint which asserts that a result must be a some value. i.e. not none
            /// </summary>
            /// <value>The some asserting constraint.</value>
            public static SomeConstraint None => new SomeConstraint();

            /// <summary>
            /// A constraint which asserts that a result must be a none value. i.e. not some.
            /// </summary>
            /// <value>The none asserting constraint.</value>
            public static NoneConstraint Some => new NoneConstraint();
#pragma warning restore S3218 // Inner class members should not shadow outer class "static" or type members
        }
#pragma warning restore CA1034 // Nested types should not be visible
    }
}
