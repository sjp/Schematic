using SJP.Schematic.Core;

namespace SJP.Schematic.Modelled.Reflection
{
    public class ReflectionCheckConstraint : DatabaseCheckConstraint
    {
        public ReflectionCheckConstraint(Identifier name, string definition)
            : base(name, definition, true) // TODO: do we want isEnabled to always be true?
        {
        }
    }
}
