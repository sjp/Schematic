using System;
using System.Collections.Generic;
using System.Linq;
using LanguageExt;
using SJP.Schematic.Core;
using SJP.Schematic.Reporting.Html.Renderers;

namespace SJP.Schematic.Reporting.Html.ViewModels.Mappers;

/// <summary>
/// Resolves the schemas a report covers, and maps each to its summary row and detail payload.
/// The resolution is shared so that the dashboard, the schemas list and the per-schema pages
/// cannot disagree about which schemas exist or what they hold.
/// </summary>
internal sealed class SchemaModelMapper
{
    /// <summary>
    /// Combines the schemas the database declares with the schemas that the report's objects are
    /// named in, ordered by name. A dialect that reports no schemas still gets a list, and a schema
    /// holding no objects is still listed as long as a user declared it. System schemas are only
    /// listed when they hold something the report covers, so that e.g. SQL Server's fixed-role
    /// schemas do not crowd out the ones a reader cares about.
    /// </summary>
    /// <exception cref="ArgumentNullException"><paramref name="data"/> is <see langword="null" />.</exception>
    public IReadOnlyList<SchemaObjects> GetSchemas(ReportData data)
    {
        ArgumentNullException.ThrowIfNull(data);

        var schemas = new Dictionary<string, SchemaObjects>(StringComparer.Ordinal);

        SchemaObjects ForSchema(string name)
        {
            if (!schemas.TryGetValue(name, out var existing))
            {
                var isDefault = string.Equals(name, data.Database.IdentifierDefaults.Schema, StringComparison.Ordinal);
                existing = new SchemaObjects(new Identifier(name)) { IsDefault = isDefault };
                schemas[name] = existing;
            }

            return existing;
        }

        // The objects come first so that a schema the database does not declare — or declines to
        // declare, as a dialect with no schema provider does — is still discovered from the names
        // of the objects the report holds.
        AddObjects(data.Tables.Select(static t => t.Name), ForSchema, static s => s.Tables);
        AddObjects(data.Views.Select(static v => v.Name), ForSchema, static s => s.Views);
        AddObjects(data.Sequences.Select(static s => s.Name), ForSchema, static s => s.Sequences);
        AddObjects(data.Synonyms.Select(static s => s.Name), ForSchema, static s => s.Synonyms);
        AddObjects(data.Routines.Select(static r => r.Name), ForSchema, static s => s.Routines);
        AddObjects(data.UserDefinedTypes.Select(static t => t.Name), ForSchema, static s => s.UserDefinedTypes);

        foreach (var schema in data.Schemas)
        {
            var name = schema.Name.LocalName;
            if (schema.IsSystem && !schemas.ContainsKey(name))
                continue;

            var resolved = ForSchema(name);
            resolved.Owner = schema.Owner;
            resolved.IsDefault = schema.IsDefault;
            resolved.IsSystem = schema.IsSystem;
        }

        return schemas.Values
            .OrderBy(static s => s.Name.LocalName, StringComparer.Ordinal)
            .ToList();
    }

    /// <exception cref="ArgumentNullException"><paramref name="schema"/> is <see langword="null" />.</exception>
    public Main.Schema MapSummary(SchemaObjects schema)
    {
        ArgumentNullException.ThrowIfNull(schema);

        return new Main.Schema(
            schema.Name,
            schema.Owner,
            schema.IsDefault,
            schema.IsSystem,
            (uint)schema.Tables.Count,
            (uint)schema.Views.Count,
            (uint)schema.Sequences.Count,
            (uint)schema.Synonyms.Count,
            (uint)schema.Routines.Count,
            (uint)schema.UserDefinedTypes.Count
        );
    }

    /// <exception cref="ArgumentNullException"><paramref name="schema"/> is <see langword="null" />.</exception>
    public Schema MapDetail(SchemaObjects schema)
    {
        ArgumentNullException.ThrowIfNull(schema);

        return new Schema(
            schema.Name,
            schema.Owner,
            schema.IsDefault,
            schema.IsSystem,
            MapObjects(schema.Tables, UrlRouter.GetTableUrl),
            MapObjects(schema.Views, UrlRouter.GetViewUrl),
            MapObjects(schema.Sequences, UrlRouter.GetSequenceUrl),
            MapObjects(schema.Synonyms, UrlRouter.GetSynonymUrl),
            MapObjects(schema.Routines, UrlRouter.GetRoutineUrl),
            MapObjects(schema.UserDefinedTypes, UrlRouter.GetUserDefinedTypeUrl)
        );
    }

    private static void AddObjects(
        IEnumerable<Identifier> objectNames,
        Func<string, SchemaObjects> forSchema,
        Func<SchemaObjects, List<Identifier>> selectTarget)
    {
        foreach (var objectName in objectNames)
        {
            // An unqualified name belongs to no schema the report can name, so it is left out
            // rather than being attributed to the default schema it may not actually live in.
            if (objectName.Schema is not string schemaName)
                continue;

            selectTarget(forSchema(schemaName)).Add(objectName);
        }
    }

    // The page is already scoped to the schema, so the objects are listed by their local names —
    // repeating the schema on every row would say nothing.
    private static IReadOnlyList<Schema.SchemaObject> MapObjects(IEnumerable<Identifier> objectNames, Func<Identifier, string> getUrl)
    {
        return objectNames
            .OrderBy(static name => name.LocalName, StringComparer.Ordinal)
            .Select(name => new Schema.SchemaObject(name.LocalName, getUrl(name)))
            .ToList();
    }

    /// <summary>
    /// A schema and the names of the objects the report holds for it. Mutable and internal to the
    /// mapping: <see cref="MapSummary"/> and <see cref="MapDetail"/> turn it into the viewmodels.
    /// </summary>
    internal sealed class SchemaObjects(Identifier name)
    {
        public Identifier Name { get; } = name ?? throw new ArgumentNullException(nameof(name));

        public Option<string> Owner { get; set; }

        public bool IsDefault { get; set; }

        public bool IsSystem { get; set; }

        public List<Identifier> Tables { get; } = [];

        public List<Identifier> Views { get; } = [];

        public List<Identifier> Sequences { get; } = [];

        public List<Identifier> Synonyms { get; } = [];

        public List<Identifier> Routines { get; } = [];

        public List<Identifier> UserDefinedTypes { get; } = [];
    }
}
