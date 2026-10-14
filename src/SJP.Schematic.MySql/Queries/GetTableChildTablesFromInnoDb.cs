using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.MySql.Queries;

internal static class GetTableChildTablesFromInnoDb
{
    internal sealed record Query : ISqlQuery<Result>
    {
        /// <summary>
        /// The referenced table, as InnoDB names it: <c>schema/table</c>.
        /// </summary>
        public required string ReferencedTableName { get; init; }
    }

    internal sealed record Result
    {
        /// <summary>
        /// The table holding the foreign key, as InnoDB names it: <c>schema/table</c>.
        /// </summary>
        public required string ChildTableName { get; init; }
    }

    // MariaDB only. InnoDB's foreign key dictionary is read from memory without opening any tables, unlike
    // referential_constraints, but reading it requires the PROCESS privilege.
    internal const string Sql = $"""

select distinct
    f.for_name as `{nameof(Result.ChildTableName)}`
from information_schema.innodb_sys_foreign f
where f.ref_name = @{nameof(Query.ReferencedTableName)}
order by f.for_name
""";
}
