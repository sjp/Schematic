namespace SJP.Schematic.Core
{
    public sealed class DatabaseIdentifierDefaultsBuilder
    {
        public DatabaseIdentifierDefaultsBuilder WithServer(string server)
        {
            _defaults.Server = server;
            return this;
        }

        public DatabaseIdentifierDefaultsBuilder WithDatabase(string database)
        {
            _defaults.Database = database;
            return this;
        }

        public DatabaseIdentifierDefaultsBuilder WithSchema(string schema)
        {
            _defaults.Schema = schema;
            return this;
        }

        public IDatabaseIdentifierDefaults Build() => _defaults;

        private readonly IdentifierDefaults _defaults = new IdentifierDefaults();

        private class IdentifierDefaults : IDatabaseIdentifierDefaults
        {
            public string Server { get; set; }

            public string Database { get; set; }

            public string Schema { get; set; }
        }
    }
}
