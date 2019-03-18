
using System;
using System.Web;
using LanguageExt;
using SJP.Schematic.Core;

namespace SJP.Schematic.Reporting.Html.ViewModels
{
    /// <summary>
    /// Internal. Not intended to be used outside of this assembly. Only required for templating.
    /// </summary>
    public sealed class Synonym : ITemplateParameter
    {
        public Synonym(
            Identifier synonymName,
            Identifier targetName,
            Option<Uri> targetUrl,
            string rootPath
        )
        {
            if (synonymName == null)
                throw new ArgumentNullException(nameof(synonymName));
            if (targetName == null)
                throw new ArgumentNullException(nameof(targetName));

            Name = synonymName.ToVisibleName();
            var targetNameText = targetName.ToVisibleName();
            TargetText = targetUrl.Match(
                uri => $"<a href=\"{ uri }\">{ HttpUtility.HtmlEncode(targetNameText) }</a>",
                () => HttpUtility.HtmlEncode(targetNameText)
            );
            RootPath = rootPath ?? throw new ArgumentNullException(nameof(rootPath));
        }

        public ReportTemplate Template { get; } = ReportTemplate.Synonym;

        public string Name { get; }

        public HtmlString TargetText { get; }

        public string RootPath { get; }
    }
}
