﻿using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.MySql.Queries;

internal static class GetAllRoutineNames
{
    internal sealed record Query : ISqlQuery<Result>
    {
        public required string SchemaName { get; init; }
    }

    internal sealed record Result
    {
        public required string SchemaName { get; init; }

        public required string RoutineName { get; init; }
    }

    internal const string Sql = @$"
select
    ROUTINE_SCHEMA as `{nameof(Result.SchemaName)}`,
    ROUTINE_NAME as `{nameof(Result.RoutineName)}`
from information_schema.routines
where ROUTINE_SCHEMA = @{nameof(Query.SchemaName)}
order by ROUTINE_SCHEMA, ROUTINE_NAME";
}