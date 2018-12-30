using LanguageExt;

namespace SJP.Schematic.Core
{
    public interface IDatabaseColumn
    {
        Identifier Name { get; }

        bool IsNullable { get; }

        bool IsComputed { get; }

        Option<string> DefaultValue { get; }

        IDbType Type { get; }

        Option<IAutoIncrement> AutoIncrement { get; }
    }
}
