using System;
using EnumsNET;
using RazorLight;

namespace SJP.Schematic.Reporting.Html
{
    internal class HtmlFormatter : IHtmlFormatter
    {
        public HtmlFormatter(ITemplateProvider templateProvider)
        {
            if (templateProvider == null)
                throw new ArgumentNullException(nameof(templateProvider));

            var project = new ReportingRazorProject(templateProvider);
            _engine = new RazorLightEngineBuilder()
                .UseProject(project)
                .SetOperatingAssembly(typeof(HtmlFormatter).Assembly)
                .UseMemoryCachingProvider()
                .Build();
        }

        public string RenderTemplate<T>(T templateParameter) where T : ITemplateParameter
        {
            if (templateParameter == null)
                throw new ArgumentNullException(nameof(templateParameter));

            var template = templateParameter.Template;
            if (!template.IsValid())
                throw new ArgumentException($"The { nameof(ReportTemplate) } provided in the template parameter must be a valid enum.", nameof(templateParameter));

            var path = template.ToString();
            return _engine.CompileRenderAsync(path, templateParameter).GetAwaiter().GetResult();
        }

        private readonly RazorLightEngine _engine;
    }
}
