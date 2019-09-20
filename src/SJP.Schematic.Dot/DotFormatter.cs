using System;
using System.Collections.Generic;
using System.Linq;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Dot
{
    /// <summary>
    /// A formatter for constructing a DOT graph for tables in a relational database.
    /// </summary>
    /// <seealso cref="IDotFormatter" />
    public class DotFormatter : IDotFormatter
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DotFormatter"/> class.
        /// </summary>
        /// <param name="identifierDefaults">The identifier defaults, used when any components of identifiers are missing.</param>
        /// <exception cref="ArgumentNullException"><paramref name="identifierDefaults"/></exception>
        public DotFormatter(IIdentifierDefaults identifierDefaults)
        {
            IdentifierDefaults = identifierDefaults ?? throw new ArgumentNullException(nameof(identifierDefaults));
        }

        /// <summary>
        /// Gets the identifier defaults.
        /// </summary>
        /// <value>Identifier defaults.</value>
        protected IIdentifierDefaults IdentifierDefaults { get; }

        /// <summary>
        /// Renders the tables as a DOT graph.
        /// </summary>
        /// <param name="tables">The tables.</param>
        /// <returns>A string containing a dot representation of the table relationship graph.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="tables"/></exception>
        public string RenderTables(IEnumerable<IRelationalDatabaseTable> tables)
        {
            if (tables == null)
                throw new ArgumentNullException(nameof(tables));

            return RenderTables(tables, DotRenderOptions.Default);
        }

        /// <summary>
        /// Renders the tables as a DOT graph.
        /// </summary>
        /// <param name="tables">The tables.</param>
        /// <param name="options">Options to configure how the DOT graph is rendered.</param>
        /// <returns>A string containing a dot representation of the table relationship graph.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="tables"/> or <paramref name="options"/></exception>
        public string RenderTables(IEnumerable<IRelationalDatabaseTable> tables, DotRenderOptions options)
        {
            if (tables == null)
                throw new ArgumentNullException(nameof(tables));
            if (options == null)
                throw new ArgumentNullException(nameof(options));

            options.ShowRowCounts = false;
            return RenderTables(tables, EmptyRowCounts, options);
        }

        /// <summary>
        /// Renders the tables as a DOT graph.
        /// </summary>
        /// <param name="tables">The tables.</param>
        /// <param name="rowCounts">Row counts for each of the provided tables.</param>
        /// <returns>A string containing a dot representation of the table relationship graph.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="tables"/> or <paramref name="rowCounts"/></exception>
        public string RenderTables(IEnumerable<IRelationalDatabaseTable> tables, IReadOnlyDictionary<Identifier, ulong> rowCounts)
        {
            if (tables == null)
                throw new ArgumentNullException(nameof(tables));
            if (rowCounts == null)
                throw new ArgumentNullException(nameof(rowCounts));

            return RenderTables(tables, rowCounts, DotRenderOptions.Default);
        }

        /// <summary>
        /// Renders the tables as a DOT graph.
        /// </summary>
        /// <param name="tables">The tables.</param>
        /// <param name="rowCounts">Row counts for each of the provided tables.</param>
        /// <param name="options">Options to configure how the DOT graph is rendered.</param>
        /// <returns>A string containing a dot representation of the table relationship graph.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="tables"/> or <paramref name="rowCounts"/> or <paramref name="options"/></exception>
        public string RenderTables(IEnumerable<IRelationalDatabaseTable> tables, IReadOnlyDictionary<Identifier, ulong> rowCounts, DotRenderOptions options)
        {
            if (tables == null)
                throw new ArgumentNullException(nameof(tables));
            if (rowCounts == null)
                throw new ArgumentNullException(nameof(rowCounts));
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

                if (!rowCounts.TryGetValue(table.Name, out var rowCount))
                    rowCount = 0;

                var tableNodeAttrs = new[] { NodeAttribute.Tooltip(tableName) };
                var tableNodeOptions = new TableNodeOptions
                {
                    IsReducedColumnSet = options.IsReducedColumnSet,
                    ShowColumnDataType = options.ShowColumnDataType,
                    IsHighlighted = table.Name == options.HighlightedTable,
                    ShowRowCounts = options.ShowRowCounts
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

                tableNodes[nodeIdentifier] = new TableNode(
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
            }

            var recordNodes = tableNodes.Values
                .OrderBy(node => node.Identifier.ToString())
                .ToList();

            var graphName = !IdentifierDefaults.Database.IsNullOrWhiteSpace()
                ? IdentifierDefaults.Database
                : "unnamed graph";
            var graphAttrs = _globalGraphAttrs.ToList();
            graphAttrs.Add(GraphAttribute.BackgroundColor(options.Theme.BackgroundColor));

            var graph = new DotGraph(
                new DotIdentifier(graphName),
                graphAttrs,
                _globalNodeAttrs,
                _globalEdgeAttrs,
                recordNodes,
                edges
            );

            return graph.ToString();
        }

        private static readonly IEnumerable<GraphAttribute> _globalGraphAttrs = new[]
        {
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

        private static readonly IReadOnlyDictionary<Identifier, ulong> EmptyRowCounts = new Dictionary<Identifier, ulong>();
    }
}
