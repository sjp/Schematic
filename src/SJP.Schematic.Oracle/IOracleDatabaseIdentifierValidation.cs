using SJP.Schematic.Core;

namespace SJP.Schematic.Oracle
{
    public interface IOracleDatabaseIdentifierValidation
    {
        bool IsValidIdentifier(Identifier identifier);
    }
}