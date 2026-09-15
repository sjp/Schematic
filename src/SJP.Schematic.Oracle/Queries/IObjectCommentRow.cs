namespace SJP.Schematic.Oracle.Queries;

/// <summary>
/// The comment columns that are shared by the any-schema and current-user comment queries for
/// tables, views and materialized views, so that every result is mapped by the same code.
/// </summary>
internal interface IObjectCommentRow
{
    string ColumnName { get; }

    string ObjectType { get; }

    string? Comment { get; }
}
