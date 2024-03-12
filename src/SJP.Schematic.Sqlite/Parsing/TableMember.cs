using System;
using System.Collections.Generic;

namespace SJP.Schematic.Sqlite.Parsing;

internal sealed class TableMember
{
    public TableMember(ColumnDefinition column)
    {
        ArgumentNullException.ThrowIfNull(column);

        Columns = column.ToEnumerable();
        Constraints = [];
    }

    public TableMember(TableConstraint constraint)
    {
        ArgumentNullException.ThrowIfNull(constraint);

        Columns = [];
        Constraints = constraint.ToEnumerable();
    }

    public IEnumerable<ColumnDefinition> Columns { get; }

    public IEnumerable<TableConstraint> Constraints { get; }
}