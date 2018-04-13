using System;
using System.Collections.Generic;
using EnumsNET;
using Scriban;

namespace SJP.Schematic.SchemaSpy.Html
{
    public class HtmlFormatter : IHtmlFormatter
    {
        public HtmlFormatter(ITemplateProvider templateProvider)
        {
            TemplateProvider = templateProvider ?? throw new ArgumentNullException(nameof(templateProvider));
        }

        protected ITemplateProvider TemplateProvider { get; }

        public string RenderTemplate(ITemplateParameter templateParameter)
        {
            if (templateParameter == null)
                throw new ArgumentNullException(nameof(templateParameter));

            var template = templateParameter.Template;
            if (!template.IsValid())
                throw new ArgumentException($"The { nameof(SchemaSpyTemplate) } provided in the template parameter must be a valid enum.", nameof(template));

            var parsedTemplate = GetTemplate(template);
            return parsedTemplate.Render(templateParameter, member => member.Name);
        }

        protected Template GetTemplate(SchemaSpyTemplate template)
        {
            if (!template.IsValid())
                throw new ArgumentException($"The { nameof(SchemaSpyTemplate) } provided must be a valid enum.", nameof(template));

            if (_templateCache.ContainsKey(template))
                return _templateCache[template];

            var templateText = TemplateProvider.GetTemplate(template);
            var parsedTemplate = Template.Parse(templateText);
            _templateCache[template] = parsedTemplate;

            return parsedTemplate;
        }

        private readonly IDictionary<SchemaSpyTemplate, Template> _templateCache = new Dictionary<SchemaSpyTemplate, Template>();
    }
}
