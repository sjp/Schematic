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
        public static NoneConstraint None => new();

        /// <summary>
        /// A constraint which asserts that a result must be a some value.
        /// </summary>
        /// <value>The some asserting constraint.</value>
        public static SomeConstraint Some => new();

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
            public static SomeConstraint None => new();

            /// <summary>
            /// A constraint which asserts that a result must be a none value. i.e. not some.
            /// </summary>
            /// <value>The none asserting constraint.</value>
            public static NoneConstraint Some => new();
#pragma warning restore S3218 // Inner class members should not shadow outer class "static" or type members
        }
    }
}
