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

            var tableNodes = document.Descendants(SvgNamespace + "g")
                .Where(g => g.Attribute("class") != null && g.Attribute("class").Value == "node")
                .ToList();

            foreach (var tableNode in tableNodes)
            {
                var titleNode = tableNode.Descendants(SvgNamespace + "title").FirstOrDefault();
                if (titleNode == null)
                    continue;

                var foundTableTextNode = false;
                var textNodes = tableNode.Descendants(SvgNamespace + "text");
                foreach (var textNode in textNodes)
                {
                    var value = textNode.Value;
                    if (!foundTableTextNode && value == "Table")
                    {
                        foundTableTextNode = true;
                        continue;
                    }

                    titleNode.SetValue(value);
                    break;
                }
            }
        }

        private static readonly XNamespace SvgNamespace = "http://www.w3.org/2000/svg";
    }
}
