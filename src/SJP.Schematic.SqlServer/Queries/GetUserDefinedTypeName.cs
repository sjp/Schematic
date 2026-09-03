using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.SqlServer.Queries;

internal static class GetUserDefinedTypeName
{
    internal sealed record Query : ISqlQuery<Result>
    {
        public required string SchemaName { get; init; }

        public required string TypeName { get; init; }
    }

    internal sealed record Result
    {
        public required string SchemaName { get; init; }

        public required string TypeName { get; init; }
    }

    internal const string Sql = @$"
select top 1 schema_name(schema_id) as [{nameof(Result.SchemaName)}], name as [{nameof(Result.TypeName)}]
from sys.types
where schema_id = schema_id(@{nameof(Query.SchemaName)}) and name = @{nameof(Query.TypeName)} and is_user_defined = 1";
}
