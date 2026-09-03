using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.SqlServer.Queries;

internal static class GetUserDefinedTypeComments
{
    internal sealed record Query : ISqlQuery<Result>
    {
        public required string SchemaName { get; init; }

        public required string TypeName { get; init; }

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
    'TYPE' as [{nameof(Result.ObjectType)}],
    t.name as [{nameof(Result.ObjectName)}],
    ep.value as [{nameof(Result.Comment)}]
from sys.types t
left join sys.extended_properties ep on t.user_type_id = ep.major_id and ep.name = @{nameof(Query.CommentProperty)} and ep.minor_id = 0 and ep.class = 6
where t.schema_id = SCHEMA_ID(@{nameof(Query.SchemaName)}) and t.name = @{nameof(Query.TypeName)} and t.is_user_defined = 1
";
}
