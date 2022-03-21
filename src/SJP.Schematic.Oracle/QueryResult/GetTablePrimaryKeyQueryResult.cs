﻿namespace SJP.Schematic.Oracle.QueryResult;

internal sealed record GetTablePrimaryKeyQueryResult
{
    public string? ConstraintName { get; init; }

    public string? EnabledStatus { get; init; }

    public string? ColumnName { get; init; }

    public int ColumnPosition { get; init; }
}