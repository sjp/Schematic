using System;
using System.Collections.Generic;

namespace SJP.Schematic.Serialization.Dto;

public class RelationalDatabaseTable
{
    public required Identifier? TableName { get; init; }

    public required DatabaseKey? PrimaryKey { get; init; }

    public required IEnumerable<DatabaseColumn> Columns { get; init; }

    public required IEnumerable<DatabaseCheckConstraint> Checks { get; init; }

    public required IEnumerable<DatabaseIndex> Indexes { get; init; }

    public required IEnumerable<DatabaseKey> UniqueKeys { get; init; }

    public required IEnumerable<DatabaseRelationalKey> ParentKeys { get; init; }

    public required IEnumerable<DatabaseRelationalKey> ChildKeys { get; init; }

    public required IEnumerable<DatabaseTrigger> Triggers { get; init; }
}