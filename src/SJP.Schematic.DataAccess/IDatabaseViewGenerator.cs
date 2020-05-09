using LanguageExt;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Comments;

namespace SJP.Schematic.DataAccess
{
    /// <summary>
    /// Defines a database view source code generator.
    /// </summary>
    /// <seealso cref="IDatabaseEntityGenerator" />
    public interface IDatabaseViewGenerator : IDatabaseEntityGenerator
    {
        /// <summary>
        /// Generates source code that enables interoperability with a given database view.
        /// </summary>
        /// <param name="view">A database view.</param>
        /// <param name="comment">Comment information for the given view.</param>
        /// <returns>A string containing source code to interact with the view.</returns>
        string Generate(IDatabaseView view, Option<IDatabaseViewComments> comment);
    }
}
