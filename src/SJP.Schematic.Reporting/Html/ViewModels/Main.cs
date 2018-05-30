using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SJP.Schematic.Core;

namespace SJP.Schematic.Reporting.Html.ViewModels
{
    internal class Main : ITemplateParameter
    {
        public ReportTemplate Template { get; } = ReportTemplate.Main;

        public string DatabaseName { get; set; }

        public string ProductName { get; set; }

        public string ProductVersion { get; set; }

        public DateTime GenerationTime => DateTime.Now;

        public uint ColumnsCount { get; set; }

        public uint ConstraintsCount { get; set; }

        public uint IndexesCount { get; set; }

        public IEnumerable<string> Schemas
        {
            get => _schemas;
            set => _schemas = value ?? throw new ArgumentNullException(nameof(value));
        }

        private IEnumerable<string> _schemas = Enumerable.Empty<string>();

        public IEnumerable<Table> Tables
        {
            get => _tables;
            set => _tables = value ?? throw new ArgumentNullException(nameof(value));
        }

        private IEnumerable<Table> _tables = Enumerable.Empty<Table>();

        public uint TablesCount => Tables.UCount();

        public string TablesTableClass => TablesCount > 0 ? CssClasses.DataTableClass : string.Empty;

        public IEnumerable<View> Views
        {
            get => _views;
            set => _views = value ?? throw new ArgumentNullException(nameof(value));
        }

        private IEnumerable<View> _views = Enumerable.Empty<View>();

        public uint ViewsCount => Views.UCount();

        public string ViewsTableClass => ViewsCount > 0 ? CssClasses.DataTableClass : string.Empty;

        public IEnumerable<Sequence> Sequences
        {
            get => _sequences;
            set => _sequences = value ?? throw new ArgumentNullException(nameof(value));
        }

        private IEnumerable<Sequence> _sequences = Enumerable.Empty<Sequence>();

        public uint SequencesCount => Sequences.UCount();

        public string SequencesTableClass => SequencesCount > 0 ? CssClasses.DataTableClass : string.Empty;

        public IEnumerable<Synonym> Synonyms
        {
            get => _synonyms;
            set => _synonyms = value ?? throw new ArgumentNullException(nameof(value));
        }

        private IEnumerable<Synonym> _synonyms = Enumerable.Empty<Synonym>();

        public uint SynonymsCount => Synonyms.UCount();

        public string SynonymsTableClass => SynonymsCount > 0 ? CssClasses.DataTableClass : string.Empty;

        internal class Table
        {
            public Table(Identifier tableName)
            {
                _tableName = tableName ?? throw new ArgumentNullException(nameof(tableName));
            }

            public Identifier TableName
            {
                get => _tableName;
                set => _tableName = value ?? throw new ArgumentNullException(nameof(value));
            }

            private Identifier _tableName;

            public string Name => _tableName.ToVisibleName();

            public string TableUrl => _tableName.ToSafeKey();

            public uint ParentsCount { get; set; }

            public uint ChildrenCount { get; set; }

            public uint ColumnCount { get; set; }

            public ulong RowCount { get; set; }
        }

        internal class View
        {
            public View(Identifier viewName)
            {
                _viewName = viewName ?? throw new ArgumentNullException(nameof(viewName));
            }

            public Identifier ViewName
            {
                get => _viewName;
                set => _viewName = value ?? throw new ArgumentNullException(nameof(value));
            }

            private Identifier _viewName;

            public string Name => _viewName.ToVisibleName();

            public string ViewUrl => _viewName.ToSafeKey();

            public uint ColumnCount { get; set; }

            public ulong RowCount { get; set; }
        }

        internal class Sequence
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
                SequenceName = sequenceName ?? throw new ArgumentNullException(nameof(sequenceName));
                Start = start;
                Increment = increment;
                MinValue = minValue;
                MaxValue = maxValue;
                Cache = cache;
                Cycle = cycle;
            }

            protected Identifier SequenceName { get; }

            public decimal Start { get; }

            public decimal Increment { get; }

            protected decimal? MinValue { get; }

            public string MinValueText => MinValue?.ToString() ?? string.Empty;

            protected decimal? MaxValue { get; }

            public string MaxValueText => MaxValue?.ToString() ?? string.Empty;

            public int Cache { get; }

            protected bool Cycle { get; }

            public string Name => SequenceName.ToVisibleName();

            public string CycleText => Cycle ? "✓" : string.Empty;

        }

        internal class Synonym
        {
            public Synonym(Identifier synonymName, Identifier target)
            {
                SynonymName = synonymName ?? throw new ArgumentNullException(nameof(synonymName));
                Target = target ?? throw new ArgumentNullException(nameof(target));
            }

            protected Identifier SynonymName { get; }

            protected Identifier Target { get; }

            public Uri TargetUrl { get; set; }

            public string Name => SynonymName.ToVisibleName();

            public string TargetName => Target.ToVisibleName();

            public string TargetText => TargetUrl != null
                ? $"<a href=\"{ TargetUrl }\">{ HttpUtility.HtmlEncode(TargetName) }</a>"
                : HttpUtility.HtmlEncode(TargetName);
        }
    }
}
