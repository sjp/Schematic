using System;
using SJP.Schematic.Core;

namespace SJP.Schematic.Modelled.Reflection.Model
{
    [AttributeUsage(AttributeTargets.Struct | AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public sealed class SchemaAttribute : ModelledSchemaAttribute
    {
        public SchemaAttribute(string schema)
            : base(new[] { Dialect.All })
        {
            if (schema.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(schema));

            Schema = schema;
        }

        public SchemaAttribute(string schema, params Type[] dialects)
            : base(dialects)
        {
            if (schema.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(schema));

            Schema = schema;
        }

        public string Schema { get; }
    }
}
