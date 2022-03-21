using System;
using System.Collections.Generic;

namespace SJP.Schematic.Sqlite.Parsing;

internal sealed class TableMember
{
    public TableMember(ColumnDefinition column)
    {
        if (column == null)
            throw new ArgumentNullException(nameof(column));

        Columns = column.ToEnumerable();
        Constraints = Array.Empty<TableConstraint>();
    }

    public TableMember(TableConstraint constraint)
    {
        if (constraint == null)
            throw new ArgumentNullException(nameof(constraint));

        Columns = Array.Empty<ColumnDefinition>();
        Constraints = constraint.ToEnumerable();
    }

    public IEnumerable<ColumnDefinition> Columns { get; }

    public IEnumerable<TableConstraint> Constraints { get; }
}