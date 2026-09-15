using System;
using System.Collections.Generic;
using SJP.Schematic.Core;
using SJP.Schematic.Reporting.Html.ViewModels.Mappers;

namespace SJP.Schematic.Reporting.Html.Renderers;

/// <summary>
/// The full set of database objects for a single report run, plus the lookups derived from them
/// (<see cref="ReferencedObjectTargets"/>, <see cref="SynonymTargets"/>,
/// <see cref="ResolvedSchemas"/>). Renderers receive this as a
/// <see cref="IDataRenderer.RenderAsync"/> parameter rather than via their constructor, so a single
/// renderer instance can be reused across calls and tested without rebuilding it per case.
/// </summary>
internal sealed class ReportData
{
    private readonly Lazy<IReadOnlyList<SchemaModelMapper.SchemaObjects>> _resolvedSchemas;

    public ReportData(
        IRelationalDatabase database,
        IReadOnlyCollection<IRelationalDatabaseTable> tables,
        IReadOnlyCollection<IDatabaseView> views,
        IReadOnlyCollection<IDatabaseSequence> sequences,
        IReadOnlyCollection<IDatabaseSynonym> synonyms,
        IReadOnlyCollection<IDatabaseRoutine> routines,
        IReadOnlyCollection<IDatabaseSchema> schemas,
        IReadOnlyCollection<IDatabaseUserDefinedType> userDefinedTypes,
        string? databaseVersion,
        ReferencedObjectTargets referencedObjectTargets,
        SynonymTargets synonymTargets,
        IReadOnlyDictionary<Identifier, ITableStatistics> tableStatistics
    )
    {
        Database = database ?? throw new ArgumentNullException(nameof(database));
        Tables = tables ?? throw new ArgumentNullException(nameof(tables));
        Views = views ?? throw new ArgumentNullException(nameof(views));
        Sequences = sequences ?? throw new ArgumentNullException(nameof(sequences));
        Synonyms = synonyms ?? throw new ArgumentNullException(nameof(synonyms));
        Routines = routines ?? throw new ArgumentNullException(nameof(routines));
        Schemas = schemas ?? throw new ArgumentNullException(nameof(schemas));
        UserDefinedTypes = userDefinedTypes ?? throw new ArgumentNullException(nameof(userDefinedTypes));
        DatabaseVersion = databaseVersion;
        ReferencedObjectTargets = referencedObjectTargets ?? throw new ArgumentNullException(nameof(referencedObjectTargets));
        SynonymTargets = synonymTargets ?? throw new ArgumentNullException(nameof(synonymTargets));
        TableStatistics = tableStatistics ?? throw new ArgumentNullException(nameof(tableStatistics));

        // Resolving the schemas walks every object in the report, and four renderers need the same
        // answer, so it is resolved on first use and then shared. Renderers run concurrently, hence
        // the default Lazy mode: exactly one of them resolves, the rest wait for that result.
        _resolvedSchemas = new Lazy<IReadOnlyList<SchemaModelMapper.SchemaObjects>>(() => SchemaModelMapper.ResolveSchemas(this));
    }

    public IRelationalDatabase Database { get; }

    public IReadOnlyCollection<IRelationalDatabaseTable> Tables { get; }

    public IReadOnlyCollection<IDatabaseView> Views { get; }

    public IReadOnlyCollection<IDatabaseSequence> Sequences { get; }

    public IReadOnlyCollection<IDatabaseSynonym> Synonyms { get; }

    public IReadOnlyCollection<IDatabaseRoutine> Routines { get; }

    public IReadOnlyCollection<IDatabaseSchema> Schemas { get; }

    public IReadOnlyCollection<IDatabaseUserDefinedType> UserDefinedTypes { get; }

    public string? DatabaseVersion { get; }

    public ReferencedObjectTargets ReferencedObjectTargets { get; }

    public SynonymTargets SynonymTargets { get; }

    /// <summary>
    /// The statistics the database records for its tables, keyed by table name. Empty when no
    /// statistics provider was given, or when the database records none.
    /// </summary>
    public IReadOnlyDictionary<Identifier, ITableStatistics> TableStatistics { get; }

    /// <summary>
    /// The schemas the report covers and the names of the objects in each, combining the schemas the
    /// database declares with the schemas the report's objects are named in. Shared by every
    /// renderer that lists schemas, so the dashboard, the schemas list, the per-schema pages and
    /// search cannot disagree about which schemas exist. Read-only once resolved.
    /// </summary>
    public IReadOnlyList<SchemaModelMapper.SchemaObjects> ResolvedSchemas => _resolvedSchemas.Value;
}
