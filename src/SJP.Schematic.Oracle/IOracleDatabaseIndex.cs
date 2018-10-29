using SJP.Schematic.Core;

namespace SJP.Schematic.Oracle
{
    public interface IOracleDatabaseIndex : IDatabaseIndex
    {
        bool GeneratedByConstraint { get; }
    }
}