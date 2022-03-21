using System.Collections.Generic;
using LanguageExt;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Comments;

namespace SJP.Schematic.DataAccess;

/// <summary>
/// Defines a database table source code generator.
/// </summary>
/// <seealso cref="IDatabaseEntityGenerator" />
public interface IDatabaseTableGenerator : IDatabaseEntityGenerator
{
    /// <summary>
    /// Generates source code that enables interoperability with a given database table.
    /// </summary>
    /// <param name="tables">All database tables within a database.</param>
    /// <param name="table">A database table to generate code for.</param>
    /// <param name="comment">Comment information for the given table.</param>
    /// <returns>A string containing source code to interact with the table.</returns>
    string Generate(IReadOnlyCollection<IRelationalDatabaseTable> tables, IRelationalDatabaseTable table, Option<IRelationalDatabaseTableComments> comment);
}