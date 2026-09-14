namespace SJP.Schematic.MySql.Queries;

/// <summary>
/// The foreign key columns shared by the parent-key and foreign-key-column queries, one row per column of
/// a foreign key, so that a foreign key is built by the same code whichever end of it is being loaded.
/// </summary>
internal interface IForeignKeyColumnRow
{
    string ChildKeyName { get; }

    string ColumnName { get; }

    int ConstraintColumnId { get; }
}
