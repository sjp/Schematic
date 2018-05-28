using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace SJP.Schematic.SchemaSpy.Dot
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
            var relationalKeyNodes = new Dictionary<DotIdentifier, DotNode>();
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
                    .Concat(primaryKey != null ? new[] { primaryKey } : Enumerable.Empty<IDatabaseKey>())
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
                    tableNodeOptions
                );
                tableNodes[nodeIdentifier] = tableNode;

                foreach (var relationalKey in parentKeys)
                {
                    var childKey = relationalKey.ChildKey;
                    var parentKey = relationalKey.ParentKey;

                    var hasParentKey = tableNames.Contains(parentKey.Table.Name);
                    if (!hasParentKey)
                        continue;

                    var childKeyIdentifierText = childKey.Table.Name.ToSafeKey() + ".";
                    childKeyIdentifierText += childKey.Name == null
                        ? childKey.GetKeyHash().ToString()
                        : childKey.Name.LocalName;
                    var childKeyIdentifier = new DotIdentifier(childKeyIdentifierText);

                    var parentKeyIdentifierText = parentKey.Table.Name.ToSafeKey() + ".";
                    parentKeyIdentifierText += parentKey.Name == null
                        ? parentKey.GetKeyHash().ToString()
                        : parentKey.Name.LocalName;
                    var parentKeyIdentifier = new DotIdentifier(parentKeyIdentifierText);

                    if (!relationalKeyNodes.ContainsKey(childKeyIdentifier))
                    {
                        var childTableName = childKey.Table.Name.ToVisibleName();
                        var constraintName = childKey.Name == null
                            ? "(Unnamed)"
                            : childKey.Name.LocalName;
                        var childKeyNodeAttrs = new[]
                        {
                            NodeAttribute.Tooltip(childTableName + "." + constraintName),
                            NodeAttribute.URL(tableUri)
                        };

                        var childKeyNode = new ConstraintNode(
                            childKeyIdentifier,
                            childKey.KeyType,
                            constraintName,
                            childKey.Columns.Select(c => c.Name.LocalName).ToList(),
                            childKey.Columns.Select(c => c.Type.Definition).ToList(),
                            childKeyNodeAttrs,
                            ConstraintNodeOptions.GetDefaultOptions(childKey.KeyType)
                        );
                        relationalKeyNodes[childKeyIdentifier] = childKeyNode;

                        var edgeStyles = new[]
                        {
                            EdgeAttribute.ArrowHead(ArrowStyleName.Dot),
                            EdgeAttribute.ArrowTail(ArrowStyleName.Dot),
                            EdgeAttribute.Direction(EdgeDirection.Both)
                        };

                        var childKeyTableIdentifier = new DotIdentifier(childKey.Table.Name.ToSafeKey());
                        foreach (var column in childKey.Columns)
                        {
                            var columnIdentifier = new DotIdentifier(column.Name.LocalName);
                            var columnEdge = new DotEdge(
                                childKeyTableIdentifier,
                                columnIdentifier,
                                childKeyIdentifier,
                                columnIdentifier,
                                edgeStyles
                            );
                            edges.Add(columnEdge);
                        }
                    }

                    if (!relationalKeyNodes.ContainsKey(parentKeyIdentifier))
                    {
                        var parentTableName = parentKey.Table.Name.ToVisibleName();
                        var constraintName = parentKey.Name == null
                            ? "(Unnamed)"
                            : parentKey.Name.LocalName;
                        var parentTableUri = new Uri(options.RootPath + "tables/" + parentKey.Table.Name.ToSafeKey() + ".html", UriKind.Relative);
                        var parentKeyNodeAttrs = new[]
                        {
                            NodeAttribute.Tooltip(parentTableName + "." + constraintName),
                            NodeAttribute.URL(parentTableUri)
                        };

                        var parentKeyNode = new ConstraintNode(
                            parentKeyIdentifier,
                            parentKey.KeyType,
                            constraintName,
                            parentKey.Columns.Select(c => c.Name.LocalName).ToList(),
                            parentKey.Columns.Select(c => c.Type.Definition).ToList(),
                            parentKeyNodeAttrs,
                            ConstraintNodeOptions.GetDefaultOptions(parentKey.KeyType)
                        );
                        relationalKeyNodes[parentKeyIdentifier] = parentKeyNode;

                        var edgeStyles = new[]
                        {
                            EdgeAttribute.ArrowHead(ArrowStyleName.Dot),
                            EdgeAttribute.ArrowTail(ArrowStyleName.Dot),
                            EdgeAttribute.Direction(EdgeDirection.Both)
                        };

                        var parentKeyTableIdentifier = new DotIdentifier(parentKey.Table.Name.ToSafeKey());
                        foreach (var column in parentKey.Columns)
                        {
                            var columnIdentifier = new DotIdentifier(column.Name.LocalName);
                            var columnEdge = new DotEdge(
                                parentKeyTableIdentifier,
                                columnIdentifier,
                                parentKeyIdentifier,
                                columnIdentifier,
                                edgeStyles
                            );
                            edges.Add(columnEdge);
                        }
                    }

                    var childKeyToParentKeyEdge = new DotEdge(
                        childKeyIdentifier,
                        parentKeyIdentifier,
                        Enumerable.Empty<EdgeAttribute>()
                    );
                    edges.Add(childKeyToParentKeyEdge);
                }
            }

            var recordNodes = tableNodes.Values
                .Concat(relationalKeyNodes.Values)
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
            var tablesList = await tables.ToList().ConfigureAwait(false);

            return await RenderTablesAsync(tablesList, DotRenderOptions.Default).ConfigureAwait(false);
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
            var tablesList = await tables.ToList().ConfigureAwait(false);

            return await RenderTablesAsyncCore(tablesList, options).ConfigureAwait(false);
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
            var relationalKeyNodes = new Dictionary<DotIdentifier, DotNode>();
            var edges = new List<DotEdge>();

            foreach (var table in tables)
            {
                var tableIdentifier = table.Name.ToSafeKey();
                var tableName = table.Name.ToVisibleName();
                var nodeIdentifier = new DotIdentifier(tableIdentifier);

                var tableColumns = await table.ColumnsAsync().ConfigureAwait(false);
                var columnNames = tableColumns.Select(c => c.Name.LocalName).ToList();
                var columnTypes = tableColumns.Select(c => c.Type.Definition).ToList();

                var primaryKey = await table.PrimaryKeyAsync().ConfigureAwait(false);
                var uniqueKeys = await table.UniqueKeysAsync().ConfigureAwait(false);
                var childKeys = await table.ChildKeysAsync().ConfigureAwait(false);
                var parentKeys = await table.ParentKeysAsync().ConfigureAwait(false);

                var keyColumnNames = uniqueKeys
                    .Concat(parentKeys.Select(fk => fk.ChildKey))
                    .Concat(primaryKey != null ? new[] { primaryKey } : Enumerable.Empty<IDatabaseKey>())
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
                    tableNodeOptions
                );
                tableNodes[nodeIdentifier] = tableNode;

                foreach (var relationalKey in parentKeys)
                {
                    var childKey = relationalKey.ChildKey;
                    var parentKey = relationalKey.ParentKey;

                    var hasParentKey = tableNames.Contains(parentKey.Table.Name);
                    if (!hasParentKey)
                        continue;

                    var childKeyIdentifierText = childKey.Table.Name.ToSafeKey() + ".";
                    childKeyIdentifierText += childKey.Name == null
                        ? childKey.GetKeyHash().ToString()
                        : childKey.Name.LocalName;
                    var childKeyIdentifier = new DotIdentifier(childKeyIdentifierText);

                    var parentKeyIdentifierText = parentKey.Table.Name.ToSafeKey() + ".";
                    parentKeyIdentifierText += parentKey.Name == null
                        ? parentKey.GetKeyHash().ToString()
                        : parentKey.Name.LocalName;
                    var parentKeyIdentifier = new DotIdentifier(parentKeyIdentifierText);

                    if (!relationalKeyNodes.ContainsKey(childKeyIdentifier))
                    {
                        var childTableName = childKey.Table.Name.ToVisibleName();
                        var constraintName = childKey.Name == null
                            ? "(Unnamed)"
                            : childKey.Name.LocalName;
                        var childKeyNodeAttrs = new[]
                        {
                            NodeAttribute.Tooltip(childTableName + "." + constraintName),
                            NodeAttribute.URL(tableUri)
                        };

                        var childKeyNode = new ConstraintNode(
                            childKeyIdentifier,
                            childKey.KeyType,
                            constraintName,
                            childKey.Columns.Select(c => c.Name.LocalName).ToList(),
                            childKey.Columns.Select(c => c.Type.Definition).ToList(),
                            childKeyNodeAttrs,
                            ConstraintNodeOptions.GetDefaultOptions(childKey.KeyType)
                        );
                        relationalKeyNodes[childKeyIdentifier] = childKeyNode;

                        var edgeStyles = new[]
                        {
                            EdgeAttribute.ArrowHead(ArrowStyleName.Dot),
                            EdgeAttribute.ArrowTail(ArrowStyleName.Dot),
                            EdgeAttribute.Direction(EdgeDirection.Both)
                        };

                        var childKeyTableIdentifier = new DotIdentifier(childKey.Table.Name.ToSafeKey());
                        foreach (var column in childKey.Columns)
                        {
                            var columnIdentifier = new DotIdentifier(column.Name.LocalName);
                            var columnEdge = new DotEdge(
                                childKeyTableIdentifier,
                                columnIdentifier,
                                childKeyIdentifier,
                                columnIdentifier,
                                edgeStyles
                            );
                            edges.Add(columnEdge);
                        }
                    }

                    if (!relationalKeyNodes.ContainsKey(parentKeyIdentifier))
                    {
                        var parentTableName = parentKey.Table.Name.ToVisibleName();
                        var constraintName = parentKey.Name == null
                            ? "(Unnamed)"
                            : parentKey.Name.LocalName;
                        var parentTableUri = new Uri(options.RootPath + "tables/" + parentKey.Table.Name.ToSafeKey() + ".html", UriKind.Relative);
                        var parentKeyNodeAttrs = new[]
                        {
                            NodeAttribute.Tooltip(parentTableName + "." + constraintName),
                            NodeAttribute.URL(parentTableUri)
                        };

                        var parentKeyNode = new ConstraintNode(
                            parentKeyIdentifier,
                            parentKey.KeyType,
                            constraintName,
                            parentKey.Columns.Select(c => c.Name.LocalName).ToList(),
                            parentKey.Columns.Select(c => c.Type.Definition).ToList(),
                            parentKeyNodeAttrs,
                            ConstraintNodeOptions.GetDefaultOptions(parentKey.KeyType)
                        );
                        relationalKeyNodes[parentKeyIdentifier] = parentKeyNode;

                        var edgeStyles = new[]
                        {
                            EdgeAttribute.ArrowHead(ArrowStyleName.Dot),
                            EdgeAttribute.ArrowTail(ArrowStyleName.Dot),
                            EdgeAttribute.Direction(EdgeDirection.Both)
                        };

                        var parentKeyTableIdentifier = new DotIdentifier(parentKey.Table.Name.ToSafeKey());
                        foreach (var column in parentKey.Columns)
                        {
                            var columnIdentifier = new DotIdentifier(column.Name.LocalName);
                            var columnEdge = new DotEdge(
                                parentKeyTableIdentifier,
                                columnIdentifier,
                                parentKeyIdentifier,
                                columnIdentifier,
                                edgeStyles
                            );
                            edges.Add(columnEdge);
                        }
                    }

                    var childKeyToParentKeyEdge = new DotEdge(
                        childKeyIdentifier,
                        parentKeyIdentifier,
                        Enumerable.Empty<EdgeAttribute>()
                    );
                    edges.Add(childKeyToParentKeyEdge);
                }
            }

            var recordNodes = tableNodes.Values
                .Concat(relationalKeyNodes.Values)
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
