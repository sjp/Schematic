namespace SJP.Schematic.Core
{
    public sealed class IdentifierDefaultsBuilder
    {
        public IdentifierDefaultsBuilder WithServer(string server)
        {
            _defaults.Server = server;
            return this;
        }

        public IdentifierDefaultsBuilder WithDatabase(string database)
        {
            _defaults.Database = database;
            return this;
        }

        public IdentifierDefaultsBuilder WithSchema(string schema)
        {
            _defaults.Schema = schema;
            return this;
        }

        public IIdentifierDefaults Build() => _defaults;

        private readonly IdentifierDefaults _defaults = new IdentifierDefaults();

        private class IdentifierDefaults : IIdentifierDefaults
        {
            public string Server { get; set; }

            public string Database { get; set; }

            public string Schema { get; set; }
        }
    }
}
