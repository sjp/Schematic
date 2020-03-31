#pragma warning disable IDE1006, S101 // Naming Styles
namespace SJP.Schematic.Sqlite.Pragma.Query
{
    /// <summary>
    /// Stores information on the functions available on a connection.
    /// </summary>
    public class pragma_function_list
    {
        /// <summary>
        /// The name of the function.
        /// </summary>
        public string name { get; set; } = default!;

        /// <summary>
        /// Whether the function is built-in, i.e. distributed with SQLite.
        /// </summary>
        public bool builtin { get; set; }

        /// <summary>
        /// The type of function. One of scalar, aggregate or window. i.e. s/w/a
        /// </summary>
        public string type { get; set; } = default!;

        /// <summary>
        /// The encoding of the function.
        /// </summary>
        public Encoding enc { get; set; }

        /// <summary>
        /// The number of arguments to the function. -1 indicates no arguments.
        /// </summary>
        public int narg { get; set; }

        /// <summary>
        /// Flags that determine some of the behaviour of the function.
        /// </summary>
        public FunctionOptions flags { get; set; }
    }
}
#pragma warning restore IDE1006, S101 // Naming Styles