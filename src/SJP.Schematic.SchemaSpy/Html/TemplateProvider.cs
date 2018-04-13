using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using EnumsNET;

namespace SJP.Schematic.SchemaSpy.Html
{
    public class TemplateProvider : ITemplateProvider
    {
        public string GetTemplate(SchemaSpyTemplate template)
        {
            if (!template.IsValid())
                throw new ArgumentException($"The { nameof(SchemaSpyTemplate) } provided must be a valid enum.", nameof(template));

            var resourceName = GetResourceName(template);
            return GetResourceAsString(resourceName);
        }

        private static string GetResourceName(SchemaSpyTemplate template)
        {
            var templateKey = template.ToString();
            if (!_templateResourceNames.Contains(templateKey))
                throw new NotSupportedException($"The given template: { templateKey } is not a supported template.");

            var templateFileName = templateKey + TemplateExtension;
            return _templateResourceNames.First(name => name.EndsWith(templateFileName));
        }

        private static string GetResourceAsString(string resourceName)
        {
            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName))
            using (var reader = new StreamReader(stream))
                return reader.ReadToEnd();
        }

        private static readonly IEnumerable<string> _templateResourceNames = Assembly.GetExecutingAssembly()
            .GetManifestResourceNames()
            .Where(name => name.Contains(".Html.Templates.") && name.EndsWith(TemplateExtension))
            .ToList();

        private const string TemplateExtension = ".scriban";
    }
}
