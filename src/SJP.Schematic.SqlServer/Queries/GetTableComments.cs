using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.SqlServer.Queries;

internal static class GetTableComments
{
    internal sealed record Query : ISqlQuery<Result>
    {
        public required string SchemaName { get; init; }

        public required string TableName { get; init; }

        public required string CommentProperty { get; init; }
    }

    internal sealed record Result
    {
        public required string ObjectType { get; init; }

        public required string ObjectName { get; init; }

        public required string? Comment { get; init; }
    }

    internal const string Sql = @$"
-- table
select
    'TABLE' as [{nameof(Result.ObjectType)}],
    t.name as [{nameof(Result.ObjectName)}],
    ep.value as [{nameof(Result.Comment)}]
from sys.tables t
left join sys.extended_properties ep on t.object_id = ep.major_id and ep.name = @{nameof(Query.CommentProperty)} and ep.minor_id = 0
where t.schema_id = SCHEMA_ID(@{nameof(Query.SchemaName)}) and t.name = @{nameof(Query.TableName)} and t.is_ms_shipped = 0

union

-- columns
select
    'COLUMN' as [{nameof(Result.ObjectType)}],
    c.name as [{nameof(Result.ObjectName)}],
    ep.value as [{nameof(Result.Comment)}]
from sys.tables t
inner join sys.columns c on t.object_id = c.object_id
left join sys.extended_properties ep on t.object_id = ep.major_id and c.column_id = ep.minor_id and ep.name = @{nameof(Query.CommentProperty)}
where t.schema_id = SCHEMA_ID(@{nameof(Query.SchemaName)}) and t.name = @{nameof(Query.TableName)} and t.is_ms_shipped = 0

union

-- checks
select
    'CHECK' as [{nameof(Result.ObjectType)}],
    cc.name as [{nameof(Result.ObjectName)}],
    ep.value as [{nameof(Result.Comment)}]
from sys.tables t
inner join sys.check_constraints cc on t.object_id = cc.parent_object_id
left join sys.extended_properties ep on cc.object_id = ep.major_id and ep.name = @{nameof(Query.CommentProperty)}
where t.schema_id = SCHEMA_ID(@{nameof(Query.SchemaName)}) and t.name = @{nameof(Query.TableName)} and t.is_ms_shipped = 0

union

-- foreign keys
select
    'FOREIGN KEY' as [{nameof(Result.ObjectType)}],
    fk.name as [{nameof(Result.ObjectName)}],
    ep.value as [{nameof(Result.Comment)}]
from sys.tables t
inner join sys.foreign_keys fk on t.object_id = fk.parent_object_id
left join sys.extended_properties ep on fk.object_id = ep.major_id and ep.name = @{nameof(Query.CommentProperty)}
where t.schema_id = SCHEMA_ID(@{nameof(Query.SchemaName)}) and t.name = @{nameof(Query.TableName)} and t.is_ms_shipped = 0

union

-- unique keys
select
    'UNIQUE' as [{nameof(Result.ObjectType)}],
    kc.name as [{nameof(Result.ObjectName)}],
    ep.value as [{nameof(Result.Comment)}]
from sys.tables t
inner join sys.key_constraints kc on t.object_id = kc.parent_object_id
left join sys.extended_properties ep on kc.object_id = ep.major_id and ep.name = @{nameof(Query.CommentProperty)}
where t.schema_id = SCHEMA_ID(@{nameof(Query.SchemaName)}) and t.name = @{nameof(Query.TableName)}
    and t.is_ms_shipped = 0 and kc.type = 'UQ'

union

-- primary key
select
    'PRIMARY' as [{nameof(Result.ObjectType)}],
    kc.name as [{nameof(Result.ObjectName)}],
    ep.value as [{nameof(Result.Comment)}]
from sys.tables t
inner join sys.key_constraints kc on t.object_id = kc.parent_object_id
left join sys.extended_properties ep on kc.object_id = ep.major_id and ep.name = @{nameof(Query.CommentProperty)}
where t.schema_id = SCHEMA_ID(@{nameof(Query.SchemaName)}) and t.name = @{nameof(Query.TableName)}
    and t.is_ms_shipped = 0and kc.type = 'PK'

union

-- indexes
select
    'INDEX' as [{nameof(Result.ObjectType)}],
    i.name as [{nameof(Result.ObjectName)}],
    ep.value as [{nameof(Result.Comment)}]
from sys.tables t
inner join sys.indexes i on t.object_id = i.object_id
left join sys.extended_properties ep on t.object_id = ep.major_id and i.index_id = ep.minor_id and ep.name = @{nameof(Query.CommentProperty)}
where t.schema_id = SCHEMA_ID(@{nameof(Query.SchemaName)}) and t.name = @{nameof(Query.TableName)} and t.is_ms_shipped = 0
    and i.is_primary_key = 0 and i.is_unique_constraint = 0
    and i.is_hypothetical = 0 and i.type <> 0 -- type = 0 is a heap, ignore

union

-- triggers
select
    'TRIGGER' as [{nameof(Result.ObjectType)}],
    tr.name as [{nameof(Result.ObjectName)}],
    ep.value as [{nameof(Result.Comment)}]
from sys.tables t
inner join sys.triggers tr on t.object_id = tr.parent_id
left join sys.extended_properties ep on tr.object_id = ep.major_id and ep.name = @{nameof(Query.CommentProperty)}
where t.schema_id = SCHEMA_ID(@{nameof(Query.SchemaName)}) and t.name = @{nameof(Query.TableName)} and t.is_ms_shipped = 0
";
}