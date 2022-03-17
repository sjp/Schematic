using System;
using System.Threading;
using System.Threading.Tasks;
using EnumsNET;
using RazorLight;

namespace SJP.Schematic.Reporting.Html;

internal sealed class HtmlFormatter : IHtmlFormatter
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

    public Task<string> RenderTemplateAsync<T>(T templateParameter, CancellationToken cancellationToken = default) where T : ITemplateParameter
    {
        if (templateParameter == null)
            throw new ArgumentNullException(nameof(templateParameter));

        var template = templateParameter.Template;
        if (!template.IsValid())
            throw new ArgumentException($"The { nameof(ReportTemplate) } provided in the template parameter must be a valid enum.", nameof(templateParameter));

        var templatePath = template.ToString();
        return _engine.CompileRenderAsync(templatePath, templateParameter);
    }

    private readonly RazorLightEngine _engine;
}
