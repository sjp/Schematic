﻿namespace SJP.Schematic.Serialization.Dto;

public class DatabaseColumn
{
    public Identifier? ColumnName { get; set; }

    public bool IsNullable { get; set; }

    public bool IsComputed { get; set; }

    public string? DefaultValue { get; set; }

    public DbType? Type { get; set; }

    public AutoIncrement? AutoIncrement { get; set; }

    public string? Definition { get; set; }
}