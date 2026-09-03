using SJP.Schematic.Core;

namespace SJP.Schematic.Lint.Rules;

/// <summary>
/// Identifies tables whose shape and contents are decided by the database rather than by whoever
/// wrote the schema, so that rules which would only ever nag about them can stay quiet.
/// </summary>
internal static class DatabaseManagedTables
{
    /// <summary>
    /// Determines whether a table exists to serve another table. A history table is written only by
    /// system versioning, and a partition or its parent is one half of a table that was declared
    /// once; in both cases the row counts, indexes and relations belong to the table they serve.
    /// </summary>
    /// <param name="table">A database table.</param>
    /// <returns><see langword="true" /> when the table is managed by the database on another table's behalf.</returns>
    public static bool IsManagedByDatabase(IRelationalDatabaseTable table)
        => table.Kind is TableKind.History or TableKind.Partition or TableKind.PartitionParent;
}
