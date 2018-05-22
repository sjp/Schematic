using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;

namespace SJP.Schematic.SchemaSpy.Dot
{
    public interface IDotFormatter
    {

    }

    public class DatabaseDotFormatter : IDotFormatter
    {
        public DatabaseDotFormatter(IDbConnection connection, IRelationalDatabase database)
        {
            Connection = connection ?? throw new ArgumentNullException(nameof(connection));
            Database = database ?? throw new ArgumentNullException(nameof(database));
            Config = new FormatOptions();
        }

        protected IDbConnection Connection { get; }

        protected IRelationalDatabase Database { get; }

        protected FormatOptions Config { get; }

        private string GetHeader()
        {
            var dot = new StringBuilder();

            var dbName = Database.DatabaseName ?? string.Empty;
            var fileVersion = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).FileVersion;

            dot.AppendLine("// Schematic version " + fileVersion);
            dot.AppendLine("digraph \"" + dbName + "\" {");
            dot.AppendLine("  graph [");
            dot.AppendLine("    rankdir=\"RL\"");
            dot.AppendLine("    bgcolor=\"" + Config.BodyBackgroundColor.ToRgbHex() + "\"");
            dot.AppendLine("    ratio=\"compress\"");
            dot.AppendLine("  ];");
            dot.AppendLine("  node [");
            dot.AppendLine("    fontname=\"Courier\"");
            dot.AppendLine("    shape=\"none\"");
            dot.AppendLine("  ];");
            dot.AppendLine("  edge [");
            dot.AppendLine("    arrowsize=\"0.8\"");
            dot.AppendLine("    arrowhead=\"open\"");
            dot.AppendLine("  ];");

            return dot.ToString();
        }

        public string RenderAllTables()
        {
            var header = GetHeader();

            var tables = Database.Tables.ToList();
            var renderedTables = tables.Select(TableToDotString).ToList();

            var primaryKeys = tables.SelectNotNull(t => t.PrimaryKey).ToList();
            var uniqueKeys = tables.SelectMany(t => t.UniqueKeys).ToList();

            var parentKeys = tables.SelectMany(t => t.ParentKeys).ToList();

            var tableConstraintNames = new Dictionary<Identifier, HashSet<string>>(); // stores known constraint names

            var idUniqueKeyNodes = new List<DatabaseKeyWithIdentifier>();
            var namedUniqueConstraints = primaryKeys.Concat(uniqueKeys)
                .Where(uk => uk.Name != null)
                .ToList();
            foreach (var namedUniqueConstraint in namedUniqueConstraints)
            {
                if (!tableConstraintNames.ContainsKey(namedUniqueConstraint.Table.Name))
                    tableConstraintNames[namedUniqueConstraint.Table.Name] = new HashSet<string>();

                tableConstraintNames[namedUniqueConstraint.Table.Name].Add(namedUniqueConstraint.Name.LocalName);
                var idUniqueConstraint = new DatabaseKeyWithIdentifier(namedUniqueConstraint);
                idUniqueKeyNodes.Add(idUniqueConstraint);
            }

            var unnamedUniqueConstraints = primaryKeys.Concat(uniqueKeys)
                .Where(uk => uk.Name == null)
                .ToList();
            foreach (var unnamedUniqueConstraint in unnamedUniqueConstraints)
            {
                if (!tableConstraintNames.ContainsKey(unnamedUniqueConstraint.Table.Name))
                    tableConstraintNames[unnamedUniqueConstraint.Table.Name] = new HashSet<string>();

                var constraints = tableConstraintNames[unnamedUniqueConstraint.Table.Name];
                var i = 1;
                string candidateIdentifier;
                do
                {
                    candidateIdentifier = "unnamed-" + i.ToString();
                    i++;
                } while (constraints.Contains(candidateIdentifier));

                var idUniqueConstraint = new DatabaseKeyWithIdentifier(unnamedUniqueConstraint, candidateIdentifier);
                idUniqueKeyNodes.Add(idUniqueConstraint);

                tableConstraintNames[unnamedUniqueConstraint.Table.Name].Add(candidateIdentifier);
            }

            var idForeignKeys = new List<DatabaseRelationalKeyWithIdentifier>();

            var namedForeignKeys = parentKeys.Where(fk => fk.ChildKey.Name != null).ToList();
            foreach (var namedForeignKey in namedForeignKeys)
            {
                if (!tableConstraintNames.ContainsKey(namedForeignKey.ChildKey.Table.Name))
                    tableConstraintNames[namedForeignKey.ChildKey.Table.Name] = new HashSet<string>();

                tableConstraintNames[namedForeignKey.ChildKey.Table.Name].Add(namedForeignKey.ChildKey.Name.LocalName);

                var parentKeyHash = namedForeignKey.ParentKey.GetKeyHash();
                var matchingIdParentKey = idUniqueKeyNodes.Find(uk => uk.HashCode == parentKeyHash);
                if (matchingIdParentKey == null)
                    throw new Exception("Could not find matching parent key for child keys");

                var idChildKey = new DatabaseKeyWithIdentifier(namedForeignKey.ChildKey);
                var idRelationalKey = new DatabaseRelationalKeyWithIdentifier(idChildKey, matchingIdParentKey);
                idForeignKeys.Add(idRelationalKey);
            }

            var unnamedForeignKeys = parentKeys.Where(fk => fk.ChildKey.Name == null).ToList();
            foreach (var unnamedForeignKey in unnamedForeignKeys)
            {
                if (!tableConstraintNames.ContainsKey(unnamedForeignKey.ChildKey.Table.Name))
                    tableConstraintNames[unnamedForeignKey.ChildKey.Table.Name] = new HashSet<string>();

                var constraints = tableConstraintNames[unnamedForeignKey.ChildKey.Table.Name];
                var i = 1;
                string candidateIdentifier;
                do
                {
                    candidateIdentifier = "unnamed-" + i.ToString();
                    i++;
                } while (constraints.Contains(candidateIdentifier));

                var idChildKey = new DatabaseKeyWithIdentifier(unnamedForeignKey.ChildKey, candidateIdentifier);

                tableConstraintNames[unnamedForeignKey.ChildKey.Table.Name].Add(candidateIdentifier);

                var parentKeyHash = unnamedForeignKey.ParentKey.GetKeyHash();
                var matchingIdParentKey = idUniqueKeyNodes.Find(uk => uk.HashCode == parentKeyHash);
                if (matchingIdParentKey == null)
                    throw new Exception("Could not find matching parent key for child keys");

                var idRelationalKey = new DatabaseRelationalKeyWithIdentifier(idChildKey, matchingIdParentKey);
                idForeignKeys.Add(idRelationalKey);
            }

            var keyNodes = new Dictionary<string, string>();
            var keyEdges = new Dictionary<string, string>();
            var columnKeyConnectors = new HashSet<string>();
            foreach (var relation in idForeignKeys)
            {
                var hasChildKey = keyNodes.ContainsKey(relation.ChildKey.Identifier);
                if (!hasChildKey)
                    keyNodes[relation.ChildKey.Identifier] = CreateRelationNode(relation.ChildKey);

                var hasParentKey = keyNodes.ContainsKey(relation.ParentKey.Identifier);
                if (!hasParentKey)
                    keyNodes[relation.ParentKey.Identifier] = CreateRelationNode(relation.ParentKey);

                var edgeIdentifier = relation.ChildKey.Identifier + relation.ParentKey.Identifier; // guids so will be unique
                if (!keyEdges.ContainsKey(edgeIdentifier))
                    keyEdges[edgeIdentifier] = CreateRelationEdge(relation);

                var childKeyColumnConnectors = CreateColumnToChildKeyConnectors(relation.ChildKey);
                foreach (var childKeyConnector in childKeyColumnConnectors)
                    columnKeyConnectors.Add(childKeyConnector);

                var parentKeyColumnConnectors = CreateParentKeyToColumnConnectors(relation.ParentKey);
                foreach (var parentKeyConnector in parentKeyColumnConnectors)
                    columnKeyConnectors.Add(parentKeyConnector);
            }

            const string footer = "}";

            return header
                + renderedTables.Join(string.Empty)
                + keyNodes.Values.Join(Environment.NewLine)
                + keyEdges.Values.Join(Environment.NewLine)
                + columnKeyConnectors.Join(Environment.NewLine)
                + footer;
        }

        protected class DatabaseRelationalKeyWithIdentifier
        {
            public DatabaseRelationalKeyWithIdentifier(DatabaseKeyWithIdentifier childKey, DatabaseKeyWithIdentifier parentKey)
            {
                ChildKey = childKey ?? throw new ArgumentNullException(nameof(childKey));
                ParentKey = parentKey ?? throw new ArgumentNullException(nameof(parentKey));
            }

            public DatabaseKeyWithIdentifier ChildKey { get; }

            public DatabaseKeyWithIdentifier ParentKey { get; }
        }

        protected class DatabaseKeyWithIdentifier
        {
            public DatabaseKeyWithIdentifier(IDatabaseKey key)
            {
                Key = key ?? throw new ArgumentNullException(nameof(key));
                var keyName = key.Name?.LocalName;
                if (keyName.IsNullOrWhiteSpace())
                    throw new ArgumentException("The given key does not have a name. Use the other constructor when the key is unnamed.", nameof(key));

                Label = keyName;
                var tableName = key.Table.Name.ToVisibleName();
                Identifier = tableName + "." + keyName;

                HashCode = key.GetKeyHash();
            }

            public DatabaseKeyWithIdentifier(IDatabaseKey key, string keyIdentifier)
            {
                Key = key ?? throw new ArgumentNullException(nameof(key));
                if (keyIdentifier.IsNullOrWhiteSpace())
                    throw new ArgumentNullException(nameof(keyIdentifier));

                var tableName = key.Table.Name.ToVisibleName();
                Identifier = tableName + "." + keyIdentifier;

                HashCode = key.GetKeyHash();
                Label = "(Unnamed)";
            }

            public IDatabaseKey Key { get; }

            public string Identifier { get; }

            public int HashCode { get; }

            public string Label { get; }
        }

        private string CreateRelationNode(DatabaseKeyWithIdentifier key)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));

            var builder = new StringBuilder();

            builder.Append("\"");
            builder.Append(key.Identifier);
            builder.AppendLine("\" [");
            if (key.Label.IsNullOrWhiteSpace())
            {
                builder.Append("label = \"");
                builder.Append(key.Label.Replace("\"", "\"\""));
                builder.AppendLine("\"");
            }
            else
            {
                var borderSize = Config.ShowColumnDataType ? 2 : 0;
                var borderSizeText = borderSize.ToString();

                string bgColor;
                if (key.Key.KeyType == DatabaseKeyType.Primary)
                    bgColor = Config.PrimaryKeyColor.ToRgbHex();
                else if (key.Key.KeyType == DatabaseKeyType.Unique)
                    bgColor = Config.UniqueKeyColor.ToRgbHex();
                else if (key.Key.KeyType == DatabaseKeyType.Foreign)
                    bgColor = Config.ForeignKeyColor.ToRgbHex();
                else
                    bgColor = Config.TableHeaderBackgroundColor.ToRgbHex();

                builder.Append("   label=<");
                builder.AppendLine("<TABLE BORDER=\"" + borderSizeText + "\" CELLBORDER=\"1\" CELLSPACING=\"0\" BGCOLOR=\"" + Config.TableBackgroundColor.ToRgbHex() + "\">");
                builder.AppendLine("<TR>");
                builder.AppendLine("<TD BGCOLOR=\"" + bgColor + "\">");
                builder.AppendLine("<FONT FACE=\"Helvetica\">");
                builder.Append("<B>");
                builder.Append(key.Key.KeyType.ToString() + " Key");
                builder.Append("</B>");
                builder.AppendLine("</FONT>");
                builder.AppendLine("</TD>");
                builder.AppendLine("</TR>");
                builder.AppendLine("      <TR>");
                builder.AppendLine("<TD BGCOLOR=\"" + bgColor + "\">");
                builder.AppendLine("<B>" + key.Key.Name.LocalName + "</B>");
                builder.AppendLine("</TD>");
                builder.AppendLine("</TR>");

                var columns = key.Key.Columns.ToList();
                foreach (var column in columns)
                {
                    var columnName = column.Name.LocalName;

                    builder.AppendLine("      <TR>");
                    builder.AppendLine("<TD PORT=\"" + columnName + "\" ");
                    builder.AppendLine("ALIGN=\"LEFT\">");
                    builder.AppendLine(columnName);
                    builder.AppendLine("</TD>");
                    builder.AppendLine("</TR>");
                }

                builder.AppendLine("</TABLE>>");
            }

            var tableIdentifier = key.Key.Table.Name.ToSafeKey();
            builder.AppendLine("    URL=\"tables/" + tableIdentifier + ".html\"");
            builder.AppendLine("    tooltip=\"" + key.Identifier + "\"");
            builder.AppendLine();

            builder.AppendLine("];");

            return builder.ToString();
        }

        private static string CreateRelationEdge(DatabaseRelationalKeyWithIdentifier relation)
        {
            if (relation == null)
                throw new ArgumentNullException(nameof(relation));

            var builder = new StringBuilder();

            builder.Append("\"");
            builder.Append(relation.ChildKey.Identifier);
            builder.Append("\" -> \"");
            builder.Append(relation.ParentKey.Identifier);
            builder.AppendLine("\";");

            return builder.ToString();
        }

        private static IEnumerable<string> CreateColumnToChildKeyConnectors(DatabaseKeyWithIdentifier childKey)
        {
            if (childKey == null)
                throw new ArgumentNullException(nameof(childKey));

            var result = new List<string>();

            var tableName = childKey.Key.Table.Name.ToSafeKey();
            var tableIdPrefix = "\"" + tableName + "\":";
            var keyIdPrefix = "\"" + childKey.Identifier + "\":";

            foreach (var column in childKey.Key.Columns)
            {
                var columnName = column.Name.LocalName;
                var columnId = "\"" + columnName + "\"";
                var tablePortId = tableIdPrefix + columnId;
                var keyPortId = keyIdPrefix + columnId;

                const string styles = "[arrowhead=dot, arrowtail=dot, dir=both]";

                var edge = tablePortId + " -> " + keyPortId + " " + styles + ";";
                result.Add(edge);
            }

            return result;
        }

        private static IEnumerable<string> CreateParentKeyToColumnConnectors(DatabaseKeyWithIdentifier parentKey)
        {
            if (parentKey == null)
                throw new ArgumentNullException(nameof(parentKey));

            var result = new List<string>();

            var tableName = parentKey.Key.Table.Name.ToSafeKey();
            var tableIdPrefix = "\"" + tableName + "\":";
            var keyIdPrefix = "\"" + parentKey.Identifier + "\":";

            foreach (var column in parentKey.Key.Columns)
            {
                var columnName = column.Name.LocalName;
                var columnId = "\"" + columnName + "\"";
                var tablePortId = tableIdPrefix + columnId;
                var keyPortId = keyIdPrefix + columnId;

                const string styles = "[arrowhead=dot, arrowtail=dot, dir=both]";

                var edge = keyPortId + " -> " + tablePortId + " " + styles + ";";
                result.Add(edge);
            }

            return result;
        }

        public string TableToDotString(IRelationalDatabaseTable table)
        {
            if (table == null)
                throw new ArgumentNullException(nameof(table));

            var builder = new StringBuilder();

            var tableIdentifier = table.Name.ToSafeKey();
            var tableName = table.Name.ToVisibleName();

            var colSpanWidth = Config.ShowColumnDataType ? 2 : 3;
            var colSpanHeaderWidth = Config.ShowColumnDataType ? 4 : 3;

            var colspan = "COLSPAN=\"" + colSpanWidth.ToString() + "\" ";
            var colspanHeader = "COLSPAN=\"" + colSpanHeaderWidth.ToString() + "\" ";

            var borderSize = Config.ShowColumnDataType ? 2 : 0;
            var borderSizeText = borderSize.ToString();

            builder.AppendLine("  \"" + tableIdentifier + "\" [");
            builder.Append("   label=<");
            builder.AppendLine("<TABLE BORDER=\"" + borderSizeText + "\" CELLBORDER=\"1\" CELLSPACING=\"0\" BGCOLOR=\"" + Config.TableBackgroundColor.ToRgbHex() + "\">");
            builder.AppendLine("      <TR>");
            builder.AppendLine("<TD " + colspanHeader + " BGCOLOR=\"" + Config.TableHeaderBackgroundColor.ToRgbHex() + "\">");
            builder.AppendLine("<FONT FACE=\"Helvetica\">");
            builder.AppendLine("<B>Table</B>");
            builder.AppendLine("</FONT>");
            builder.AppendLine("</TD>");
            builder.AppendLine("</TR>");
            builder.AppendLine("      <TR>");
            builder.AppendLine("<TD " + colspanHeader + " BGCOLOR=\"" + Config.TableHeaderBackgroundColor.ToRgbHex() + "\">");
            builder.AppendLine("<B>" + tableName + "</B>");
            builder.AppendLine("</TD>");
            builder.AppendLine("</TR>");

            var primaryKey = table.PrimaryKey;
            var uniqueKeys = table.UniqueKeys.ToList();
            var foreignKeys = table.ParentKeys.ToList();
            var childKeys = table.ChildKeys.ToList();
            var rowCount = Connection.GetRowCount(Database.Dialect, table.Name);

            var hasSkippedColumns = false;

            var columns = table.Columns.ToList();
            foreach (var column in columns)
            {
                var columnName = column.Name.LocalName;

                var isPrimaryKey = primaryKey != null && primaryKey.Columns.Any(c => c.Name.LocalName == columnName);
                var isUniqueKey = uniqueKeys.Any(uk => uk.Columns.Any(ukc => ukc.Name.LocalName == columnName));
                var isForeignKey = foreignKeys.Any(fk => fk.ChildKey.Columns.Any(fkc => fkc.Name.LocalName == columnName));

                var showColumn = Config.ShowAllColumns || isPrimaryKey || isUniqueKey || isForeignKey;
                if (!showColumn)
                {
                    hasSkippedColumns = true;
                    continue;
                }

                builder.AppendLine("      <TR>");
                builder.AppendLine("<TD PORT=\"" + columnName + "\" " + colspan);
                builder.AppendLine("ALIGN=\"LEFT\">");
                builder.AppendLine("<TABLE BORDER=\"0\" CELLSPACING=\"0\" ALIGN=\"LEFT\">");
                builder.AppendLine("<TR ALIGN=\"LEFT\">");

                builder.AppendLine("<TD ALIGN=\"LEFT\">");
                builder.AppendLine(columnName);
                builder.AppendLine("</TD>");
                builder.AppendLine("</TR>");
                builder.AppendLine("</TABLE>");
                builder.AppendLine("</TD>");

                if (Config.ShowColumnDataType)
                {
                    builder.AppendLine("<TD PORT=\"");
                    builder.AppendLine(columnName);
                    builder.AppendLine(".type\" ALIGN=\"LEFT\">");
                    builder.AppendLine(column.Type.Definition);
                    builder.AppendLine("</TD>");
                }
                builder.AppendLine("</TR>");
            }

            if (hasSkippedColumns)
                builder.AppendLine("      <TR><TD PORT=\"elipses\" COLSPAN=\"3\" ALIGN=\"LEFT\">…</TD></TR>");

            builder.AppendLine("      <TR>");
            builder.AppendLine("<TD ALIGN=\"LEFT\" BGCOLOR=\"" + Config.TableHeaderBackgroundColor.ToRgbHex() + "\">");
            builder.AppendLine("<FONT FACE=\"Helvetica\">");
            var parentText = foreignKeys.Count > 0
                ? "&lt; " + foreignKeys.Count
                : "  ";

            builder.AppendLine(parentText);

            builder.AppendLine("</FONT></TD>");
            builder.AppendLine("<TD ALIGN=\"RIGHT\" BGCOLOR=\"" + Config.TableHeaderBackgroundColor.ToRgbHex() + "\">");
            builder.AppendLine("<FONT FACE=\"Helvetica\">");

            builder.Append(rowCount);
            builder.Append(" row");
            if (rowCount != 1)
                builder.AppendLine("s");

            builder.AppendLine("</FONT></TD>");

            builder.AppendLine("<TD ALIGN=\"RIGHT\" BGCOLOR=\"" + Config.TableHeaderBackgroundColor.ToRgbHex() + "\">");
            builder.AppendLine("<FONT FACE=\"Helvetica\">");

            var childKeyText = childKeys.Count > 0
                ? childKeys.Count + " &gt;"
                : "  ";
            builder.AppendLine(childKeyText);

            builder.AppendLine("</FONT></TD></TR>");

            builder.AppendLine("    </TABLE>>");
            builder.AppendLine("    URL=\"tables/" + tableIdentifier + ".html\"");
            builder.AppendLine("    tooltip=\"" + tableName + "\"");
            builder.AppendLine("  ];");

            return builder.ToString();
        }

        public class FormatOptions
        {
            public string RootPath { get; set; } = "../";

            public bool ShowAllColumns { get; set; } = true;

            public bool ShowColumnDataType { get; set; }

            public Color BodyBackgroundColor { get; set; } = Color.White;

            public Color TableBackgroundColor { get; set; } = Color.White;

            public Color TableHeaderBackgroundColor { get; set; } = ColorTranslator.FromHex("#BFE3C6");

            public Color PrimaryKeyColor { get; set; } = ColorTranslator.FromHex("#EFEBA8");

            public Color ForeignKeyColor { get; set; } = ColorTranslator.FromHex("#E5E5E5");

            public Color UniqueKeyColor { get; set; } = ColorTranslator.FromHex("#B8D0DD");
        }
    }

    public class DotFormatter : IDotFormatter
    {
        protected string ToDot(IDotNode node)
        {
            if (node == null)
                throw new ArgumentNullException(nameof(node));

            return string.Empty;
        }

        protected string ToDot(IDotConnector connector)
        {
            if (connector == null)
                throw new ArgumentNullException(nameof(connector));

            var edge = new StringBuilder();

            var childTableIdentifier = ToSafeDotIdentifier(connector.Key.ChildKey.Table.Name);
            var parentTableIdentifier = ToSafeDotIdentifier(connector.Key.ParentKey.Table.Name);

            edge.Append(childTableIdentifier);
            edge.Append(":w -> ");
            edge.Append(parentTableIdentifier);
            edge.Append(":e ");

            // parent end of connector
            edge.Append("[arrowhead=none");
            edge.Append(" dir=back]");

            return edge.ToString();
        }

        protected static string ToSafeDotIdentifier(Identifier identifier)
        {
            if (identifier == null)
                throw new ArgumentNullException(nameof(identifier));

            var builder = new StringBuilder("\"");

            if (identifier.Server != null)
            {
                builder.Append(ToSafeDotIdentifier(identifier.Server));
                builder.Append(".");
            }

            if (identifier.Database != null)
            {
                builder.Append(ToSafeDotIdentifier(identifier.Database));
                builder.Append(".");
            }

            if (identifier.Schema != null)
            {
                builder.Append(ToSafeDotIdentifier(identifier.Schema));
                builder.Append(".");
            }

            builder.Append(ToSafeDotIdentifier(identifier.LocalName));
            builder.Append("\"");

            return builder.ToString();
        }

        protected static string ToSafeDotIdentifier(string identifier)
        {
            if (identifier.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(identifier));

            return identifier.Replace("\"", "\\\"");
        }
    }

    // inject this into a dotnode and a dotconnector so they can write ToDot() returning a string?

}
