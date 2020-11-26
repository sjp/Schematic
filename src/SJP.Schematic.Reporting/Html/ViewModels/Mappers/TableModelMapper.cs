using System;
using System.Collections.Generic;
using System.Linq;
using SJP.Schematic.Core;
using SJP.Schematic.Dot;

namespace SJP.Schematic.Reporting.Html.ViewModels.Mappers
{
    internal sealed class TableModelMapper
    {
        public TableModelMapper(
            IIdentifierDefaults identifierDefaults,
            IReadOnlyDictionary<Identifier, ulong> rowCounts,
            RelationshipFinder relationship
        )
        {
            IdentifierDefaults = identifierDefaults ?? throw new ArgumentNullException(nameof(identifierDefaults));
            RowCounts = rowCounts ?? throw new ArgumentNullException(nameof(rowCounts));
            RelationshipFinder = relationship ?? throw new ArgumentNullException(nameof(relationship));
        }

        private IIdentifierDefaults IdentifierDefaults { get; }

        private IReadOnlyDictionary<Identifier, ulong> RowCounts { get; }

        private RelationshipFinder RelationshipFinder { get; }

        private const string RootPath = "../";

        public Table Map(IRelationalDatabaseTable table)
        {
            if (table == null)
                throw new ArgumentNullException(nameof(table));

            var tableColumns = table.Columns.Select(static (c, i) => new { Column = c, Ordinal = i + 1 }).ToList();
            var primaryKey = table.PrimaryKey;
            var uniqueKeys = table.UniqueKeys.ToList();
            var parentKeys = table.ParentKeys.ToList();
            var childKeys = table.ChildKeys.ToList();
            var checks = table.Checks.ToList();
            var triggers = table.Triggers.ToList();

            var columns = new List<Table.Column>();
            foreach (var tableColumn in tableColumns)
            {
                var col = tableColumn.Column;
                var columnName = col.Name.LocalName;
                var qualifiedColumnName = table.Name.ToVisibleName() + "." + columnName;

                var isPrimaryKey = primaryKey.Match(pk => pk.Columns.Any(c => string.Equals(c.Name.LocalName, columnName, StringComparison.Ordinal)), static () => false);
                var isUniqueKey = uniqueKeys.Any(uk => uk.Columns.Any(ukc => string.Equals(ukc.Name.LocalName, columnName, StringComparison.Ordinal)));
                var isParentKey = parentKeys.Any(fk => fk.ChildKey.Columns.Any(fkc => string.Equals(fkc.Name.LocalName, columnName, StringComparison.Ordinal)));

                var matchingParentKeys = parentKeys.Where(fk => fk.ChildKey.Columns.Any(fkc => string.Equals(fkc.Name.LocalName, columnName, StringComparison.Ordinal))).ToList();
                var columnParentKeys = new List<Table.ParentKey>();
                foreach (var parentKey in matchingParentKeys)
                {
                    var columnIndexes = parentKey.ChildKey.Columns
                        .Select((c, i) => string.Equals(c.Name.LocalName, columnName, StringComparison.Ordinal) ? i : -1)
                        .Where(static i => i >= 0)
                        .ToList();

                    var parentColumnNames = parentKey.ParentKey.Columns
                        .Where((_, i) => columnIndexes.Contains(i))
                        .Select(static c => c.Name.LocalName)
                        .ToList();

                    var childKeyName = parentKey.ChildKey.Name.Match(static name => name.LocalName, static () => string.Empty);
                    var columnFks = parentColumnNames.ConvertAll(colName =>
                        new Table.ParentKey(
                            childKeyName,
                            parentKey.ParentTable,
                            colName,
                            qualifiedColumnName,
                            RootPath
                        ));

                    columnParentKeys.AddRange(columnFks);
                }

                var matchingChildKeys = childKeys.Where(ck => ck.ParentKey.Columns.Any(ckc => string.Equals(ckc.Name.LocalName, columnName, StringComparison.Ordinal))).ToList();
                var columnChildKeys = new List<Table.ChildKey>();
                foreach (var childKey in matchingChildKeys)
                {
                    var columnIndexes = childKey.ParentKey.Columns
                        .Select((c, i) => string.Equals(c.Name.LocalName, columnName, StringComparison.Ordinal) ? i : -1)
                        .Where(static i => i >= 0)
                        .ToList();

                    var childColumnNames = childKey.ChildKey.Columns
                        .Where((_, i) => columnIndexes.Contains(i))
                        .Select(static c => c.Name.LocalName)
                        .ToList();

                    var childKeyName = childKey.ChildKey.Name.Match(static name => name.LocalName, static () => string.Empty);
                    var columnFks = childColumnNames.ConvertAll(colName =>
                        new Table.ChildKey(
                            childKeyName,
                            childKey.ChildTable,
                            colName,
                            qualifiedColumnName,
                            RootPath
                        ));

                    columnChildKeys.AddRange(columnFks);
                }

                var column = new Table.Column(
                    columnName,
                    tableColumn.Ordinal,
                    tableColumn.Column.IsNullable,
                    tableColumn.Column.Type.Definition,
                    tableColumn.Column.DefaultValue,
                    isPrimaryKey,
                    isUniqueKey,
                    isParentKey,
                    columnChildKeys,
                    columnParentKeys
                );
                columns.Add(column);
            }

            var tableIndexes = table.Indexes.ToList();
            var renderIndexes = tableIndexes.ConvertAll(index =>
                new Table.Index(
                    index.Name?.LocalName,
                    index.IsUnique,
                    index.Columns.Select(static c => c.Expression).ToList(),
                    index.Columns.Select(static c => c.Order).ToList(),
                    index.IncludedColumns.Select(static c => c.Name.LocalName).ToList()
                ));

            var renderPrimaryKey = primaryKey
                .Map(pk => new Table.PrimaryKeyConstraint(
                    pk.Name.Match(static name => name.LocalName, static () => string.Empty),
                    pk.Columns.Select(static c => c.Name.LocalName).ToList()
                ));

            var renderUniqueKeys = uniqueKeys
                .ConvertAll(uk => new Table.UniqueKey(
                    uk.Name.Match(static name => name.LocalName, static () => string.Empty),
                    uk.Columns.Select(static c => c.Name.LocalName).ToList()
                ));

            var renderParentKeys = parentKeys.ConvertAll(pk =>
                new Table.ForeignKey(
                    pk.ChildKey.Name.Match(static name => name.LocalName, static () => string.Empty),
                    pk.ChildKey.Columns.Select(static c => c.Name.LocalName).ToList(),
                    pk.ParentTable,
                    pk.ParentKey.Name.Match(static name => name.LocalName, static () => string.Empty),
                    pk.ParentKey.Columns.Select(static c => c.Name.LocalName).ToList(),
                    pk.DeleteAction,
                    pk.UpdateAction,
                    RootPath
                ));

            var renderChecks = checks.ConvertAll(c =>
                new Table.CheckConstraint(
                    c.Name.Match(static name => name.LocalName, static () => string.Empty),
                    c.Definition
                ));

            var renderTriggers = triggers.ConvertAll(tr =>
                new Table.Trigger(
                    table.Name,
                    tr.Name.LocalName,
                    tr.Definition,
                    tr.QueryTiming,
                    tr.TriggerEvent
                ));

            var oneDegreeTables = RelationshipFinder.GetTablesByDegrees(table, 1);
            var twoDegreeTables = RelationshipFinder.GetTablesByDegrees(table, 2);

            var dotFormatter = new DotFormatter(IdentifierDefaults);
            var renderOptions = new DotRenderOptions { HighlightedTable = table.Name };
            var oneDegreeDot = dotFormatter.RenderTables(oneDegreeTables, RowCounts, renderOptions);
            var twoDegreeDot = dotFormatter.RenderTables(twoDegreeTables, RowCounts, renderOptions);

            var diagrams = new[]
            {
                new Table.Diagram(table.Name, "One Degree", oneDegreeDot, true),
                new Table.Diagram(table.Name, "Two Degrees", twoDegreeDot, false)
            };

            if (!RowCounts.TryGetValue(table.Name, out var rowCount))
                rowCount = 0;

            return new Table(
                table.Name,
                columns,
                renderPrimaryKey,
                renderUniqueKeys,
                renderParentKeys,
                renderChecks,
                renderIndexes,
                renderTriggers,
                diagrams,
                RootPath,
                rowCount
            );
        }
    }
}
