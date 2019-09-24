using System;

namespace SJP.Schematic.Core
{
    public sealed class IdentifierDefaultsBuilder
    {
        public IdentifierDefaultsBuilder()
        {
            _defaults = new IdentifierDefaults(null, null, null);
        }

        private IdentifierDefaultsBuilder(IdentifierDefaults identifierDefaults)
        {
            _defaults = identifierDefaults ?? throw new ArgumentNullException(nameof(identifierDefaults));
        }

        public IdentifierDefaultsBuilder WithServer(string server)
        {
            return new IdentifierDefaultsBuilder(
                new IdentifierDefaults(server, _defaults.Database, _defaults.Schema)
            );
        }

        public IdentifierDefaultsBuilder WithDatabase(string database)
        {
            return new IdentifierDefaultsBuilder(
                new IdentifierDefaults(_defaults.Server, database, _defaults.Schema)
            );
        }

        public IdentifierDefaultsBuilder WithSchema(string schema)
        {
            return new IdentifierDefaultsBuilder(
                new IdentifierDefaults(_defaults.Server, _defaults.Database, schema)
            );
        }

        public IIdentifierDefaults Build() => _defaults;

        private readonly IdentifierDefaults _defaults;
    }
}
