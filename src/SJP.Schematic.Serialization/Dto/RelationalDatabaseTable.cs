using System;
using System.Collections.Generic;

namespace SJP.Schematic.Serialization.Dto;

public class RelationalDatabaseTable
{
    public Identifier? TableName { get; set; }

    public DatabaseKey? PrimaryKey { get; set; }

    public IEnumerable<DatabaseColumn> Columns { get; set; } = Array.Empty<DatabaseColumn>();

    public IEnumerable<DatabaseCheckConstraint> Checks { get; set; } = Array.Empty<DatabaseCheckConstraint>();

    public IEnumerable<DatabaseIndex> Indexes { get; set; } = Array.Empty<DatabaseIndex>();

    public IEnumerable<DatabaseKey> UniqueKeys { get; set; } = Array.Empty<DatabaseKey>();

    public IEnumerable<DatabaseRelationalKey> ParentKeys { get; set; } = Array.Empty<DatabaseRelationalKey>();

    public IEnumerable<DatabaseRelationalKey> ChildKeys { get; set; } = Array.Empty<DatabaseRelationalKey>();

    public IEnumerable<DatabaseTrigger> Triggers { get; set; } = Array.Empty<DatabaseTrigger>();
}
