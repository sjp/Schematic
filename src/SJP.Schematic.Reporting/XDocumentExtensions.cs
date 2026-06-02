using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using SJP.Schematic.Dot;
using SJP.Schematic.Dot.Themes;

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
    /// Injects a <c>&lt;style&gt;</c> block that recolours the diagram from the default light theme to its
    /// dark counterpart under the OS dark-mode preference. Equivalent to calling
    /// <see cref="AddDarkModeStyles(XDocument, IGraphTheme, IGraphTheme)"/> with
    /// <see cref="GraphThemes.Default"/> and <see cref="GraphThemes.Dark"/>.
    /// </summary>
    public static void AddDarkModeStyles(this XDocument document)
        => document.AddDarkModeStyles(GraphThemes.Default, GraphThemes.Dark);

    /// <summary>
    /// Injects a <c>&lt;style&gt;</c> block that recolours the diagram from <paramref name="lightTheme"/> to
    /// <paramref name="darkTheme"/> when the OS reports a dark colour scheme. Graphviz bakes the theme
    /// colours in as inline <c>fill</c>/<c>stroke</c> presentation attributes, and because the SVG is embedded
    /// via an <c>&lt;object&gt;</c> it forms its own document — the surrounding app's <c>.dark</c> class can't
    /// reach inside it. So we lean on the same signal the app's colour-scheme hook uses, the OS
    /// <c>prefers-color-scheme</c>, via a media query; the diagram then flips in lockstep with the rest of the
    /// UI. CSS rules outrank presentation attributes in the cascade, so the overlay recolours without touching
    /// individual elements.
    /// </summary>
    /// <param name="document">The rendered Graphviz SVG.</param>
    /// <param name="lightTheme">The theme the SVG was rendered with — its colours are the ones to override.</param>
    /// <param name="darkTheme">The theme to switch to under a dark colour scheme.</param>
    public static void AddDarkModeStyles(this XDocument document, IGraphTheme lightTheme, IGraphTheme darkTheme)
    {
        ArgumentNullException.ThrowIfNull(document);
        ArgumentNullException.ThrowIfNull(lightTheme);
        ArgumentNullException.ThrowIfNull(darkTheme);

        var root = document.Root;
        if (root == null)
            return;

        root.AddFirst(new XElement(StyleElement, new XCData(BuildDarkModeCss(lightTheme, darkTheme))));
    }

    /// <summary>
    /// Builds the dark-mode override stylesheet by pairing the light theme's colours with the dark theme's.
    /// </summary>
    /// <remarks>
    /// The recolouring is driven entirely by the two themes: every colour the light theme paints into the SVG
    /// is matched by its actual value and remapped to the dark theme's equivalent, which is what keeps this
    /// general rather than tied to one palette. Backgrounds (and edge arrowheads, which Graphviz fills with the
    /// edge colour) are matched on <c>fill</c>; cell borders and edge lines are matched on <c>stroke</c>. Text
    /// carries no <c>fill</c> attribute (it defaults to black) so it is lightened with a bare <c>text</c> rule.
    /// Attribute matching is case-insensitive because Graphviz lower-cases hex in its output whereas
    /// <see cref="RgbColor"/> emits upper-case.
    /// </remarks>
    private static string BuildDarkModeCss(IGraphTheme light, IGraphTheme dark)
    {
        // Colours Graphviz emits as `fill`: every background, plus the edge colour (arrowheads are filled).
        var fillPairs = new[]
        {
            (light.BackgroundColor, dark.BackgroundColor),
            (light.TableBackgroundColor, dark.TableBackgroundColor),
            (light.HeaderBackgroundColor, dark.HeaderBackgroundColor),
            (light.FooterBackgroundColor, dark.FooterBackgroundColor),
            (light.PrimaryKeyHeaderBackgroundColor, dark.PrimaryKeyHeaderBackgroundColor),
            (light.UniqueKeyHeaderBackgroundColor, dark.UniqueKeyHeaderBackgroundColor),
            (light.ForeignKeyHeaderBackgroundColor, dark.ForeignKeyHeaderBackgroundColor),
            (light.HighlightedTableBackgroundColor, dark.HighlightedTableBackgroundColor),
            (light.HighlightedHeaderBackgroundColor, dark.HighlightedHeaderBackgroundColor),
            (light.HighlightedFooterBackgroundColor, dark.HighlightedFooterBackgroundColor),
            (light.HighlightedPrimaryKeyHeaderBackgroundColor, dark.HighlightedPrimaryKeyHeaderBackgroundColor),
            (light.HighlightedUniqueKeyHeaderBackgroundColor, dark.HighlightedUniqueKeyHeaderBackgroundColor),
            (light.HighlightedForeignKeyHeaderBackgroundColor, dark.HighlightedForeignKeyHeaderBackgroundColor),
            (light.EdgeColor, dark.EdgeColor),
        };

        // Colours Graphviz emits as `stroke`: the table border colours applied to cells, plus the edge lines.
        var strokePairs = new[]
        {
            (light.TableBorderColor, dark.TableBorderColor),
            (light.HighlightedTableBorderColor, dark.HighlightedTableBorderColor),
            (light.EdgeColor, dark.EdgeColor),
        };

        var builder = new StringBuilder();
        builder.AppendLine("@media (prefers-color-scheme: dark) {");
        builder.AppendLine(CultureInfo.InvariantCulture, $"  text {{ fill: {dark.TableForegroundColor}; }}");
        AppendColorRules(builder, "fill", fillPairs);
        AppendColorRules(builder, "stroke", strokePairs);
        builder.Append('}');
        return builder.ToString();
    }

    /// <summary>
    /// Appends one <c>[property="from" i] { property: to; }</c> rule per colour pair, de-duplicating by source
    /// colour. Several theme slots commonly share a colour (header == footer; the graph, table and column
    /// backgrounds are often identical), and the same source must not be emitted twice.
    /// </summary>
    private static void AppendColorRules(StringBuilder builder, string property, IEnumerable<(RgbColor From, RgbColor To)> pairs)
    {
        var mappedSources = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var (from, to) in pairs)
        {
            var source = from.ToString();
            if (!mappedSources.Add(source))
                continue;

            builder.AppendLine(CultureInfo.InvariantCulture, $"  [{property}=\"{source}\" i] {{ {property}: {to}; }}");
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
    private static readonly XName StyleElement = SvgNamespace + "style";
    private static readonly XName XlinkHrefAttribute = XlinkNamespace + "href";
    private static readonly XName HrefAttribute = "href";
    private static readonly XName TargetAttribute = "target";
}