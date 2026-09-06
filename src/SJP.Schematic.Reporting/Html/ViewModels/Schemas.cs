using System;
using System.Collections.Generic;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Reporting.Html.ViewModels;

/// <summary>
/// The schemas summary payload (<c>data/schemas.json</c>): the namespaces the report's objects
/// are declared in, each with a breakdown of what it holds.
/// </summary>
public sealed class Schemas
{
    public Schemas(IEnumerable<Main.Schema> schemas)
    {
        if (schemas.NullOrAnyNull())
            throw new ArgumentNullException(nameof(schemas));

        SchemasCount = schemas.UCount();
        AllSchemas = schemas;
    }

    public uint SchemasCount { get; }

    public IEnumerable<Main.Schema> AllSchemas { get; }
}
