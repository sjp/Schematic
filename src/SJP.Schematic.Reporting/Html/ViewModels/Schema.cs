using System;
using System.Collections.Generic;
using LanguageExt;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Reporting.Html.ViewModels;

/// <summary>
/// The per-schema detail payload (<c>data/schemas/&lt;safeKey&gt;.json</c>): what the schema
/// declares, as links to each object's own page.
/// </summary>
public sealed class Schema
{
    public Schema(
        Identifier schemaName,
        Option<string> owner,
        bool isDefault,
        bool isSystem,
        IEnumerable<SchemaObject> tables,
        IEnumerable<SchemaObject> views,
        IEnumerable<SchemaObject> sequences,
        IEnumerable<SchemaObject> synonyms,
        IEnumerable<SchemaObject> routines,
        IEnumerable<SchemaObject> userDefinedTypes
    )
    {
        ArgumentNullException.ThrowIfNull(schemaName);

        // A schema is not itself qualified by another schema, so its identifier carries only a
        // local name.
        Name = schemaName.LocalName;
        SchemaUrl = UrlRouter.GetSchemaUrl(schemaName);
        Owner = owner.Match(static o => o ?? string.Empty, static () => string.Empty);
        IsDefault = isDefault;
        IsSystem = isSystem;

        Tables = tables ?? throw new ArgumentNullException(nameof(tables));
        TablesCount = tables.UCount();

        Views = views ?? throw new ArgumentNullException(nameof(views));
        ViewsCount = views.UCount();

        Sequences = sequences ?? throw new ArgumentNullException(nameof(sequences));
        SequencesCount = sequences.UCount();

        Synonyms = synonyms ?? throw new ArgumentNullException(nameof(synonyms));
        SynonymsCount = synonyms.UCount();

        Routines = routines ?? throw new ArgumentNullException(nameof(routines));
        RoutinesCount = routines.UCount();

        UserDefinedTypes = userDefinedTypes ?? throw new ArgumentNullException(nameof(userDefinedTypes));
        UserDefinedTypesCount = userDefinedTypes.UCount();

        ObjectCount = TablesCount + ViewsCount + SequencesCount + SynonymsCount + RoutinesCount + UserDefinedTypesCount;
    }

    public string Name { get; }

    public string SchemaUrl { get; }

    /// <summary>The principal that owns the schema. Empty when the database records none.</summary>
    public string Owner { get; }

    public bool IsDefault { get; }

    public bool IsSystem { get; }

    public IEnumerable<SchemaObject> Tables { get; }

    public uint TablesCount { get; }

    public IEnumerable<SchemaObject> Views { get; }

    public uint ViewsCount { get; }

    public IEnumerable<SchemaObject> Sequences { get; }

    public uint SequencesCount { get; }

    public IEnumerable<SchemaObject> Synonyms { get; }

    public uint SynonymsCount { get; }

    public IEnumerable<SchemaObject> Routines { get; }

    public uint RoutinesCount { get; }

    public IEnumerable<SchemaObject> UserDefinedTypes { get; }

    public uint UserDefinedTypesCount { get; }

    /// <summary>
    /// The total number of objects the report holds for this schema, i.e. the sum of the per-type
    /// counts.
    /// </summary>
    public uint ObjectCount { get; }

    /// <summary>
    /// An object declared within the schema, as a link to its own page.
    /// </summary>
    public sealed class SchemaObject
    {
        public SchemaObject(string name, string url)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Url = url ?? throw new ArgumentNullException(nameof(url));
        }

        public string Name { get; }

        public string Url { get; }
    }
}
