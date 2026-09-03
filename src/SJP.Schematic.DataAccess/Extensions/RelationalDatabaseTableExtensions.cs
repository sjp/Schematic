using System;
using System.Collections.Generic;
using System.Linq;
using SJP.Schematic.Core;

namespace SJP.Schematic.DataAccess.Extensions;

/// <summary>
/// Extension methods for working with a table when generating data access code.
/// </summary>
public static class RelationalDatabaseTableExtensions
{
    /// <summary>
    /// The columns of a table that a generated class should map, in table order.
    /// </summary>
    /// <remarks>
    /// A hidden column is left out. A generated class describes a row as <c>SELECT *</c> returns it,
    /// and a hidden column is absent from that, so mapping one would make every query built from the
    /// class ask for a column it did not receive. The exception is a column the schema names
    /// structurally, in a key or an index: a class that cannot identify its own row, or state the
    /// value on the near side of a relationship, is of no use, and a configuration that points an
    /// index at a member the class does not have will not compile.
    /// </remarks>
    /// <param name="table">A database table.</param>
    /// <returns>The columns to generate members for.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="table"/> is <see langword="null" />.</exception>
    public static IEnumerable<IDatabaseColumn> GetMappedColumns(this IRelationalDatabaseTable table)
    {
        ArgumentNullException.ThrowIfNull(table);

        if (!table.Columns.Any(static c => c.IsHidden))
            return table.Columns;

        var referencedColumnNames = table.PrimaryKey
            .Match(static pk => pk.Columns.AsEnumerable(), Enumerable.Empty<IDatabaseColumn>)
            .Concat(table.UniqueKeys.SelectMany(static uk => uk.Columns))
            .Concat(table.ParentKeys.SelectMany(static fk => fk.ChildKey.Columns))
            .Concat(table.ChildKeys.SelectMany(static ck => ck.ParentKey.Columns))
            .Concat(table.Indexes.SelectMany(static i => i.Columns).SelectMany(static c => c.DependentColumns))
            .Select(static c => c.Name)
            .ToHashSet();

        return table.Columns
            .Where(c => !c.IsHidden || referencedColumnNames.Contains(c.Name))
            .ToList();
    }
}
