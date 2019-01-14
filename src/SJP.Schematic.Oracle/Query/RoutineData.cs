namespace SJP.Schematic.Oracle.Query
{
    internal class RoutineData
    {
        public string SchemaName { get; set; }

        public string RoutineName { get; set; }

        public string RoutineType { get; set; }

        public int LineNumber { get; set; }

        public string Text { get; set; }
    }
}