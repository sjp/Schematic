﻿using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.SqlServer.Queries;

internal static class GetViewDefinition
{
    internal sealed record Query : ISqlQuery<string>
    {
        public required string SchemaName { get; init; }

        public required string ViewName { get; init; }
    }

    internal const string Sql = @$"
select sm.definition
from sys.sql_modules sm
inner join sys.views v on sm.object_id = v.object_id
where schema_name(v.schema_id) = @{nameof(Query.SchemaName)} and v.name = @{nameof(Query.ViewName)} and v.is_ms_shipped = 0";
}