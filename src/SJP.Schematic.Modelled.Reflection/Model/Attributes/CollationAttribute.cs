using System;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Modelled.Reflection.Model
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public sealed class CollationAttribute : ModelledSchemaAttribute
    {
        // should only be applied to a specific vendor (or set of vendors)
        public CollationAttribute(string schemaName, string collationName, params Type[] dialects)
             : base(dialects)
        {
            if (schemaName.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(schemaName));
            if (collationName.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(collationName));

            CollationName = new Identifier(schemaName, collationName);
        }

        public CollationAttribute(string collationName, params Type[] dialects)
            : base(dialects)
        {
            if (collationName.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(collationName));

            CollationName = collationName;
        }

        public Identifier CollationName { get; }
    }
}
