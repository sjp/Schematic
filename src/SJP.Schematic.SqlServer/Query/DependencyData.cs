namespace SJP.Schematic.SqlServer.Query
{
    public class DependencyData
    {
        public string ReferencingSchemaName { get; set; }

        public string ReferencingObjectName { get; set; }

        public string ReferencingObjectType { get; set; }

        public string ReferencedSchemaName { get; set; }

        public string ReferencedObjectName { get; set; }

        public string ReferencedObjectType { get; set; }
    }
}
