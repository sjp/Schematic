using System;
using System.Linq;
using System.Xml.Linq;

namespace SJP.Schematic.Reporting;

internal static class XDocumentExtensions
{
    /// <summary>
    /// Wraps each table node in the Graphviz SVG with an SVG <c>&lt;a&gt;</c> that links to the
    /// table's in-app hash route, so clicking a node navigates the SPA.
    /// </summary>
    /// <remarks>
    /// The SVG is embedded via an <c>&lt;object&gt;</c>, so links resolve against the SVG file's own
    /// URL (<c>data/diagrams/&lt;id&gt;.svg</c>) rather than the SPA. The href therefore climbs back
    /// out to the report root (<c>../../index.html</c>) and appends the hash route, and uses
    /// <c>target="_top"</c> so navigation happens in the top-level browsing context. When the SPA is
    /// already open at <c>index.html</c> this resolves to a same-document fragment change, so the hash
    /// router soft-navigates; it is also correct (if heavier) when the report is not yet loaded. This
    /// must run <em>before</em> <see cref="ReplaceTitlesWithTableNames"/>, which overwrites the node
    /// title that carries the safe-key route param.
    /// </remarks>
    public static void RewriteTableNodeUrls(this XDocument document)
    {
        ArgumentNullException.ThrowIfNull(document);

        var tableNodes = document.Descendants(GroupElement)
            .Where(g => string.Equals(g.Attribute(ClassAttribute)?.Value, NodeClass, StringComparison.Ordinal))
            .ToList();

        foreach (var tableNode in tableNodes)
        {
            // The node id Graphviz writes into <title> is the table's safe key (the route param).
            var titleNode = tableNode.Element(TitleElement);
            var safeKey = titleNode?.Value;
            if (string.IsNullOrEmpty(safeKey))
                continue;

            var href = "../../index.html#/tables/" + safeKey;

            // Graphviz already emits an <a xlink:title="…"> per node (from the tooltip attribute);
            // reuse it so we don't nest anchors. Fall back to wrapping the drawable children when no
            // anchor is present.
            var anchor = tableNode.Descendants(AnchorElement).FirstOrDefault();
            if (anchor == null)
            {
                anchor = new XElement(AnchorElement);
                var movableNodes = tableNode.Elements()
                    .Where(e => e.Name != TitleElement)
                    .ToList();
                foreach (var movable in movableNodes)
                    movable.Remove();

                anchor.Add(movableNodes);
                tableNode.Add(anchor);
            }

            anchor.SetAttributeValue(XlinkHrefAttribute, href);
            anchor.SetAttributeValue(HrefAttribute, href);
            anchor.SetAttributeValue(TargetAttribute, "_top");
        }
    }

    public static void ReplaceTitlesWithTableNames(this XDocument document)
    {
        ArgumentNullException.ThrowIfNull(document);

        var tableNodes = document.Descendants(GroupElement)
            .Where(g => string.Equals(g.Attribute(ClassAttribute)?.Value, NodeClass, StringComparison.Ordinal))
            .ToList();

        foreach (var tableNode in tableNodes)
        {
            var titleNode = tableNode.Descendants(TitleElement).FirstOrDefault();
            if (titleNode == null)
                continue;

            var foundTableTextNode = false;
            var textNodes = tableNode.Descendants(TextElement);
            foreach (var textNode in textNodes)
            {
                var value = textNode.Value;
                if (!foundTableTextNode && string.Equals(value, TableTitle, StringComparison.Ordinal))
                {
                    foundTableTextNode = true;
                    continue;
                }

                titleNode.SetValue(value);
                break;
            }
        }
    }

    private const string NodeClass = "node";
    private const string TableTitle = "Table";
    private static readonly XNamespace SvgNamespace = "http://www.w3.org/2000/svg";
    private static readonly XNamespace XlinkNamespace = "http://www.w3.org/1999/xlink";
    private static readonly XName ClassAttribute = "class";
    private static readonly XName GroupElement = SvgNamespace + "g";
    private static readonly XName TitleElement = SvgNamespace + "title";
    private static readonly XName TextElement = SvgNamespace + "text";
    private static readonly XName AnchorElement = SvgNamespace + "a";
    private static readonly XName XlinkHrefAttribute = XlinkNamespace + "href";
    private static readonly XName HrefAttribute = "href";
    private static readonly XName TargetAttribute = "target";
}