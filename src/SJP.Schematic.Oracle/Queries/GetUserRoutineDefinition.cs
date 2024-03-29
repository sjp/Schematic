﻿using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Oracle.Queries;

internal static class GetUserRoutineDefinition
{
    internal sealed record Query : ISqlQuery<string>
    {
        public required string RoutineName { get; init; }
    }

    internal const string Sql = @$"
select TEXT
from SYS.USER_SOURCE
where NAME = :{nameof(Query.RoutineName)} AND TYPE IN ('FUNCTION', 'PROCEDURE')
order by LINE";
}