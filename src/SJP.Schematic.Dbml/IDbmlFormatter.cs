using System;
using System.Collections.Generic;
using SJP.Schematic.Core;

namespace SJP.Schematic.Dbml;

/// <summary>
/// Defines a formatter that turns a set of database tables into a DBML definition.
/// </summary>
public interface IDbmlFormatter
{
    /// <summary>
    /// Renders database tables as a DBML format.
    /// </summary>
    /// <param name="tables">A collection of database tables.</param>
    /// <returns>A string, in DBML format.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="tables"/> is <see langword="null" /> or has <see langword="null" /> values.</exception>
    string RenderTables(IReadOnlyCollection<IRelationalDatabaseTable> tables);
}