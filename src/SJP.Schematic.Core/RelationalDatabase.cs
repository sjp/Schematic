using System;

namespace SJP.Schematic.Core
{
    public abstract class RelationalDatabase
    {
        protected RelationalDatabase(IIdentifierDefaults identifierDefaults)
        {
            IdentifierDefaults = identifierDefaults ?? throw new ArgumentNullException(nameof(identifierDefaults));
        }

        public IIdentifierDefaults IdentifierDefaults { get; }
    }
}
