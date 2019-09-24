namespace SJP.Schematic.Core
{
    public class IdentifierDefaults : IIdentifierDefaults
    {
        public IdentifierDefaults(string server, string database, string schema)
        {
            Server = server;
            Database = database;
            Schema = schema;
        }

        public string Server { get; }

        public string Database { get; }

        public string Schema { get; }
    }
}
