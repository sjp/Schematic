using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.MySql.Queries;

internal static class GetTableDetails
{
    internal sealed record Query : ISqlQuery<Result>
    {
        public required string SchemaName { get; init; }

        public required string TableName { get; init; }
    }

    /// <summary>
    /// One of a table's options, check constraints or triggers, as told apart by <see cref="Kind"/>.
    /// </summary>
    internal sealed record Result
    {
        /// <summary><c>O</c> for the table's options, <c>C</c> for a check constraint or <c>T</c> for a trigger.</summary>
        public required string Kind { get; init; }

        /// <summary>The table collation for the options row, otherwise the check constraint or trigger name.</summary>
        public required string? Name { get; init; }

        /// <summary>The table's <c>create_options</c> for the options row, otherwise the check clause or trigger body.</summary>
        public required string? Definition { get; init; }

        /// <summary>Whether a check constraint is enforced, i.e. <c>YES</c> or <c>NO</c>.</summary>
        public required string? Enforced { get; init; }

        /// <summary>When a trigger fires, e.g. <c>BEFORE</c>.</summary>
        public required string? Timing { get; init; }

        /// <summary>The event a trigger fires on, e.g. <c>INSERT</c>.</summary>
        public required string? TriggerEvent { get; init; }
    }

    internal const string OptionsKind = "O";

    internal const string CheckKind = "C";

    internal const string TriggerKind = "T";

    // A table's options, check constraints and triggers are each read from their own catalog table, so they
    // are combined with union all rather than joined. Each branch filters on a constant schema and table name,
    // which is what lets MariaDB look its catalog table up by key instead of opening every table on the server.
    // The text columns are cast so that every branch has the same collation: MySQL reports its catalog columns
    // with differing collations, which a union cannot combine.
    //
    // create_options lists 'partitioned' for a partitioned table, which is how the partitions are known to be
    // worth reading. information_schema.partitions opens the whole table even when it is not partitioned.
    private const string OptionsBranch = $"""

select
    '{OptionsKind}' as `{nameof(Result.Kind)}`,
    cast(t.table_collation as char) as `{nameof(Result.Name)}`,
    cast(t.create_options as char) as `{nameof(Result.Definition)}`,
    cast(null as char) as `{nameof(Result.Enforced)}`,
    cast(null as char) as `{nameof(Result.Timing)}`,
    cast(null as char) as `{nameof(Result.TriggerEvent)}`
from information_schema.tables t
where t.table_schema = @{nameof(Query.SchemaName)} and t.table_name = @{nameof(Query.TableName)} and t.table_type = 'BASE TABLE'
""";

    private const string TriggersBranch = $"""

union all
select
    '{TriggerKind}',
    cast(tr.trigger_name as char),
    cast(tr.action_statement as char),
    null,
    cast(tr.action_timing as char),
    cast(tr.event_manipulation as char)
from information_schema.triggers tr
where tr.event_object_schema = @{nameof(Query.SchemaName)} and tr.event_object_table = @{nameof(Query.TableName)}
""";

    private const string ChecksBranch = $"""

union all
select
    '{CheckKind}',
    cast(cc.constraint_name as char),
    cast(cc.check_clause as char),
    cast(tc.enforced as char),
    null,
    null
from information_schema.table_constraints tc
inner join information_schema.check_constraints cc on tc.table_schema = cc.constraint_schema and tc.constraint_name = cc.constraint_name
where tc.table_schema = @{nameof(Query.SchemaName)} and tc.table_name = @{nameof(Query.TableName)} and tc.constraint_type = 'CHECK'
""";

    // For a server whose table_constraints has no enforced column, and so no NOT ENFORCED syntax, meaning
    // a check constraint is always enforced.
    private const string ChecksWithoutEnforcedBranch = $"""

union all
select
    '{CheckKind}',
    cast(cc.constraint_name as char),
    cast(cc.check_clause as char),
    'YES',
    null,
    null
from information_schema.table_constraints tc
inner join information_schema.check_constraints cc on tc.table_schema = cc.constraint_schema and tc.constraint_name = cc.constraint_name
where tc.table_schema = @{nameof(Query.SchemaName)} and tc.table_name = @{nameof(Query.TableName)} and tc.constraint_type = 'CHECK'
""";

    // For a server whose check_constraints has a table_name column (MariaDB). MariaDB names check constraints
    // per table, so two tables in one schema can both have one called CONSTRAINT_1, and matching on the
    // constraint name alone would return the other table's check too. MariaDB has no NOT ENFORCED syntax,
    // so a check constraint is always enforced.
    private const string ChecksByTableNameBranch = $"""

union all
select
    '{CheckKind}',
    cast(cc.constraint_name as char),
    cast(cc.check_clause as char),
    'YES',
    null,
    null
from information_schema.check_constraints cc
where cc.constraint_schema = @{nameof(Query.SchemaName)} and cc.table_name = @{nameof(Query.TableName)}
""";

    internal const string Sql = OptionsBranch + ChecksBranch + TriggersBranch;

    internal const string SqlWithoutEnforced = OptionsBranch + ChecksWithoutEnforcedBranch + TriggersBranch;

    internal const string SqlByTableName = OptionsBranch + ChecksByTableNameBranch + TriggersBranch;

    // For a server with no information_schema.check_constraints, which so has no check constraints to read.
    internal const string SqlWithoutChecks = OptionsBranch + TriggersBranch;
}
