using LanguageExt;

namespace SJP.Schematic.Core
{
    public interface IDatabaseCheckConstraint : IDatabaseOptional
    {
        Option<Identifier> Name { get; }

        string Definition { get; }
    }
}
