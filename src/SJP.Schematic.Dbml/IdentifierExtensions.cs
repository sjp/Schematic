using System;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Utilities;

namespace SJP.Schematic.Dbml
{
    internal static class IdentifierExtensions
    {
        public static string ToVisibleName(this Identifier identifier)
        {
            if (identifier == null)
                throw new ArgumentNullException(nameof(identifier));

            var builder = StringBuilderCache.Acquire();

            if (identifier.Schema != null)
            {
                builder.Append(identifier.Schema);
                builder.Append("_");
            }

            builder.Append(identifier.LocalName);

            return builder.GetStringAndRelease().RemoveQuotingCharacters();
        }
    }
}
