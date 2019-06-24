using System.Collections.Generic;

namespace SJP.Schematic.Migrations
{
    public interface IMigrationsSqlGenerator
    {
        IReadOnlyList<ISqlCommand> GenerateSql(IEnumerable<IMigrationOperation> operations);
    }
}
