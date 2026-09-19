namespace SJP.Schematic.MySql.Queries;

/// <summary>
/// The foreign key columns shared by the parent-key and foreign-key-column queries, one row per column of
/// a foreign key, so that a foreign key is built by the same code whichever end of it is being loaded.
/// </summary>
/// <remarks>
/// Both queries order their rows by constraint name and ordinal position, so a key's columns already arrive
/// in key order and the ordinal is not needed to rebuild the key. The constraint name is likewise known to
/// the caller before the rows are grouped by it.
/// </remarks>
internal interface IForeignKeyColumnRow
{
    string ColumnName { get; }
}
