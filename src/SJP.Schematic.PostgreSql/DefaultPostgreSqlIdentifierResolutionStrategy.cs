using System;
using System.Collections.Generic;
using System.Linq;
using SJP.Schematic.Core;

namespace SJP.Schematic.PostgreSql
{
    public class DefaultPostgreSqlIdentifierResolutionStrategy : IIdentifierResolutionStrategy
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

            var database = identifier.Database != null && identifier.Database.Any(char.IsUpper)
                ? identifier.Database.ToLowerInvariant()
                : identifier.Database;

            var server = identifier.Server;

            return schemaNames
                .SelectMany(schema =>
                    localNames.Select(localName =>
                        Identifier.CreateQualifiedIdentifier(server, database, schema, localName)));
        }

        private static IEnumerable<string> GetResolutionOrder(string identifierComponent)
        {
            var isLowerCase = identifierComponent.All(char.IsLower);
            if (!isLowerCase)
                yield return identifierComponent.ToLowerInvariant();

            yield return identifierComponent;
        }
    }
}
