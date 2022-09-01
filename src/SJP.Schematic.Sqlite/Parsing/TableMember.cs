using System;
using System.Collections.Generic;

namespace SJP.Schematic.Sqlite.Parsing;

internal sealed class TableMember
{
    public TableMember(ColumnDefinition column)
    {
        ArgumentNullException.ThrowIfNull(column);

        Columns = column.ToEnumerable();
        Constraints = Array.Empty<TableConstraint>();
    }

    public TableMember(TableConstraint constraint)
    {
        ArgumentNullException.ThrowIfNull(constraint);

        Columns = Array.Empty<ColumnDefinition>();
        Constraints = constraint.ToEnumerable();
    }

    public IEnumerable<ColumnDefinition> Columns { get; }

    public IEnumerable<TableConstraint> Constraints { get; }
}