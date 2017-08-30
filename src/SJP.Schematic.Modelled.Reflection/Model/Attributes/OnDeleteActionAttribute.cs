using System;
using EnumsNET;

namespace SJP.Schematic.Modelled.Reflection.Model
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true, Inherited = true)]
    public sealed class OnDeleteActionAttribute : ModelledSchemaAttribute
    {
        public OnDeleteActionAttribute(ForeignKeyAction action)
            : base(new[] { Dialect.All })
        {
            if (!action.IsValid())
                throw new ArgumentException($"The { nameof(ForeignKeyAction) } provided must be a valid enum.", nameof(action));

            Action = action;
        }

        public OnDeleteActionAttribute(ForeignKeyAction action, params Type[] dialects)
            : base(dialects)
        {
            if (!action.IsValid())
                throw new ArgumentException($"The { nameof(ForeignKeyAction) } provided must be a valid enum.", nameof(action));

            Action = action;
        }

        public ForeignKeyAction Action { get; }
    }
}
