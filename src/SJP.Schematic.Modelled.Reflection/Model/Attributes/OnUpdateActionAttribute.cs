using System;
using EnumsNET;
using SJP.Schematic.Core;

namespace SJP.Schematic.Modelled.Reflection.Model
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true, Inherited = true)]
    public sealed class OnUpdateActionAttribute : ModelledSchemaAttribute
    {
        public OnUpdateActionAttribute(ReferentialAction action)
            : base(new[] { Dialect.All })
        {
            if (!action.IsValid())
                throw new ArgumentException($"The { nameof(ReferentialAction) } provided must be a valid enum.", nameof(action));

            Action = action;
        }

        public OnUpdateActionAttribute(ReferentialAction action, params Type[] dialects)
            : base(dialects)
        {
            if (!action.IsValid())
                throw new ArgumentException($"The { nameof(ReferentialAction) } provided must be a valid enum.", nameof(action));

            Action = action;
        }

        public ReferentialAction Action { get; }
    }
}
