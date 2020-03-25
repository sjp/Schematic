using System;
using System.Collections.Generic;
using LanguageExt;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Sqlite.Parsing
{
    public sealed class ParsedTableData
    {
        public ParsedTableData(
            string definition,
            IReadOnlyCollection<Column> columns,
            Option<PrimaryKey> primaryKey,
            IReadOnlyCollection<UniqueKey> uniqueKeys,
            IReadOnlyCollection<ForeignKey> parentKeys,
            IReadOnlyCollection<Check> checks
        )
        {
            if (definition.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(definition));
            if (columns == null || columns.Empty())
                throw new ArgumentNullException(nameof(columns));

            Definition = definition;
            Columns = columns;
            PrimaryKey = primaryKey;
            UniqueKeys = uniqueKeys ?? throw new ArgumentNullException(nameof(uniqueKeys));
            Checks = checks ?? throw new ArgumentNullException(nameof(checks));
            ParentKeys = parentKeys ?? throw new ArgumentNullException(nameof(parentKeys));
        }

        private ParsedTableData(string definition)
        {
            if (definition.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(definition));

            Definition = definition;
            PrimaryKey = Option<PrimaryKey>.None;
            Columns = Array.Empty<Column>();
            UniqueKeys = Array.Empty<UniqueKey>();
            Checks = Array.Empty<Check>();
            ParentKeys = Array.Empty<ForeignKey>();
        }

        public string Definition { get; }

        public IEnumerable<Column> Columns { get; }

        public Option<PrimaryKey> PrimaryKey { get; }

        public IEnumerable<UniqueKey> UniqueKeys { get; }

        public IEnumerable<Check> Checks { get; }

        public IEnumerable<ForeignKey> ParentKeys { get; }

        public static ParsedTableData Empty(string definition) => new ParsedTableData(definition);
    }
}
