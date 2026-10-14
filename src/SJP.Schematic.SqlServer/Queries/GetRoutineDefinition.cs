using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.SqlServer.Queries;

internal static class GetRoutineDefinition
{
    internal sealed record Query : ISqlQuery<Result>
    {
        public required string SchemaName { get; init; }

        public required string RoutineName { get; init; }
    }

    /// <summary>
    /// One of a routine's parameters, alongside details of the routine itself. A routine without parameters
    /// returns a single row whose parameter columns are all <see langword="null" />.
    /// </summary>
    internal sealed record Result
    {
        /// <summary>
        /// The routine body, only present on the first row so that it is not repeated for every parameter.
        /// Also <see langword="null" /> when the routine was created <c>WITH ENCRYPTION</c> or the caller
        /// lacks <c>VIEW DEFINITION</c> on it.
        /// </summary>
        public required string? Definition { get; init; }

        /// <summary>
        /// The <c>sys.objects.type</c> code, i.e. one of the four values the query filters on.
        /// It is <c>char(2)</c> in the catalog, so the query trims it.
        /// </summary>
        public required string RoutineTypeCode { get; init; }

        /// <summary>
        /// The parameter's position in the signature, or <see langword="null" /> for a routine without parameters.
        /// Zero is not a parameter but the return value of a scalar function, which the catalog stores alongside
        /// the parameters.
        /// </summary>
        public required int? Ordinal { get; init; }

        /// <summary>
        /// The parameter name, including its leading <c>@</c>. Empty for a function's return value.
        /// </summary>
        public required string? ParameterName { get; init; }

        public required string? ColumnTypeSchema { get; init; }

        public required string? ColumnTypeName { get; init; }

        public required int? MaxLength { get; init; }

        public required int? Precision { get; init; }

        public required int? Scale { get; init; }

        /// <summary>
        /// Whether the parameter is declared <c>OUTPUT</c>. Such a parameter still accepts a value
        /// on the way in, so it describes an in/out parameter rather than an output-only one.
        /// </summary>
        public required bool? IsOutput { get; init; }

        /// <summary>
        /// The default the parameter takes when no argument is supplied. SQL Server only records
        /// this for CLR routines, so it is always <see langword="null" /> for the T-SQL modules
        /// this provider exposes.
        /// </summary>
        public required string? DefaultValue { get; init; }
    }

    // sys.parameters holds a scalar function's return value as parameter_id 0, so the caller
    // separates that row out rather than reporting it as a parameter.
    // default_value is sql_variant, which Dapper does not convert, so it is converted here.
    internal const string Sql = @$"
select
    case when row_number() over (order by p.parameter_id) = 1 then m.definition end as [{nameof(Result.Definition)}],
    rtrim(o.type) as [{nameof(Result.RoutineTypeCode)}],
    p.parameter_id as [{nameof(Result.Ordinal)}],
    p.name as [{nameof(Result.ParameterName)}],
    schema_name(st.schema_id) as [{nameof(Result.ColumnTypeSchema)}],
    st.name as [{nameof(Result.ColumnTypeName)}],
    p.max_length as [{nameof(Result.MaxLength)}],
    p.precision as [{nameof(Result.Precision)}],
    p.scale as [{nameof(Result.Scale)}],
    p.is_output as [{nameof(Result.IsOutput)}],
    case when p.has_default_value = 1 then convert(nvarchar(max), p.default_value) end as [{nameof(Result.DefaultValue)}]
from sys.objects o
inner join sys.sql_modules m on m.object_id = o.object_id
left join sys.parameters p on p.object_id = o.object_id
left join sys.types st on p.user_type_id = st.user_type_id
where o.schema_id = schema_id(@{nameof(Query.SchemaName)}) and o.name = @{nameof(Query.RoutineName)} and o.is_ms_shipped = 0
    and o.type in ('P', 'FN', 'IF', 'TF')
order by p.parameter_id";
}
