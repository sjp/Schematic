﻿namespace SJP.Schematic.SqlServer.Query;

internal sealed record GetSequenceDefinitionQuery
{
    public string SchemaName { get; init; } = default!;

    public string SequenceName { get; init; } = default!;
}