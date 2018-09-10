using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using SJP.Schematic.Core;
using SJP.Schematic.Reporting.Dot;

namespace SJP.Schematic.Reporting.Html.ViewModels.Mappers
{
    internal sealed class TableModelMapper :
        IDatabaseModelMapper<IRelationalDatabaseTable, Table>
    {
        public TableModelMapper(IDbConnection connection, IDatabaseDialect dialect)
        {
            Connection = connection ?? throw new ArgumentNullException(nameof(connection));
            Dialect = dialect ?? throw new ArgumentNullException(nameof(dialect));
        }

        private IDbConnection Connection { get; }

        private IDatabaseDialect Dialect { get; }

        public Table Map(IRelationalDatabaseTable dbObject)
        {
            if (dbObject == null)
                throw new ArgumentNullException(nameof(dbObject));

            var rowCount = Connection.GetRowCount(Dialect, dbObject.Name);
            var tableColumns = dbObject.Columns.Select((c, i) => new { Column = c, Ordinal = i + 1 }).ToList();
            var primaryKey = dbObject.PrimaryKey;
            var uniqueKeys = dbObject.UniqueKeys.ToList();
            var parentKeys = dbObject.ParentKeys.ToList();
            var childKeys = dbObject.ChildKeys.ToList();
            var checks = dbObject.Checks.ToList();

            var columns = new List<Table.Column>();
            foreach (var tableColumn in tableColumns)
            {
                var col = tableColumn.Column;
                var columnName = col.Name.LocalName;
                var qualifiedColumnName = dbObject.Name.ToVisibleName() + "." + columnName;

                var isPrimaryKey = primaryKey != null && primaryKey.Columns.Any(c => c.Name.LocalName == columnName);
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

                    var columnFks = parentColumnNames.Select(colName =>
                        new Table.ParentKey(
                            parentKey.ChildKey.Name?.LocalName,
                            parentKey.ParentKey.Table.Name,
                            colName,
                            qualifiedColumnName
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

                    var columnFks = childColumnNames.Select(colName =>
                        new Table.ChildKey(
                            childKey.ChildKey.Name?.LocalName,
                            childKey.ChildKey.Table.Name,
                            colName,
                            qualifiedColumnName
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

            var tableIndexes = dbObject.Indexes.ToList();
            var mappedIndexes = tableIndexes.Select(index =>
                new Table.Index(
                    index.Name?.LocalName,
                    index.IsUnique,
                    index.Columns.Select(c => c.GetExpression(Dialect)).ToList(),
                    index.Columns.Select(c => c.Order).ToList(),
                    index.IncludedColumns.Select(c => c.Name.LocalName).ToList()
                )).ToList();

            var renderPrimaryKey = primaryKey != null
                ? new Table.PrimaryKeyConstraint(
                      primaryKey.Name?.LocalName,
                      primaryKey.Columns.Select(c => c.Name.LocalName).ToList()
                  )
                : null;

            var renderUniqueKeys = uniqueKeys
                .Select(uk => new Table.UniqueKey(
                    uk.Name?.LocalName,
                    uk.Columns.Select(c => c.Name.LocalName).ToList()
                )).ToList();

            var renderParentKeys = parentKeys.Select(pk =>
                new Table.ForeignKey(
                    pk.ChildKey.Name?.LocalName,
                    pk.ChildKey.Columns.Select(c => c.Name.LocalName).ToList(),
                    pk.ParentKey.Table.Name,
                    pk.ParentKey.Name?.LocalName,
                    pk.ParentKey.Columns.Select(c => c.Name.LocalName).ToList(),
                    pk.DeleteRule,
                    pk.UpdateRule
                )).ToList();

            var renderChecks = checks.Select(c =>
                new Table.CheckConstraint(
                    c.Name?.LocalName,
                    c.Definition
                )).ToList();

            var relationshipBuilder = new RelationshipFinder();
            var oneDegreeTables = relationshipBuilder.GetTablesByDegrees(dbObject, 1);
            var twoDegreeTables = relationshipBuilder.GetTablesByDegrees(dbObject, 2);

            var dotFormatter = new DatabaseDotFormatter(Connection, dbObject.Database);
            var renderOptions = new DotRenderOptions { HighlightedTable = dbObject.Name };
            var oneDegreeDot = dotFormatter.RenderTables(oneDegreeTables, renderOptions);
            var twoDegreeDot = dotFormatter.RenderTables(twoDegreeTables, renderOptions);

            var diagrams = new[]
            {
                new Table.Diagram(dbObject.Name, "One", oneDegreeDot, true),
                new Table.Diagram(dbObject.Name, "Two", twoDegreeDot, false)
            };

            return new Table(
                dbObject.Name,
                columns,
                renderPrimaryKey,
                renderUniqueKeys,
                renderParentKeys,
                renderChecks,
                mappedIndexes,
                diagrams,
                "../",
                rowCount
            );
        }

        public Task<Table> MapAsync(IRelationalDatabaseTable dbObject)
        {
            if (dbObject == null)
                throw new ArgumentNullException(nameof(dbObject));

            return MapAsyncCore(dbObject);
        }

        private async Task<Table> MapAsyncCore(IRelationalDatabaseTable dbObject)
        {
            var rowCount = await Connection.GetRowCountAsync(Dialect, dbObject.Name).ConfigureAwait(false);
            var dbColumns = await dbObject.ColumnsAsync().ConfigureAwait(false);
            var tableColumns = dbColumns.Select((c, i) => new { Column = c, Ordinal = i + 1 }).ToList();
            var primaryKey = await dbObject.PrimaryKeyAsync().ConfigureAwait(false);
            var uniqueKeys = await dbObject.UniqueKeysAsync().ConfigureAwait(false);
            var parentKeys = await dbObject.ParentKeysAsync().ConfigureAwait(false);
            var childKeys = await dbObject.ChildKeysAsync().ConfigureAwait(false);
            var checks = await dbObject.ChecksAsync().ConfigureAwait(false);
            var tableIndexes = await dbObject.IndexesAsync().ConfigureAwait(false);

            var columns = new List<Table.Column>();
            foreach (var tableColumn in tableColumns)
            {
                var col = tableColumn.Column;
                var columnName = col.Name.LocalName;
                var qualifiedColumnName = dbObject.Name.ToVisibleName() + "." + columnName;

                var isPrimaryKey = primaryKey != null && primaryKey.Columns.Any(c => c.Name.LocalName == columnName);
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

                    var columnFks = parentColumnNames.Select(colName =>
                        new Table.ParentKey(
                            parentKey.ChildKey.Name?.LocalName,
                            parentKey.ParentKey.Table.Name,
                            colName,
                            qualifiedColumnName
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

                    var columnFks = childColumnNames.Select(colName =>
                        new Table.ChildKey(
                            childKey.ChildKey.Name?.LocalName,
                            childKey.ChildKey.Table.Name,
                            colName,
                            qualifiedColumnName
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

            var mappedIndexes = tableIndexes.Select(index =>
                new Table.Index(
                    index.Name?.LocalName,
                    index.IsUnique,
                    index.Columns.Select(c => c.GetExpression(Dialect)).ToList(),
                    index.Columns.Select(c => c.Order).ToList(),
                    index.IncludedColumns.Select(c => c.Name.LocalName).ToList()
                )).ToList();

            var renderPrimaryKey = primaryKey != null
                ? new Table.PrimaryKeyConstraint(
                      primaryKey.Name?.LocalName,
                      primaryKey.Columns.Select(c => c.Name.LocalName).ToList()
                  )
                : null;

            var renderUniqueKeys = uniqueKeys
                .Select(uk => new Table.UniqueKey(
                    uk.Name?.LocalName,
                    uk.Columns.Select(c => c.Name.LocalName).ToList()
                )).ToList();

            var renderParentKeys = parentKeys.Select(pk =>
                new Table.ForeignKey(
                    pk.ChildKey.Name?.LocalName,
                    pk.ChildKey.Columns.Select(c => c.Name.LocalName).ToList(),
                    pk.ParentKey.Table.Name,
                    pk.ParentKey.Name?.LocalName,
                    pk.ParentKey.Columns.Select(c => c.Name.LocalName).ToList(),
                    pk.DeleteRule,
                    pk.UpdateRule
                )).ToList();

            var renderChecks = checks.Select(c =>
                new Table.CheckConstraint(
                    c.Name?.LocalName,
                    c.Definition
                )).ToList();

            var relationshipBuilder = new RelationshipFinder();
            var oneDegreeTables = await relationshipBuilder.GetTablesByDegreesAsync(dbObject, 1).ConfigureAwait(false);
            var twoDegreeTables = await relationshipBuilder.GetTablesByDegreesAsync(dbObject, 2).ConfigureAwait(false);

            var dotFormatter = new DatabaseDotFormatter(Connection, dbObject.Database);
            var renderOptions = new DotRenderOptions { HighlightedTable = dbObject.Name };
            var oneDegreeDot = await dotFormatter.RenderTablesAsync(oneDegreeTables, renderOptions).ConfigureAwait(false);
            var twoDegreeDot = await dotFormatter.RenderTablesAsync(twoDegreeTables, renderOptions).ConfigureAwait(false);

            var diagrams = new[]
            {
                new Table.Diagram(dbObject.Name, "One", oneDegreeDot, true),
                new Table.Diagram(dbObject.Name, "Two", twoDegreeDot, false)
            };

            return new Table(
                dbObject.Name,
                columns,
                renderPrimaryKey,
                renderUniqueKeys,
                renderParentKeys,
                renderChecks,
                mappedIndexes,
                diagrams,
                "../",
                rowCount
            );
        }
    }
}
