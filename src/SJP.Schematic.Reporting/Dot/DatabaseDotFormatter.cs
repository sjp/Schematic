using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SJP.Schematic.Reporting.Dot
{
    public class DatabaseDotFormatter : IDatabaseDotFormatter
    {
        public DatabaseDotFormatter(IDbConnection connection, IDatabaseDialect dialect, IIdentifierDefaults identifierDefaults)
        {
            Connection = connection ?? throw new ArgumentNullException(nameof(connection));
            Dialect = dialect ?? throw new ArgumentNullException(nameof(dialect));
            IdentifierDefaults = identifierDefaults ?? throw new ArgumentNullException(nameof(identifierDefaults));
        }

        protected IDbConnection Connection { get; }

        protected IDatabaseDialect Dialect { get; }

        protected IIdentifierDefaults IdentifierDefaults { get; }

        public Task<string> RenderTablesAsync(IEnumerable<IRelationalDatabaseTable> tables, CancellationToken cancellationToken)
        {
            if (tables == null)
                throw new ArgumentNullException(nameof(tables));

            return RenderTablesAsync(tables, DotRenderOptions.Default, cancellationToken);
        }

        public Task<string> RenderTablesAsync(IEnumerable<IRelationalDatabaseTable> tables, DotRenderOptions options, CancellationToken cancellationToken)
        {
            if (tables == null)
                throw new ArgumentNullException(nameof(tables));
            if (options == null)
                throw new ArgumentNullException(nameof(options));

            return RenderTablesAsyncCore(tables, options, cancellationToken);
        }

        private async Task<string> RenderTablesAsyncCore(IEnumerable<IRelationalDatabaseTable> tables, DotRenderOptions options, CancellationToken cancellationToken)
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
                    .Concat(primaryKey.Match(pk => new[] { pk }, Array.Empty<IDatabaseKey>))
                    .SelectMany(key => key.Columns.Select(c => c.Name.LocalName))
                    .Distinct()
                    .ToList();

                var childKeysCount = childKeys.ToList().UCount();
                var parentKeysCount = parentKeys.ToList().UCount();
                var rowCount = await Connection.GetRowCountAsync(Dialect, table.Name, cancellationToken).ConfigureAwait(false);

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

                primaryKey.IfSome(pk =>
                {
                    var primaryKeyNameText = pk.Name.Match(
                        pkName => pkName.LocalName,
                        () => pk.GetKeyHash(table.Name).ToString()
                    );
                    var primaryKeyName = pk.Name.Match(pkName => pkName.LocalName, () => string.Empty);

                    var primaryKeyConstraint = new TableConstraint(
                        primaryKeyNameText,
                        pk.KeyType,
                        primaryKeyName,
                        pk.Columns.Select(c => c.Name.LocalName).ToList(),
                        pk.Columns.Select(c => c.Type.Definition).ToList()
                    );
                    tableConstraints.Add(primaryKeyConstraint);
                });

                foreach (var uniqueKey in uniqueKeys)
                {
                    var uniqueKeyNameText = uniqueKey.Name.Match(
                        ukName => ukName.LocalName,
                        () => uniqueKey.GetKeyHash(table.Name).ToString()
                    );
                    var uniqueKeyName = uniqueKey.Name.Match(pkName => pkName.LocalName, () => string.Empty);

                    var uniqueKeyConstraint = new TableConstraint(
                        uniqueKeyNameText,
                        uniqueKey.KeyType,
                        uniqueKeyName,
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
                    var childKeyName = childKey.Name.Match(
                        ckName => ckName.LocalName,
                        () => childKey.GetKeyHash(relationalKey.ChildTable).ToString()
                    );

                    var parentKeyTableName = relationalKey.ParentTable.ToSafeKey();
                    var parentKeyName = parentKey.Name.Match(
                        fkName => fkName.LocalName,
                        () => parentKey.GetKeyHash(relationalKey.ParentTable).ToString()
                    );

                    var childKeyToParentKeyEdge = new DotEdge(
                        new DotIdentifier(childKeyTableName),
                        new DotIdentifier(childKeyName),
                        new DotIdentifier(parentKeyTableName),
                        new DotIdentifier(parentKeyName),
                        Array.Empty<EdgeAttribute>()
                    );
                    edges.Add(childKeyToParentKeyEdge);

                    var childKeyConstraintName = childKey.Name.Match(name => name.LocalName, () => string.Empty);
                    var tableConstraint = new TableConstraint(
                        childKeyName,
                        childKey.KeyType,
                        childKeyConstraintName,
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

            var graphName = !IdentifierDefaults.Database.IsNullOrWhiteSpace()
                ? IdentifierDefaults.Database
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

        private static readonly IEnumerable<GraphAttribute> _globalGraphAttrs = new[]
        {
            GraphAttribute.BackgroundColor(new RgbColor("#FFFFFF")),
            GraphAttribute.RankDirection(RankDirection.RL),
            GraphAttribute.Ratio(GraphRatio.Compress)
        };

        private static readonly IEnumerable<NodeAttribute> _globalNodeAttrs = new[]
        {
            NodeAttribute.FontFace(FontFace.Courier),
            NodeAttribute.EmptyNodeShape()
        };

        private static readonly IEnumerable<EdgeAttribute> _globalEdgeAttrs = new[]
        {
            EdgeAttribute.ArrowHead(ArrowStyleName.Open)
        };
    }
}
