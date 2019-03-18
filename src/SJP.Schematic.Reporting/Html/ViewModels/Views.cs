using System;
using System.Collections.Generic;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Reporting.Html.ViewModels
{
    /// <summary>
    /// Internal. Not intended to be used outside of this assembly. Only required for templating.
    /// </summary>
    public sealed class Views : ITemplateParameter
    {
        public Views(IEnumerable<Main.View> views)
        {
            if (views == null || views.AnyNull())
                throw new ArgumentNullException(nameof(views));

            ViewsCount = views.UCount();
            ViewsTableClass = ViewsCount > 0 ? CssClasses.DataTableClass : string.Empty;
            AllViews = views;
        }

        public ReportTemplate Template { get; } = ReportTemplate.Views;

        public uint ViewsCount { get; }

        public HtmlString ViewsTableClass { get; }

        public IEnumerable<Main.View> AllViews { get; }
    }
}
