using System;
using System.Collections.Generic;
using System.Web;
using SJP.Schematic.Core;

namespace SJP.Schematic.Reporting.Html.ViewModels
{
    internal sealed class Main : ITemplateParameter
    {
        public Main(
            string databaseName,
            string databaseVersion,
            uint columnsCount,
            uint constraintsCount,
            uint indexesCount,
            IEnumerable<string> schemas,
            IEnumerable<Table> tables,
            IEnumerable<View> views,
            IEnumerable<Sequence> sequences,
            IEnumerable<Synonym> synonyms
        )
        {
            DatabaseName = databaseName ?? string.Empty;
            DatabaseVersion = databaseVersion ?? string.Empty;

            ColumnsCount = columnsCount;
            ConstraintsCount = constraintsCount;
            IndexesCount = indexesCount;

            Schemas = schemas ?? throw new ArgumentNullException(nameof(schemas));
            SchemasCount = schemas.UCount();

            Tables = tables ?? throw new ArgumentNullException(nameof(tables));
            TablesCount = tables.UCount();
            TablesTableClass = TablesCount > 0 ? CssClasses.DataTableClass : string.Empty;

            Views = views ?? throw new ArgumentNullException(nameof(views));
            ViewsCount = views.UCount();
            ViewsTableClass = ViewsCount > 0 ? CssClasses.DataTableClass : string.Empty;

            Sequences = sequences ?? throw new ArgumentNullException(nameof(sequences));
            SequencesCount = sequences.UCount();
            SequencesTableClass = SequencesCount > 0 ? CssClasses.DataTableClass : string.Empty;

            Synonyms = synonyms ?? throw new ArgumentNullException(nameof(synonyms));
            SynonymsCount = synonyms.UCount();
            SynonymsTableClass = SynonymsCount > 0 ? CssClasses.DataTableClass : string.Empty;
        }

        public ReportTemplate Template { get; } = ReportTemplate.Main;

        public string DatabaseName { get; }

        public string DatabaseVersion { get; }

        public DateTime GenerationTime => DateTime.Now;

        public uint ColumnsCount { get; }

        public uint ConstraintsCount { get; }

        public uint IndexesCount { get; }

        public IEnumerable<string> Schemas { get; }

        public uint SchemasCount { get; }

        public IEnumerable<Table> Tables { get; }

        public uint TablesCount { get; }

        public string TablesTableClass { get; }

        public IEnumerable<View> Views { get; }

        public uint ViewsCount { get; }

        public string ViewsTableClass { get; }

        public IEnumerable<Sequence> Sequences { get; }

        public uint SequencesCount { get; }

        public string SequencesTableClass { get; }

        public IEnumerable<Synonym> Synonyms { get; }

        public uint SynonymsCount { get; }

        public string SynonymsTableClass { get; }

        internal sealed class Table
        {
            public Table(
                Identifier tableName,
                uint parentsCount,
                uint childrenCount,
                uint columnCount,
                ulong rowCount
            )
            {
                if (tableName == null)
                    throw new ArgumentNullException(nameof(tableName));

                Name = tableName.ToVisibleName();
                TableUrl = tableName.ToSafeKey();

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

        internal sealed class View
        {
            public View(Identifier viewName, uint columnCount, ulong rowCount)
            {
                if (viewName == null)
                    throw new ArgumentNullException(nameof(viewName));

                Name = viewName.ToVisibleName();
                ViewUrl = viewName.ToSafeKey();
                ColumnCount = columnCount;
                RowCount = rowCount;
            }

            public string Name { get; }

            public string ViewUrl { get; }

            public uint ColumnCount { get; }

            public ulong RowCount { get; }
        }

        internal sealed class Sequence
        {
            public Sequence(
                Identifier sequenceName,
                decimal start,
                decimal increment,
                decimal? minValue,
                decimal? maxValue,
                int cache,
                bool cycle
            )
            {
                if (sequenceName == null)
                    throw new ArgumentNullException(nameof(sequenceName));

                Name = sequenceName.ToVisibleName();
                Start = start;
                Increment = increment;
                MinValueText = minValue?.ToString() ?? string.Empty;
                MaxValueText = maxValue?.ToString() ?? string.Empty;
                Cache = cache;
                CycleText = cycle ? "✓" : string.Empty;
            }

            public decimal Start { get; }

            public decimal Increment { get; }

            public string MinValueText { get; }

            public string MaxValueText { get; }

            public int Cache { get; }

            public string Name { get; }

            public string CycleText { get; }
        }

        internal sealed class Synonym
        {
            public Synonym(Identifier synonymName, Identifier target, Uri targetUrl)
            {
                if (synonymName == null)
                    throw new ArgumentNullException(nameof(synonymName));
                if (target == null)
                    throw new ArgumentNullException(nameof(target));

                Name = synonymName.ToVisibleName();

                var targetName = target.ToVisibleName();
                TargetText = targetUrl != null
                    ? $"<a href=\"{ targetUrl }\">{ HttpUtility.HtmlEncode(targetName) }</a>"
                    : HttpUtility.HtmlEncode(targetName);
            }

            public string Name { get; }

            public string TargetText { get; }
        }
    }
}
