using System;
using System.Collections.Generic;
using LanguageExt;
using SJP.Schematic.Core;

namespace SJP.Schematic.Reporting.Html.ViewModels;

/// <summary>
/// The dashboard summary payload (<c>data/main.json</c>): database name/version plus the
/// schema-wide object counts. The per-type lists live in their own summary files
/// (<c>tables.json</c>, <c>views.json</c>, …); this only carries what the dashboard renders.
/// </summary>
public sealed class Main
{
    public Main(
        string? databaseName,
        string databaseVersion,
        uint columnsCount,
        uint constraintsCount,
        uint indexesCount,
        IReadOnlyCollection<string> schemas,
        uint tablesCount,
        uint viewsCount,
        uint sequencesCount,
        uint synonymsCount,
        uint routinesCount
    )
    {
        DatabaseName = databaseName ?? "Unnamed Database";
        DatabaseVersion = databaseVersion ?? string.Empty;

        ColumnsCount = columnsCount;
        ConstraintsCount = constraintsCount;
        IndexesCount = indexesCount;

        Schemas = schemas ?? throw new ArgumentNullException(nameof(schemas));
        SchemasCount = (uint)schemas.Count;

        TablesCount = tablesCount;
        ViewsCount = viewsCount;
        SequencesCount = sequencesCount;
        SynonymsCount = synonymsCount;
        RoutinesCount = routinesCount;
    }

    public string DatabaseName { get; }

    public string DatabaseVersion { get; }

    public uint ColumnsCount { get; }

    public uint ConstraintsCount { get; }

    public uint IndexesCount { get; }

    public IReadOnlyCollection<string> Schemas { get; }

    public uint SchemasCount { get; }

    public uint TablesCount { get; }

    public uint ViewsCount { get; }

    public uint SequencesCount { get; }

    public uint SynonymsCount { get; }

    public uint RoutinesCount { get; }

    /// <summary>
    /// A row in the tables summary list (<c>data/tables.json</c>). Shared by <see cref="Tables"/>.
    /// </summary>
    public sealed class Table
    {
        public Table(
            Identifier tableName,
            uint parentsCount,
            uint childrenCount,
            uint columnCount,
            ulong rowCount
        )
        {
            ArgumentNullException.ThrowIfNull(tableName);

            Name = tableName.ToVisibleName();
            TableUrl = UrlRouter.GetTableUrl(tableName);

            ParentsCount = parentsCount;
            ChildrenCount = childrenCount;
            ColumnCount = columnCount;
            RowCount = rowCount;
        }

        public string Name { get; }

        public string TableUrl { get; }

        public uint ParentsCount { get; }

        public uint ChildrenCount { get; }

        public uint ColumnCount { get; }

        public ulong RowCount { get; }
    }

    /// <summary>
    /// A row in the views summary list (<c>data/views.json</c>). Shared by <see cref="Views"/>.
    /// </summary>
    public sealed class View
    {
        public View(Identifier viewName, uint columnCount, bool isMaterialized)
        {
            ArgumentNullException.ThrowIfNull(viewName);

            Name = viewName.ToVisibleName();
            ViewUrl = UrlRouter.GetViewUrl(viewName);
            ColumnCount = columnCount;
            IsMaterialized = isMaterialized;
        }

        public string Name { get; }

        public string ViewUrl { get; }

        public uint ColumnCount { get; }

        public bool IsMaterialized { get; }
    }

    /// <summary>
    /// A row in the sequences summary list (<c>data/sequences.json</c>). Shared by <see cref="Sequences"/>.
    /// </summary>
    public sealed class Sequence
    {
        public Sequence(
            Identifier sequenceName,
            decimal start,
            decimal increment,
            Option<decimal> minValue,
            Option<decimal> maxValue,
            int cache,
            bool cycle
        )
        {
            ArgumentNullException.ThrowIfNull(sequenceName);

            Name = sequenceName.ToVisibleName();
            SequenceUrl = UrlRouter.GetSequenceUrl(sequenceName);

            Start = start;
            Increment = increment;
            MinValue = minValue.MatchUnsafe(static mv => mv, static () => (decimal?)null);
            MaxValue = maxValue.MatchUnsafe(static mv => mv, static () => (decimal?)null);
            Cache = cache;
            Cycle = cycle;
        }

        public string Name { get; }

        public string SequenceUrl { get; }

        public decimal Start { get; }

        public decimal Increment { get; }

        public decimal? MinValue { get; }

        public decimal? MaxValue { get; }

        public int Cache { get; }

        public bool Cycle { get; }
    }

    /// <summary>
    /// A row in the synonyms summary list (<c>data/synonyms.json</c>). Shared by <see cref="Synonyms"/>.
    /// The synonym target is a structured <c>{ targetName, targetUrl? }</c> pair (the hash route is
    /// omitted when the target cannot be resolved to a known object).
    /// </summary>
    public sealed class Synonym
    {
        public Synonym(Identifier synonymName, Identifier target, Option<Uri> targetUrl)
        {
            ArgumentNullException.ThrowIfNull(synonymName);
            ArgumentNullException.ThrowIfNull(target);

            Name = synonymName.ToVisibleName();
            SynonymUrl = UrlRouter.GetSynonymUrl(synonymName);

            TargetName = target.ToVisibleName();
            TargetUrl = targetUrl.MatchUnsafe(static uri => uri.ToString(), static () => (string?)null);
        }

        public string Name { get; }

        public string SynonymUrl { get; }

        public string TargetName { get; }

        public string? TargetUrl { get; }
    }

    /// <summary>
    /// A row in the routines summary list (<c>data/routines.json</c>). Shared by <see cref="Routines"/>.
    /// </summary>
    public sealed class Routine
    {
        public Routine(Identifier routineName)
        {
            ArgumentNullException.ThrowIfNull(routineName);

            Name = routineName.ToVisibleName();
            RoutineUrl = UrlRouter.GetRoutineUrl(routineName);
        }

        public string Name { get; }

        public string RoutineUrl { get; }
    }
}
