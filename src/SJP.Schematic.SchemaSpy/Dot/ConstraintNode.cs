using EnumsNET;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace SJP.Schematic.SchemaSpy.Dot
{
    internal sealed class ConstraintNode : DotNode
    {
        public ConstraintNode(
            DotIdentifier identifier,
            DatabaseKeyType keyType,
            string constraintName,
            IEnumerable<string> columnNames,
            IEnumerable<string> columnTypes,
            IEnumerable<NodeAttribute> nodeAttrs,
            ConstraintNodeOptions options
        )
            : base(identifier)
        {
            if (!keyType.IsValid())
                throw new ArgumentException($"The { nameof(DatabaseKeyType) } provided must be a valid enum.", nameof(keyType));
            _tableTitle = _keyTypeTitles[keyType];

            if (constraintName.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(constraintName));
            _constraintName = constraintName;

            _columnNames = columnNames?.ToList() ?? throw new ArgumentNullException(nameof(columnNames));
            _columnTypes = columnTypes?.ToList() ?? throw new ArgumentNullException(nameof(columnTypes));
            _nodeAttrs = nodeAttrs ?? throw new ArgumentNullException(nameof(nodeAttrs));
            _options = options ?? throw new ArgumentNullException(nameof(options));
        }

        protected override string BuildDot()
        {
            var builder = new StringBuilder();

            builder.Append(Identifier);
            builder.AppendLine(" [");

            const string indent = "  ";
            builder.Append(indent).AppendLine("label=<");

            var borderSize = _options.ShowColumnDataType ? 2 : 0;
            var borderSizeText = borderSize.ToString();

            var table = new XElement(HtmlElement.Table,
                new XAttribute(HtmlAttribute.Border, borderSizeText),
                new XAttribute(HtmlAttribute.CellBorder, 1),
                new XAttribute(HtmlAttribute.CellSpacing, 0),
                new XAttribute(HtmlAttribute.BackgroundColor, _options.TableBackgroundColor)
            );

            var tableHeaderRow = new XElement(HtmlElement.TableRow,
                new XElement(HtmlElement.TableCell,
                    new XAttribute(HtmlAttribute.BackgroundColor, _options.HeaderBackgroundColor),
                    new XElement(HtmlElement.Font,
                        new XAttribute(HtmlAttribute.FontFace, nameof(FontFace.Helvetica)),
                        new XElement(HtmlElement.Bold, _tableTitle))));
            var keyNameHeaderRow = new XElement(HtmlElement.TableRow,
                new XElement(HtmlElement.TableCell,
                    new XAttribute(HtmlAttribute.BackgroundColor, _options.HeaderBackgroundColor),
                    new XElement(HtmlElement.Bold, _constraintName)));

            table.Add(tableHeaderRow);
            table.Add(keyNameHeaderRow);

            var columnRows = _columnNames.Select((c, i) =>
            {
                var columnRow = new XElement(HtmlElement.TableRow);
                var columnNameCell = new XElement(HtmlElement.TableCell,
                    new XAttribute(HtmlAttribute.Port, c),
                    new XAttribute(HtmlAttribute.Align, "LEFT"),
                    c);

                columnRow.Add(columnNameCell);

                if (_options.ShowColumnDataType)
                {
                    var columnType = _columnTypes[i];
                    var columnTypeCell = new XElement(HtmlElement.TableCell,
                        new XAttribute(HtmlAttribute.Port, columnType),
                        new XAttribute(HtmlAttribute.Align, "LEFT"));

                    columnRow.Add(columnTypeCell);
                }

                return columnRow;
            }).ToList();

            foreach (var columnRow in columnRows)
                table.Add(columnRow);

            // do not use SaveOptions.None as indented XML causes incorrect formatting in Graphviz
            var labelContent = table.ToString(SaveOptions.DisableFormatting);

            const string labelIndent = "    ";
            builder.Append(labelIndent).AppendLine(labelContent);

            builder.Append(indent).AppendLine(">");

            foreach (var nodeAttr in _nodeAttrs)
                builder.Append(indent).AppendLine(nodeAttr.ToString());

            builder.Append("]");

            return builder.ToString();
        }

        private static readonly IReadOnlyDictionary<DatabaseKeyType, string> _keyTypeTitles = new Dictionary<DatabaseKeyType, string>
        {
            [DatabaseKeyType.Foreign] = "Foreign Key",
            [DatabaseKeyType.Unique] = "Unique Key",
            [DatabaseKeyType.Primary] = "Primary Key"
        };

        private readonly string _tableTitle;
        private readonly string _constraintName;
        private readonly IReadOnlyList<string> _columnNames;
        private readonly IReadOnlyList<string> _columnTypes;
        private readonly IEnumerable<NodeAttribute> _nodeAttrs;
        private readonly ConstraintNodeOptions _options;
    }
}
