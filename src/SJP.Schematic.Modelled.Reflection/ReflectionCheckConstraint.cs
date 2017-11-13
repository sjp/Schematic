using SJP.Schematic.Core;

namespace SJP.Schematic.Modelled.Reflection
{
    public class ReflectionCheckConstraint : DatabaseCheckConstraint
    {
        public ReflectionCheckConstraint(IRelationalDatabaseTable table, Identifier name, string definition)
            : base(table, name, definition, true) // TODO: do we want isEnabled to always be true?
        {
        }
    }
}
