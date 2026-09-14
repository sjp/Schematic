namespace SJP.Schematic.PostgreSql.Queries;

/// <summary>
/// The foreign key columns shared by the parent-key and child-key queries, one row per column of the
/// foreign key, so that a foreign key is built by the same code whichever end of it is being loaded.
/// </summary>
internal interface IForeignKeyColumnRow
{
    string ChildKeyName { get; }

    string ColumnName { get; }

    int ConstraintColumnId { get; }

    string DeleteAction { get; }

    string UpdateAction { get; }

    bool IsValidated { get; }

    bool IsDeferrable { get; }

    bool IsInitiallyDeferred { get; }

    string MatchType { get; }

    bool IsSetNullColumn { get; }
}
