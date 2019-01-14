using LanguageExt;
using SJP.Schematic.Core;

namespace SJP.Schematic.Oracle
{
    public interface IOracleDatabasePackage : IDatabaseRoutine
    {
        string Specification { get; }

        Option<string> Body { get; }
    }
}