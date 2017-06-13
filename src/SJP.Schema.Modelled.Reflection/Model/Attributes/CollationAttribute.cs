using System;
using SJP.Schema.Core;

namespace SJP.Schema.Modelled.Reflection.Model
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public sealed class CollationAttribute : ModelledSchemaAttribute
    {
        // should only be applied to a specific vendor (or set of vendors)
        public CollationAttribute(string collationName, params Type[] dialects)
            : base(dialects)
        {
            if (collationName.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(collationName));

            CollationName = collationName;
        }

        public string CollationName { get; }
    }
}
