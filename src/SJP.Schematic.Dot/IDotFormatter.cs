using System.Collections.Generic;
using SJP.Schematic.Core;

namespace SJP.Schematic.Dot
{
    /// <summary>
    /// Defines a formatter that is capable of producing a DOT graph from a set of tables.
    /// </summary>
    public interface IDotFormatter
    {
        /// <summary>
        /// Renders the tables as a DOT graph.
        /// </summary>
        /// <param name="tables">The tables.</param>
        /// <returns>A string containing a dot representation of the table relationship graph.</returns>
        string RenderTables(IEnumerable<IRelationalDatabaseTable> tables);

        /// <summary>
        /// Renders the tables as a DOT graph.
        /// </summary>
        /// <param name="tables">The tables.</param>
        /// <param name="options">Options to configure how the DOT graph is rendered.</param>
        /// <returns>A string containing a dot representation of the table relationship graph.</returns>
        string RenderTables(IEnumerable<IRelationalDatabaseTable> tables, DotRenderOptions options);

        /// <summary>
        /// Renders the tables as a DOT graph.
        /// </summary>
        /// <param name="tables">The tables.</param>
        /// <param name="rowCounts">Row counts for each of the provided tables.</param>
        /// <returns>A string containing a dot representation of the table relationship graph.</returns>
        string RenderTables(IEnumerable<IRelationalDatabaseTable> tables, IReadOnlyDictionary<Identifier, ulong> rowCounts);

        /// <summary>
        /// Renders the tables as a DOT graph.
        /// </summary>
        /// <param name="tables">The tables.</param>
        /// <param name="rowCounts">Row counts for each of the provided tables.</param>
        /// <param name="options">Options to configure how the DOT graph is rendered.</param>
        /// <returns>A string containing a dot representation of the table relationship graph.</returns>
        string RenderTables(IEnumerable<IRelationalDatabaseTable> tables, IReadOnlyDictionary<Identifier, ulong> rowCounts, DotRenderOptions options);
    }
}
