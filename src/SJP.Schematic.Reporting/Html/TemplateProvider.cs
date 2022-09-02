using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Reflection;
using EnumsNET;
using Microsoft.Extensions.FileProviders;

namespace SJP.Schematic.Reporting.Html;

internal sealed class TemplateProvider : ITemplateProvider
{
    public string GetTemplate(ReportTemplate template)
    {
        if (!template.IsValid())
            throw new ArgumentException($"The { nameof(ReportTemplate) } provided must be a valid enum.", nameof(template));
        if (Cache.TryGetValue(template, out var cachedTemplate))
            return cachedTemplate;

        var resource = GetResource(template);
        var templateStr = GetResourceAsString(resource);

        Cache.TryAdd(template, templateStr);
        return templateStr;
    }

    private static IFileInfo GetResource(ReportTemplate template)
    {
        var templateKey = template.ToString();
        var templateFileName = templateKey + TemplateExtension;

        var resourceFiles = _fileProvider.GetDirectoryContents("/");
        var templateResource = resourceFiles.FirstOrDefault(r => r.Name.EndsWith(templateFileName, StringComparison.Ordinal));
        if (templateResource == null)
            throw new NotSupportedException($"The given template: { templateKey } is not a supported template.");

        return templateResource;
    }

    private static string GetResourceAsString(IFileInfo fileInfo)
    {
        ArgumentNullException.ThrowIfNull(fileInfo);

        Stream? stream = null;
        try
        {
            stream = fileInfo.CreateReadStream();
            using var reader = new StreamReader(stream);
            stream = null;
            return reader.ReadToEnd();
        }
        finally
        {
            stream?.Dispose();
        }
    }

    private static readonly ConcurrentDictionary<ReportTemplate, string> Cache = new();

    private static readonly IFileProvider _fileProvider = new EmbeddedFileProvider(Assembly.GetExecutingAssembly(), Assembly.GetExecutingAssembly().GetName().Name + ".Html.Templates");
    private const string TemplateExtension = ".cshtml";
}