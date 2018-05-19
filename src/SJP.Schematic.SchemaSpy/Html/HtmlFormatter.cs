using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using EnumsNET;
using Scriban;
using Scriban.Runtime;
using SJP.Schematic.Core.Extensions;

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
                throw new ArgumentException($"The { nameof(SchemaSpyTemplate) } provided in the template parameter must be a valid enum.", nameof(templateParameter));

            var parsedTemplate = GetTemplate(template);
            if (parsedTemplate.HasErrors)
            {
                var message = parsedTemplate.Messages.Select(m => m.Message).Join(", ");
                throw new InvalidOperationException("Unable to render the template as it is not valid. Error messages: " + message);
            }

            var scriptObject = new ScriptObject();
            scriptObject.Import(templateParameter, renamer: MemberRenamer);

            var context = new TemplateContext
            {
                LoopLimit = int.MaxValue,
                MemberRenamer = MemberRenamer,
                StrictVariables = true
            };
            context.PushGlobal(scriptObject);

            return parsedTemplate.Render(context);
        }

        protected Template GetTemplate(SchemaSpyTemplate template)
        {
            if (!template.IsValid())
                throw new ArgumentException($"The { nameof(SchemaSpyTemplate) } provided must be a valid enum.", nameof(template));

            if (_templateCache.TryGetValue(template, out var result))
                return result;

            var templateText = TemplateProvider.GetTemplate(template);
            var parsedTemplate = Template.Parse(templateText);
            _templateCache.TryAdd(template, parsedTemplate);

            return parsedTemplate;
        }

        private static string MemberRenamer(MemberInfo member) => member.Name;

        private readonly ConcurrentDictionary<SchemaSpyTemplate, Template> _templateCache = new ConcurrentDictionary<SchemaSpyTemplate, Template>();
    }
}
