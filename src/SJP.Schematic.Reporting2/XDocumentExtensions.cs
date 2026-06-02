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

    /// <summary>
    /// Injects a <c>&lt;style&gt;</c> block that recolours the diagram for dark mode. Graphviz bakes the
    /// theme colours in as inline <c>fill</c>/<c>stroke</c> presentation attributes, and because the SVG is
    /// embedded via an <c>&lt;object&gt;</c> it forms its own document — the surrounding app's <c>.dark</c>
    /// class can't reach inside it. So we lean on the same signal the app's colour-scheme hook uses, the OS
    /// <c>prefers-color-scheme</c>, via a media query; the diagram then flips in lockstep with the rest of
    /// the UI. CSS rules outrank presentation attributes in the cascade, so each known palette colour can be
    /// remapped to a dark-friendly equivalent without touching individual elements.
    /// </summary>
    /// <remarks>
    /// Text nodes carry no <c>fill</c> attribute (they default to black), so a bare <c>text</c> rule lightens
    /// them. The colour keys mirror the default <see cref="Dot.Themes.IGraphTheme"/> palette as Graphviz emits
    /// it (lower-cased hex, plus the <c>black</c> keyword for borders and arrowheads); attribute matching is
    /// case-insensitive to stay robust if Graphviz ever changes the casing.
    /// </remarks>
    public static void AddDarkModeStyles(this XDocument document)
    {
        ArgumentNullException.ThrowIfNull(document);

        var root = document.Root;
        if (root == null)
            return;

        root.AddFirst(new XElement(StyleElement, new XCData(DarkModeCss)));
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

    // Dark-mode recolouring. Keys are the default-theme palette as Graphviz emits it; values are dark
    // equivalents chosen to mirror the app's dark surfaces (neutral backgrounds) while keeping the
    // header/key accent hues recognisable. See AddDarkModeStyles for why this is a media query.
    private const string DarkModeCss = """
        @media (prefers-color-scheme: dark) {
          text { fill: #e6e6e6; }
          [fill="#ffffff" i] { fill: #1e1e1e; }                          /* table / column / graph background */
          [stroke="black" i], [stroke="#000000" i] { stroke: #6f6f6f; }  /* borders and relationship edges */
          [fill="black" i], [fill="#000000" i] { fill: #6f6f6f; }        /* edge arrowheads */
          [fill="#bfe3c6" i] { fill: #2f4f37; }                          /* header / footer */
          [fill="#7dde90" i] { fill: #3a7d4e; }                          /* highlighted header / footer */
          [fill="#efeba8" i] { fill: #4f4a1f; }                          /* primary key header */
          [fill="#d7cd28" i] { fill: #6b6410; }                          /* highlighted primary key header */
          [fill="#b8d0dd" i] { fill: #2e4651; }                          /* unique key header */
          [fill="#8fb3c7" i] { fill: #3f6d85; }                          /* highlighted unique key header */
          [fill="#e5e5e5" i] { fill: #3a3a3a; }                          /* foreign key header */
          [fill="#b0b0b0" i] { fill: #5a5a5a; }                          /* highlighted foreign key header */
        }
        """;

    private static readonly XNamespace SvgNamespace = "http://www.w3.org/2000/svg";
    private static readonly XNamespace XlinkNamespace = "http://www.w3.org/1999/xlink";
    private static readonly XName ClassAttribute = "class";
    private static readonly XName GroupElement = SvgNamespace + "g";
    private static readonly XName TitleElement = SvgNamespace + "title";
    private static readonly XName TextElement = SvgNamespace + "text";
    private static readonly XName AnchorElement = SvgNamespace + "a";
    private static readonly XName StyleElement = SvgNamespace + "style";
    private static readonly XName XlinkHrefAttribute = XlinkNamespace + "href";
    private static readonly XName HrefAttribute = "href";
    private static readonly XName TargetAttribute = "target";
}