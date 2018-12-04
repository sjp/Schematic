using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace SJP.Schematic.Reporting.Dot
{
    public class DatabaseDotFormatter : IDatabaseDotFormatter, IDatabaseDotFormatterAsync
    {
        public DatabaseDotFormatter(IDbConnection connection, IRelationalDatabase database)
        {
            Connection = connection ?? throw new ArgumentNullException(nameof(connection));
            Database = database ?? throw new ArgumentNullException(nameof(database));
        }

        protected IDbConnection Connection { get; }

        protected IRelationalDatabase Database { get; }

        public string RenderDatabase() => RenderTables(Database.Tables, DotRenderOptions.Default);

        public string RenderDatabase(DotRenderOptions options) => RenderTables(Database.Tables, options);

        public string RenderTables(IEnumerable<IRelationalDatabaseTable> tables) => RenderTables(tables, DotRenderOptions.Default);

        public string RenderTables(IEnumerable<IRelationalDatabaseTable> tables, DotRenderOptions options)
        {
            if (tables == null)
                throw new ArgumentNullException(nameof(tables));
            if (options == null)
                throw new ArgumentNullException(nameof(options));

            var tableNames = tables.Select(t => t.Name).ToList();

            var tableNodes = new Dictionary<DotIdentifier, DotNode>();
            var edges = new List<DotEdge>();

            foreach (var table in tables)
            {
                var tableIdentifier = table.Name.ToSafeKey();
                var tableName = table.Name.ToVisibleName();
                var nodeIdentifier = new DotIdentifier(tableIdentifier);

                var columnNames = table.Columns.Select(c => c.Name.LocalName).ToList();
                var columnTypes = table.Columns.Select(c => c.Type.Definition).ToList();

                var primaryKey = table.PrimaryKey;
                var uniqueKeys = table.UniqueKeys.ToList();
                var childKeys = table.ChildKeys.ToList();
                var parentKeys = table.ParentKeys.ToList();

                var keyColumnNames = uniqueKeys
                    .Concat(parentKeys.Select(fk => fk.ChildKey))
                    .Concat(primaryKey != null ? new[] { primaryKey } : Array.Empty<IDatabaseKey>())
                    .SelectMany(key => key.Columns.Select(c => c.Name.LocalName))
                    .Distinct()
                    .ToList();

                var childKeysCount = childKeys.UCount();
                var parentKeysCount = parentKeys.UCount();
                var rowCount = Connection.GetRowCount(Database.Dialect, table.Name);

                var tableUri = new Uri(options.RootPath + "tables/" + table.Name.ToSafeKey() + ".html", UriKind.Relative);
                var tableNodeAttrs = new[]
                {
                    NodeAttribute.URL(tableUri),
                    NodeAttribute.Tooltip(tableName)
                };
                var tableNodeOptions = new TableNodeOptions
                {
                    IsReducedColumnSet = options.IsReducedColumnSet,
                    ShowColumnDataType = options.ShowColumnDataType,
                    IsHighlighted = table.Name == options.HighlightedTable
                };

                var tableConstraints = new List<TableConstraint>();

                if (primaryKey != null)
                {
                    var primaryKeyNameText = primaryKey.Name == null
                        ? primaryKey.GetKeyHash(table.Name).ToString()
                        : primaryKey.Name.LocalName;

                    var primaryKeyConstraint = new TableConstraint(
                        primaryKeyNameText,
                        primaryKey.KeyType,
                        primaryKey.Name.LocalName,
                        primaryKey.Columns.Select(c => c.Name.LocalName).ToList(),
                        primaryKey.Columns.Select(c => c.Type.Definition).ToList()
                    );
                    tableConstraints.Add(primaryKeyConstraint);
                }

                foreach (var uniqueKey in uniqueKeys)
                {
                    var uniqueKeyNameText = uniqueKey.Name == null
                        ? uniqueKey.GetKeyHash(table.Name).ToString()
                        : uniqueKey.Name.LocalName;

                    var uniqueKeyConstraint = new TableConstraint(
                        uniqueKeyNameText,
                        uniqueKey.KeyType,
                        uniqueKey.Name.LocalName,
                        uniqueKey.Columns.Select(c => c.Name.LocalName).ToList(),
                        uniqueKey.Columns.Select(c => c.Type.Definition).ToList()
                    );
                    tableConstraints.Add(uniqueKeyConstraint);
                }

                foreach (var relationalKey in parentKeys)
                {
                    var childKey = relationalKey.ChildKey;
                    var parentKey = relationalKey.ParentKey;

                    var hasParentKey = tableNames.Contains(relationalKey.ParentTable);
                    if (!hasParentKey)
                        continue;

                    var childKeyTableName = relationalKey.ChildTable.ToSafeKey();
                    var childKeyName = childKey.Name == null
                        ? childKey.GetKeyHash(relationalKey.ChildTable).ToString()
                        : childKey.Name.LocalName;

                    var parentKeyTableName = relationalKey.ParentTable.ToSafeKey();
                    var parentKeyName = parentKey.Name == null
                        ? parentKey.GetKeyHash(relationalKey.ParentTable).ToString()
                        : parentKey.Name.LocalName;

                    var childKeyToParentKeyEdge = new DotEdge(
                        new DotIdentifier(childKeyTableName),
                        new DotIdentifier(childKeyName),
                        new DotIdentifier(parentKeyTableName),
                        new DotIdentifier(parentKeyName),
                        Array.Empty<EdgeAttribute>()
                    );
                    edges.Add(childKeyToParentKeyEdge);

                    var tableConstraint = new TableConstraint(
                        childKeyName,
                        childKey.KeyType,
                        childKey.Name.LocalName,
                        childKey.Columns.Select(c => c.Name.LocalName).ToList(),
                        childKey.Columns.Select(c => c.Type.Definition).ToList()
                    );
                    tableConstraints.Add(tableConstraint);
                }

                var tableNode = new TableNode(
                    nodeIdentifier,
                    tableName,
                    columnNames,
                    columnTypes,
                    keyColumnNames,
                    childKeysCount,
                    parentKeysCount,
                    rowCount,
                    tableNodeAttrs,
                    tableConstraints,
                    tableNodeOptions
                );
                tableNodes[nodeIdentifier] = tableNode;
            }

            var recordNodes = tableNodes.Values
                .OrderBy(node => node.Identifier.ToString())
                .ToList();

            var graphName = !Database.DatabaseName.IsNullOrWhiteSpace()
                ? Database.DatabaseName
                : "unnamed graph";

            var graph = new DotGraph(
                new DotIdentifier(graphName),
                _globalGraphAttrs,
                _globalNodeAttrs,
                _globalEdgeAttrs,
                recordNodes,
                edges
            );

            return graph.ToString();
        }

        public async Task<string> RenderDatabaseAsync()
        {
            var tables = await Database.TablesAsync().ConfigureAwait(false);

            return await RenderTablesAsync(tables, DotRenderOptions.Default).ConfigureAwait(false);
        }

        public Task<string> RenderDatabaseAsync(DotRenderOptions options)
        {
            if (options == null)
                throw new ArgumentNullException(nameof(options));

            return RenderDatabaseAsyncCore(options);
        }

        private async Task<string> RenderDatabaseAsyncCore(DotRenderOptions options)
        {
            var tables = await Database.TablesAsync().ConfigureAwait(false);

            return await RenderTablesAsyncCore(tables, options).ConfigureAwait(false);
        }

        public Task<string> RenderTablesAsync(IEnumerable<IRelationalDatabaseTable> tables)
        {
            if (tables == null)
                throw new ArgumentNullException(nameof(tables));

            return RenderTablesAsync(tables, DotRenderOptions.Default);
        }

        public Task<string> RenderTablesAsync(IEnumerable<IRelationalDatabaseTable> tables, DotRenderOptions options)
        {
            if (tables == null)
                throw new ArgumentNullException(nameof(tables));
            if (options == null)
                throw new ArgumentNullException(nameof(options));

            return RenderTablesAsyncCore(tables, options);
        }

        private async Task<string> RenderTablesAsyncCore(IEnumerable<IRelationalDatabaseTable> tables, DotRenderOptions options)
        {
            var tableNames = tables.Select(t => t.Name).ToList();

            var tableNodes = new Dictionary<DotIdentifier, DotNode>();
            var edges = new List<DotEdge>();

            foreach (var table in tables)
            {
                var tableIdentifier = table.Name.ToSafeKey();
                var tableName = table.Name.ToVisibleName();
                var nodeIdentifier = new DotIdentifier(tableIdentifier);

                var tableColumns = table.Columns;
                var columnNames = tableColumns.Select(c => c.Name.LocalName).ToList();
                var columnTypes = tableColumns.Select(c => c.Type.Definition).ToList();

                var primaryKey = table.PrimaryKey;
                var uniqueKeys = table.UniqueKeys;
                var childKeys = table.ChildKeys;
                var parentKeys = table.ParentKeys;

                var keyColumnNames = uniqueKeys
                    .Concat(parentKeys.Select(fk => fk.ChildKey))
                    .Concat(primaryKey != null ? new[] { primaryKey } : Array.Empty<IDatabaseKey>())
                    .SelectMany(key => key.Columns.Select(c => c.Name.LocalName))
                    .Distinct()
                    .ToList();

                var childKeysCount = childKeys.ToList().UCount();
                var parentKeysCount = parentKeys.ToList().UCount();
                var rowCount = await Connection.GetRowCountAsync(Database.Dialect, table.Name).ConfigureAwait(false);

                var tableUri = new Uri(options.RootPath + "tables/" + table.Name.ToSafeKey() + ".html", UriKind.Relative);
                var tableNodeAttrs = new[]
                {
                    NodeAttribute.URL(tableUri),
                    NodeAttribute.Tooltip(tableName)
                };
                var tableNodeOptions = new TableNodeOptions
                {
                    IsReducedColumnSet = options.IsReducedColumnSet,
                    ShowColumnDataType = options.ShowColumnDataType,
                    IsHighlighted = table.Name == options.HighlightedTable
                };

                var tableConstraints = new List<TableConstraint>();

                if (primaryKey != null)
                {
                    var primaryKeyNameText = primaryKey.Name == null
                        ? primaryKey.GetKeyHash(table.Name).ToString()
                        : primaryKey.Name.LocalName;

                    var primaryKeyConstraint = new TableConstraint(
                        primaryKeyNameText,
                        primaryKey.KeyType,
                        primaryKey.Name.LocalName,
                        primaryKey.Columns.Select(c => c.Name.LocalName).ToList(),
                        primaryKey.Columns.Select(c => c.Type.Definition).ToList()
                    );
                    tableConstraints.Add(primaryKeyConstraint);
                }

                foreach (var uniqueKey in uniqueKeys)
                {
                    var uniqueKeyNameText = uniqueKey.Name == null
                        ? uniqueKey.GetKeyHash(table.Name).ToString()
                        : uniqueKey.Name.LocalName;

                    var uniqueKeyConstraint = new TableConstraint(
                        uniqueKeyNameText,
                        uniqueKey.KeyType,
                        uniqueKey.Name.LocalName,
                        uniqueKey.Columns.Select(c => c.Name.LocalName).ToList(),
                        uniqueKey.Columns.Select(c => c.Type.Definition).ToList()
                    );
                    tableConstraints.Add(uniqueKeyConstraint);
                }

                foreach (var relationalKey in parentKeys)
                {
                    var childKey = relationalKey.ChildKey;
                    var parentKey = relationalKey.ParentKey;

                    var hasParentKey = tableNames.Contains(relationalKey.ParentTable);
                    if (!hasParentKey)
                        continue;

                    var childKeyTableName = relationalKey.ChildTable.ToSafeKey();
                    var childKeyName = childKey.Name == null
                        ? childKey.GetKeyHash(relationalKey.ChildTable).ToString()
                        : childKey.Name.LocalName;

                    var parentKeyTableName = relationalKey.ParentTable.ToSafeKey();
                    var parentKeyName = parentKey.Name == null
                        ? parentKey.GetKeyHash(relationalKey.ParentTable).ToString()
                        : parentKey.Name.LocalName;

                    var childKeyToParentKeyEdge = new DotEdge(
                        new DotIdentifier(childKeyTableName),
                        new DotIdentifier(childKeyName),
                        new DotIdentifier(parentKeyTableName),
                        new DotIdentifier(parentKeyName),
                        Array.Empty<EdgeAttribute>()
                    );
                    edges.Add(childKeyToParentKeyEdge);

                    var tableConstraint = new TableConstraint(
                        childKeyName,
                        childKey.KeyType,
                        childKey.Name.LocalName,
                        childKey.Columns.Select(c => c.Name.LocalName).ToList(),
                        childKey.Columns.Select(c => c.Type.Definition).ToList()
                    );
                    tableConstraints.Add(tableConstraint);
                }

                var tableNode = new TableNode(
                    nodeIdentifier,
                    tableName,
                    columnNames,
                    columnTypes,
                    keyColumnNames,
                    childKeysCount,
                    parentKeysCount,
                    rowCount,
                    tableNodeAttrs,
                    tableConstraints,
                    tableNodeOptions
                );
                tableNodes[nodeIdentifier] = tableNode;
            }

            var recordNodes = tableNodes.Values
                .OrderBy(node => node.Identifier.ToString())
                .ToList();

            var graphName = !Database.DatabaseName.IsNullOrWhiteSpace()
                ? Database.DatabaseName
                : "unnamed graph";
            var graph = new DotGraph(
                new DotIdentifier(graphName),
                _globalGraphAttrs,
                _globalNodeAttrs,
                _globalEdgeAttrs,
                recordNodes,
                edges
            );

            return graph.ToString();
        }

        private readonly static IEnumerable<GraphAttribute> _globalGraphAttrs = new[]
        {
            GraphAttribute.BackgroundColor(new RgbColor("#FFFFFF")),
            GraphAttribute.RankDirection(RankDirection.RL),
            GraphAttribute.Ratio(GraphRatio.Compress)
        };

        private readonly static IEnumerable<NodeAttribute> _globalNodeAttrs = new[]
        {
            NodeAttribute.FontFace(FontFace.Courier),
            NodeAttribute.EmptyNodeShape()
        };

        private readonly static IEnumerable<EdgeAttribute> _globalEdgeAttrs = new[]
        {
            EdgeAttribute.ArrowHead(ArrowStyleName.Open)
        };
    }
}
