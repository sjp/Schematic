﻿namespace SJP.Schematic.SqlServer.Queries;

internal static class GetSequenceComments
{
    internal sealed record Query
    {
        public required string SchemaName { get; init; }

        public required string SequenceName { get; init; }

        public required string CommentProperty { get; init; }
    }

    internal sealed record Result
    {
        public required string ObjectType { get; init; }

        public required string ObjectName { get; init; }

        public required string? Comment { get; init; }
    }

    internal const string Sql = @$"
select
    'SEQUENCE' as [{nameof(Result.ObjectType)}],
    s.name as [{nameof(Result.ObjectName)}],
    ep.value as [{nameof(Result.Comment)}]
from sys.sequences s
left join sys.extended_properties ep on s.object_id = ep.major_id and ep.name = @{nameof(Query.CommentProperty)} and ep.minor_id = 0
where s.schema_id = SCHEMA_ID(@{nameof(Query.SchemaName)}) and s.name = @{nameof(Query.SequenceName)} and s.is_ms_shipped = 0
";
}