namespace SJP.Schematic.PostgreSql.Query
{
    internal class ForeignKeyData
    {
        public string ChildKeyName { get; set; }

        public string ColumnName { get; set; }

        public string ParentSchemaName { get; set; }

        public string ParentTableName { get; set; }

        public int ConstraintColumnId { get; set; }

        public string ParentKeyName { get; set; }

        public string ParentKeyType { get; set; }

        public string DeleteAction { get; set; }

        public string UpdateAction { get; set; }
    }
}
