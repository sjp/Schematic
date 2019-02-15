namespace SJP.Schematic.PostgreSql.Query
{
    internal class IndexColumns
    {
        public string IndexName { get; set; }

        public bool IsUnique { get; set; }

        public bool IsPrimary { get; set; }

        public int KeyColumnCount { get; set; }

        public int IndexColumnId { get; set; }

        public string IndexColumnExpression { get; set; }

        public bool IsDescending { get; set; }

        public bool IsFunctional { get; set; }
    }
}
