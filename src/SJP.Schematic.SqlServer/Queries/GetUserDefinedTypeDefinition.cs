using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.SqlServer.Queries;

internal static class GetUserDefinedTypeDefinition
{
    internal sealed record Query : ISqlQuery<Result>
    {
        public required string SchemaName { get; init; }

        public required string TypeName { get; init; }
    }

    internal sealed record Result : IUserDefinedTypeDefinitionRow
    {
        public required bool IsTableType { get; init; }

        public required bool IsAssemblyType { get; init; }

        public required bool IsNullable { get; init; }

        public required int MaxLength { get; init; }

        public required int Precision { get; init; }

        public required int Scale { get; init; }

        public required string? Collation { get; init; }

        /// <summary>
        /// The built-in type an alias type is defined over, <see langword="null" /> for a table or
        /// assembly type, which are not defined over a system type.
        /// </summary>
        public required string? BaseTypeName { get; init; }

        public required string? AssemblyName { get; init; }

        public required string? AssemblyClass { get; init; }

        /// <summary>
        /// The definition of a default bound to the type with <c>sp_bindefault</c>, if one is bound.
        /// </summary>
        public required string? DefaultValue { get; init; }
    }

    internal const string Sql = @$"
select
    t.is_table_type as [{nameof(Result.IsTableType)}],
    t.is_assembly_type as [{nameof(Result.IsAssemblyType)}],
    t.is_nullable as [{nameof(Result.IsNullable)}],
    t.max_length as [{nameof(Result.MaxLength)}],
    t.precision as [{nameof(Result.Precision)}],
    t.scale as [{nameof(Result.Scale)}],
    t.collation_name as [{nameof(Result.Collation)}],
    bt.name as [{nameof(Result.BaseTypeName)}],
    a.name as [{nameof(Result.AssemblyName)}],
    ast.assembly_class as [{nameof(Result.AssemblyClass)}],
    dm.definition as [{nameof(Result.DefaultValue)}]
from sys.types t
left join sys.types bt on t.system_type_id = bt.user_type_id and bt.is_user_defined = 0
left join sys.assembly_types ast on t.user_type_id = at.user_type_id
left join sys.assemblies a on ast.assembly_id = a.assembly_id
left join sys.sql_modules dm on t.default_object_id = dm.object_id
where t.schema_id = schema_id(@{nameof(Query.SchemaName)}) and t.name = @{nameof(Query.TypeName)} and t.is_user_defined = 1";
}
