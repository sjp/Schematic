using System;
using System.Linq;
using System.Xml.Linq;

namespace SJP.Schematic.Reporting
{
    internal static class XDocumentExtensions
    {
        public static void ReplaceTitlesWithTableNames(this XDocument document)
        {
            if (document == null)
                throw new ArgumentNullException(nameof(document));

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
        private static readonly XName ClassAttribute = "class";
        private static readonly XName GroupElement = SvgNamespace + "g";
        private static readonly XName TitleElement = SvgNamespace + "title";
        private static readonly XName TextElement = SvgNamespace + "text";
    }
}
