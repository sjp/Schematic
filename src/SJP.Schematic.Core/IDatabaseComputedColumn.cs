using LanguageExt;

namespace SJP.Schematic.Core
{
    public interface IDatabaseComputedColumn : IDatabaseColumn
    {
        /// <summary>
        /// The definition of the computed column. Optional as some providers (e.g. Oracle) allow the definition to be missing.
        /// </summary>
        Option<string> Definition { get; }
    }
}
