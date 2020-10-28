namespace SJP.Schematic.PostgreSql.Query
{
    internal sealed class ForeignKeyData
    {
        public string ChildKeyName { get; set; } = default!;

        public string ColumnName { get; set; } = default!;

        public string ParentSchemaName { get; set; } = default!;

        public string ParentTableName { get; set; } = default!;

        public int ConstraintColumnId { get; set; }

        public string ParentKeyName { get; set; } = default!;

        public string ParentKeyType { get; set; } = default!;

        public string DeleteAction { get; set; } = default!;

        public string UpdateAction { get; set; } = default!;
    }
}
