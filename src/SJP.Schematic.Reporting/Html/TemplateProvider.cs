using System;
using System.IO;
using System.Linq;
using System.Reflection;
using EnumsNET;
using Microsoft.Extensions.FileProviders;

namespace SJP.Schematic.Reporting.Html
{
    internal class TemplateProvider : ITemplateProvider
    {
        public string GetTemplate(ReportTemplate template)
        {
            if (!template.IsValid())
                throw new ArgumentException($"The { nameof(ReportTemplate) } provided must be a valid enum.", nameof(template));

            var resource = GetResource(template);
            return GetResourceAsString(resource);
        }

        private static IFileInfo GetResource(ReportTemplate template)
        {
            var templateKey = template.ToString();
            var templateFileName = templateKey + TemplateExtension;

            var resourceFiles = _fileProvider.GetDirectoryContents("/");
            var templateResource = resourceFiles.FirstOrDefault(r => r.Name.EndsWith(templateFileName));
            if (templateResource == null)
                throw new NotSupportedException($"The given template: { templateKey } is not a supported template.");

            return templateResource;
        }

        private static string GetResourceAsString(IFileInfo fileInfo)
        {
            if (fileInfo == null)
                throw new ArgumentNullException(nameof(fileInfo));

            using (var stream = fileInfo.CreateReadStream())
            using (var reader = new StreamReader(stream))
                return reader.ReadToEnd();
        }

        private static readonly IFileProvider _fileProvider = new EmbeddedFileProvider(Assembly.GetExecutingAssembly(), Assembly.GetExecutingAssembly().GetName().Name + ".Html.Templates");
        private const string TemplateExtension = ".cshtml";
    }
}
