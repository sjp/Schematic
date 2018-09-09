using System;
using System.Collections.Generic;
using System.Linq;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Utilities;

namespace SJP.Schematic.Oracle
{
    public class DefaultOracleIdentifierResolutionStrategy : IIdentifierResolutionStrategy
    {
        public IEnumerable<Identifier> GetResolutionOrder(Identifier identifier)
        {
            if (identifier == null)
                throw new ArgumentNullException(nameof(identifier));

            var localNames = GetResolutionOrder(identifier.LocalName);

            // fast path for basic table lookup
            if (identifier.Schema == null)
                return localNames.Select(Identifier.CreateQualifiedIdentifier);

            var schemaNames = GetResolutionOrder(identifier.Schema);

            var hasLowerDatabase = identifier.Database != null && identifier.Database.Any(char.IsLower);
            var database = hasLowerDatabase ? identifier.Database.ToUpperInvariant() : identifier.Database;

            var server = identifier.Server;

            return schemaNames
                .SelectMany(schema =>
                    localNames.Select(localName =>
                        Identifier.CreateQualifiedIdentifier(server, database, schema, localName)));
        }

        private static IEnumerable<string> GetResolutionOrder(string identifierComponent)
        {
            var isUpperCase = identifierComponent.All(char.IsUpper);
            if (!isUpperCase)
                yield return identifierComponent.ToUpperInvariant();

            yield return identifierComponent;
        }
    }
}
