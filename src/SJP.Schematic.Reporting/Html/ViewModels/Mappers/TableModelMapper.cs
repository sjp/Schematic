using System;
using System.Collections.Generic;
using System.Linq;
using SJP.Schematic.Core;
using SJP.Schematic.Reporting.Dot;

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

            var tableColumns = table.Columns.Select((c, i) => new { Column = c, Ordinal = i + 1 }).ToList();
            var primaryKey = table.PrimaryKey;
            var uniqueKeys = table.UniqueKeys.ToList();
            var parentKeys = table.ParentKeys.ToList();
            var childKeys = table.ChildKeys.ToList();
            var checks = table.Checks.ToList();

            var columns = new List<Table.Column>();
            foreach (var tableColumn in tableColumns)
            {
                var col = tableColumn.Column;
                var columnName = col.Name.LocalName;
                var qualifiedColumnName = table.Name.ToVisibleName() + "." + columnName;

                var isPrimaryKey = primaryKey.Match(pk => pk.Columns.Any(c => c.Name.LocalName == columnName), () => false);
                var isUniqueKey = uniqueKeys.Any(uk => uk.Columns.Any(ukc => ukc.Name.LocalName == columnName));
                var isParentKey = parentKeys.Any(fk => fk.ChildKey.Columns.Any(fkc => fkc.Name.LocalName == columnName));

                var matchingParentKeys = parentKeys.Where(fk => fk.ChildKey.Columns.Any(fkc => fkc.Name.LocalName == columnName)).ToList();
                var columnParentKeys = new List<Table.ParentKey>();
                foreach (var parentKey in matchingParentKeys)
                {
                    var columnIndexes = parentKey.ChildKey.Columns
                        .Select((c, i) => c.Name.LocalName == columnName ? i : -1)
                        .Where(i => i >= 0)
                        .ToList();

                    var parentColumnNames = parentKey.ParentKey.Columns
                        .Where((_, i) => columnIndexes.Contains(i))
                        .Select(c => c.Name.LocalName)
                        .ToList();

                    var childKeyName = parentKey.ChildKey.Name.Match(name => name.LocalName, () => string.Empty);
                    var columnFks = parentColumnNames.Select(colName =>
                        new Table.ParentKey(
                            childKeyName,
                            parentKey.ParentTable,
                            colName,
                            qualifiedColumnName,
                            RootPath
                        )).ToList();

                    columnParentKeys.AddRange(columnFks);
                }

                var matchingChildKeys = childKeys.Where(ck => ck.ParentKey.Columns.Any(ckc => ckc.Name.LocalName == columnName)).ToList();
                var columnChildKeys = new List<Table.ChildKey>();
                foreach (var childKey in matchingChildKeys)
                {
                    var columnIndexes = childKey.ParentKey.Columns
                        .Select((c, i) => c.Name.LocalName == columnName ? i : -1)
                        .Where(i => i >= 0)
                        .ToList();

                    var childColumnNames = childKey.ChildKey.Columns
                        .Where((_, i) => columnIndexes.Contains(i))
                        .Select(c => c.Name.LocalName)
                        .ToList();

                    var childKeyName = childKey.ChildKey.Name.Match(name => name.LocalName, () => string.Empty);
                    var columnFks = childColumnNames.Select(colName =>
                        new Table.ChildKey(
                            childKeyName,
                            childKey.ChildTable,
                            colName,
                            qualifiedColumnName,
                            RootPath
                        )).ToList();

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
            var mappedIndexes = tableIndexes.Select(index =>
                new Table.Index(
                    index.Name?.LocalName,
                    index.IsUnique,
                    index.Columns.Select(c => c.Expression).ToList(),
                    index.Columns.Select(c => c.Order).ToList(),
                    index.IncludedColumns.Select(c => c.Name.LocalName).ToList()
                )).ToList();

            var renderPrimaryKey = primaryKey
                .Map(pk => new Table.PrimaryKeyConstraint(
                    pk.Name.Match(name => name.LocalName, () => string.Empty),
                    pk.Columns.Select(c => c.Name.LocalName).ToList()
                ));

            var renderUniqueKeys = uniqueKeys
                .Select(uk => new Table.UniqueKey(
                    uk.Name.Match(name => name.LocalName, () => string.Empty),
                    uk.Columns.Select(c => c.Name.LocalName).ToList()
                )).ToList();

            var renderParentKeys = parentKeys.Select(pk =>
                new Table.ForeignKey(
                    pk.ChildKey.Name.Match(name => name.LocalName, () => string.Empty),
                    pk.ChildKey.Columns.Select(c => c.Name.LocalName).ToList(),
                    pk.ParentTable,
                    pk.ParentKey.Name.Match(name => name.LocalName, () => string.Empty),
                    pk.ParentKey.Columns.Select(c => c.Name.LocalName).ToList(),
                    pk.DeleteAction,
                    pk.UpdateAction,
                    RootPath
                )).ToList();

            var renderChecks = checks.Select(c =>
                new Table.CheckConstraint(
                    c.Name.Match(name => name.LocalName, () => string.Empty),
                    c.Definition
                )).ToList();

            var oneDegreeTables = RelationshipFinder.GetTablesByDegrees(table, 1);
            var twoDegreeTables = RelationshipFinder.GetTablesByDegrees(table, 2);

            var dotFormatter = new DatabaseDotFormatter(IdentifierDefaults);
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
                mappedIndexes,
                diagrams,
                RootPath,
                rowCount
            );
        }
    }
}
